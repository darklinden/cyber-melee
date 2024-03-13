using System.Collections;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using FixMath;
using Google.FlatBuffers;
using Proto;
using UnityEngine;
using Wtf;


namespace FlatConfigs
{
    public partial class Handler
    {
        public OrbitMapData OrbitMapData { get; internal set; }
    }

    public class Orbit
    {
        public List<F64Vec3> OrbitPosList { get; set; }
    }

    public class OrbitMapData : IConfigLoader
    {
        public int Priority => 0;
        public bool LoadOnStart => true;
        public bool DataLoaded { get; private set; } = false;

        public void Initialize(Handler configs)
        {
            configs.OrbitMapData = this;
        }

        private Dictionary<int, List<Orbit>> m_OrbitMapDataDict = new Dictionary<int, List<Orbit>>();

        public List<Orbit> ToOrbit(Proto.OrbitMapDataRow orbitMapDataRow)
        {
            var orbits = new List<Orbit>();

            Log.AssertIsTrue(orbitMapDataRow.Camp1Length == orbitMapDataRow.Camp2Length, "OrbitMapData: Camp1Length != Camp2Length");

            for (int i = 0; i < orbitMapDataRow.Camp1Length; i++)
            {
                var orbitP1 = orbitMapDataRow.Camp1(i);
                var orbitP2 = orbitMapDataRow.Camp2(i);

                orbits.Add(new Orbit
                {
                    OrbitPosList = new List<F64Vec3>
                    {
                        F64Vec3.FromInt(orbitP1.Value.X, orbitP1.Value.Y, orbitP1.Value.Z) / F64.FromDouble(Constants.PRECISION),
                        F64Vec3.FromInt(orbitP2.Value.X, orbitP2.Value.Y, orbitP2.Value.Z) / F64.FromDouble(Constants.PRECISION)
                    }
                });
            }
            return orbits;
        }

        public async UniTask AsyncLoad()
        {
            if (!DataLoaded)
            {
                string kOrbitMapDataPath = $"Assets/Addrs/{i18n.Locale}/Configs/OrbitMapData.bytes";
                var loaded = await LoadUtil.AsyncLoad<TextAsset>(kOrbitMapDataPath);
                var OrbitMapDataBytes = new ByteBuffer(loaded.bytes);

                var mapData = Proto.OrbitMapData.GetRootAsOrbitMapData(OrbitMapDataBytes);
                for (int i = 0; i < mapData.RowsLength; i++)
                {
                    var row = mapData.Rows(i);
                    m_OrbitMapDataDict.Add(row.Value.Id, ToOrbit(row.Value));
                }

                DataLoaded = true;

                LoadUtil.Release(kOrbitMapDataPath);
            }
        }

        public List<Orbit> GetData(int id)
        {
            if (m_OrbitMapDataDict.TryGetValue(id, out var data))
            {
                return data;
            }
            return null;
        }
    }
}