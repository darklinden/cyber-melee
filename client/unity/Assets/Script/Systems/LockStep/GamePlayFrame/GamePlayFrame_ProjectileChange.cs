using Proto;
using UnityEngine;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_ProjectileChange : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.ProjectileChange;

        public int ProjectileId { get; internal set; }
        public ulong CharacterFrom { get; internal set; }
        public ProjectileType ProjectileType { get; internal set; }
        public ulong CharacterTarget { get; internal set; }
        public Vector3 Position { get; internal set; }
        public long Frame { get; internal set; }

        public EOTType EOTType { get; internal set; }
        public long EOTValue0 { get; internal set; }
        public long EOTValue1 { get; internal set; }

        private bool m_Disposed = false;
        public static GamePlayFrame_ProjectileChange Create(
            int projectileId,
            ProjectileType projectileType,
            ulong characterFrom,
            ulong characterTarget,
            Vector3 position,
            EOTType eotType,
            long eotValue0,
            long eotValue1)
        {
            var p = AnyPool<GamePlayFrame_ProjectileChange>.Get();
            p.m_Disposed = false;
            p.ProjectileId = projectileId;
            p.ProjectileType = projectileType;
            p.CharacterFrom = characterFrom;
            p.CharacterTarget = characterTarget;
            p.Position = position;
            p.EOTType = eotType;
            p.EOTValue0 = eotValue0;
            p.EOTValue1 = eotValue1;
            return p;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            ProjectileId = 0;
            ProjectileType = ProjectileType.NONE;
            CharacterFrom = 0;
            CharacterTarget = 0;
            Position = Vector3.zero;
            EOTType = EOTType.None;
            EOTValue0 = 0;
            EOTValue1 = 0;
            AnyPool<GamePlayFrame_ProjectileChange>.Return(this);
        }
    }
}