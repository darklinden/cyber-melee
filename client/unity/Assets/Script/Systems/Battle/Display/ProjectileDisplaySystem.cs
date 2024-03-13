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
    internal class ProjectileDisplaySystem : MonoBehaviour, ISystemBase
    {
        private Transform m_Tm = null;
        public Transform Tm => m_Tm != null ? m_Tm : (m_Tm = transform);
        public Dictionary<string, ISystemBase> SubSystems { get; } = new Dictionary<string, ISystemBase>();
        public bool IsInitialized { get; private set; } = false;

        private GameObjectTypedPool ProjectileBodyPool = null;
        public void Initialize()
        {

        }

        public async UniTask AsyncInitialize()
        {
            ProjectileBodyPool = GameObjectTypedPool.CreateInstance(Tm, "ProjectileBodyPool");
            var enumList = Enum.GetValues(typeof(ProjectileType));
            ProjectileBodyPool.Initialize(enumList.Length - 1);

            for (int i = 0; i < enumList.Length; i++)
            {
                var type = (ProjectileType)enumList.GetValue(i);
                if (type == ProjectileType.NONE) continue;
                var prefabPath = $"Assets/Addrs/en-US/Prefabs/Projectile/{type}.prefab";
                var prefab = await LoadUtil.AsyncLoad<GameObject>(prefabPath);
                ProjectileBodyPool.SetPrefab((int)type, prefab, prefabPath);
                await ProjectileBodyPool.PrewarmAsync((int)type, 10);
            }

            var eventBus = Context.Inst.EventBus;
            eventBus.OnDispatchPlayProjectileChange += OnDispatchPlayProjectileChange;
            eventBus.OnDispatchPlayProjectileSpawn += OnDispatchPlayProjectileSpawn;
            eventBus.OnDispatchPlayProjectileArrive += OnDispatchPlayProjectileArrive;
            eventBus.OnDispatchPlayProjectileRemove += OnDispatchPlayProjectileRemove;

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            ProjectileBodyPool?.DeinitializeAll();
            Destroy(ProjectileBodyPool.gameObject);
            ProjectileBodyPool = null;

            var eventBus = Context.Inst.EventBus;
            eventBus.OnDispatchPlayProjectileSpawn -= OnDispatchPlayProjectileSpawn;
            eventBus.OnDispatchPlayProjectileChange -= OnDispatchPlayProjectileChange;
            eventBus.OnDispatchPlayProjectileArrive -= OnDispatchPlayProjectileArrive;
            eventBus.OnDispatchPlayProjectileRemove -= OnDispatchPlayProjectileRemove;

            IsInitialized = false;
        }

        private Dictionary<int, ProjectileBody> RunningProjectiles = new Dictionary<int, ProjectileBody>();
        private List<int> ProjectilesShouldRemove = new List<int>();

        private void OnDispatchPlayProjectileArrive(long frame, GamePlayFrame_ProjectileArrive gamePlayFrame_ProjectileArrive)
        {
            Log.D("OnDispatchPlayProjectileArrive:", gamePlayFrame_ProjectileArrive);
            ProjectilesShouldRemove.Add(gamePlayFrame_ProjectileArrive.ProjectileId);
        }

        private void OnDispatchPlayProjectileChange(long frame, GamePlayFrame_ProjectileChange gamePlayFrame_ProjectileChange)
        {
            Log.D("OnDispatchPlayProjectileChange:", gamePlayFrame_ProjectileChange);
            switch (gamePlayFrame_ProjectileChange.EOTType)
            {
                case EOTType.Eliminate:
                    {
                        ProjectilesShouldRemove.Add(gamePlayFrame_ProjectileChange.ProjectileId);
                    }
                    break;
                case EOTType.HitBack:
                    {
                        if (RunningProjectiles.TryGetValue(gamePlayFrame_ProjectileChange.ProjectileId, out var projectileBody))
                        {
                            projectileBody.HitBack();
                        }
                    }
                    break;
                case EOTType.PowerUp:
                    {
                        if (RunningProjectiles.TryGetValue(gamePlayFrame_ProjectileChange.ProjectileId, out var projectileBody))
                        {
                            projectileBody.PowerUp(gamePlayFrame_ProjectileChange.EOTValue0 != 0);
                        }
                    }
                    break;
                case EOTType.Duration:
                    {
                        if (RunningProjectiles.TryGetValue(gamePlayFrame_ProjectileChange.ProjectileId, out var projectileBody))
                        {
                            projectileBody.Duration((int)gamePlayFrame_ProjectileChange.EOTValue0, (int)gamePlayFrame_ProjectileChange.EOTValue1);
                        }
                    }
                    break;
                case EOTType.ATKPower:
                    {
                        if (RunningProjectiles.TryGetValue(gamePlayFrame_ProjectileChange.ProjectileId, out var projectileBody))
                        {
                            projectileBody.Buff((int)gamePlayFrame_ProjectileChange.EOTValue0);
                        }
                    }
                    break;
                case EOTType.BleedTrigger:
                    {
                        if (RunningProjectiles.TryGetValue(gamePlayFrame_ProjectileChange.ProjectileId, out var projectileBody))
                        {
                            projectileBody.Bleed((int)gamePlayFrame_ProjectileChange.EOTValue0);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDispatchPlayProjectileSpawn(long frame, GamePlayFrame_ProjectileSpawn gamePlayFrame_ProjectileSpawn)
        {
            Log.D("OnDispatchPlayProjectileSpawn:", gamePlayFrame_ProjectileSpawn);
            var projectileBodyGo = ProjectileBodyPool.Get((int)gamePlayFrame_ProjectileSpawn.ProjectileType);
            var projectileBody = projectileBodyGo.AddOrGetComponent<ProjectileBody>();
            projectileBody.SetData(gamePlayFrame_ProjectileSpawn);
            RunningProjectiles.Add(gamePlayFrame_ProjectileSpawn.ProjectileId, projectileBody);
        }

        private void OnDispatchPlayProjectileRemove(long frame, GamePlayFrame_ProjectileRemove gamePlayFrame_ProjectileRemove)
        {
            Log.D("OnDispatchPlayProjectileRemove:", gamePlayFrame_ProjectileRemove);
            ProjectilesShouldRemove.Add(gamePlayFrame_ProjectileRemove.ProjectileId);
        }

        private void Update()
        {
            if (ProjectilesShouldRemove.Count > 0)
            {
                foreach (var id in ProjectilesShouldRemove)
                {
                    if (RunningProjectiles.TryGetValue(id, out var projectileBody))
                    {
                        projectileBody.gameObject.SetActive(false);
                        ProjectileBodyPool.Return(projectileBody.gameObject, (int)projectileBody.ProjectileType);
                    }
                    RunningProjectiles.Remove(id);
                }
                ProjectilesShouldRemove.Clear();
            }

            var deltaTime = Time.deltaTime;
            foreach (var kv in RunningProjectiles)
            {
                var projectileBody = kv.Value;
                projectileBody.Move(deltaTime);
            }
        }
    }
}