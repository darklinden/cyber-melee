using System;

namespace Lockstep
{
    // 定义操作逻辑
    public interface IGameActionFrame
    {
        GameActionFrameType ActionType { get; }
        ulong PlayerId { get; }
    }

    public static class GameActionFrameFactory
    {
        public static IGameActionFrame Create(
            ulong playerId,
            int type,
            int paramCount,
            Func<int, int> getParam)
        {
            switch (type)
            {
                case (int)GameActionFrameType.Skill:
                    return GameActionFrameSkill.Create(playerId, paramCount, getParam);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}