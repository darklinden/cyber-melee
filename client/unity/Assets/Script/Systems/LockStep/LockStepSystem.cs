using System.Collections.Generic;
using Wtf;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using App;
using Battle;

namespace Lockstep
{
    public class LockStepSystem : MonoBehaviour, ISystemBase
    {
        // 时间线 | ------------------------------------- |
        // 逻辑帧 | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |
        //       ^播放逻辑帧       ^已计算的逻辑帧
        //       |   当前时间窗口  |   计算时间窗口, 用于计算下一逻辑帧
        //              ^ 可接收用户操作, 操作将在下个窗口内生效

        // 服务器下发的逻辑帧 计算后一个逻辑帧需要前一个逻辑帧的服务器确认
        public long ServerProcessedGameFrame { get; private set; } = -1;

        // 已计算的逻辑帧 只有计算过的逻辑帧才能播放
        public long CalcProcessedGameFrame { get; private set; } = 0;

        // 播放逻辑帧
        public long PlayingGameFrame { get; private set; } = 0;
        public long PlayingGameFrameTick { get; private set; } = 0;

        // 游戏开始时间
        public long GameStartServerTick { get; private set; } = 0;

        private Transform m_Tm = null;
        public Transform Tm
        {
            get
            {
                if (m_Tm == null)
                {
                    m_Tm = transform;
                }
                return m_Tm;
            }
        }

        public bool IsInitialized { get; private set; } = false;

        public void Initialize()
        {
            PlayingGameFrame = -1;
            PlayingGameFrameTick = 0;
            CalcProcessedGameFrame = -1;
            ServerProcessedGameFrame = -1;

            IsInitialized = true;
        }

        public UniTask AsyncInitialize()
        {
            throw new NotImplementedException();
        }

        public void Deinitialize()
        {

        }

        private TimeSystem m_TimeSystem = null;
        public TimeSystem TimeSystem => m_TimeSystem != null ? m_TimeSystem : (m_TimeSystem = Context.Inst.GetSystem<TimeSystem>());
        private Battle.BattleSystem m_BattleSystem = null;
        public Battle.BattleSystem BattleSystem => m_BattleSystem != null ? m_BattleSystem : (m_BattleSystem = Context.Inst.GetSystem<Battle.BattleSystem>());
        private Battle.ProjectileCalcSystem m_ProjectileSystem = null;
        private Battle.ProjectileCalcSystem ProjectileSystem => m_ProjectileSystem != null ? m_ProjectileSystem : (m_ProjectileSystem = Context.Inst.GetSystem<ProjectileCalcSystem>());
        private Service.ServiceSystem m_ServiceSystem = null;
        private Service.ServiceSystem ServiceSystem => m_ServiceSystem != null ? m_ServiceSystem : (m_ServiceSystem = Context.Inst.GetSystem<Service.ServiceSystem>());

        public Dictionary<string, ISystemBase> SubSystems => null;

        // 在游戏启动前预处理, 首个逻辑帧不处理用户操作
        public void ResetState(long gameStartServerTick)
        {
            GameStartServerTick = gameStartServerTick;
            PlayingGameFrame = -1;
            PlayingGameFrameTick = 0;
            CalcProcessedGameFrame = -1;
            ServerProcessedGameFrame = -1;
            m_GameplayTimePassed = 0;
        }

        // 在游戏启动前预处理, 首个逻辑帧不处理用户操作
        public void PrepareGame()
        {
            CalcProcessedGameFrame = 0;
            PlayingGameFrameTick = GameStartServerTick;
            LockStepCalcPlay(CalcProcessedGameFrame);
        }

        /// <summary>
        /// lockstep update
        /// </summary>
        /// <param name="serverTime"></param>
        /// <param name="frameMsPassed"></param>
        public void RoutineLockStepForward()
        {
            // 计算服务器已确认的逻辑帧
            var frame = ServerProcessedGameFrame;
            while (CalcProcessedGameFrame < frame)
            {
                CalcProcessedGameFrame += 1;
                LockStepCalcPlay(CalcProcessedGameFrame);
            }

            PlayerFrameUtil.ClearFinishedData(BattleSystem.PlayerCalc, this);
            ProjectileSystem.ClearFinishedData(this, TimeSystem, BattleSystem);
        }

