using Proto;
using System.Collections.Generic;
using Wtf;
using App;

namespace Battle
{
    internal struct SkillDataForSort
    {
        internal int SkillId;
        internal int FrameEnd;
        internal int FrameAffect;
        internal int SkillWeight;
        internal int SkillCounter;

        internal int Range;
        internal int CD;
        internal int ProjectileSpeed;
    }

    internal static class PlayerSkillUtil
    {
        /// <summary>
        /// 使用指定帧的状态计算距离某角色最接近的敌人角色
        /// </summary>
        /// <param name="player"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        internal static Player GetEnemy(
            PlayerCalcSystem playerCalc,
            Player player,
            long frame)
        {
            if (player == null) return null;

            var campId = player.CampId;
            var enemyCharacters = playerCalc.GetPlayerIdsByNotCampId(campId);
            var enemyCharacterId = enemyCharacters[0];
            var enemy = playerCalc.GetPlayerById(enemyCharacterId);
            if (enemy != null)
            {
                var property = PlayerFrameUtil.GetProp(enemy, frame);
                if (!property.IsDead && property.Hp > 0)
                {
                    return enemy;
                }
            }

            return null;
        }

        internal enum TargetValidationResult
        {
            OldTargetValid,
            NewTargetValid,
            NoTargetValid,
        }

        /// <summary>
        /// 检查当前帧目标是否有效，无效则重新寻找
        /// </summary>
        /// <returns> 0: 目标未更改 1: 目标已更改 2: 目标更改为空 </returns>
        internal static TargetValidationResult ValidateTarget(
            PlayerCalcSystem playerCalc,
            Player player,
            long frame)
        {
            var targetChanged = TargetValidationResult.OldTargetValid;

            var property = PlayerFrameUtil.GetProp(player, frame);

            // 如果目标失效，重新寻找目标
            var target = property.Target == 0
                ? null
                : playerCalc.GetPlayerById(property.Target);

            // 如果目标已死亡，重新寻找目标
            if (target == null || PlayerFrameUtil.GetProp(target, frame).Hp <= 0)
            {
                var oldTarget = target != null ? target.PlayerId : 0;
                target = GetEnemy(playerCalc, player, frame);
                var newTarget = target != null ? target.PlayerId : 0;
                property.Target = newTarget;
                Log.D(player.PlayerId, "Target Changed", oldTarget, "To", newTarget);
                if (oldTarget != newTarget)
                {
                    targetChanged = newTarget == 0 ? TargetValidationResult.NoTargetValid : TargetValidationResult.NewTargetValid;
                }
            }
            return targetChanged;
        }

        internal static void InitSkills(Player player, CharacterDataRowT characterData)
        {
            // 初始化技能列表
            player.CastSkill = characterData.CastSkill;
            player.SpecialSkill = characterData.SpecialSkill;

            // 设置技能初始冷却时间
            var prop = PlayerFrameUtil.GetProp(player, 0);
            prop.SkillCDCounters = XList<CD_Counter>.Get();

            // var castSkillData = Configs.Instance.SkillData.GetData(player.CastSkill);
            // Log.AssertIsTrue(castSkillData != null, "Character InitCharacter SkillData Not Found", player.CastSkill);
            // prop.SkillMarkCd(castSkillData.SkillType, castSkillData.Cooldown / (int)Constants.FRAME_LENGTH_MS);

            // var specialSkillData = Configs.Instance.SkillData.GetData(player.SpecialSkill);
            // Log.AssertIsTrue(specialSkillData != null, "Character InitCharacter SkillData Not Found", player.SpecialSkill);
            // prop.SkillMarkCd(specialSkillData.SkillType, specialSkillData.Cooldown / (int)Constants.FRAME_LENGTH_MS);
        }

        internal static bool IsSkillAvailable(Player player, long frame, RunningSkill skill)
        {
            var prop = PlayerFrameUtil.GetProp(player, frame);
            if (prop.RunningSkill.HasValue)
            {
                return false;
            }
            if (prop.SkillCd(skill.SkillType) > 0)
            {
                return false;
            }
            return true;
        }
    }
}
