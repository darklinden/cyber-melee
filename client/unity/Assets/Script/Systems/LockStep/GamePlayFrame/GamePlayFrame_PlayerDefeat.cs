using Proto;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_PlayerDefeat : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.PlayerDefeat;

        public ulong CharacterId { get; internal set; }

        private bool m_Disposed = false;
        internal static IGamePlayFrame Create(
            ulong characterId
        )
        {
            var p = AnyPool<GamePlayFrame_PlayerDefeat>.Get();
            p.m_Disposed = false;
            p.CharacterId = characterId;
            return p;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            CharacterId = 0;
            AnyPool<GamePlayFrame_PlayerDefeat>.Return(this);
        }
    }
}