using System;
using Proto;
using UnityEngine;
using Wtf;

namespace Lockstep
{
    public class GamePlayFrame_PlayerHPChanged : IGamePlayFrame
    {
        public GamePlayFrameType PlayType => GamePlayFrameType.PlayerHPChanged;

        public ulong CharacterFrom { get; internal set; }
        public ulong CharacterTarget { get; internal set; }

        public SkillType SkillType { get; internal set; }
        public ProjectileType ProjectileType { get; internal set; }
        public long Damage { get; internal set; }

        public long Hp { get; internal set; }
        public long MaxHp { get; internal set; }

        private bool m_Disposed = false;
        internal static IGamePlayFrame Create(
            ulong characterFrom,
            ulong characterTarget,
            SkillType skillType,
            ProjectileType projectileType,
            long damage,
            long hp,
            long maxHp
        )
        {
            var p = AnyPool<GamePlayFrame_PlayerHPChanged>.Get();
            p.m_Disposed = false;
            p.CharacterFrom = characterFrom;
            p.CharacterTarget = characterTarget;
            p.SkillType = skillType;
            p.ProjectileType = projectileType;
            p.Damage = damage;
            p.Hp = hp;
            p.MaxHp = maxHp;
            return p;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            CharacterFrom = 0;
            CharacterTarget = 0;
            SkillType = 0;
            ProjectileType = 0;
            Damage = 0;
            Hp = 0;
            MaxHp = 0;
            AnyPool<GamePlayFrame_PlayerHPChanged>.Return(this);
        }
    }
}