using System;
using Proto;

namespace Lockstep
{
    public struct GameActionFrameSkill : IGameActionFrame
    {
        public readonly GameActionFrameType ActionType => GameActionFrameType.Skill;
        public ulong PlayerId { get; set; }

        public SkillType SkillType;
        public int OrbitIndex;

        public static IGameActionFrame Create(
            ulong playerId,
            int paramCount,
            Func<int, int> getParam)
        {
            if (paramCount != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(paramCount), paramCount, null);
            }

            return new GameActionFrameSkill
            {
                PlayerId = playerId,
                SkillType = (SkillType)getParam(0),
                OrbitIndex = getParam(1)
            };
        }
    }
}
