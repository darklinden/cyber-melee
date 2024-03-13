
namespace App
{
    public class EventBus
    {
        public event Events.OnBattleStartLoading OnBattleStartLoading;
        public void BattleStartLoading()
        {
            OnBattleStartLoading?.Invoke();
        }

        public event Events.OnBattleLoadingProgress OnBattleLoadingProgress;
        public void BattleLoadingProgress(ulong playerId, int progress)
        {
            OnBattleLoadingProgress?.Invoke(playerId, progress);
        }

        public event Events.OnBattleLoadingCompleted OnBattleLoadingCompleted;
        public void BattleLoadingCompleted()
        {
            OnBattleLoadingCompleted?.Invoke();
        }

        public event Events.OnSkillActionBroadcast OnSkillActionBroadcast;
        public void SkillActionBroadcast(Proto.SkillType skillType, ulong playerId, long tick)
        {
            OnSkillActionBroadcast?.Invoke(skillType, playerId, tick);
        }

        // ----------------- GamePlayFrame -----------------
        public event Events.OnDispatchPlayPlayerEnter OnDispatchPlayPlayerEnter;
        public event Events.OnDispatchPlayPlayerHPChanged OnDispatchPlayPlayerHPChanged;
        public event Events.OnDispatchPlayPlayerDefeat OnDispatchPlayPlayerDefeat;
        public event Events.OnDispatchPlaySkillStart OnDispatchPlaySkillStart;
        public event Events.OnDispatchPlaySkillAffect OnDispatchPlaySkillAffect;
        public event Events.OnDispatchPlaySkillEnded OnDispatchPlaySkillEnded;
        public event Events.OnDispatchPlayProjectileSpawn OnDispatchPlayProjectileSpawn;
        public event Events.OnDispatchPlayProjectileChange OnDispatchPlayProjectileChange;
        public event Events.OnDispatchPlayProjectileArrive OnDispatchPlayProjectileArrive;
        public event Events.OnDispatchPlayProjectileRemove OnDispatchPlayProjectileRemove;
        public event Events.OnDispatchPlayBulletBufferChanged OnDispatchPlayBulletBufferChanged;
        public void DispatchPlay(long frame, Lockstep.IGamePlayFrame gamePlayFrame)
        {
            switch (gamePlayFrame.PlayType)
            {
                case Lockstep.GamePlayFrameType.PlayerEnter:
                    OnDispatchPlayPlayerEnter?.Invoke(frame, (Lockstep.GamePlayFrame_PlayerEnter)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.PlayerHPChanged:
                    OnDispatchPlayPlayerHPChanged?.Invoke(frame, (Lockstep.GamePlayFrame_PlayerHPChanged)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.PlayerDefeat:
                    OnDispatchPlayPlayerDefeat?.Invoke(frame, (Lockstep.GamePlayFrame_PlayerDefeat)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.SkillStart:
                    OnDispatchPlaySkillStart?.Invoke(frame, (Lockstep.GamePlayFrame_SkillStart)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.SkillAffect:
                    OnDispatchPlaySkillAffect?.Invoke(frame, (Lockstep.GamePlayFrame_SkillAffect)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.SkillEnded:
                    OnDispatchPlaySkillEnded?.Invoke(frame, (Lockstep.GamePlayFrame_SkillEnded)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.ProjectileSpawn:
                    OnDispatchPlayProjectileSpawn?.Invoke(frame, (Lockstep.GamePlayFrame_ProjectileSpawn)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.ProjectileChange:
                    OnDispatchPlayProjectileChange?.Invoke(frame, (Lockstep.GamePlayFrame_ProjectileChange)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.ProjectileArrive:
                    OnDispatchPlayProjectileArrive?.Invoke(frame, (Lockstep.GamePlayFrame_ProjectileArrive)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.ProjectileRemove:
                    OnDispatchPlayProjectileRemove?.Invoke(frame, (Lockstep.GamePlayFrame_ProjectileRemove)gamePlayFrame);
                    break;
                case Lockstep.GamePlayFrameType.BulletBufferChanged:
                    OnDispatchPlayBulletBufferChanged?.Invoke(frame, (Lockstep.GamePlayFrame_BulletBufferChanged)gamePlayFrame);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }

        // ----------------- Service -----------------

        public event Events.ServiceStateChanged OnServiceStateChanged;
        public void ServiceStateChanged(Service.GameServiceState fromState, Service.GameServiceState toState)
        {
            OnServiceStateChanged?.Invoke(fromState, toState);
        }
    }
}
