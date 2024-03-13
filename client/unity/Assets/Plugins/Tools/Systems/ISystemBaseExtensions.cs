using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wtf
{
    public static class ISystemBaseExtensions
    {
        public static async UniTask DoInitialize(this ISystemBase system)
        {
            Log.D("DoInitialize:", system.GetType().Name);

            if (!system.IsInitialized)
            {
                system.Initialize();
            }

            if (!system.IsInitialized)
            {
                await system.AsyncInitialize();
            }

            if (!system.IsInitialized)
            {
                Log.E("DoInitialize: failed to initialize", system.GetType().Name);
            }
        }

        public static T GetSystem<T>(this ISystemBase context) where T : Component, ISystemBase
        {
            var key = typeof(T).Name;
            var subSystems = context.SubSystems;
            if (subSystems == null)
            {
                return null;
            }
            if (!subSystems.TryGetValue(key, out var system))
            {
                return null;
            }
            return (T)system;
        }

        public static ISystemBase GetSystem(this ISystemBase context, string systemName)
        {
            var subSystems = context.SubSystems;
            if (subSystems == null)
            {
                return null;
            }
            if (!subSystems.TryGetValue(systemName, out var system))
            {
                return null;
            }
            return system;
        }

        public static void RemoveSystem<T>(this ISystemBase context) where T : Component, ISystemBase
        {
            var key = typeof(T).Name;
            var subSystems = context.SubSystems;
            if (subSystems == null)
            {
                return;
            }
            if (subSystems.TryGetValue(key, out var system))
            {
                subSystems.Remove(key);
                var sc = system as Component;
                Object.Destroy(sc.gameObject);
            }
        }

        public static T AddSystem<T>(this ISystemBase context) where T : Component, ISystemBase
        {
            var parentName = context.GetType().Name;
            var systemName = typeof(T).Name;
            Log.D(parentName, "AddSystem:", systemName);
            if (context.SubSystems == null)
            {
                Log.E(parentName, "AddSystem: subSystems is null", systemName);
                return null;
            }
            GameObject gameObject = new GameObject(systemName);
            gameObject.transform.SetParent(context.Tm);
            var system = gameObject.AddComponent<T>();
            context.SubSystems[systemName] = system;
            return system;
        }

        public static T AddExistSystem<T>(this ISystemBase context, GameObject prefabObject, Transform parentTm = null) where T : Component, ISystemBase
        {
            var parentName = context.GetType().Name;
            var systemName = typeof(T).Name;
            Log.D(parentName, "AddExistSystem:", systemName);
            if (context.SubSystems == null)
            {
                Log.E(parentName, "AddExistSystem: subSystems is null", systemName);
                return null;
            }
            GameObject go = GameObject.Instantiate(prefabObject);
            if (parentTm == null)
            {
                go.transform.SetParent(context.Tm);
            }
            else
            {
                go.transform.SetParent(parentTm);
            }
            go.name = systemName;
            var system = go.GetComponent<T>();
            if (system == null)
            {
                system = go.AddComponent<T>();
            }
            context.SubSystems[systemName] = system;
            return system;
        }

        public static async UniTask InitializeSystems(this ISystemBase context)
        {
            Log.D("InitializeSystems:", context.GetType().Name, context.SubSystems.Count);

            // wait for all systems to initialize
            var subSystems = context.SubSystems;
            if (subSystems == null)
            {
                return;
            }

            var tasks = XList<UniTask>.Get(subSystems.Count);
            foreach (var kv in subSystems)
            {
                tasks.Add(kv.Value.DoInitialize());
            }
            await UniTask.WhenAll(tasks);
            tasks.Dispose();
        }

        public static void DeinitializeSystems(this ISystemBase context)
        {
            // wait for all systems to initialize
            var subSystems = context.SubSystems;
            if (subSystems == null)
            {
                return;
            }

            foreach (var kv in subSystems)
            {
                if (kv.Value.IsInitialized)
                    kv.Value.Deinitialize();
            }
        }
    }
}