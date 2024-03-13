using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wtf.Editor
{
    [InitializeOnLoad]
    public class PlayInBackground
    {
        const string ITEM = "Tools/Play In Background";

        static PlayInBackground()
        {
            EditorApplication.delayCall += InitMenu;
        }

        static void InitMenu()
        {
            Menu.SetChecked(ITEM, Application.runInBackground);
        }

        [MenuItem(ITEM, priority = 300)]
        public static void TogglePlayInBackground()
        {
            Menu.SetChecked(ITEM, !Menu.GetChecked(ITEM));
            Application.runInBackground = Menu.GetChecked(ITEM);
        }
    }
}