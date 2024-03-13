using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Wtf.Editor
{
    public class ClearPreferences
    {
        [MenuItem("Tools/Clear Player Preferences", false, 60)]
        static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Player Preferences Cleared");
        }

        [MenuItem("Tools/Clear Editor Preferences", false, 61)]
        static void ClearEditorPrefs()
        {
            EditorPrefs.DeleteAll();
            Debug.Log("Editor Preferences Cleared");
        }
    }
}