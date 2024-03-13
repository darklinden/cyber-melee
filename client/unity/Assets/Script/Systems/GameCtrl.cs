using System.Collections;
using UnityEngine;
using Google.FlatBuffers;
using Wtf;
using Cysharp.Threading.Tasks;
using App;
using Proto;
using Battle;
using System.Collections.Generic;
using UserInterface;
using System;
using Lockstep;

public class GameCtrl : MonoBehaviour, ISystemBase
{
    public Transform Tm => transform;

    public bool IsInitialized { get; private set; } = false;

    public Dictionary<string, ISystemBase> SubSystems => null;

    public void Initialize()
    {
        EventDispatcher.RemoveAllListeners(this);
        EventDispatcher.AddListener<ByteBuffer>(Structs.Lockstep.BattleStartBroadcast.route, OnBattleStartBroadcast, this);
        EventDispatcher.AddListener<ByteBuffer>(Structs.Lockstep.BattleLoadProgressBroadcast.route, OnBattleLoadProgressBroadcast, this);
        EventDispatcher.AddListener<ByteBuffer>(Structs.Lockstep.BattleStartedBroadcast.route, OnBattleStartedBroadcast, this);
        EventDispatcher.AddListener<ByteBuffer>(Structs.Lockstep.BattleActionBroadcast.route, OnBattleActionBroadcast, this);
        EventDispatcher.AddListener<ByteBuffer>(Structs.Lockstep.BattleShouldFinishBroadcast.route, OnBattleShouldFinishBroadcast, this);
        EventDispatcher.AddListener<ByteBuffer>(Structs.Lockstep.BattleFinishedBroadcast.route, OnBattleFinishedBroadcast, this);
        EventDispatcher.AddListener<ByteBuffer>(Structs.Lockstep.BattleReconnectedGameState.route, OnBattleReconnectedGameState, this);

        IsInitialized = true;
    }

    public UniTask AsyncInitialize()
    {
        throw new System.NotImplementedException();
    }

    public void Deinitialize()
    {
        EventDispatcher.RemoveAllListeners(this);
    }

    // 请求进入战斗排队
    public async UniTask RequestEnterBattle(string username, int characterId)
    {
        var service = Context.Inst.GetSystem<Service.ServiceSystem>();
        var enterSuccess = await service.AsyncEnter(username, characterId);
        if (enterSuccess)
        {
            var pi = service.Data.PlayerInfo;
            Log.D("WaitForBattleStart", pi.PlayerId, pi.NickName);
            Toast.ShowLoading("等待玩家加入");
        }
        else
        {
            Log.D("RequestEnterBattle", "failed");
        }
    }

    // 收到开始战斗消息，开始加载战斗场景
    private void OnBattleStartBroadcast(ByteBuffer e)
    {
        var result = ServerBroadcastBattleStart.GetRootAsServerBroadcastBattleStart(e);

#if SERVICE_LOG
        Log.D("OnBattleStartBroadcast", result.UnPack());
#endif

        var serviceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();
        serviceSystem.Data.BattleInfo.SetData(result, serviceSystem.Data.PlayerInfo.PlayerId);

        StartCoroutine(RoutineLoadBattle(serviceSystem.Data.BattleInfo));
    }

    // 加载战斗场景，同步加载进度
    private GameObject m_LoadingObj = null;
    private IEnumerator RoutineLoadBattle(Service.BattleInfo battleInfo)
    {
        var loading = Resources.Load<GameObject>("CharactersLoading");
        m_LoadingObj = Instantiate(loading, null);
        DontDestroyOnLoad(m_LoadingObj);

        yield return null;

        Context.Inst.EventBus.BattleStartLoading();

        var battleSystem = Context.Inst.GetSystem<Battle.BattleSystem>();
        var timeSystem = Context.Inst.GetSystem<TimeSystem>();
        var serviceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();

        // 切换战斗场景
        yield return LoadUtil.AsyncLoadScene("Assets/Addrs/en-US/Scenes/Game.unity").ToCoroutine();

        yield return null;

        Toast.HideLoading();

        while (!timeSystem.HasSynced)
        {
            yield return null;
        }

        // 初始化战斗
        yield return battleSystem.AsyncBattleStart().ToCoroutine();

        Context.Inst.EventBus.BattleLoadingCompleted();

        var lastTime = timeSystem.FrameServerTickTimeMs;
        var i = 0;
        var mePlayerId = serviceSystem.Data.PlayerInfo.PlayerId;

        while (i < 100)
        {
            if (timeSystem.FrameServerTickTimeMs - lastTime >= 300)
            {
                lastTime = timeSystem.FrameServerTickTimeMs;
                i += 20;

                Log.D("RequestLoadBattle", mePlayerId, i);

                var builder = FlatBufferBuilder.InstanceDefault;
                var lp = ClientPushBattleLoadProgress.CreateClientPushBattleLoadProgress(
                    builder,
                    i);
                builder.Finish(lp.Value);
                Pinus.Notify(Structs.Lockstep.BattleLoadProgressPush.route, builder.DataBuffer);
                builder.Clear();
            }

            yield return null;
        }
    }

    private Lockstep.LockStepSystem m_LockStepSystem = null;
    private Lockstep.LockStepSystem LockStepSystem => m_LockStepSystem != null ? m_LockStepSystem : (m_LockStepSystem = Context.Inst.GetSystem<Lockstep.LockStepSystem>());

    private Battle.BattleSystem m_BattleSystem = null;
    private Battle.BattleSystem BattleSystem => m_BattleSystem != null ? m_BattleSystem : (m_BattleSystem = Context.Inst.GetSystem<Battle.BattleSystem>());

