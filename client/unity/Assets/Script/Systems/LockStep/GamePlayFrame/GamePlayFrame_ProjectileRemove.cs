using Proto;
using UnityEngine;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_ProjectileRemove : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.ProjectileRemove;

        public int ProjectileId { get; internal set; }
        public ProjectileType ProjectileType { get; internal set; }

        private bool m_Disposed = false;
        public static GamePlayFrame_ProjectileRemove Create(
            int projectileId,
            ProjectileType projectileType)
        {
            var p = AnyPool<GamePlayFrame_ProjectileRemove>.Get();
            p.m_Disposed = false;
            p.ProjectileId = projectileId;
            p.ProjectileType = projectileType;
            return p;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            ProjectileId = 0;
            ProjectileType = ProjectileType.NONE;
            AnyPool<GamePlayFrame_ProjectileRemove>.Return(this);
        }
    }
}