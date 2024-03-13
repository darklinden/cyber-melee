using System;
using System.Collections.Generic;
using Proto;
using Google.FlatBuffers;
using Wtf;
using App;
using Lockstep;

namespace Battle
{
    internal static class PlayerEOTUtil
    {
        private static List<EffectOverTime> m_EOT_Calc = new List<EffectOverTime>();
        private static List<EffectOverTime> EOTOfType(XList<EffectOverTime> list, EOTType eot)
        {
            m_EOT_Calc.Clear();
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].EotType == eot) m_EOT_Calc.Add(list[i]);
            }
            return m_EOT_Calc;
        }

        internal static void AddEffectOverTime(
            Player player,
            long frame,
            EOTType eotType,
            ulong playerFrom,
            ulong playerTarget,
            SkillType skillAssociated,
            ProjectileType projectileAssociated)
        {
            Log.D("AddEffectOverTime", eotType, playerFrom, playerTarget, skillAssociated, projectileAssociated);
            var eotData = Configs.Instance.EffectOverTimeData.GetData(eotType);
            if (eotData == null)
            {
                Log.E("CharacterEffectOverTimeUtil.AddEffectOverTime EOT Not Found", eotType);
            }

            var prop = PlayerFrameUtil.GetProp(player, frame);
            var stacks = EOTOfType(prop.EOTList, eotType);

            stacks.Sort((a, b) => (int)(a.StartFrame - b.StartFrame));
            while (stacks.Count > eotData.MaxStack)
            {
                stacks.RemoveAt(0);
            }

            var eot = EffectOverTime.Get();
            eot.EotType = eotType;
            eot.EotKind = eotData.Kind;
            eot.StartFrame = frame;
            Log.AssertIsTrue(eotData.Duration == -1 || eotData.Duration % Constants.FRAME_LENGTH_MS == 0,
                "Add EOT Duration Not Int Frame", eotType, eotData.Duration, Constants.FRAME_LENGTH_MS);
            eot.FrameCount = eotData.Duration != -1 ? eotData.Duration / (int)Constants.FRAME_LENGTH_MS : -1;
            eot.PlayerFrom = playerFrom;
            eot.PlayerTarget = playerTarget;
            eot.SkillAssociated = skillAssociated;
            eot.ProjectileAssociated = projectileAssociated;

            switch (eotData.Kind)
            {
                case EOTKind.Damage:
                case EOTKind.Heal:
                    {
                        var atk = prop.StatProp[(int)StatPropType.Attack];
                        eot.DOH_PerInt = (long)(atk * eotData.Dph / Constants.PRECISION);
                        eot.DOH_IntFrameCount = eotData.DohInterval / (int)Constants.FRAME_LENGTH_MS;
                    }
                    break;
                case EOTKind.Buff:
                case EOTKind.Debuff:
                    {
                        if (eotData.Props.Count > 0)
                        {
                            eot.StatPropAdd = XList<(StatPropType, long)>.Get();
                            eot.StatPropPct = XList<(StatPropType, long)>.Get();
                            for (var i = 0; i < eotData.Props.Count; i++)
                            {
                                var p = eotData.Props[i];
                                switch (p.ValueType)
                                {
                                    case StatPropValueType.VAL:
                                        eot.StatPropAdd.Add((p.PropType, p.PropValue));
                                        break;
                                    case StatPropValueType.MUL:
                                        eot.StatPropPct.Add((p.PropType, p.PropValue));
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case EOTKind.Special:
                case EOTKind.Trigger:
                    {
                        // pass
                    }
                    break;
                default:
                    {
                        Log.E("EOTKind Not Found", eotData.Kind);
                    }
                    break;
            }

            Log.D("AddEffectOverTime EOTList Before", prop.EOTList.Count);
            prop.EOTList.Add(eot);
            Log.D("AddEffectOverTime EOTList After", prop.EOTList.Count);
        }

        public struct DamageOrHealByEOT
        {
            public ulong CharacterFrom;
            public ulong CharacterTarget;

            public SkillType SkillType;
            public ProjectileType ProjectileType;
            public long Doh;
        }

        private static List<DamageOrHealByEOT> m_DamageOrHeals = new List<DamageOrHealByEOT>();
        private static Dictionary<int, long> m_VALDict = new Dictionary<int, long>();
        private static Dictionary<int, long> m_MULDict = new Dictionary<int, long>();

        internal static void CalcEOT(
            Player player,
            PlayerFrameProp prop,
            long frame,
            out List<DamageOrHealByEOT> doh)
        {
            doh = m_DamageOrHeals;
            doh.Clear();

            prop.StatProp.Clear();
            foreach (var statKV in player.StatProp)
            {
                var k = statKV.Key;
                var propV = statKV.Value;
                prop.StatProp[k] = propV;
            }

            m_VALDict.Clear();
            m_MULDict.Clear();

            if (prop.EOTList.Count > 0)
            {
                for (var i = prop.EOTList.Count - 1; i >= 0; i--)
                {
                    var eot = prop.EOTList[i];
                    Log.D("CalcEOT",
                        "EotType", eot.EotType,
                        "EotKind", eot.EotKind,
                        "StartFrame", eot.StartFrame,
                        "FrameCount", eot.FrameCount,
                        "DOH_PerInt", eot.DOH_PerInt,
                        "DOH_IntFrameCount", eot.DOH_IntFrameCount);

                    switch (eot.EotKind)
                    {
                        case EOTKind.Damage:
                        case EOTKind.Heal:
                            {
                                // 每次起效帧数 = 每次起效时长 / 帧时长
                                // 需要起效时间间隔是帧时长的倍数
                                // 比如 2 秒 4 次伤害， 帧时长 50ms，起效时间间隔是 500ms, 起效帧数间隔是 2000 / 4 / 50 = 10
                                if (frame == eot.StartFrame)
                                {
                                    // 获得状态第一帧，不计算伤害
                                }
                                else if (eot.FrameCount != -1 && frame >= eot.StartFrame + eot.FrameCount)
                                {
                                    // 伤害结束
                                    // 产生最后一次伤害
                                    if (eot.DOH_PerInt > 0)
                                    {
                                        doh.Add(new DamageOrHealByEOT
                                        {
                                            CharacterFrom = eot.PlayerFrom,
                                            CharacterTarget = eot.PlayerTarget,
                                            SkillType = eot.SkillAssociated,
                                            ProjectileType = eot.ProjectileAssociated,
                                            Doh = eot.EotKind == EOTKind.Heal ? eot.DOH_PerInt : -eot.DOH_PerInt,
                                        });
                                    }
                                    // 清除状态
                                    eot.Dispose();
                                    prop.EOTList.RemoveAt(i);
                                }
                                else if (frame % eot.DOH_IntFrameCount == 0)
                                {
                                    // 产生伤害
                                    if (eot.DOH_PerInt > 0)
                                    {
                                        doh.Add(new DamageOrHealByEOT
                                        {
                                            CharacterFrom = eot.PlayerFrom,
                                            CharacterTarget = eot.PlayerTarget,
                                            SkillType = eot.SkillAssociated,
                                            ProjectileType = eot.ProjectileAssociated,
                                            Doh = eot.EotKind == EOTKind.Heal ? eot.DOH_PerInt : -eot.DOH_PerInt,
                                        });
                                    }
                                }
                                else
                                {
                                    // pass
                                }
                            }
                            break;
                        case EOTKind.Buff:
                        case EOTKind.Debuff:
                            {
                                if (eot.FrameCount != -1 && frame >= eot.StartFrame + eot.FrameCount)
                                {
                                    // 清除状态
                                    eot.Dispose();
                                    prop.EOTList.RemoveAt(i);
                                }
                                else
                                {
                                    var sig = eot.EotKind == EOTKind.Buff ? 1 : -1;
                                    if (eot.StatPropAdd != null)
                                    {
                                        for (var j = 0; j < eot.StatPropAdd.Count; j++)
                                        {
                                            var p = eot.StatPropAdd[j];
                                            if (!m_VALDict.TryGetValue((int)p.Item1, out var v))
                                            {
                                                v = 0;
                                            }
                                            m_VALDict[(int)p.Item1] = v + p.Item2 * sig;
                                        }
                                    }

                                    if (eot.StatPropPct != null)
                                    {
                                        for (var j = 0; j < eot.StatPropPct.Count; j++)
                                        {
                                            var p = eot.StatPropPct[j];
                                            if (!m_MULDict.TryGetValue((int)p.Item1, out var v))
                                            {
                                                v = 0;
                                            }
                                            m_MULDict[(int)p.Item1] = v + p.Item2 * sig;
                                        }
                                    }
                                }
                            }
                            break;
                        case EOTKind.Special:
                            {
                                // pass
                                if (eot.FrameCount != -1 && frame >= eot.StartFrame + eot.FrameCount)
                                {
                                    eot.Dispose();
                                    prop.EOTList.RemoveAt(i);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            foreach (var kv in m_VALDict)
            {
                var k = kv.Key;
                var v = kv.Value;
                if (prop.StatProp.TryGetValue(k, out var propV))
                {
                    prop.StatProp[k] = propV + v;
                }
                else
                {
                    prop.StatProp[k] = v;
                }
            }

            foreach (var kv in m_MULDict)
            {
                var k = kv.Key;
                var v = kv.Value;
                if (prop.StatProp.TryGetValue(k, out var propV))
                {
                    prop.StatProp[k] = (long)(propV * (Constants.PRECISION + v) / Constants.PRECISION);
                }
                else
                {
                    prop.StatProp[k] = 0;
                }
            }
        }
    }
}