    // 收到战斗加载进度，同步加载进度 
    private void OnBattleLoadProgressBroadcast(ByteBuffer e)
    {
        var args = Proto.ServerBroadcastBattleLoadProgress.GetRootAsServerBroadcastBattleLoadProgress(e);
        Context.Inst.EventBus.BattleLoadingProgress(args.PlayerId, args.Progress);
    }

    // 收到战斗开始消息，同步开始时间，开始战斗
    private void OnBattleStartedBroadcast(ByteBuffer e)
    {
        var args = ServerBroadcastBattleStarted.GetRootAsServerBroadcastBattleStarted(e);

#if SERVICE_LOG
        Log.D("OnBattleStartedBroadcast", args.UnPack(), TimeSystem.ServerTickTimeMs);
#endif

        var lockstepSystem = Context.Inst.GetSystem<Lockstep.LockStepSystem>();
        lockstepSystem.ResetState((long)args.StartServerTime);

        StartCoroutine(BattleRoutine());
    }

    private TimeSystem m_TimeSystem = null;
    private TimeSystem TimeSystem => m_TimeSystem != null ? m_TimeSystem : (m_TimeSystem = Context.Inst.GetSystem<TimeSystem>());

    private void OnBattleActionBroadcast(ByteBuffer e)
    {
        var action = ServerBroadcastBattleAction.GetRootAsServerBroadcastBattleAction(e);

#if SERVICE_LOG
        Log.D("OnBattleActionBroadcast", TimeSystem.ServerTickTimeMs, action.UnPack());
#endif

        var lockstepSystem = Context.Inst.GetSystem<Lockstep.LockStepSystem>();
        lockstepSystem.AddActions(action);
    }

    private void OnBattleShouldFinishBroadcast(ByteBuffer e)
    {
        var args = ServerBroadcastBattleShouldFinish.GetRootAsServerBroadcastBattleShouldFinish(e);
#if SERVICE_LOG
        Log.D("OnBattleShouldFinishBroadcast", args.UnPack());
#endif

        var timePassed = (long)args.ServerTime - LockStepSystem.GameStartServerTick;
        var frameToFinish = timePassed / Constants.FRAME_LENGTH_MS;
        if (LockStepSystem.CalcProcessedGameFrame >= frameToFinish)
        {
            BattleSystem.CheckBattleTimeOver(frameToFinish);
        }
        else
        {
            Log.D("OnBattleShouldFinishBroadcast", "WaitForFinish", frameToFinish, args.ServerTime);
        }
    }

    private void OnBattleFinishedBroadcast(ByteBuffer e)
    {
        var args = ServerBroadcastBattleFinished.GetRootAsServerBroadcastBattleFinished(e);
#if SERVICE_LOG
        Log.D("OnBattleFinishedBroadcast", args.UnPack());
#endif
        var battleContext = Context.Inst.GetSystem<Battle.BattleSystem>();
        battleContext.BattleFinished(args);
    }

    private void OnBattleReconnectedGameState(ByteBuffer e)
    {
        // TODO: reset game state and speed up to current frame

        // var action = ServerBroadcastBattleAction.GetRootAsServerBroadcastBattleAction(e);
        // var lockstepSystem = Context.Inst.GetSystem<LockStepSystem>();
        // lockstepSystem.AddActions(action);
    }

    internal void PlayerUseSkill(SkillType skillType, int OrbitIndex)
    {
        if (!BattleSystem.IsBattleRunning) return;

        if (skillType == SkillType.Cast)
        {
            Log.D("PlayerUseSkill", skillType, OrbitIndex);
        }
        else
        {
            Log.D("SpecialSkill Use", skillType);
        }

        var builder = FlatBufferBuilder.InstanceDefault;

        ClientPushBattleAction.StartActionParamsVector(builder, 2);
        builder.AddInt(OrbitIndex);
        builder.AddInt((int)skillType);
        var actionParams = builder.EndVector();

        var ba = ClientPushBattleAction.CreateClientPushBattleAction(
            builder,
            (int)Lockstep.GameActionFrameType.Skill,
            actionParams);
        builder.Finish(ba.Value);

#if SERVICE_LOG
        var action = ClientPushBattleAction.GetRootAsClientPushBattleAction(builder.DataBuffer);
        Log.D("PlayerUseSkill", action.UnPack());
#endif

        Pinus.Notify(Structs.Lockstep.BattleActionPush.route, builder.DataBuffer);
        builder.Clear();
    }

    // 战斗逻辑
    private IEnumerator BattleRoutine()
    {
        var battleContext = Context.Inst.GetSystem<Battle.BattleSystem>();
        var lockstepSystem = Context.Inst.GetSystem<Lockstep.LockStepSystem>();
        var timeSystem = Context.Inst.GetSystem<TimeSystem>();
        Log.D("BattleRoutine Should Start");

        while (true)
        {
            if (!timeSystem.HasSynced)
            {
                yield return null;
            }
            else
            {
                // 等待时间同步完成
                break;
            }
        }

        lockstepSystem.PrepareGame();

        if (m_LoadingObj != null)
        {
            Destroy(m_LoadingObj);
            m_LoadingObj = null;
        }

        // 等待开始
        Log.D("BattleRoutine Waiting");
        while (timeSystem.ServerTickTimeMs < lockstepSystem.GameStartServerTick)
        {
            yield return null;
        }

        Log.D("BattleRoutine Started", timeSystem.ServerTickTimeMs, lockstepSystem.PlayingGameFrame);

        battleContext.IsBattleRunning = true;

        // 当当前帧同步时间大于上一次服务器时间帧，计算下一逻辑帧并更新当前服务器时间
        while (true)
        {
            // 如果战斗已经标记为结束则跳出
            if (!battleContext.IsBattleRunning) break;

            lockstepSystem.RoutineLockStepForward();

            // 跳空帧以防卡死
            yield return null;
        }
    }
}
