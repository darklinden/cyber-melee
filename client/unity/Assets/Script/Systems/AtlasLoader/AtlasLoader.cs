using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using Wtf;

namespace App
{
    public class AtlasLoader : MonoBehaviour, ISystemBase
    {
        public bool IsInitialized { get; private set; } = false;

        private Transform m_Tm = null;
        public Transform Tm
        {
            get
            {
                if (m_Tm == null)
                {
                    m_Tm = transform;
                }
                return m_Tm;
            }
        }

        // <AtlasGroup, <SpriteName, Sprite>>
        private Dictionary<int, Dictionary<string, Sprite>> Sprites = new Dictionary<int, Dictionary<string, Sprite>>();
        private HashSet<string> LoadingAtlas = new HashSet<string>();
        private HashSet<SpriteAtlas> LoadedAtlas = new HashSet<SpriteAtlas>();

        public void Initialize()
        {
            Log.D("AtlasLoader.Initialize");
            SpriteAtlasManager.atlasRequested += (name, callback) =>
            {
                Log.W($"Atlas requested: {name}");

                if (LoadingAtlas.Contains(name)) return;
                LoadingAtlas.Add(name);
                if (name != "Sprites") return;

                // var AtlasAddr = GetAtlasAddr(AtlasGroup.SkillIcons);

                Addressables.LoadAssetAsync<SpriteAtlas>(name).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Log.W("Loaded atlas:", name);
                        LoadedAtlas.Add(handle.Result);
                        callback(handle.Result);
                    }
                    else
                    {
                        Log.E("Failed to load atlas:", name);
                    }
                    LoadingAtlas.Remove(name);
                };
            };

            IsInitialized = true;
        }

        public UniTask AsyncInitialize()
        {
            throw new NotImplementedException();
        }

        public void Deinitialize()
        {
            Log.D("AtlasLoader.Deinitialize");
            foreach (var atlas in LoadedAtlas)
            {
                Addressables.Release(atlas);
            }
            IsInitialized = false;
        }

        public void Unload(AtlasGroup atlasGroup)
        {
            if (atlasGroup == AtlasGroup.NoSprite)
            {
                return;
            }
            if (!Sprites.TryGetValue((int)atlasGroup, out var spriteDict))
            {
                return;
            }

            foreach (var sprite in spriteDict.Values)
            {
                Addressables.Release(sprite);
            }

            spriteDict.Clear();
            Sprites.Remove((int)atlasGroup);
        }

        public async UniTask<Sprite> AsyncLoadSprite(AtlasGroup atlasGroup, string name)
        {
            // get sprite from cache
            if (!Sprites.TryGetValue((int)atlasGroup, out var spriteDict))
            {
                spriteDict = new Dictionary<string, Sprite>();
                Sprites[(int)atlasGroup] = spriteDict;
            }

            if (spriteDict.TryGetValue(name, out var sprite))
            {
                return sprite;
            }

            var addr = GetSpriteAddr(atlasGroup, name);
            sprite = await AsyncLoadSpriteByAddr(addr);
            spriteDict[name] = sprite;

            return sprite;
        }

        private static string NoSpriteAddr => $"Assets/Addrs/{i18n.Locale}/Sprites/NoSprite.spriteatlasv2[NoSprite]";

        public Dictionary<string, ISystemBase> SubSystems => null;

        private string GetSpriteAddr(AtlasGroup atlasGroup, string name)
        {
            if (atlasGroup == AtlasGroup.NoSprite)
            {
                // Hard code NoSprite
                return NoSpriteAddr;
            }
            return atlasGroup.ToAddr() + "[" + name + "]";
        }

        private async UniTask<Sprite> AsyncLoadSpriteByAddr(string addr)
        {
            Sprite sprite = null;
            try
            {
                var handle = Addressables.LoadAssetAsync<Sprite>(addr);
                await handle;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    sprite = handle.Result;
                }
            }
            catch (Exception)
            {
                // Log.W("Failed to load sprite:", addr);
            }

            if (Application.isPlaying && sprite == null)
            {
                sprite = await AsyncLoadSpriteByAddr(GetSpriteAddr(AtlasGroup.NoSprite, string.Empty));
                Log.W("Failed to load sprite, Show Placeholder:", addr);
            }

            return sprite;
        }

    }
}