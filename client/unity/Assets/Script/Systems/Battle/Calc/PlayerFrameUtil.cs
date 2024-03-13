using System.Collections.Generic;
using System.Text;
using App;

namespace Battle
{
    internal static class PlayerFrameUtil
    {
        internal static PlayerFrameProp GetProp(Player player, long frame)
        {
            PlayerFrameProp prop = null;
            if (player.FrameProperties.TryGetValue(frame, out prop))
            {
                return prop;
            }
#if UNITY_EDITOR && DEBUG
            else
            {
                var sb = new StringBuilder();
                sb.Append("[");
                for (int i = 0; i < player.Frames.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(player.Frames[i]);
                }
                sb.Append("]");
                Log.D("PlayerFrameUtil.GetProp", sb.ToString());
                Log.E("PlayerFrameUtil.GetProp", player.PlayerId, "Frame", frame, "Not Found");
            }
#endif
            return null;
        }

        internal static PlayerFrameProp GetOrCopyProp(Player player, long frame)
        {
            PlayerFrameProp prop = null;
            UnityEngine.Profiling.Profiler.BeginSample("PlayerFrameUtil.GetOrCopyProp");
            if (!player.FrameProperties.TryGetValue(frame, out prop))
            {
                var tempFrame = frame - 1;
                while (tempFrame >= 0)
                {
                    if (player.FrameProperties.TryGetValue(tempFrame, out var tempProp))
                    {
                        prop = PlayerFrameProp.CopyFrom(tempProp);
                        player.FrameProperties[frame] = prop;
                        player.Frames.Add(frame);
                        Log.D("PlayerFrameUtil.GetOrCopyProp", "player", player.PlayerId, "copy", tempFrame, "to", frame);
                        break;
                    }
                    tempFrame--;
                }
            }

            Log.AssertIsTrue(prop != null, "PlayerFrameUtil.GetOrCopyProp Property Not Initialized", player.PlayerId);
            UnityEngine.Profiling.Profiler.EndSample();
            return prop;
        }

        internal static void SetProp(Player character, long frame, PlayerFrameProp property)
        {
            // Log.D("PlayerFrameUtil.SetProp", character.CharacterId, frame);
            UnityEngine.Profiling.Profiler.BeginSample("PlayerFrameUtil.SetProp");
            character.FrameProperties[frame] = property;
            var hasFrame = false;
            for (int i = character.Frames.Count - 1; i >= 0; i--)
            {
                if (character.Frames[i] == frame)
                {
                    hasFrame = true;
                    break;
                }
                if (character.Frames[i] < frame)
                {
                    break;
                }
            }
            if (!hasFrame)
            {
                character.Frames.Add(frame);
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        internal static void ClearFinishedData(
            Battle.PlayerCalcSystem characterManager,
            Lockstep.LockStepSystem lockStepSystem)
        {
            // 比如当前已缓存到 64 帧, 当前播放帧是第 60 帧，保留32帧现场，那么 60 - 32 = 28 帧之前的数据都可以清理掉
            var frameTop = lockStepSystem.PlayingGameFrame - Constants.FRAME_COUNT_TO_CACHE;

            // 清理角色属性
            // 上次清理时的帧数，如果没清理过，则是0，比如清理过一次 即为 10
            var pos = characterManager.CharacterFrameFree == Constants.INVALID_VALUE
                ? 0
                : characterManager.CharacterFrameFree + 1;

            if (frameTop >= pos)
            {
                for (int i = 0; i < characterManager.PlayerCount; i++)
                {
                    var character = characterManager.GetPlayerAtIndex(i);

                    if (character.Frames.Count > 0)
                    {
                        var minFrame = character.Frames[0];
                        if (minFrame < frameTop)
                        {
                            int j = 0;
                            for (; j < character.Frames.Count; j++)
                            {
                                if (character.Frames[j] < frameTop)
                                {
                                    // Log.D("ClearFinishedData Remove", character.CharacterId, character.Frames[j]);
                                    UnityEngine.Profiling.Profiler.BeginSample("PlayerFrameUtil.ClearFinishedData");
                                    var data = character.FrameProperties[character.Frames[j]];
                                    data.Dispose();
                                    character.FrameProperties.Remove(character.Frames[j]);
                                    UnityEngine.Profiling.Profiler.EndSample();
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (j >= 0 && j < character.Frames.Count)
                            {
                                character.Frames.RemoveRange(0, j);
                            }
                        }
                    }
                }
                characterManager.CharacterFrameFree = frameTop;
            }
        }
    }
}