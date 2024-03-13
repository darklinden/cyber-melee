using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Wtf
{
    public class LoadUtil
    {
        const int MAX_ATTEMPTS = 3;

        // singleton
        private static LoadUtil sm_LoadUtil = null;
        public static LoadUtil Instances
        {
            get
            {
                if (sm_LoadUtil == null)
                {
                    sm_LoadUtil = new LoadUtil();
                }
                return sm_LoadUtil;
            }
        }

        // loading lock
        private readonly HashSet<string> LoadingPathsLock = new HashSet<string>();
        private async UniTask WaitingForUnlock(string path)
        {
            if (LoadingPathsLock.Contains(path))
            {
                var timeOut = 10f;
                // wait for lock unlock
                while (true)
                {
                    await UniTask.Delay(100, true, PlayerLoopTiming.TimeUpdate);

                    timeOut -= 0.1f;
                    if (timeOut <= 0)
                    {
                        Log.E("LoadUtil.WaitLock timeout: ", path);
                        break;
                    }
                    if (!LoadingPathsLock.Contains(path))
                    {
                        break;
                    }
                }
            }
        }

        private bool IsLocked(string path)
        {
            return LoadingPathsLock.Contains(path);
        }

        private void Lock(string path)
        {
            LoadingPathsLock.Add(path);
        }

        private void Unlock(string path)
        {
            LoadingPathsLock.Remove(path);
        }

        // save failed paths to avoid too many attempts
        private readonly Dictionary<string, int> FailedPaths = new Dictionary<string, int>();

        // load loaded prototypes
        private readonly Dictionary<string, UnityEngine.Object> LoadedObjects = new Dictionary<string, UnityEngine.Object>(128);
        private UnityEngine.Object GetLoaded(string path)
        {
            if (LoadedObjects.TryGetValue(path, out UnityEngine.Object obj))
            {
                return obj;
            }
            if (FailedPaths.TryGetValue(path, out var failedCount))
            {
                if (failedCount > MAX_ATTEMPTS)
                {
                    return null;
                }
            }
            return obj;
        }

        public static T Get<T>(string path) where T : UnityEngine.Object
        {
            return Instances.GetLoaded(path) as T;
        }

        // loaded paths by label
        private readonly Dictionary<string, List<string>> LoadedPathsByLabel = new Dictionary<string, List<string>>(64);
        private async UniTask<List<string>> AsyncLoadPathsByLabel_(string label)
        {
            // 如果正在加载中，等待加载完成
            if (IsLocked(label))
            {
                await WaitingForUnlock(label);
            }

            // if cached already, return
            if (LoadedPathsByLabel.TryGetValue(label, out List<string> paths))
            {
                return paths;
            }

            // if failed too many times, return
            if (FailedPaths.TryGetValue(label, out var failedCount))
            {
                if (failedCount > MAX_ATTEMPTS)
                {
#if UNITY_EDITOR && DEBUG
                Log.E("LoadUtil.loadPathsByLabel failed too many times", label);
#else
                    return null;
#endif
                }
            }

            Lock(label);

            var handle = Addressables.LoadResourceLocationsAsync(label);
            await handle;

            paths = new List<string>();
            if (handle.Result != null && handle.Result.Count > 0)
            {
                foreach (var loc in handle.Result)
                    paths.Add(loc.PrimaryKey);
            }
            LoadedPathsByLabel[label] = paths;
            Addressables.Release(handle);

            Unlock(label);

            return paths;
        }

        public static async UniTask<List<string>> AsyncLoadPathsByLabel(string label)
        {
            return await Instances.AsyncLoadPathsByLabel_(label);
        }

        // load by path
        private async UniTask<T> AsyncLoad_<T>(string path) where T : UnityEngine.Object
        {
            // 如果正在加载中，等待加载完成
            if (IsLocked(path))
            {
                await WaitingForUnlock(path);
            }

            // if cached already, return
            if (LoadedObjects.TryGetValue(path, out UnityEngine.Object obj))
            {
                return obj as T;
            }

            // if failed too many times, return
            if (FailedPaths.TryGetValue(path, out var failedCount))
            {
                if (failedCount > MAX_ATTEMPTS)
                {
#if UNITY_EDITOR && DEBUG
                Log.E("LoadUtil.load too many times", path);
#endif
                    return null;
                }
            }

            Lock(path);
            try
            {
                var handle = Addressables.LoadAssetAsync<T>(path);
                await handle;
                obj = null;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    obj = handle.Result;
                    LoadedObjects.Add(path, obj);
                }
            }
            catch (Exception)
            {
                // Log.E("LodUtil.load Exception", e, path);
            }

            if (obj == null)
            {
                int count = 0;
                if (!FailedPaths.TryGetValue(path, out count))
                {
                    count = 0;
                }
                count++;
                FailedPaths[path] = count;
                Log.E("LoadUtil.load Failed", path, count);
            }

            Unlock(path);

            return obj as T;
        }

        public static async UniTask<T> AsyncLoad<T>(string path) where T : UnityEngine.Object
        {
            return await Instances.AsyncLoad_<T>(path);
        }

        private readonly Dictionary<string, HashSet<string>> LoadedGroup = new Dictionary<string, HashSet<string>>(64);
        public static async UniTask<T> AsyncLoad<T>(string path, string group) where T : UnityEngine.Object
        {
            var loaded = await Instances.AsyncLoad_<T>(path);
            if (loaded != null)
            {
                if (!Instances.LoadedGroup.TryGetValue(group, out var paths))
                {
                    paths = new HashSet<string>();
                    Instances.LoadedGroup[group] = paths;
                }
                paths.Add(path);
                return loaded;
            }
            return null;
        }

        public static void UnloadGroup(string group)
        {
            if (Instances.LoadedGroup.TryGetValue(group, out var paths))
            {
                foreach (var path in paths)
                {
                    Release(path);
                }
                Instances.LoadedGroup.Remove(group);
            }
        }

        private void Release_(string pathOrLabel)
        {
            if (LoadedPathsByLabel.TryGetValue(pathOrLabel, out List<string> paths))
            {
                // release all paths by label
                foreach (var path in paths)
                {
                    if (LoadedObjects.TryGetValue(path, out UnityEngine.Object obj))
                    {
                        if (obj != null) Addressables.Release(obj);
                        LoadedObjects.Remove(path);
                    }
                }
                LoadedPathsByLabel.Remove(pathOrLabel);
            }
            else
            {
                // release single path
                if (LoadedObjects.TryGetValue(pathOrLabel, out UnityEngine.Object obj))
                {
                    if (obj != null) Addressables.Release(obj);
                    LoadedObjects.Remove(pathOrLabel);
                }
            }
        }

        public static void Release(string pathOrLabel)
        {
            Instances.Release_(pathOrLabel);
        }

        public static async UniTask<bool> AsyncLoadScene(string path)
        {
            var handle = Addressables.LoadSceneAsync(path, UnityEngine.SceneManagement.LoadSceneMode.Single);
            await handle;
            return handle.Status == AsyncOperationStatus.Succeeded;
        }
    }
}