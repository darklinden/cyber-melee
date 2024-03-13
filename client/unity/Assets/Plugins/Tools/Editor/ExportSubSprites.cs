using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Wtf.Editor
{
    public class ExportSubSprites
    {
        const string MENU_TITLE = "Assets/Export Sub-Sprites";

        [MenuItem(MENU_TITLE)]
        public static void DoExportSubSprites()
        {
            var folder = EditorUtility.OpenFolderPanel("Export subsprites into what folder?", "", "");
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D)
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                    foreach (var sprite in sprites)
                    {
                        var extracted = ExtractAndName(sprite);
                        SaveSubSprite(extracted, folder);
                    }
                }
                else
                {
                    var sprite = obj as Sprite;
                    if (sprite == null) continue;
                    var extracted = ExtractAndName(sprite);
                    SaveSubSprite(extracted, folder);
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("Done Exporting Sub-Sprites!");
        }

        [MenuItem(MENU_TITLE, true)]
        private static bool CanExportSubSprites()
        {
            return Selection.activeObject is Sprite || Selection.activeObject is Texture2D;
        }

        // Since a sprite may exist anywhere on a tex2d, this will crop out the sprite's claimed region and return a new, cropped, tex2d.
        private static Texture2D ExtractAndName(Sprite sprite)
        {
            var texture = sprite.texture;

            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                texture.width,
                                texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D cacheTexture2D = new Texture2D(texture.width, texture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            cacheTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            cacheTexture2D.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            // "cacheTexture2D" now has the same pixels from "texture" and it's re

            var output = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var r = sprite.textureRect;
            var pixels = cacheTexture2D.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
            output.SetPixels(pixels);
            output.Apply();
            output.name = sprite.name;
            return output;
        }

        private static void SaveSubSprite(Texture2D tex, string saveToDirectory)
        {
            if (!System.IO.Directory.Exists(saveToDirectory)) System.IO.Directory.CreateDirectory(saveToDirectory);
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(saveToDirectory, tex.name + ".png"), tex.EncodeToPNG());
        }
    }
}