using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;


namespace Wtf
{
    public sealed class UILoader
    {
        // --- Singleton ---
        private static UILoader sm_UILoader = null;
        private static UILoader Instance => sm_UILoader != null ? sm_UILoader : sm_UILoader = new UILoader();
        // --- Singleton ---

        private string _scene = null;
        private string scene
        {
            get
            {
                if (_scene == null)
                {
                    _scene = SceneManager.GetActiveScene().name;
                }
                return _scene;
            }

            set
            {
                _scene = value;
            }
        }

        private Dictionary<string, Dictionary<string, GameObject>> _panelsOnCurrentScene;
        private Dictionary<int, HashSet<string>> _panelsWithGroups;

        private UILoader()
        {
            Log.D(GetType().Name, "constructor");
            SceneManager.activeSceneChanged += ChangedActiveScene;
            _panelsOnCurrentScene = new Dictionary<string, Dictionary<string, GameObject>>();
            _panelsWithGroups = new Dictionary<int, HashSet<string>>();
        }

        private void ChangedActiveScene(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
        {
            Log.D("ChangedActiveScene: ",
                current != null ? string.Concat(" current: ", current.name) : "no current name",
                next != null ? string.Concat(" next: ", next.name) : "no next name");

            scene = next.name;

            _panelsWithGroups.Clear();
            foreach (var k in _panelsOnCurrentScene.Keys)
            {
                if (k == scene) continue;

                Dictionary<string, GameObject> dict = null;
                if (_panelsOnCurrentScene.TryGetValue(k, out dict))
                {
                    if (dict != null && dict.Keys.Count > 0)
                    {
                        foreach (var j in dict.Keys)
                        {
                            if (dict[j] != null)
                            {
                                if (!Addressables.ReleaseInstance(dict[j])) UnityEngine.Object.Destroy(dict[j]);
                            }
                        }
                        dict.Clear();
                    }
                }
            }

            Resources.UnloadUnusedAssets();
        }

        private void SetPanel(string path, GameObject panel)
        {
            Dictionary<string, GameObject> dict = null;
            if (!_panelsOnCurrentScene.TryGetValue(scene, out dict))
            {
                dict = new Dictionary<string, GameObject>();
                _panelsOnCurrentScene[scene] = dict;
            }
            dict[path] = panel;
        }

        private GameObject GetPanel(string path)
        {
            Dictionary<string, GameObject> dict = null;
            if (!_panelsOnCurrentScene.TryGetValue(scene, out dict))
            {
                dict = new Dictionary<string, GameObject>();
                _panelsOnCurrentScene[scene] = dict;
            }

            GameObject obj = null;
            if (!dict.TryGetValue(path, out obj)) obj = null;
            return obj;
        }

        private bool _HasShownPanel(int group = 0)
        {
            bool has = false;
            if (_panelsWithGroups.TryGetValue(group, out HashSet<string> set))
            {
                if (set != null && set.Count > 0) has = true;
            }
            return has;
        }

        public static bool HasShownPanel(int group = 0)
        {
            return Instance._HasShownPanel(group);
        }

        private void _MarkPanelWithGroup(int group, string key)
        {
            if (!_panelsWithGroups.TryGetValue(group, out HashSet<string> set))
            {
                set = new HashSet<string>();
                _panelsWithGroups[group] = set;
            }
            set.Add(key);
        }

        public static void MarkPanelWithGroup(int group, string key)
        {
            Instance._MarkPanelWithGroup(group, key);
        }

        private void _RemoveMarkPanelWithGroup(int group, string key)
        {
            if (_panelsWithGroups.TryGetValue(group, out HashSet<string> set))
            {
                if (set != null && set.Count > 0)
                {
                    set.Remove(key);
                }
            }
        }

        public static void RemoveMarkPanelWithGroup(int group, string key)
        {
            Instance._RemoveMarkPanelWithGroup(group, key);
        }

        private void _RemoveMarkPanelWithGroup(string key)
        {
            foreach (var k in _panelsWithGroups.Keys)
            {
                _RemoveMarkPanelWithGroup(k, key);
            }
        }

        public static void RemoveMarkPanelWithGroup(string key)
        {
            Instance._RemoveMarkPanelWithGroup(key);
        }

        public static void Show(string PanelPath)
        {
            Instance._AsyncShow<object>(PanelPath, null).Forget();
        }

        public static async UniTask<CommonPanel> AsyncShow(string PanelPath)
        {
            return await Instance._AsyncShow<object>(PanelPath, null);
        }

        public static void Show<DataType>(string PanelPath, DataType data)
        {
            Instance._AsyncShow<DataType>(PanelPath, data).Forget();
        }

        public static async UniTask<CommonPanel> AsyncShow<DataType>(string PanelPath, DataType data)
        {
            return await Instance._AsyncShow<DataType>(PanelPath, data);
        }

        private async UniTask<CommonPanel> _AsyncShow<DataType>(string PanelPath, DataType data)
        {
            Log.D("UILoader", "AsyncShow", PanelPath);
            string path = PanelPath.EndsWith(".prefab") ? PanelPath : string.Concat(PanelPath, ".prefab");
            CommonPanel panelComponent = null;
            var p = GetPanel(path);
            if (p != null)
            {
                panelComponent = p.GetComponent<CommonPanel>();
                if (panelComponent != null)
                {
                    try
                    {
                        panelComponent.SetClosedCallback(release =>
                        {
                            RemoveMarkPanelWithGroup(panelComponent.PanelGroup, path);
                            if (release) SetPanel(path, null);
                        });

                        if (data != null)
                        {
                            var panelWithData = panelComponent as CommonPanelWithData<DataType>;
                            if (panelWithData != null)
                            {
                                panelWithData.SetData(data);
                            }
                            else
                            {
                                Log.E("UILoader", PanelPath, "No CommonPanelWithData Component");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.E(e);
                    }
                }
                else
                {
                    Log.E("UILoader", PanelPath, "No CommonPanel Component");
                }

                // Bring Node To Front
                RectTransform trans = p.GetComponent<RectTransform>();
                trans.localPosition = Vector3.zero;
                trans.SetAsLastSibling();

                panelComponent.AOpen();

                MarkPanelWithGroup(panelComponent.PanelGroup, path);

                return panelComponent;
            }

            // WaitView.Show();

            var prefab = await LoadUtil.AsyncLoad<GameObject>(path);
            if (prefab == null)
            {
                Log.E("UILoader load path not exist", path);
                return null;
            }

            var panel = GameObject.Instantiate(prefab);

            var rt = panel.transform as RectTransform;
            if (rt != null)
            {
                SetPanel(path, rt.gameObject);

                panelComponent = panel.GetComponent<CommonPanel>();
                if (panelComponent != null)
                {
                    try
                    {
                        panelComponent.SetClosedCallback(release =>
                        {
                            RemoveMarkPanelWithGroup(panelComponent.PanelGroup, path);
                            if (release) SetPanel(path, null);
                        });

                        if (data != null)
                        {
                            var panelWithData = panelComponent as CommonPanelWithData<DataType>;
                            if (panelWithData != null)
                            {
                                panelWithData.SetData(data);
                            }
                            else
                            {
                                Log.E("UILoader", PanelPath, "No CommonPanelWithData Component");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.E(e);
                    }
                }
                else
                {
                    Log.E("UILoader", PanelPath, "No CommonPanel Component");
                }

                var canvas = PanelCanvas.Cover;
                rt.SetParent(canvas, false);
                rt.localPosition = Vector3.zero;
                rt.SetAsLastSibling();

                panelComponent.AOpen();

                MarkPanelWithGroup(panelComponent.PanelGroup, path);
            }
            else
            {
                Log.E("UILoader", PanelPath, "No RectTransform Component");
            }

            return panelComponent;
        }

        public static void Hide(string PanelPath)
        {
            string path = PanelPath.EndsWith(".prefab") ? PanelPath : string.Concat(PanelPath, ".prefab");
            Instance._Hide(path);
        }

        private void _Hide(string PanelPath)
        {
            string path = PanelPath.EndsWith(".prefab") ? PanelPath : string.Concat(PanelPath, ".prefab");

            var hasError = true;
            var p = GetPanel(path);
            if (p != null && p.activeSelf)
            {
                CommonPanel c = p.GetComponent<CommonPanel>();
                if (c != null)
                {
                    try
                    {
                        RemoveMarkPanelWithGroup(c.PanelGroup, path);
                        c.AClose();
                        hasError = false;
                    }
                    catch (Exception e)
                    {
                        Log.E(e);
                    }
                }
            }

            if (hasError)
            {
                RemoveMarkPanelWithGroup(path);
            }
        }

        public static void HideAll()
        {
            Instance._HideAll();
        }

        private void _HideAll()
        {
            _panelsWithGroups.Clear();

            foreach (var kv in _panelsOnCurrentScene)
            {
                foreach (var subKv in kv.Value)
                {
                    GameObject p = subKv.Value;
                    if (p && p.activeSelf)
                    {
                        CommonPanel c = p.GetComponent<CommonPanel>();
                        if (c != null)
                        {
                            try
                            {
                                c.AClose();
                            }
                            catch (Exception e)
                            {
                                Log.E(e);
                            }
                        }
                    }
                }
            }
        }
    }
}