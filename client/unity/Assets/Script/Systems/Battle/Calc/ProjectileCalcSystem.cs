using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Lockstep;
using Proto;
using UnityEngine;
using Wtf;

namespace Battle
{
    /// <summary>
    /// 投射物管理器
    /// 角色释放的技能, 会产生投射物
    /// </summary>
    internal class ProjectileCalcSystem : MonoBehaviour, ISystemBase
    {
        // 分帧属性
        internal List<long> Frames = new List<long>(64);
        // < Frame : List<Projectile> >
        internal Dictionary<long, XList<Projectile>> FrameProjectiles = new Dictionary<long, XList<Projectile>>(64);

        internal Projectile CreateProjectile(int projectileId = Constants.INVALID_VALUE)
        {
            Projectile projectile = Projectile.Get();
            projectile.ProjectileId = projectileId == Constants.INVALID_VALUE ? GetProjectileId() : projectileId;
            return projectile;
        }

        internal XList<Projectile> GetOrCopyFrameProjectiles(long frame)
        {
            XList<Projectile> projectiles = null;
            UnityEngine.Profiling.Profiler.BeginSample("ProjectileManager.GetOrCopyFrameProjectiles");
            if (!FrameProjectiles.TryGetValue(frame, out projectiles))
            {
                XList<Projectile> tempProp = null;
                if (FrameProjectiles.TryGetValue(frame - 1, out tempProp))
                {
                    projectiles = XList<Projectile>.Get(tempProp.Count);
                    foreach (var p in tempProp)
                    {
                        Projectile projectile = Projectile.CopyFrom(p);
                        projectiles.Add(projectile);
                    }
                    Frames.Add(frame);
                    FrameProjectiles[frame] = projectiles;
                    Log.D("ProjectileManager.GetOrCopyFrameProjectiles", "copy", frame - 1, "to", frame);
                }
            }

            // if (prop == null) Log.W("ProjectileManager.GetOrCopyFrameProjectiles Not Initialized");
            UnityEngine.Profiling.Profiler.EndSample();
            return projectiles;
        }

        internal void SetProjectile(long frame, Projectile projectile)
        {
            XList<Projectile> projectiles = null;
            if (!FrameProjectiles.TryGetValue(frame, out projectiles))
            {
                projectiles = XList<Projectile>.Get(1);
                FrameProjectiles.Add(frame, projectiles);
            }
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].ProjectileId == projectile.ProjectileId)
                {
                    projectiles[i] = projectile;
                    return;
                }
            }
            projectiles.Add(projectile);
        }

        internal int ProjectileIdCounter = 1;

        internal long FrameFree { get; set; }

        public Transform Tm => transform;

        public bool IsInitialized { get; private set; } = false;

        public Dictionary<string, ISystemBase> SubSystems => null;

        internal int GetProjectileId()
        {
            if (ProjectileIdCounter == int.MaxValue - 1)
            {
                ProjectileIdCounter = 1;
            }
            return ProjectileIdCounter++;
        }

        internal void ReleaseFrame(long frame)
        {
            if (FrameProjectiles.TryGetValue(frame, out var projectiles))
            {
                for (int i = 0; i < projectiles.Count; i++)
                {
                    projectiles[i].Dispose();
                }
                projectiles.Dispose();
                FrameProjectiles.Remove(frame);
            }
        }

        internal void Reset()
        {
            foreach (var projectiles in FrameProjectiles.Values)
            {
                for (int i = 0; i < projectiles.Count; i++)
                {
                    projectiles[i].Dispose();
                }
                projectiles.Dispose();
            }
            FrameProjectiles.Clear();
        }

        internal void ClearFinishedData(
            LockStepSystem lockstepSystem,
            TimeSystem timeSystem,
            BattleSystem battleSystem
        )
        {
            // 比如当前已缓存到 64 帧, 当前播放帧是第 60 帧，保留32帧现场，那么 60 - 32 = 28 帧之前的数据都可以清理掉
            var frameTop = lockstepSystem.PlayingGameFrame - Constants.FRAME_COUNT_TO_CACHE;
            // 清理角色属性
            // 上次清理时的帧数，如果没清理过，则是0，比如清理过一次 即为 10
            var pos = FrameFree == Constants.INVALID_VALUE ? 0 : FrameFree + 1;
            if (frameTop >= pos && Frames.Count > 0)
            {
                var minFrame = Frames[0];
                if (minFrame < frameTop)
                {
                    int j = 0;
                    for (; j < Frames.Count; j++)
                    {
                        if (Frames[j] < frameTop)
                        {
                            // Log.D("ClearFinishedData Remove", character.CharacterId, character.Frames[j]);
                            UnityEngine.Profiling.Profiler.BeginSample("ProjectileManager.ClearFinishedData");
                            var data = FrameProjectiles[Frames[j]];
                            foreach (var p in data)
                            {
                                p.Dispose();
                            }
                            data.Dispose();
                            FrameProjectiles.Remove(Frames[j]);
                            UnityEngine.Profiling.Profiler.EndSample();
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (j >= 0 && j < Frames.Count)
                    {
                        Frames.RemoveRange(0, j);
                    }

                    FrameFree = frameTop;
                }
            }
        }

        public void Initialize()
        {
            IsInitialized = true;
        }

        public UniTask AsyncInitialize()
        {
            throw new NotImplementedException();
        }

        public void Deinitialize()
        {
            IsInitialized = false;
        }
    }
}