using Wtf;
using Battle;
using App;
using Proto;
using FixMath;
using UnityEngine;
using System.Collections.Generic;

namespace Lockstep
{
    internal static class LockStepSkill
    {
        private static List<ProjectileType> m_BulletTypeList = new List<ProjectileType>();
        internal static void Run(
            LockStepSystem lockStepMgr,
            long frame,
            ListGamePlayFrame framePlay,

            PlayerCalcSystem playerCalc,
            ProjectileCalcSystem projectileCalc
        )
        {
            if (!lockStepMgr.LockStepServerSyncQueue.TryGetValue(frame, out XList<IGameActionFrame> frameActions))
            {
                frameActions = null;
            }

            if (frameActions != null && frameActions.Count > 0)
            {
                Log.D("Has frame action", frame, frameActions.Count);
            }

            // 处理技能和移动
            for (int i = 0; i < playerCalc.PlayerCount; i++)
            {
                var player = playerCalc.GetPlayerAtIndex(i);
                if (player == null)
                {
                    Log.D("Character is null", i);
                    continue;
                }

                var prop = PlayerFrameUtil.GetProp(player, frame);
                if (prop == null)
                {
                    Log.D("CharacterProp is null", player.PlayerId);
                    continue;
                }

                if (prop.IsDead)
                {
                    Log.D("Character is dead", player.PlayerId, prop.Hp);
                    continue;
                }

                // 为未死亡角色计算攻击目标
                var changed = PlayerSkillUtil.ValidateTarget(
                    playerCalc,
                    player,
                    frame);
                if (changed == PlayerSkillUtil.TargetValidationResult.NoTargetValid)
                {
                    // 没有可以攻击的目标，检测游戏结束
                    Log.D("No target valid for character", player.PlayerId);
                    continue;
                }

                GameActionFrameSkill? actionSkill = null;
                if (frameActions != null && frameActions.Count > 0)
                {
                    for (int j = frameActions.Count - 1; j >= 0; j--)
                    {
                        if (frameActions[j].PlayerId == player.PlayerId)
                        {
                            if (frameActions[j] is GameActionFrameSkill action)
                            {
                                if (!actionSkill.HasValue)
                                {
                                    actionSkill = action;
                                    frameActions.RemoveAt(j);
                                }
                                break;
                            }
                        }
                    }
                }

                RunningSkill? skill = null;
                if (actionSkill.HasValue)
                {
                    if (actionSkill.Value.SkillType == player.CastSkill && prop.BulletBuffer.Count <= 0)
                    {
                        Log.W("Cast Skill No bullet in buffer", player.PlayerId);
                    }
                    else if (prop.SkillCd(actionSkill.Value.SkillType) > 0)
                    {
                        Log.W("Skill CD", player.PlayerId, actionSkill.Value.SkillType);
                    }
                    else
                    {
                        var bullet = ProjectileType.NONE;
                        if (actionSkill.Value.SkillType == player.CastSkill)
                        {
                            bullet = prop.BulletBuffer.Dequeue();
                            framePlay.Plays.Add(GamePlayFrame_BulletBufferChanged.Create(
                                player.PlayerId,
                                XList<ProjectileType>.CopyFrom(prop.BulletBuffer)
                            ));
                        }

                        var skillConfig = Configs.Instance.SkillData.GetData(actionSkill.Value.SkillType);
                        skill = new RunningSkill
                        {
                            SkillType = actionSkill.Value.SkillType,
                            StartFrame = frame,
                            AffectFrame = frame + skillConfig.CastDur / (int)Constants.FRAME_LENGTH_MS,
                            EndFrame = frame + skillConfig.AnimDur / (int)Constants.FRAME_LENGTH_MS,
                            CD = skillConfig.Cooldown / (int)Constants.FRAME_LENGTH_MS,
                            ProjectileType = bullet,
                            OrbitIndex = actionSkill.Value.OrbitIndex
                        };
                        Log.W("StartSkill By Sync", frame, player.PlayerId, skill.Value.SkillType);
                    }
                }

                if (skill.HasValue)
                {
                    // 有需要施放的技能
                    if (prop.RunningSkill.HasValue)
                    {
                        // 且当前有技能正在执行
                        var runningSkill = prop.RunningSkill.Value;
                        if (runningSkill.AffectFrame > frame)
                        {
                            // 当前技能还未触发 理论上不会出现这种情况
                            Log.E("Skill Affecting", player.PlayerId, runningSkill.SkillType);
                        }

                        // 如果当前技能已经触发, 取消当前技能后摇
                        Log.D("Skill End", player.PlayerId, runningSkill.SkillType);
                        // 技能结束
                        framePlay.Plays.Add(GamePlayFrame_SkillEnded.Create(
                            player.PlayerId,
                            frame,
                            runningSkill.SkillType));

                        prop.RunningSkill = null;
                    }
                }

                // 如果有执行中的技能，等待技能结束
                if (prop.RunningSkill.HasValue)
                {
                    var runningSkill = prop.RunningSkill.Value;

                    // 是否已经需要触发技能效果
                    if (runningSkill.AffectFrame == frame)
                    {
                        Log.D("Skill Affect", frame, player.PlayerId, runningSkill);
                        // 技能触发
                        framePlay.Plays.Add(GamePlayFrame_SkillAffect.Create(
                            player.PlayerId,
                            frame,
                            runningSkill.SkillType));

                        if (runningSkill.SkillType == player.CastSkill)
                        {
                            var tc = playerCalc.GetPlayerById(prop.Target);
                            if (tc != null)
                            {
                                var tp = PlayerFrameUtil.GetProp(tc, frame);
                                if (!tp.IsDead || tp.Hp > 0)
                                {
                                    // 指向某一角色，创建单一 Projectile
                                    var projectileConfig = Configs.Instance.ProjectileData.GetData(runningSkill.ProjectileType);

                                    var projectile = projectileCalc.CreateProjectile();
                                    projectile.CharacterFrom = player.PlayerId;
                                    projectile.CharacterTarget = prop.Target;
                                    projectile.ProjectileType = runningSkill.ProjectileType;
                                    projectile.ColliderRadius = projectileConfig.CircleColliderRadius; // 无视距离的技能，移速为 Constants.INVALID_VALUE
                                    projectile.AttackSnapshot = prop.StatProp[(int)Proto.StatPropType.Attack];
                                    projectile.PosNow = player.OrbitPosList[runningSkill.OrbitIndex];
                                    projectile.OrbitIndex = runningSkill.OrbitIndex;
                                    projectile.Speed = projectileConfig.Speed;
                                    projectile.Damage = projectileConfig.Damage;
                                    projectile.MaxLifeTime = projectileConfig.MaxLifeTime;
                                    projectileCalc.SetProjectile(frame, projectile);

                                    if (prop.EOTList.Count > 0)
                                    {
                                        for (int j = prop.EOTList.Count - 1; j >= 0; j--)
                                        {
                                            var eot = prop.EOTList[j];
                                            if (eot.EotKind != EOTKind.Trigger) continue;
                                            var eotConfig = Configs.Instance.EffectOverTimeData.GetData(eot.EotType);
                                            bool remove = false;
                                            foreach (var trigger in eotConfig.Triggers)
                                            {
                                                if (trigger.TriggerType == EOTTriggerFireType.ProjSpawnToProj)
                                                {
                                                    LockStepDealWithProjectiles.ProjectileAddEOT(
                                                        projectile,
                                                        trigger.Eot,
                                                        frame,
                                                        out var eotValue0,
                                                        out var eotValue1);
                                                    if (eotConfig.SpecialParam == 1)
                                                    {
                                                        // trigger 类型的 eot, 如果特殊参数为 1, 则触发后移除
                                                        remove = true;
                                                    }
                                                }
                                            }
                                            if (remove)
                                            {
                                                prop.EOTList.RemoveAt(j);
                                            }
                                        }
                                    }

                                    var dir = F64Vec3.NormalizeFastest(tc.OrbitPosList[runningSkill.OrbitIndex] - player.OrbitPosList[runningSkill.OrbitIndex])
                                            * F64.FromInt(projectile.Speed) / F64.FromFloat(Constants.PRECISION);

                                    framePlay.Plays.Add(GamePlayFrame_ProjectileSpawn.Create(
                                        projectile.ProjectileId,
                                        projectile.ProjectileType,
                                        player.PlayerId,
                                        prop.Target,
                                        projectile.PosNow.ToVector3(),
                                        dir.ToVector3(),
                                        frame,
                                        projectileConfig.MaxLifeTime));
                                }
                            }
                        }
                        else if (runningSkill.SkillType == player.SpecialSkill)
                        {
                            // 特殊效果
                            switch (runningSkill.SkillType)
                            {
                                case SkillType.Swap:
                                    {
                                        if (prop.BulletBuffer.Count >= 2)
                                        {
                                            Log.D("SpecialSkill Swap Bullet", player.PlayerId, prop.BulletBuffer[0], prop.BulletBuffer[1]);
                                            (prop.BulletBuffer[0], prop.BulletBuffer[1]) = (prop.BulletBuffer[1], prop.BulletBuffer[0]);
                                            framePlay.Plays.Add(GamePlayFrame_BulletBufferChanged.Create(
                                                player.PlayerId,
                                                XList<ProjectileType>.CopyFrom(prop.BulletBuffer)
                                            ));
                                        }
                                    }
                                    break;
                                case SkillType.Random:
                                    {
                                        if (prop.BulletBuffer.Count > 0)
                                        {
                                            m_BulletTypeList.Clear();
                                            foreach (var kv in Configs.Instance.ProjectileData.ProjectileDataDict)
                                            {
                                                if (kv.Value.ProjectileType != prop.BulletBuffer[0])
                                                    m_BulletTypeList.Add(kv.Value.ProjectileType);
                                            }
                                            prop.BulletBuffer[0] = m_BulletTypeList[SeedRandom.Value(player.PlayerId, SeedType.Skill).Range(0, m_BulletTypeList.Count - 1)];
                                            Log.D("SpecialSkill Random Bullet", player.PlayerId, prop.BulletBuffer[0]);
                                            framePlay.Plays.Add(GamePlayFrame_BulletBufferChanged.Create(
                                                player.PlayerId,
                                                XList<ProjectileType>.CopyFrom(prop.BulletBuffer)
                                            ));
                                        }
                                    }
                                    break;
                                case SkillType.Remove:
                                    {
                                        if (prop.BulletBuffer.Count > 0)
                                        {
                                            prop.BulletBuffer.RemoveAt(0);
                                            Log.D("SpecialSkill Remove Bullet", player.PlayerId, prop.BulletBuffer.Count);
                                            framePlay.Plays.Add(GamePlayFrame_BulletBufferChanged.Create(
                                                player.PlayerId,
                                                XList<ProjectileType>.CopyFrom(prop.BulletBuffer)
                                            ));
                                        }
                                    }
                                    break;
                                case SkillType.PowerUp:
                                    {
                                        Log.D("SpecialSkill PowerUp", player.PlayerId);
                                        PlayerEOTUtil.AddEffectOverTime(
                                            player,
                                            frame,
                                            EOTType.PowerUp,
                                            player.PlayerId,
                                            player.PlayerId,
                                            runningSkill.SkillType,
                                            ProjectileType.NONE);
                                    }
                                    break;
                                default:
                                    {
                                        Log.E("Unknown Special Skill", player.PlayerId, runningSkill.SkillType);
                                    }
                                    break;
                            }
                        }
                    }

                    // 是否已经需要触发技能结束
                    else if (runningSkill.EndFrame <= frame)
                    {
                        Log.D("Skill End", player.PlayerId, runningSkill.SkillType);
                        // 技能结束
                        framePlay.Plays.Add(GamePlayFrame_SkillEnded.Create(
                            player.PlayerId,
                            frame,
                            runningSkill.SkillType));
                        prop.RunningSkill = null;
                    }

                    // 有技能正在执行，等待技能结束
                    continue;
                }

                if (skill.HasValue)
                {
                    prop.RunningSkill = skill;
                    Log.D("Skill Start", player.PlayerId, skill.Value);
                    prop.SkillMarkCd(skill.Value.SkillType, skill.Value.CD);
                    framePlay.Plays.Add(GamePlayFrame_SkillStart.Create(
                        player.PlayerId,
                        frame,
                        skill.Value.SkillType));
                }
            }
        }
    }
}