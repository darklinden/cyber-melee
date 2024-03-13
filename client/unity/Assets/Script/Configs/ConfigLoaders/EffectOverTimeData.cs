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
        public EffectOverTimeData EffectOverTimeData { get; internal set; }
    }

    public class EffectOverTimeData : IConfigLoader
    {
        public int Priority => 0;
        public bool LoadOnStart => true;
        public bool DataLoaded { get; private set; } = false;

        public void Initialize(Handler configs)
        {
            configs.EffectOverTimeData = this;
        }

        private Dictionary<int, EffectOverTimeDataRowT> m_EffectOverTimeDataDict = new Dictionary<int, EffectOverTimeDataRowT>();

        public async UniTask AsyncLoad()
        {
            if (!DataLoaded)
            {
                string kEffectOverTimeDataPath = $"Assets/Addrs/{i18n.Locale}/Configs/EffectOverTimeData.bytes";
                var loaded = await LoadUtil.AsyncLoad<TextAsset>(kEffectOverTimeDataPath);
                var EffectOverTimeDataBytes = new ByteBuffer(loaded.bytes);

                var EffectOverTimeData = Proto.EffectOverTimeData.GetRootAsEffectOverTimeData(EffectOverTimeDataBytes);
                for (int i = 0; i < EffectOverTimeData.RowsLength; i++)
                {
                    var row = EffectOverTimeData.Rows(i);
                    m_EffectOverTimeDataDict.Add((int)row.Value.Eot, row.Value.UnPack());
                }

                DataLoaded = true;

                LoadUtil.Release(kEffectOverTimeDataPath);
            }
        }

        public Proto.EffectOverTimeDataRowT GetData(EOTType eotType)
        {
            return GetData((int)eotType);
        }

        public Proto.EffectOverTimeDataRowT GetData(int eotType)
        {
            if (m_EffectOverTimeDataDict.TryGetValue(eotType, out var data))
            {
                return data;
            }
            return null;
        }
    }
}