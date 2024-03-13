
using Lockstep;

namespace App
{
    public static class Events
    {
        public delegate void OnBattleStartLoading();
        public delegate void OnBattleLoadingProgress(ulong playerId, int progress);
        public delegate void OnBattleLoadingCompleted();
        public delegate void OnSkillActionBroadcast(Proto.SkillType skillType, ulong playerId, long tick);

        // ----------------- GamePlayFrame -----------------
        public delegate void OnDispatchPlayPlayerEnter(long frame, GamePlayFrame_PlayerEnter gamePlayFrame_CharacterEnter);
        public delegate void OnDispatchPlayPlayerHPChanged(long frame, GamePlayFrame_PlayerHPChanged gamePlayFrame_CharacterHurt);
        public delegate void OnDispatchPlayPlayerDefeat(long frame, GamePlayFrame_PlayerDefeat gamePlayFrame_CharacterDead);
        public delegate void OnDispatchPlaySkillStart(long frame, GamePlayFrame_SkillStart gamePlayFrame_SkillStart);
        public delegate void OnDispatchPlaySkillAffect(long frame, GamePlayFrame_SkillAffect gamePlayFrame_SkillAffect);
        public delegate void OnDispatchPlaySkillEnded(long frame, GamePlayFrame_SkillEnded gamePlayFrame_SkillEnded);
        public delegate void OnDispatchPlayProjectileSpawn(long frame, GamePlayFrame_ProjectileSpawn gamePlayFrame_ProjectileSpawn);
        public delegate void OnDispatchPlayProjectileChange(long frame, GamePlayFrame_ProjectileChange gamePlayFrame_ProjectileChange);
        public delegate void OnDispatchPlayProjectileArrive(long frame, GamePlayFrame_ProjectileArrive gamePlayFrame_ProjectileArrive);
        public delegate void OnDispatchPlayProjectileRemove(long frame, GamePlayFrame_ProjectileRemove gamePlayFrame_ProjectileRemove);
        public delegate void OnDispatchPlayBulletBufferChanged(long frame, GamePlayFrame_BulletBufferChanged gamePlayFrame_BulletBufferChanged);

        // ----------------- Service -----------------
        public delegate void ServiceStateChanged(Service.GameServiceState fromState, Service.GameServiceState toState);
    }
}