using System;
using System.Collections.Generic;
using Wtf;
using Proto;
using UnityEngine;
using App;
using FixMath;

namespace Battle
{
    internal class Player : IDisposable
    {
        internal int CampId { get; set; } = 0;
        internal ulong PlayerId { get; set; } = 0;

        // 基础属性池 < StatPropType, value >
        internal Dictionary<int, long> StatProp = new Dictionary<int, long>();

        // 技能池
        internal SkillType CastSkill;
        internal SkillType SpecialSkill;

        internal long BulletGenerateCdFrameCount;
        internal long BulletBufferSize;

        // 分帧属性
        internal XList<long> Frames { get; set; }
        internal Dictionary<long, PlayerFrameProp> FrameProperties = new Dictionary<long, PlayerFrameProp>(64);

        internal List<F64Vec3> OrbitPosList { get; set; }
        internal F64Vec3 OrbitCenter { get; set; }

        private bool m_Disposed = false;
        public static Player Get()
        {
            var character = AnyPool<Player>.Get();
            character.m_Disposed = false;
            return character;
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;

            PlayerId = 0;
            CampId = 0;

            StatProp.Clear();

            CastSkill = SkillType.NONE;
            SpecialSkill = SkillType.NONE;

            if (Frames != null)
            {
                Frames.Dispose();
                Frames = null;
            }
            if (FrameProperties != null)
            {
                foreach (var kv in FrameProperties)
                {
                    kv.Value.Dispose();
                }
                FrameProperties.Clear();
                FrameProperties = null;
            }
            if (OrbitPosList != null)
            {
                OrbitPosList.Clear();
                OrbitPosList = null;
            }
        }
    }
}
