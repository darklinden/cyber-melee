using System;
using Proto;
using Wtf;


namespace Battle
{
    // 对应一个单独的EOT
    internal class EffectOverTime : IDisposable
    {
        // 对应的EOTId
        public Proto.EOTType EotType;
        public Proto.EOTKind EotKind;

        public ulong PlayerFrom;
        public ulong PlayerTarget;
        public SkillType SkillAssociated;
        public ProjectileType ProjectileAssociated;

        // 起始帧号
        public long StartFrame;
        // 总帧数
        public int FrameCount;

        // 每跳伤害 / 每跳治疗 Damage Over Time Or Heal Over Time Per Interval
        public long DOH_PerInt;
        // 间隔帧数
        public int DOH_IntFrameCount;

        // 当前逻辑帧 基础属性 buff & debuff 池, 对应 StatPropType
        public XList<(Proto.StatPropType, long)> StatPropAdd;

        // 当前逻辑帧 基础属性 buff & debuff 池, 对应 StatPropType
        public XList<(Proto.StatPropType, long)> StatPropPct;

        private bool m_Disposed = false;

        public static EffectOverTime Get()
        {
            var eot = AnyPool<EffectOverTime>.Get();
            eot.m_Disposed = false;
            return eot;
        }

        public static EffectOverTime CopyFrom(EffectOverTime e)
        {
            var eot = Get();
            eot.EotType = e.EotType;
            eot.EotKind = e.EotKind;
            eot.PlayerFrom = e.PlayerFrom;
            eot.PlayerTarget = e.PlayerTarget;
            eot.SkillAssociated = e.SkillAssociated;
            eot.ProjectileAssociated = e.ProjectileAssociated;
            eot.StartFrame = e.StartFrame;
            eot.FrameCount = e.FrameCount;
            eot.DOH_PerInt = e.DOH_PerInt;
            eot.DOH_IntFrameCount = e.DOH_IntFrameCount;
            if (e.StatPropAdd != null)
            {
                eot.StatPropAdd = XList<(Proto.StatPropType, long)>.Get(e.StatPropAdd.Count);
                for (var i = 0; i < e.StatPropAdd.Count; i++)
                {
                    eot.StatPropAdd.Add(e.StatPropAdd[i]);
                }
            }
            if (e.StatPropPct != null)
            {
                eot.StatPropPct = XList<(Proto.StatPropType, long)>.Get(e.StatPropPct.Count);
                for (var i = 0; i < e.StatPropPct.Count; i++)
                {
                    eot.StatPropPct.Add(e.StatPropPct[i]);
                }
            }
            return eot;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            EotType = Proto.EOTType.None;
            EotKind = Proto.EOTKind.None;
            PlayerFrom = 0;
            PlayerTarget = 0;
            StartFrame = 0;
            FrameCount = 0;
            DOH_PerInt = 0;
            DOH_IntFrameCount = 0;
            StatPropAdd?.Dispose();
            StatPropAdd = null;
            StatPropPct?.Dispose();
            StatPropPct = null;
            AnyPool<EffectOverTime>.Return(this);
        }
    }
}