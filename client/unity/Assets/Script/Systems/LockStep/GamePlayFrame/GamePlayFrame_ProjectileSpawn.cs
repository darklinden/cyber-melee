using Proto;
using UnityEngine;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_ProjectileSpawn : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.ProjectileSpawn;

        public int ProjectileId { get; internal set; }
        public ProjectileType ProjectileType { get; internal set; }
        public ulong CharacterFrom { get; internal set; }
        public ulong CharacterTarget { get; internal set; }
        public Vector3 Position { get; internal set; }
        public Vector3 NormalDirection { get; internal set; }
        public long Frame { get; internal set; }
        public long Duration { get; internal set; }

        private bool m_Disposed = false;
        public static GamePlayFrame_ProjectileSpawn Create(
            int projectileId,
            ProjectileType projectileType,
            ulong characterFrom,
            ulong characterTarget,
            Vector3 position,
            Vector3 normalDirection,
            long frame,
            long duration)
        {
            var p = AnyPool<GamePlayFrame_ProjectileSpawn>.Get();
            p.m_Disposed = false;
            p.ProjectileId = projectileId;
            p.ProjectileType = projectileType;
            p.CharacterFrom = characterFrom;
            p.CharacterTarget = characterTarget;
            p.Position = position;
            p.NormalDirection = normalDirection;
            p.Frame = frame;
            p.Duration = duration;
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
            NormalDirection = Vector3.zero;
            Frame = 0;
            Duration = 0;
            AnyPool<GamePlayFrame_ProjectileSpawn>.Return(this);
        }
    }
}