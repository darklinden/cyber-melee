using System.Collections.Generic;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

namespace Wtf.Editor
{
    public class SpriteAtlasToAddress
    {
        const string MENU_TITLE = "Assets/Addressable Atlases In Folder";

        [MenuItem(MENU_TITLE, true)]
        private static bool CanGen()
        {
            if (Selection.activeObject == null) return false;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) return false;
            if (AssetDatabase.IsValidFolder(path) == false) return false;
            return true;
        }

        [MenuItem(MENU_TITLE, false, 64)]
        public static void GenAtlasForSpritesFolder()
        {
            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            // list folder's in folder
            string[] guids = AssetDatabase.FindAssets("t:spriteatlas", new[] { folderPath });
            if (guids == null || guids.Length == 0)
            {
                Log.E("guids is null or empty");
                return;
            }

            var atlasPaths = new List<string>(guids.Length);
            foreach (var guid in guids)
            {
                var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
                atlasPaths.Add(atlasPath);
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetProfileSettings profile = settings.profileSettings;
            string profileID = settings.profileSettings.GetProfileId("Default");
            settings.activeProfileId = profileID;

            var groupName = "SpriteAtlas";
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group == null)
            {
                Log.E("group is null");
                // group = settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema));
            }
            settings.AddLabel(groupName, false);//添加组标签

            // remove all config assets for android build
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>(1024);
            group.GatherAllAssets(entries, true, true, true);
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.guid)) continue;
                Log.D("entry.guid = ", entry.guid);
                Log.D("entry.address = ", entry.address);
                group.RemoveAssetEntry(entry);
            }

            foreach (var atlasPath in atlasPaths)
            {
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(atlasPath), group, false, false);
                entry.address = atlasPath;
            }

            AssetDatabase.Refresh();
        }
    }
}