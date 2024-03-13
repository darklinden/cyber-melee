using Proto;
using UnityEngine;
using Wtf;
using Battle;
using App;
using System.Collections.Generic;
using FixMath;

namespace Lockstep
{
    internal static class LockStepDealWithProjectiles
    {
        public static bool XZInRange(F64Vec3 l0, F64Vec3 l1, F64Vec3 p)
        {
            var ZInRange = p.Z >= l0.Z && p.Z <= l1.Z
                  || p.Z >= l1.Z && p.Z <= l0.Z;
            var XSame = l0.X == l1.X && l0.X == p.X;
            return ZInRange && XSame;


            // var v0 = l0 - p;
            // var v1 = l1 - p;

            // // less than 90
            // var dot = F64Vec3.Dot(v0, v1);
            // // Log.D("XzVecSameDirectAndDistanceInc", dot, v0, v1, p);
            // if (dot > 0)
            // {
            //     var len1 = F64Vec3.LengthSqr(v1);
            //     var len0 = F64Vec3.LengthSqr(v0);
            //     // Log.D("XzVecSameDirectAndDistanceInc", len1, len0);
            //     return len1 > len0;
            // }

            // return false;
        }

        private static bool MoveAndCheckArrived(
            long frame,
            Projectile projectile,
            F64Vec3 nextPos,
            F64Vec3 targetPos)
        {
            bool isArrived = false;

            // 攻击目标的受击半径
            var targetDetect = F64.FromFloat(projectile.ColliderRadius / Constants.PRECISION);

            // 判断是否到达目标
            if (F64Vec3.DistanceSqr(nextPos, targetPos) < targetDetect * targetDetect)
            {
                Log.D("Projectile Arrive",
                    frame,
                    projectile.ProjectileType,
                    projectile.CharacterTarget,
                    projectile.PosNow);
                // 到达目标
                projectile.PosNow = targetPos;

                isArrived = true;
            }

            // 判断是否穿越目标
            else if (XZInRange(projectile.PosNow, nextPos, targetPos))
            {
                Log.D("Projectile Cross",
                    frame,
                    projectile.ProjectileType,
                    projectile.CharacterTarget,
                    projectile.PosNow,
                    nextPos,
                    targetPos);
                // 穿越目标
                projectile.PosNow = targetPos;
                isArrived = true;
            }
            else
            {
                // Log.D("Projectile Move",
                //     projectile.ProjectileType,
                //     projectile.PosNow, "->", nextPos,
                //     "target", projectile.CharacterTarget,
                //     targetPos);
                projectile.PosNow = nextPos;
            }

            return isArrived;
        }

        internal static void ProjectileAddEOT(
            Projectile projectile,
            EOTType eotType,
            long frame,
            out long eotValue0,
            out long eotValue1
        )
        {
            eotValue0 = 0;
            eotValue1 = 0;

            var eotConfig = Configs.Instance.EffectOverTimeData.GetData(eotType);

            // 可叠加层数处理
            if (EOTStackCount(projectile.EOTList, eotType) >= eotConfig.MaxStack)
            {
                return;
            }

            // 可触发 eot
            var eot = EffectOverTime.Get();
            eot.EotType = eotType;
            Log.D("ProjectileAddEOT", frame, eot.EotType, projectile.ProjectileType);
            eot.EotKind = eotConfig.Kind;
            eot.StartFrame = frame;
            Log.AssertIsTrue(eotConfig.Duration == -1 || eotConfig.Duration % Constants.FRAME_LENGTH_MS == 0,
                "CharacterEffectOnTimeUtil.Add EOT Duration Not Int Frame", eotConfig.Duration);
            eot.FrameCount = (int)(eotConfig.Duration != -1 ? eotConfig.Duration / Constants.FRAME_LENGTH_MS : -1);

            eot.DOH_PerInt = 0;
            eot.DOH_IntFrameCount = 0;

            if (eot.StatPropPct == null) eot.StatPropPct = XList<(StatPropType, long)>.Get(1);
            if (eot.StatPropAdd == null) eot.StatPropAdd = XList<(StatPropType, long)>.Get(1);
            for (var l = 0; l < eotConfig.Props.Count; l++)
            {
                var pr = eotConfig.Props[l];
                switch (pr.ValueType)
                {
                    case StatPropValueType.VAL:
                        {
                            eot.StatPropAdd.Add((pr.PropType, pr.PropValue));
                        }
                        break;
                    case StatPropValueType.MUL:
                        {
                            eot.StatPropPct.Add((pr.PropType, pr.PropValue));
                        }
                        break;
                }
            }

            projectile.EOTList.Add(eot);

            switch (eotType)
            {
                case EOTType.ATKPower:
                    {
                        eotValue0 = EOTStackCount(projectile.EOTList, eotType);
                    }
                    break;
                case EOTType.Duration:
                    {
                        eotValue0 = EOTStackCount(projectile.EOTList, eotType);
                        projectile.MaxLifeTime += eotConfig.SpecialParam;
                        eotValue1 = projectile.MaxLifeTime;
                    }
                    break;
                default:
                    break;
            }
        }

