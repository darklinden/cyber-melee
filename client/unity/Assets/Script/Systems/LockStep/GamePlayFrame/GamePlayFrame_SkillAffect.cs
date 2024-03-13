using Proto;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_SkillAffect : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.SkillAffect;

        public ulong CharacterId;
        public long Frame;
        public SkillType SkillType;

        private bool m_Disposed = false;
        public static GamePlayFrame_SkillAffect Create(
            ulong characterId,
            long frame,
            SkillType skillType)
        {
            var p = AnyPool<GamePlayFrame_SkillAffect>.Get();
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
            AnyPool<GamePlayFrame_SkillAffect>.Return(this);
        }
    }
}