using Proto;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_SkillEnded : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.SkillEnded;

        public ulong CharacterId;
        public long Frame;
        public SkillType SkillType;

        private bool m_Disposed = false;
        public static GamePlayFrame_SkillEnded Create(
            ulong characterId,
            long frame,
            SkillType skillType)
        {
            var p = AnyPool<GamePlayFrame_SkillEnded>.Get();
            p.m_Disposed = false;
            p.CharacterId = characterId;
            p.Frame = frame;
            p.SkillType = skillType;
            return p;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            CharacterId = 0;
            Frame = 0;
            SkillType = SkillType.NONE;
            AnyPool<GamePlayFrame_SkillEnded>.Return(this);
        }
    }
}