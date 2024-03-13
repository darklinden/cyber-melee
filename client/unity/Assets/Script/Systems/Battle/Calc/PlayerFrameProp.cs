using System;
using UnityEngine;
using Proto;
using Wtf;
using System.Collections.Generic;

namespace Battle
{
    // 每逻辑帧角色属性计算
    internal class PlayerFrameProp : IDisposable
    {
#if DEBUG
        private static int m_DebugId = 0;
        private static int GenerateDebugId()
        {
            if (m_DebugId == int.MaxValue - 10) m_DebugId = 0;
            return m_DebugId++;
        }
        public int DebugId { get; private set; }
#endif

        // 当前逻辑帧 属性快照
        public Dictionary<int, long> StatProp { get; } = new Dictionary<int, long>();

        // 时间持续效果
        public XList<EffectOverTime> EOTList { get; set; }

        // bullet buffer
        public XList<ProjectileType> BulletBuffer { get; set; }

        public ulong Target { get; set; }

        // 当前逻辑帧 当前血量
        public long Hp;

        // 当前逻辑帧 是否死亡
        public bool IsDead = false;

        // 当前逻辑帧 正在释放的技能
        public RunningSkill? RunningSkill;

        // 当前逻辑帧 技能倒计时情况
        public XList<CD_Counter> SkillCDCounters { get; set; }

        // 持续效果是否已经冷却完成
        public int SkillCd(SkillType skillType)
        {
            if (SkillCDCounters == null || SkillCDCounters.Count == 0) return 0;
            var iSkillType = (int)skillType;
            for (var i = 0; i < SkillCDCounters.Count; i++)
            {
                if (SkillCDCounters[i].Id == iSkillType) return SkillCDCounters[i].Counter;
            }
            return 0;
        }

        public void SkillMarkCd(SkillType skillType, int count)
        {
            if (SkillCDCounters == null) SkillCDCounters = XList<CD_Counter>.Get(1);
            SkillCDCounters.Add(new CD_Counter
            {
                Id = (int)skillType,
                Counter = count
            });
        }

        private bool m_IsDisposed = false;
        public bool IsDisposed => m_IsDisposed;
        public static PlayerFrameProp Get()
        {
            var p = AnyPool<PlayerFrameProp>.Get();
#if DEBUG
            p.DebugId = GenerateDebugId();
#endif
            p.m_IsDisposed = false;
            return p;
        }

        public static PlayerFrameProp CopyFrom(PlayerFrameProp p)
        {
            var prop = Get();
            if (p.EOTList != null)
            {
                prop.EOTList = XList<EffectOverTime>.Get(p.EOTList.Count);
                for (var i = 0; i < p.EOTList.Count; i++)
                {
                    prop.EOTList.Add(EffectOverTime.CopyFrom(p.EOTList[i]));
                }
            }

            prop.Target = p.Target;
            prop.Hp = p.Hp;
            prop.IsDead = p.IsDead;
            prop.RunningSkill = p.RunningSkill;
            if (p.SkillCDCounters != null)
            {
                prop.SkillCDCounters = XList<CD_Counter>.CopyFrom(p.SkillCDCounters);
            }
            else
            {
                prop.SkillCDCounters = null;
            }

#if DEBUG
            Log.D("PlayerFrameProp.CopyFrom", "BulletBuffer from", p.DebugId, "to", prop.DebugId);
#endif

            prop.BulletBuffer = XList<ProjectileType>.CopyFrom(p.BulletBuffer);
            return prop;
        }

        public void Dispose()
        {
            if (m_IsDisposed) return;
            m_IsDisposed = true;

            StatProp.Clear();

            if (EOTList != null)
            {
                for (var i = 0; i < EOTList.Count; i++)
                {
                    EOTList[i].Dispose();
                }
                EOTList.Dispose();
                EOTList = null;
            }

            if (BulletBuffer != null)
            {
                BulletBuffer.Dispose();
                BulletBuffer = null;
            }

            Target = 0;
            Hp = 0;
            IsDead = false;
            RunningSkill = null;

            if (SkillCDCounters != null)
            {
                SkillCDCounters.Dispose();
                SkillCDCounters = null;
            }

            AnyPool<PlayerFrameProp>.Return(this);
        }

        public void ClearEOT()
        {
            if (EOTList != null)
            {
                for (var i = 0; i < EOTList.Count; i++)
                {
                    EOTList[i].Dispose();
                }
                EOTList.Dispose();
            }
        }
    }
}