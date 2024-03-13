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
        public SkillData SkillData { get; internal set; }
    }

    public class SkillData : IConfigLoader
    {
        public int Priority => 0;
        public bool LoadOnStart => true;
        public bool DataLoaded { get; private set; } = false;

        public void Initialize(Handler configs)
        {
            configs.SkillData = this;
        }

        private Dictionary<int, SkillDataRowT> m_SkillDataDict = new Dictionary<int, SkillDataRowT>();

        public async UniTask AsyncLoad()
        {
            if (!DataLoaded)
            {
                string kSkillDataPath = $"Assets/Addrs/{i18n.Locale}/Configs/SkillData.bytes";
                var loaded = await LoadUtil.AsyncLoad<TextAsset>(kSkillDataPath);
                var SkillDataBytes = new ByteBuffer(loaded.bytes);

                var SkillData = Proto.SkillData.GetRootAsSkillData(SkillDataBytes);
                for (int i = 0; i < SkillData.RowsLength; i++)
                {
                    var row = SkillData.Rows(i);
                    m_SkillDataDict.Add((int)row.Value.SkillType, row.Value.UnPack());
                }

                DataLoaded = true;

                LoadUtil.Release(kSkillDataPath);
            }
        }

        public SkillDataRowT GetData(SkillType skillType)
        {
            return GetData((int)skillType);
        }

        public SkillDataRowT GetData(int skillType)
        {
            if (m_SkillDataDict.TryGetValue(skillType, out var data))
            {
                return data;
            }
            return null;
        }
    }
}