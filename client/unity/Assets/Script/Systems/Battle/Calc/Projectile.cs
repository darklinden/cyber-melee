using System;
using App;
using FixMath;
using UnityEngine;
using Wtf;

namespace Battle
{
    internal class Projectile : IDisposable
    {
        // 投射物ID
        internal int ProjectileId;
        // 投射物类型
        internal Proto.SkillType SkillType;
        internal Proto.ProjectileType ProjectileType;

        // 投射物 移速
        internal int Speed;
        // 投射物 碰撞体半径
        internal int ColliderRadius;
        // 投射物 伤害快照
        internal long AttackSnapshot;
        // 投射物 伤害系数 1000 = 1
        internal int Damage;
        // 投射物 最大生命周期
        internal int MaxLifeTime;

        // 当前逻辑帧 施放者
        internal ulong CharacterFrom;
        // 当前逻辑帧 目标
        internal ulong CharacterTarget;

        // 当前逻辑帧 位置
        internal F64Vec3 PosNow;
        internal int OrbitIndex;

        // 时间持续效果
        internal XList<EffectOverTime> EOTList { get; set; }

        // 计算使用
        internal F64Vec3 PosCache { get; set; }

        private bool m_Disposed = false;
        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;

            ProjectileId = -1;
            SkillType = Proto.SkillType.NONE;
            ProjectileType = Proto.ProjectileType.NONE;
            Speed = 0;
            ColliderRadius = 0;
            AttackSnapshot = 0;
            Damage = 0;
            MaxLifeTime = 0;
            CharacterFrom = 0;
            CharacterTarget = 0;
            PosNow = F64Vec3.Zero;
            OrbitIndex = -1;

            if (EOTList != null)
            {
                foreach (var eot in EOTList)
                {
                    eot.Dispose();
                }
                EOTList.Dispose();
                EOTList = null;
            }
        }

        internal static Projectile Get()
        {
            var p = AnyPool<Projectile>.Get();
            p.m_Disposed = false;
            p.EOTList = XList<EffectOverTime>.Get();
            return p;
        }

        internal static Projectile CopyFrom(Projectile p)
        {
            var projectile = Get();
            projectile.ProjectileId = p.ProjectileId;
            projectile.SkillType = p.SkillType;
            projectile.ProjectileType = p.ProjectileType;
            projectile.Speed = p.Speed;
            projectile.ColliderRadius = p.ColliderRadius;
            projectile.AttackSnapshot = p.AttackSnapshot;
            projectile.Damage = p.Damage;
            projectile.MaxLifeTime = p.MaxLifeTime;
            projectile.CharacterFrom = p.CharacterFrom;
            projectile.CharacterTarget = p.CharacterTarget;
            projectile.PosNow = p.PosNow;
            projectile.OrbitIndex = p.OrbitIndex;

            if (p.EOTList != null && p.EOTList.Count > 0)
            {
                for (var i = 0; i < p.EOTList.Count; i++)
                {
                    projectile.EOTList.Add(EffectOverTime.CopyFrom(p.EOTList[i]));
                }
            }
            return projectile;
        }
    }
}
