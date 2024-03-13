using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Wtf.Editor
{
    public class FontReplacer : EditorWindow
    {
        private const string EditorPrefsKey = "Tools.FontReplacer";
        private const string MenuItemName = "Tools/Fonts/Replace Fonts...";

        private Font _src;
        private Font _dest;
        private bool _includePrefabs;

        [MenuItem(MenuItemName)]
        public static void DisplayWindow()
        {
            var window = GetWindow<FontReplacer>(true, "Replace Fonts");
            var position = window.position;
            position.size = new Vector2(position.size.x, 151);
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.Show();
        }

        public void OnEnable()
        {
            var path = EditorPrefs.GetString(EditorPrefsKey + ".src");
            if (path != string.Empty)
                _src = AssetDatabase.LoadAssetAtPath<Font>(path) ?? Resources.GetBuiltinResource<Font>(path);

            path = EditorPrefs.GetString(EditorPrefsKey + ".dest");
            if (path != string.Empty)
                _dest = AssetDatabase.LoadAssetAtPath<Font>(path) ?? Resources.GetBuiltinResource<Font>(path);

            _includePrefabs = EditorPrefs.GetBool(EditorPrefsKey + ".includePrefabs", false);
        }

        public void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PrefixLabel("Find:");
            _src = (Font)EditorGUILayout.ObjectField(_src, typeof(Font), false);

            EditorGUILayout.Space();
            EditorGUILayout.PrefixLabel("Replace with:");
            _dest = (Font)EditorGUILayout.ObjectField(_dest, typeof(Font), false);

            EditorGUILayout.Space();
            _includePrefabs = EditorGUILayout.ToggleLeft("Include Prefabs", _includePrefabs);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(EditorPrefsKey + ".src", GetAssetPath(_src, "ttf"));
                EditorPrefs.SetString(EditorPrefsKey + ".dest", GetAssetPath(_dest, "ttf"));
                EditorPrefs.SetBool(EditorPrefsKey + ".includePrefabs", _includePrefabs);
            }

            GUI.color = Color.green;
            if (GUILayout.Button("Replace All", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
            {
                ReplaceFonts(_src, _dest, _includePrefabs);
            }
            GUI.color = Color.white;
        }

        private static void ReplaceFonts(Font src, Font dest, bool includePrefabs)
        {
            var sceneMatches = 0;
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var scenePath = scene.path;
                var gos = new List<GameObject>(scene.GetRootGameObjects());
                foreach (var go in gos)
                {
                    sceneMatches += ReplaceFonts(src, dest, go.GetComponentsInChildren<Text>(true), scenePath);
                }

                if (sceneMatches > 0)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            }

            var prefabMatches = 0;
            if (includePrefabs)
            {
                var prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
                var prefabPaths = prefabGUIDs.Select(guid => AssetDatabase.GUIDToAssetPath(guid));

                foreach (var prefabPath in prefabPaths)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var matches = ReplaceFonts(src, dest, prefab.GetComponentsInChildren<Text>(true), prefabPath);
                    if (matches > 0)
                    {
                        prefabMatches += matches;
                        // Debug.LogFormat("Replaced font in {0}", prefabPath);
                        EditorUtility.SetDirty(prefab);
                    }
                }
            }

            Debug.LogFormat("Replaced {0} font(s), {1} in scenes, {2} in prefabs", sceneMatches + prefabMatches, sceneMatches, prefabMatches);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        private static int ReplaceFonts(Font src, Font dest, IEnumerable<Text> texts, string assetPath)
        {
            var matches = 0;
            var textsFiltered = src != null ? texts.Where(text => text.font == src) : texts;
            foreach (var text in textsFiltered)
            {
                text.font = dest;
                UnityEngine.Debug.LogFormat("Replaced font in {0} on {1}", assetPath, GetGameObjectPath(text.gameObject));
                matches++;
            }
            return matches;
        }

        private static string GetAssetPath(Object assetObject, string defaultExtension)
        {
            var path = AssetDatabase.GetAssetPath(assetObject);
            if (path.StartsWith("Library/", System.StringComparison.InvariantCultureIgnoreCase))
                path = assetObject.name + "." + defaultExtension;
            return path;
        }
    }
}