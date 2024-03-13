using System;
using Proto;
using UnityEngine;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_PlayerEnter : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.PlayerEnter;

        public ulong CharacterId { get; internal set; }
        public ulong PlayerId { get; internal set; }
        public int CampId { get; internal set; }

        public long Hp { get; internal set; }

        private bool m_Disposed = false;
        internal static IGamePlayFrame Create(
            ulong characterId,
            ulong playerId,
            int campId,
            long hp
            )
        {
            var p = AnyPool<GamePlayFrame_PlayerEnter>.Get();
            p.m_Disposed = false;
            p.CharacterId = characterId;
            p.PlayerId = playerId;
            p.CampId = campId;
            p.Hp = hp;
            return p;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            CharacterId = 0;
            PlayerId = 0;
            CampId = 0;
            Hp = 0;
            AnyPool<GamePlayFrame_PlayerEnter>.Return(this);
        }
    }
}