        private static int EOTStackCount(IList<EffectOverTime> eotList, EOTType eotType)
        {
            int count = 0;
            for (int i = 0; i < eotList.Count; i++)
            {
                if (eotList[i].EotType == eotType)
                    count += 1;
            }
            return count;
        }

        private static long CalcDamage(Projectile projectile)
        {
            var attack = projectile.AttackSnapshot;
            if (projectile.EOTList.Count > 0)
            {
                long attack_add = 0;
                long attack_pct = 1000;

                for (var j = 0; j < projectile.EOTList.Count; j++)
                {
                    var eot = projectile.EOTList[j];
                    if (eot.EotKind == EOTKind.Buff)
                    {
                        if (eot.StatPropAdd.Count > 0)
                        {
                            for (var k = 0; k < eot.StatPropAdd.Count; k++)
                            {
                                var propKV = eot.StatPropAdd[k];
                                if (propKV.Item1 == StatPropType.Attack)
                                {
                                    attack_add += propKV.Item2;
                                }
                            }
                        }
                        if (eot.StatPropPct.Count > 0)
                        {
                            for (var k = 0; k < eot.StatPropPct.Count; k++)
                            {
                                var propKV = eot.StatPropPct[k];
                                if (propKV.Item1 == StatPropType.Attack)
                                {
                                    attack_pct += propKV.Item2;
                                }
                            }
                        }
                    }
                }

                attack += attack_add;
                attack = (long)(attack * attack_pct / Constants.PRECISION);
            }

            var dmg = (long)(attack * projectile.Damage / Constants.PRECISION);
            if (dmg <= 0)
            {
                Log.E("Projectile Damage is 0", projectile.ProjectileType, dmg);
                dmg = 0;
            }
            return dmg;
        }


        // 空间换时间 用于碰撞检测
        private static List<Projectile> m_Proj1 = new List<Projectile>();
        private static List<Projectile> m_Proj2 = new List<Projectile>();

        internal static void Run(
            XList<IGameActionFrame> actionList,
            long frame,
            ListGamePlayFrame framePlay,

            PlayerCalcSystem playerCalc,
            ProjectileCalcSystem projectileSystem,
            Service.ServiceSystem serviceSystem
        )
        {
            // 处理飞行物
            // 继承上一帧的飞行物
            var projectiles = projectileSystem.GetOrCopyFrameProjectiles(frame);
            if (projectiles == null || projectiles.Count == 0)
            {
                // 本帧没有飞行物
                return;
            }

            // 如果上一帧有飞行物

            // 清理计算列表
            m_Proj1.Clear();
            m_Proj2.Clear();

            var campId1 = serviceSystem.Data.BattleInfo.CampIds[0];
            var campId2 = serviceSystem.Data.BattleInfo.CampIds[1];

            // 填充计算列表
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];

