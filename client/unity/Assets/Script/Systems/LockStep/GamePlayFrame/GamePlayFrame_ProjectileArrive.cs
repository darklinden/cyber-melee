using Proto;
using UnityEngine;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_ProjectileArrive : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.ProjectileArrive;

        public int ProjectileId { get; internal set; }
        public ulong CharacterFrom { get; internal set; }
        public ProjectileType ProjectileType { get; internal set; }
        public ulong CharacterTarget { get; internal set; }
        public Vector3 Position { get; internal set; }
        public long Frame { get; internal set; }

        private bool m_Disposed = false;
        public static GamePlayFrame_ProjectileArrive Create(
            int projectileId,
            ProjectileType projectileType,
            ulong characterFrom,
            ulong characterTarget,
            Vector3 position)
        {
            var p = AnyPool<GamePlayFrame_ProjectileArrive>.Get();
            p.m_Disposed = false;
            p.ProjectileId = projectileId;
            p.ProjectileType = projectileType;
            p.CharacterFrom = characterFrom;
            p.CharacterTarget = characterTarget;
            p.Position = position;
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
            AnyPool<GamePlayFrame_ProjectileArrive>.Return(this);
        }
    }
}