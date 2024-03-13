using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.FlatBuffers;
using Proto;
using UnityEngine;
using Wtf;


namespace FlatConfigs
{
    public partial class Handler
    {
        public ProjectileData ProjectileData { get; internal set; }
    }

    public class ProjectileData : IConfigLoader
    {
        public int Priority => 0;
        public bool LoadOnStart => true;
        public bool DataLoaded { get; private set; } = false;

        public void Initialize(Handler configs)
        {
            configs.ProjectileData = this;
        }

        private Dictionary<int, ProjectileDataRowT> m_ProjectileDataDict = new Dictionary<int, ProjectileDataRowT>();
        public Dictionary<int, ProjectileDataRowT> ProjectileDataDict => m_ProjectileDataDict;

        public async UniTask AsyncLoad()
        {
            if (!DataLoaded)
            {
                string kProjectileDataPath = $"Assets/Addrs/{i18n.Locale}/Configs/ProjectileData.bytes";
                var loaded = await LoadUtil.AsyncLoad<TextAsset>(kProjectileDataPath);
                var ProjectileDataBytes = new ByteBuffer(loaded.bytes);

                var ProjectileData = Proto.ProjectileData.GetRootAsProjectileData(ProjectileDataBytes);
                for (int i = 0; i < ProjectileData.RowsLength; i++)
                {
                    var row = ProjectileData.Rows(i);
                    m_ProjectileDataDict.Add((int)row.Value.ProjectileType, row.Value.UnPack());
                }

                DataLoaded = true;

                LoadUtil.Release(kProjectileDataPath);
            }
        }

        public ProjectileDataRowT GetData(ProjectileType projectileType)
        {
            return GetData((int)projectileType);
        }

        public ProjectileDataRowT GetData(int projectileType)
        {
            if (m_ProjectileDataDict.TryGetValue(projectileType, out var data))
            {
                return data;
            }
            return null;
        }
    }
}