                var eotList = projectile.EOTList;
                if (eotList != null && eotList.Count > 0)
                {
                    bool shouldRemove = false;
                    for (int j = eotList.Count - 1; j >= 0; j--)
                    {
                        var eot = eotList[j];
                        switch (eot.EotType)
                        {
                            case EOTType.HitBack:
                                {
                                    // 弹反 交换 CharacterFrom 和 CharacterTarget
                                    (projectile.CharacterFrom, projectile.CharacterTarget)
                                        = (projectile.CharacterTarget, projectile.CharacterFrom);
                                    // 移除 EOT
                                    eot.Dispose();
                                    eotList.RemoveAt(j);
                                }
                                break;
                            case EOTType.Eliminate:
                                {
                                    // 移除飞行物
                                    Log.D("Projectile EOT Eliminate", frame, eot.EotType);
                                    shouldRemove = true;
                                }
                                break;
                            default:
                                {
                                    if (eot.FrameCount > 0  // 有持续时间
                                        && eot.StartFrame + eot.FrameCount <= frame) // 已经结束
                                    {
                                        // 移除 EOT
                                        Log.D("Projectile EOT End", frame, eot.EotType);
                                        eot.Dispose();
                                        eotList.RemoveAt(j);
                                    }
                                }
                                break;
                        }
                        if (shouldRemove) break;
                    }
                    if (shouldRemove)
                    {
                        // 移除飞行物
                        framePlay.Plays.Add(GamePlayFrame_ProjectileRemove.Create(
                            projectile.ProjectileId,
                            projectile.ProjectileType
                        ));
                        projectile.Dispose();
                        projectiles.RemoveAt(i);
                        continue;
                    }
                }

                var from = playerCalc.GetPlayerById(projectile.CharacterFrom);
                if (from == null)
                {
                    // 移除飞行物
                    Log.E("飞行物的来源不存在", frame, projectile.CharacterFrom);
                    framePlay.Plays.Add(GamePlayFrame_ProjectileRemove.Create(
                        projectile.ProjectileId,
                        projectile.ProjectileType
                    ));
                    projectile.Dispose();
                    projectiles.RemoveAt(i);
                    continue;
                }

                var fromProp = PlayerFrameUtil.GetProp(from, frame);
                if (fromProp.IsDead)
                {
                    // 移除飞行物
                    Log.D("飞行物的来源已死亡", frame, projectile.CharacterFrom);
                    framePlay.Plays.Add(GamePlayFrame_ProjectileRemove.Create(
                        projectile.ProjectileId,
                        projectile.ProjectileType
                    ));
                    projectile.Dispose();
                    projectiles.RemoveAt(i);
                    continue;
                }

                if (projectile.CharacterTarget == 0)
                {
                    // 移除飞行物
                    Log.D("飞行物的目标不存在", frame, projectile.CharacterTarget);
                    framePlay.Plays.Add(GamePlayFrame_ProjectileRemove.Create(
                        projectile.ProjectileId,
                        projectile.ProjectileType
                    ));
                    projectile.Dispose();
                    projectiles.RemoveAt(i);
                    continue;
                }

                var target = playerCalc.GetPlayerById(projectile.CharacterTarget);
                if (target == null)
                {
                    // 移除飞行物
                    Log.D("飞行物的目标不存在", frame, projectile.CharacterTarget);
                    framePlay.Plays.Add(GamePlayFrame_ProjectileRemove.Create(
                        projectile.ProjectileId,
                        projectile.ProjectileType
                    ));
                    projectile.Dispose();
                    projectiles.RemoveAt(i);
                    continue;
                }

                projectile.MaxLifeTime -= (int)Constants.FRAME_LENGTH_MS;
                if (projectile.MaxLifeTime <= 0)
                {
                    // 移除飞行物
                    Log.D("飞行物的寿命结束", frame, projectile.ProjectileType, projectile.CharacterTarget);
                    framePlay.Plays.Add(GamePlayFrame_ProjectileRemove.Create(
                        projectile.ProjectileId,
                        projectile.ProjectileType
                    ));
                    projectile.Dispose();
                    projectiles.RemoveAt(i);
                    continue;
                }

                // 移动
                var moveTarget = playerCalc.GetPlayerById(projectile.CharacterTarget);
                var targetPos = moveTarget.OrbitPosList[projectile.OrbitIndex];
                projectile.PosCache = projectile.PosNow;
                var moveDir = F64Vec3.NormalizeFastest(targetPos - projectile.PosNow);
                var moveDistance = F64.FromFloat(projectile.Speed * Constants.FRAME_LENGTH_SEC / Constants.PRECISION);
                var movedPos = projectile.PosNow + moveDir * moveDistance;

