using UnityEngine;
using UnityEditor;

namespace Wtf.Editor
{
    public class ListEmptyFolders
    {

        [MenuItem("Assets/List Empty Folders")]
        public static void ListEmptyFolders_InFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            ListEmptyFolders_InFolder(folderPath);
            Debug.Log("Done List Empty Folders!");
        }

        static void ListEmptyFolders_InFolder(string folderPath)
        {
            // enumerate all folders in the folder
            string[] guids = AssetDatabase.FindAssets("t:Folder", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // ListEmptyFolders_InFolder(path);

                // check if the folder is empty
                string[] subGuids = AssetDatabase.FindAssets("t:Object", new[] { path });
                if (subGuids.Length == 0)
                {
                    Debug.Log("found:" + path);
                }
                else
                {
                    // Debug.Log("check: " + path);
                }
            }
        }

        [MenuItem("Assets/List Empty Folders", true)]
        private static bool CanListEmptyFolders_InFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            return AssetDatabase.IsValidFolder(folderPath);
        }
    }
}