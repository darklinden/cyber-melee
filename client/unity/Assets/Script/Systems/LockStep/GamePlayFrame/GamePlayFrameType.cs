namespace Lockstep
{
    // 需要“播放”的动作类型
    public enum GamePlayFrameType
    {
        None,

        // 角色出现
        PlayerEnter,

        // 播放伤害或治疗生效
        PlayerHPChanged,

        // 播放死亡 - 死亡动画等
        PlayerDefeat,

        // 使用技能 动作启动
        SkillStart,

        // 使用技能 效果触发
        SkillAffect,

        // 使用技能 动作结束
        SkillEnded,

        // 投射物生成
        ProjectileSpawn,

        // 投射物改变
        ProjectileChange,

        // 投射物到达
        ProjectileArrive,

        // 投射物消失
        ProjectileRemove,

        BulletBufferChanged,
    }
}