                // 是否到达
                if (MoveAndCheckArrived(frame, projectile, movedPos, targetPos))
                {
                    Log.D("飞行物到达", frame, projectile.ProjectileType, projectile.CharacterTarget, projectile.PosNow);

                    // 到达目标

                    // 触发事件
                    framePlay.Plays.Add(GamePlayFrame_ProjectileArrive.Create(
                        projectile.ProjectileId,
                        projectile.ProjectileType,
                        projectile.CharacterFrom,
                        projectile.CharacterTarget,
                        projectile.PosNow.ToVector3()
                    ));

                    // 目前全是单体指向技能
                    var proj1TargetPlayer = playerCalc.GetPlayerById(projectile.CharacterTarget);
                    var proj1TargetProp = PlayerFrameUtil.GetProp(proj1TargetPlayer, frame);

                    // 计算伤害
                    var dmg = CalcDamage(projectile);

                    proj1TargetProp.Hp -= dmg;
                    if (proj1TargetProp.Hp <= 0) proj1TargetProp.Hp = 0;
                    var maxHp = proj1TargetProp.StatProp[(int)StatPropType.HP];

                    framePlay.Plays.Add(GamePlayFrame_PlayerHPChanged.Create(
                        projectile.CharacterFrom,
                        projectile.CharacterTarget,
                        projectile.SkillType,
                        projectile.ProjectileType,
                        dmg,
                        proj1TargetProp.Hp,
                        maxHp
                    ));

                    if (projectile.EOTList.Count > 0)
                    {
                        for (var j = 0; j < projectile.EOTList.Count; j++)
                        {
                            var eot = projectile.EOTList[j];
                            Log.D("Projectile Arrive 1 EOT", eot);
                            if (eot.EotKind == EOTKind.Trigger)
                            {
                                // 触发型的 EOT
                                var eotConfig = Configs.Instance.EffectOverTimeData.GetData(eot.EotType);
                                Log.AssertIsTrue(eotConfig.Triggers.Count > 0, "EOTKind.Trigger EOTConfig.Triggers.Count == 0");

                                foreach (var trigger in eotConfig.Triggers)
                                {
                                    Log.D("Projectile Arrive Trigger", eot.EotType, trigger);
                                    if (trigger.TriggerType != EOTTriggerFireType.ProjArriveToEnemy) continue;

                                    switch (eot.EotType)
                                    {
                                        case EOTType.BleedTrigger:
                                            {
                                                PlayerEOTUtil.AddEffectOverTime(
                                                    proj1TargetPlayer,
                                                    frame,
                                                    trigger.Eot,
                                                    projectile.CharacterFrom,
                                                    projectile.CharacterTarget,
                                                    projectile.SkillType,
                                                    projectile.ProjectileType);
                                            }
                                            break;
                                        default:
                                            Log.E("EOTKind.Trigger EOTType Not Support", eot.EotType);
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    // 移除飞行物
                    projectile.Dispose();
                    projectiles.RemoveAt(i);
                    continue;
                }

                if (from.CampId == campId1)
                {
                    m_Proj1.Add(projectile);
                }
                else if (from.CampId == campId2)
                {
                    m_Proj2.Add(projectile);
                }
            }

            // 计算飞行物移动和碰撞, 只有不同阵营的飞行物才会碰撞
            for (int i = m_Proj1.Count - 1; i >= 0; i--)
            {
                var proj1 = m_Proj1[i];

                for (int j = m_Proj2.Count - 1; j >= 0; j--)
                {
                    var proj2 = m_Proj2[j];

                    // 碰撞检测
                    if (proj1.ProjectileType == proj2.ProjectileType)
                    {
                        // 同类型的飞行物不会碰撞
                        continue;
                    }

                    // 两个飞行物的碰撞半径
                    var detect = F64.FromInt(proj1.ColliderRadius + proj2.ColliderRadius) / F64.FromInt(Constants.PRECISION);

                    var isCollide =
                        (F64Vec3.DistanceSqr(proj1.PosNow, proj2.PosNow) < detect * detect)
                        || (F64Vec3.DistanceSqr(proj1.PosCache, proj2.PosNow) < detect * detect)
                        || (F64Vec3.DistanceSqr(proj1.PosNow, proj2.PosCache) < detect * detect)
                        || XZInRange(proj1.PosCache, proj1.PosNow, proj2.PosCache)
                        || XZInRange(proj1.PosCache, proj1.PosNow, proj2.PosNow)
                        || XZInRange(proj2.PosCache, proj2.PosNow, proj1.PosCache)
                        || XZInRange(proj2.PosCache, proj2.PosNow, proj1.PosNow);

                    if (isCollide)
                    {
                        // 相克处理
                        Log.D("Projectile Collide", frame,
                            proj1.ProjectileType,
                            proj1.PosNow,
                            proj2.ProjectileType,
                            proj2.PosNow);

                        var config1 = Configs.Instance.ProjectileData.GetData(proj1.ProjectileType);
                        var config2 = Configs.Instance.ProjectileData.GetData(proj2.ProjectileType);

                        if (config2.MatchUp == config1.ProjectileType)
                        {
                            // 2 克 1 且 1 有 PowerUp 直接移除 2
                            if (proj1.EOTList.Count > 0)
                            {
                                bool hasPowerUp = false;
                                for (var k = proj1.EOTList.Count - 1; k >= 0; k--)
                                {
                                    var eot = proj1.EOTList[k];
                                    if (eot.EotType == EOTType.PowerUp)
                                    {
                                        hasPowerUp = true;
                                        eot.Dispose();
                                        proj1.EOTList.RemoveAt(k);
                                        break;
                                    }
                                }
                                if (hasPowerUp)
                                {
                                    // 1 克 2
                                    framePlay.Plays.Add(GamePlayFrame_ProjectileRemove.Create(
                                        proj2.ProjectileId,
                                        proj2.ProjectileType
                                    ));
                                    projectiles.Remove(proj2);
                                    proj2.Dispose();
                                    m_Proj2.RemoveAt(j);
                                    continue;
                                }
                            }
                        }

                        if (config1.MatchUp == config2.ProjectileType)
                        {
                            // 1 克 2 且 2 有 PowerUp 直接移除 1
                            if (proj2.EOTList.Count > 0)
                            {
                                bool hasPowerUp = false;
                                for (var k = proj2.EOTList.Count - 1; k >= 0; k--)
                                {
                                    var eot = proj2.EOTList[k];
                                    if (eot.EotType == EOTType.PowerUp)
                                    {
                                        hasPowerUp = true;
                                        eot.Dispose();
                                        proj2.EOTList.RemoveAt(k);
                                        break;
                                    }
                                }
                                if (hasPowerUp)
                                {
                                    // 2 克 1
                                    framePlay.Plays.Add(GamePlayFrame_ProjectileRemove.Create(
                                        proj1.ProjectileId,
                                        proj1.ProjectileType
                                    ));
                                    projectiles.Remove(proj1);
                                    proj1.Dispose();
                                    m_Proj1.RemoveAt(i);
                                    break;
                                }
                            }
                        }

                        for (int k = 0; k < config1.Trigger.Count; k++)
                        {
                            var trigger1 = config1.Trigger[k];
                            if (trigger1.TriggerType != EOTTriggerFireType.ProjCollideToProj) continue;
                            if (trigger1.TriggerParam0 != (int)proj2.ProjectileType) continue;

                            ProjectileAddEOT(proj1, trigger1.Eot, frame, out var eotValue0, out var eotValue1);

                            framePlay.Plays.Add(GamePlayFrame_ProjectileChange.Create(
                                proj1.ProjectileId,
                                proj1.ProjectileType,
                                proj1.CharacterFrom,
                                proj1.CharacterTarget,
                                proj1.PosNow.ToVector3(),
                                trigger1.Eot,
                                eotValue0,
                                eotValue1
                            ));
                        }

                        for (int k = 0; k < config2.Trigger.Count; k++)
                        {
                            var trigger2 = config2.Trigger[k];
                            if (trigger2.TriggerType != EOTTriggerFireType.ProjCollideToProj) continue;
                            if (trigger2.TriggerParam0 != (int)proj1.ProjectileType) continue;

                            ProjectileAddEOT(proj2, trigger2.Eot, frame, out var eotValue0, out var eotValue1);

                            framePlay.Plays.Add(GamePlayFrame_ProjectileChange.Create(
                                proj2.ProjectileId,
                                proj2.ProjectileType,
                                proj2.CharacterFrom,
                                proj2.CharacterTarget,
                                proj2.PosNow.ToVector3(),
                                trigger2.Eot,
                                eotValue0,
                                eotValue1
                            ));
                        }
                    }
                }
            }
        }
    }
}