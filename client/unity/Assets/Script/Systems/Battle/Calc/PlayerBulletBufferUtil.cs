using System;
using System.Collections.Generic;
using Proto;
using Google.FlatBuffers;
using Wtf;
using App;
using Lockstep;

namespace Battle
{
    internal static class PlayerBulletBufferUtil
    {
        private static List<ProjectileType> m_BulletTypeList = null;
        private static List<ProjectileType> BulletTypes
        {
            get
            {
                if (m_BulletTypeList == null)
                {
                    m_BulletTypeList = new List<ProjectileType>();
                    foreach (var kv in Configs.Instance.ProjectileData.ProjectileDataDict)
                    {
                        m_BulletTypeList.Add(kv.Value.ProjectileType);
                    }
                }
                return m_BulletTypeList;
            }
        }

        private static ProjectileType RandomBulletType(Player player)
        {
            return BulletTypes[SeedRandom.Value(player.PlayerId, SeedType.BulletGen).Range(0, BulletTypes.Count - 1)];
        }

        internal static bool CalcBulletBuffer(
            Player player,
            PlayerFrameProp prop,
            long frame)
        {
            bool hasChanged = false;
            if (frame % player.BulletGenerateCdFrameCount == 0)
            {
                // 生成子弹
                if (prop.BulletBuffer.Count < player.BulletBufferSize)
                {
                    if (frame == 0)
                    {
                        // 第一帧生成子弹 满上
                        Log.D("First frame generate bullet", player.PlayerId, player.BulletBufferSize);
                        while (prop.BulletBuffer.Count < player.BulletBufferSize)
                        {
                            prop.BulletBuffer.Add(RandomBulletType(player));
                        }
                        hasChanged = true;
                    }
                    else
                    {
                        // 否则 +1
                        Log.D("Generate bullet", player.PlayerId, prop);
                        prop.BulletBuffer.Add(RandomBulletType(player));
                        hasChanged = true;
                    }
                }
            }
            return hasChanged;
        }
    }
}