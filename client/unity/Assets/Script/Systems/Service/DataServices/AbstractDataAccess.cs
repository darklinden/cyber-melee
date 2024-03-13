using Cysharp.Threading.Tasks;
using Google.FlatBuffers;

namespace Service
{
    public abstract class AbstractDataAccess
    {
        internal ServiceSystem Ctx { get; }

        // 数据访问方式 websocket or http
        public abstract DataServiceAccessType AccessType { get; }

        public AbstractDataAccess(ServiceSystem serviceSystem)
        {
            Ctx = serviceSystem;
            Initialize();
        }

        // 初始化对象
        public virtual void Initialize() { }
        // 清理对象
        public virtual void Deinitialize() { }

        public virtual bool IsSocketReady()
        {
            var ready =
                // Http 服务已经登录成功
                Ctx.Data.PlayerInfo.IsValid
                // Ws 服务已经连接成功
                && Ctx.GameServiceState == GameServiceState.GameLoginSuccess;
            return ready;
        }

        public virtual async UniTask<bool> PrepareRequest()
        {
            if (IsSocketReady() == false)
            {
                // 重连逻辑
                var reloginSuccess = await Ctx.ReconnectAndEnter();
                if (reloginSuccess == false)
                {
                    Log.E(GetType().Name, "AsyncRequest Relogin Failed");
                    return false;
                }
            }

            return true;
        }

        public virtual async UniTask AsyncReleaseData(ByteBuffer data, int frameCount)
        {
            await UniTask.DelayFrame(frameCount, PlayerLoopTiming.TimeUpdate);
            data.Dispose();
        }

        /// <summary>
        /// 延迟 3 帧后 将 flatbuffers 使用的 字节流 回池
        /// </summary>
        /// <param name="data"></param>
        public virtual void DelayReleaseData(IFlatbufferObject data)
        {
            AsyncReleaseData(data.ByteBuffer, 3).Forget();
        }

        // 从服务器获取数据
        // public virtual async UniTaskVoid Request() { }
    }
}