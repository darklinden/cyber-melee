using Proto;

namespace Battle
{
    internal struct RunningSkill
    {
        internal SkillType SkillType;
        internal long StartFrame;
        internal long AffectFrame;
        internal long EndFrame;
        // Other Data

        internal int CD;
        internal ProjectileType ProjectileType;

        public int OrbitIndex;
    }
}