        // 玩家经服务器同步的操作, 由服务器下发, 需要在后续帧计算中使用
        // 如果未收到服务器同步的操作, 则暂停游戏
        public Dictionary<long, XList<IGameActionFrame>> LockStepServerSyncQueue = new Dictionary<long, XList<IGameActionFrame>>();
        internal void AddActions(Proto.ServerBroadcastBattleAction actions)
        {
            var time = (long)actions.ServerTime;
            var serverFrame = (time - GameStartServerTick) / Constants.FRAME_LENGTH_MS;
            var targetFrame = serverFrame + Constants.FRAME_ACTION_DELAY_COUNT;

            XList<IGameActionFrame> actionList;
            if (!LockStepServerSyncQueue.TryGetValue(targetFrame, out actionList))
            {
                actionList = XList<IGameActionFrame>.Get();
                LockStepServerSyncQueue.Add(targetFrame, actionList);
            }

            if (ServerProcessedGameFrame < serverFrame) ServerProcessedGameFrame = serverFrame;
            if (actions.ActionsLength == 0)
            {
                return;
            }

            Log.D("AddActions for frame", targetFrame, "PlayingGameFrame", PlayingGameFrame, "CalcProcessedGameFrame", CalcProcessedGameFrame);

            for (int i = 0; i < actions.ActionsLength; i++)
            {
                var action = actions.Actions(i).Value;
                var frameAction = GameActionFrameFactory.Create(
                    action.PlayerId,
                    action.ActionType,
                    action.ActionParamsLength,
                    action.ActionParams);
                actionList.Add(frameAction);
                if (frameAction.ActionType == GameActionFrameType.Skill)
                {
                    var skillAction = (GameActionFrameSkill)frameAction;
                    Context.Inst.EventBus.SkillActionBroadcast(skillAction.SkillType, skillAction.PlayerId, time);
                }
            }
        }

        // 计算出的逻辑帧操作
        // <帧号, 帧操作>
        // 比如帧1执行技能，计算到帧10出现伤害，那么帧1的操作中包含技能，帧10的操作中包含伤害
        private Dictionary<long, ListGamePlayFrame> LockStepPlayFrames = new Dictionary<long, ListGamePlayFrame>();

        // 计算指定时间的帧操作
        private void LockStepCalcPlay(long frame)
        {
            Log.D("LockStep Prepare Frame", frame);

            var battleSystem = BattleSystem;
            var characterMgr = battleSystem.PlayerCalc;
            var projectileMgr = ProjectileSystem;

            XList<IGameActionFrame> actionList;
            if (!LockStepServerSyncQueue.TryGetValue(frame, out actionList))
            {
                actionList = null;
            }

            // 获取之前创建的逻辑帧, 如果没有则创建
            ListGamePlayFrame framePlay;
            if (!LockStepPlayFrames.TryGetValue(frame, out framePlay))
            {
                framePlay = ListGamePlayFrame.Get();
            }

            // 需要做的事情

            // 继承上一帧的属性
            LockStepPrepareChars.Run(actionList, frame, framePlay, characterMgr);

            // 已经剔除了死亡角色，计算了buff信息

            // 处理飞行物
            LockStepDealWithProjectiles.Run(
                actionList,
                frame,
                framePlay,
                characterMgr,
                projectileMgr,
                ServiceSystem);

            // 处理技能和移动
            LockStepSkill.Run(this, frame, framePlay, characterMgr, projectileMgr);

            LockStepPlayFrames[frame] = framePlay;

            Log.D("LockStep Prepare Frame End", frame);
        }

        private long m_GameplayTimePassed = 0;
        public long GameplayTimePassed => m_GameplayTimePassed;

        public void ResetGameTimePassed()
        {
            m_GameplayTimePassed = 0;
        }

        private void FixedUpdate()
        {
            if (TimeSystem.HasSynced == false) return;
            if (BattleSystem.IsBattleRunning == false) return;

            // 当前帧的起始时间
            var nowTick = TimeSystem.ServerTickTimeMs;
            // 游戏已经开始的时间 = 当前帧的时间 - 游戏开始时间
            var startedTick = nowTick - GameStartServerTick;
            // 如果游戏还没开始, 则返回
            if (startedTick < 0) return;

            // 当前帧的时间余数
            var modTick = startedTick % Constants.FRAME_LENGTH_MS;
            // 当前逻辑帧号 = (游戏已经开始的时间 - 时间余数) / 逻辑帧长度
            var nowFrame = (startedTick - modTick) / Constants.FRAME_LENGTH_MS;

            // Log.D("LockStep FixedUpdate", nowFrame);
            // 检测是否有帧事件需要触发
            while (PlayingGameFrame < CalcProcessedGameFrame)
            {
                // seek to next frame
                PlayingGameFrame += 1;
                PlayingGameFrameTick += Constants.FRAME_LENGTH_MS;
                m_GameplayTimePassed += Constants.FRAME_LENGTH_MS;

                Log.D("PlayFrame", PlayingGameFrame, "nowFrame", nowFrame, "CalcProcessedGameFrame", CalcProcessedGameFrame);

                // play calculated frames
                // update characters
                if (LockStepPlayFrames.TryGetValue(PlayingGameFrame, out var framePlay))
                {
                    LockStepPlayFrames.Remove(PlayingGameFrame);

                    if (framePlay.Plays.Count > 0)
                    {
                        for (int i = 0; i < framePlay.Plays.Count; i++)
                        {
                            var play = framePlay.Plays[i];
                            Context.Inst.EventBus.DispatchPlay(PlayingGameFrame, play);
                            play.Dispose();
                        }
                    }

                    framePlay.Dispose();
                }
                BattleSystem.CheckBattleEnded(PlayingGameFrame);
            }
        }
    }
}