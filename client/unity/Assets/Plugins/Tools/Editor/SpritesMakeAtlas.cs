using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;

namespace Wtf.Editor
{
    public class SpritesMakeAtlas
    {
        const string MENU_TITLE = "Assets/Generate Atlas For Sprites In Folder";

        [MenuItem(MENU_TITLE, true)]
        private static bool CanGen()
        {
            if (Selection.activeObject == null) return false;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) return false;
            if (AssetDatabase.IsValidFolder(path) == false) return false;
            return true;
        }

        private static List<string> ExchangeTexturesToSprites(string folderPath)
        {
            // enumerate all textures in the folder
            List<string> spriteFiles = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.spritePackingTag = "UI";

                    TextureImporterSettings importerSettings = new TextureImporterSettings();
                    textureImporter.ReadTextureSettings(importerSettings);

                    importerSettings.spriteMeshType = SpriteMeshType.FullRect;
                    importerSettings.spriteExtrude = 0;
                    importerSettings.spriteAlignment = (int)SpriteAlignment.Center;
                    importerSettings.spritePivot = new Vector2(0.5f, 0.5f);
                    importerSettings.readable = false;
                    importerSettings.textureType = TextureImporterType.Sprite;
                    importerSettings.spriteMode = (int)SpriteImportMode.Single;
                    importerSettings.wrapMode = TextureWrapMode.Clamp;
                    importerSettings.alphaSource = TextureImporterAlphaSource.FromInput;
                    importerSettings.alphaIsTransparency = true;
                    importerSettings.sRGBTexture = true;
                    importerSettings.filterMode = FilterMode.Point;
                    importerSettings.mipmapEnabled = false;
                    importerSettings.npotScale = TextureImporterNPOTScale.None;
                    importerSettings.aniso = 1;
                    // for pixel art
                    importerSettings.spritePixelsPerUnit = 1;

                    textureImporter.SetTextureSettings(importerSettings);
                    textureImporter.SaveAndReimport();
                }
                spriteFiles.Add(path);
            }
            AssetDatabase.Refresh();
            return spriteFiles;
        }

        public static void GenAtlasInFolder(string folderPath)
        {
            var spriteFiles = ExchangeTexturesToSprites(folderPath);

            var sprites = new List<Sprite>();
            foreach (var spriteFile in spriteFiles)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFile);
                sprites.Add(sprite);
            }

            // create atlas
            var atlasPath = folderPath + ".spriteatlasv2";

            var atlas = new SpriteAtlasAsset();
            atlas.SetIncludeInBuild(true);
            atlas.SetPackingSettings(new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                padding = 2,
                enableRotation = true,
                enableTightPacking = false
            });
            atlas.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                format = TextureImporterFormat.RGBA32,
                maxTextureSize = 2048,
                textureCompression = TextureImporterCompression.Compressed,
                overridden = true
            });
            atlas.SetTextureSettings(new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false,
                sRGB = false,
                filterMode = FilterMode.Point,
                anisoLevel = 1
            });
            atlas.Add(sprites.ToArray());
            SpriteAtlasAsset.Save(atlas, atlasPath);
        }

        [MenuItem(MENU_TITLE, false, 64)]
        public static void GenAtlasForSpritesInFolder()
        {
            var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            GenAtlasInFolder(folderPath);
            AssetDatabase.Refresh();
            Debug.Log("Generate atlas for sprites in folder: " + folderPath + " successfully!");
        }
    }
}