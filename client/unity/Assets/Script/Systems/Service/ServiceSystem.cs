using App;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wtf;
using UserInterface;
using System.Collections.Generic;

namespace Service
{
    public class ServiceSystem : MonoBehaviour, ISystemBase
    {
        public Transform Tm => throw new System.NotImplementedException();
        public bool IsInitialized { get; private set; } = false;
        public IDataServices Service { get; set; }
        public IDataStorage Data { get; set; }

        public GameServiceState GameServiceState { get; private set; }

        public Dictionary<string, ISystemBase> SubSystems => null;

        public TimeLock ReconnectLock = new TimeLock("ReconnectLock", Constants.SERVICE_CONNECT_TIMEOUT);

        public void Initialize()
        {
            Log.D("ServiceSystem.Initialize");
        }

        public async UniTask AsyncInitialize()
        {
            Log.D("ServiceContext.routineInitialize");

            if (Service == null)
            {
                Service = new DataServices(this);
            }
            Service.Initialize();

            if (Data == null)
            {
                Data = new DataStorage(this);
            }
            Data.Initialize();

            GameServiceState = GameServiceState.Disconnected;

            Context.Inst.EventBus.OnServiceStateChanged += OnServiceStateChanged;

            AsyncKeepConnection().Forget();

            IsInitialized = true;

            await UniTask.Yield();
        }

        private async UniTask AsyncKeepConnection()
        {
            while (true)
            {
                // 间隔时间 检测一次 连接状态和登录状态
                await UniTask.Delay(Constants.SERVICE_CONNECTION_CHECK_INTERVAL);

                // 如果尚未登陆, 则不需要检测
                if (Data.PlayerInfo.IsValid == false) continue;

                // 如果已经连接, 则不需要检测
                if (GameServiceState == GameServiceState.GameLoginSuccess) continue;

                // TODO: 重新连接
            }
        }

        public void Deinitialize()
        {
            Log.D("ServiceContext.Deinitialize");
            if (IsInitialized == false) return;
            IsInitialized = false;
            Context.Inst.EventBus.OnServiceStateChanged -= OnServiceStateChanged;
        }

        private void OnServiceStateChanged(GameServiceState fromState, GameServiceState toState)
        {
            Log.D("ServiceContext.OnServiceStateChanged:", fromState, "->", toState);
        }

        public void GameServiceStateChange(GameServiceState toState)
        {
            var fromState = GameServiceState;
            GameServiceState = toState;
            Context.Inst.EventBus.ServiceStateChanged(fromState, toState);
        }

        internal async UniTask<bool> AsyncEnter(string username, int characterId)
        {
            Toast.ShowLoading();

            await Service.PlayerEnter.AsyncRequest(username, characterId);

            if (GameServiceState == GameServiceState.GameLoginSuccess)
            {
                return true;
            }

            // 登陆失败
            Toast.HideLoading();
            GameServiceStateChange(GameServiceState.Disconnected);
            var panel = await UILoader.AsyncShow(PanelTips.PanelPath, new PanelTips.Data
            {
                Title = "提示",
                Content = "连接失败, 请检查网络后重试",
            });

            await panel.AwaitClose();
            return false;
        }

        // 重新登录 如果返回 false, 则直接退出游戏
        // quiet 是否忽略警告 
        // 返回 true 表示重新登录成功
        internal async UniTask<bool> ReconnectAndEnter()
        {
            // 如果没有登录, 则直接退出
            if (Data.PlayerInfo.IsValid == false)
            {
                Log.E("ReconnectAndEnter failed, not PlayerInfo");
                return false;
            }

            // 开始重新登录
            if (ReconnectLock.IsLocked)
            {
                Log.D("ReconnectAndEnter waiting for lock");
                // 当正在重新登录时, 等待上一个登录完成
                while (ReconnectLock.IsLocked)
                {
                    await UniTask.Delay(200, true);
                }

                // 返回当前状态
                return GameServiceState == GameServiceState.GameLoginSuccess;
            }

            Log.D("ReconnectAndEnter start");

            // 添加超时锁
            ReconnectLock.Lock();

            bool tryReconnect = true;
            bool loginSuccess = false;

            while (tryReconnect)
            {
                loginSuccess = await Service.PlayerReconnect.AsyncRequest();

                // 重新登录成功
                if (loginSuccess) break;

                Toast.HideLoading();
                // 重新登录失败, 弹出提示框
                var panelTip = await UILoader.AsyncShow(PanelTips.PanelPath, new PanelTips.Data
                {
                    Title = "提示",
                    Content = "登陆失败, 请检查网络后重试",
                    TitleBtnOK = "再次尝试",
                    TitleBtnCancel = "返回登录",
                    OnPanelClosed = (PanelTips.Choice choice) =>
                    {
                        tryReconnect = choice == PanelTips.Choice.OK;
                    },
                });

                // 等待面板关闭
                await panelTip.AwaitClose();
                await UniTask.Delay(200, true);

                if (tryReconnect == false)
                {
                    await Service.WsConnect.AsyncDisconnect();
                    Data.ClearAll();
                    break;
                }
            }

            ReconnectLock.Unlock();

            return loginSuccess;
        }

        internal async UniTask AsyncExit()
        {
            await Service.WsConnect.AsyncDisconnect();
            Data.ClearAll();
        }
    }
}
