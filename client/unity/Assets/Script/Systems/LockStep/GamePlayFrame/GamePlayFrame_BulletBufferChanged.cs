using Proto;
using UnityEngine;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_BulletBufferChanged : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.BulletBufferChanged;

        public ulong PlayerId { get; internal set; }
        public XList<ProjectileType> BulletBuffer { get; internal set; }

        private bool m_Disposed = false;
        public static GamePlayFrame_BulletBufferChanged Create(
            ulong playerId,
            XList<ProjectileType> bulletBuffer)
        {
            var p = AnyPool<GamePlayFrame_BulletBufferChanged>.Get();
            p.m_Disposed = false;
            p.PlayerId = playerId;
            p.BulletBuffer = bulletBuffer;
            return p;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            PlayerId = 0;
            if (BulletBuffer != null)
            {
                BulletBuffer.Dispose();
                BulletBuffer = null;
            }
            AnyPool<GamePlayFrame_BulletBufferChanged>.Return(this);
        }
    }
}