using Wtf;
using Proto;
using Battle;

namespace Lockstep
{
    public static class LockStepPrepareChars
    {
        public static void Run(
            XList<IGameActionFrame> actionList,
            long frame,
            ListGamePlayFrame framePlay,

            PlayerCalcSystem playerCalc
        )
        {
            // 继承上一帧的属性
            for (int i = 0; i < playerCalc.PlayerCount; i++)
            {
                var player = playerCalc.GetPlayerAtIndex(i);
                if (player == null)
                {
                    Log.E("Character is null");
                    continue;
                }

                var prop = PlayerFrameUtil.GetOrCopyProp(player, frame);
                if (prop == null)
                {
                    Log.E("CharacterProp is null");
                    continue;
                }

                PlayerEOTUtil.CalcEOT(player, prop, frame, out var eotDamageOrHeal);

                if (eotDamageOrHeal.Count > 0)
                {
                    var maxHp = prop.StatProp[(int)StatPropType.HP];
                    foreach (var doh in eotDamageOrHeal)
                    {
                        prop.Hp += doh.Doh;
                        if (prop.Hp <= 0)
                        {
                            prop.Hp = 0;
                        }
                        framePlay.Plays.Add(GamePlayFrame_PlayerHPChanged.Create(
                            doh.CharacterFrom,
                            doh.CharacterTarget,
                            doh.SkillType,
                            doh.ProjectileType,
                            doh.Doh,
                            prop.Hp,
                            maxHp));
                    }
                }

                // 计算技能冷却时间
                if (prop.SkillCDCounters != null && prop.SkillCDCounters.Count > 0)
                {
                    for (int j = prop.SkillCDCounters.Count - 1; j >= 0; j--)
                    {
                        var cd = prop.SkillCDCounters[j];
                        cd.Counter -= 1;
                        if (cd.Counter > 0)
                        {
                            prop.SkillCDCounters[j] = cd;
                        }
                        else
                        {
                            prop.SkillCDCounters.RemoveAt(j);
                        }
                    }
                }

                // 检测死亡
                if (!prop.IsDead)
                {
                    if (prop.Hp <= 0)
                    {
                        // 角色上一帧已死亡且无死亡触发器，本帧设置为已死亡
                        // TODO: 角色 无死亡触发器
                        // && !CharacterAttributeUtil.CharacterHasTriggerForType(character, AttributeTriggerType.BeKill)
                        prop.IsDead = true;

                        // 死亡后，清理所有 持续效果
                        prop.ClearEOT();

                        framePlay.Plays.Add(GamePlayFrame_PlayerDefeat.Create(
                            player.PlayerId
                        ));
                    }
                }

                var hasChanged = PlayerBulletBufferUtil.CalcBulletBuffer(player, prop, frame);
                if (hasChanged)
                {
                    framePlay.Plays.Add(GamePlayFrame_BulletBufferChanged.Create(
                        player.PlayerId,
                        XList<ProjectileType>.CopyFrom(prop.BulletBuffer)
                    ));
                }
                Log.D("BulletBuffer", player.PlayerId, prop.BulletBuffer);

                if (frame == 0)
                {
                    // 第一帧，角色进入战场
                    framePlay.Plays.Add(GamePlayFrame_PlayerEnter.Create(
                        player.PlayerId,
                        player.PlayerId,
                        player.CampId,
                        prop.Hp
                    ));
                }
            }
        }
    }
}