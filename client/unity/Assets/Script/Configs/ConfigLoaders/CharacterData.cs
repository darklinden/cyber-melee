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
        public CharacterData CharacterData { get; internal set; }
    }

    public class CharacterData : IConfigLoader
    {
        public int Priority => 0;
        public bool LoadOnStart => true;
        public bool DataLoaded { get; private set; } = false;

        public void Initialize(Handler configs)
        {
            configs.CharacterData = this;
        }

        private Dictionary<int, CharacterDataRowT> m_CharacterDataDict = new Dictionary<int, CharacterDataRowT>();
        public Dictionary<int, CharacterDataRowT> CharacterDataDict => m_CharacterDataDict;

        public async UniTask AsyncLoad()
        {
            if (!DataLoaded)
            {
                string kCharacterDataPath = $"Assets/Addrs/{i18n.Locale}/Configs/CharacterData.bytes";
                var loaded = await LoadUtil.AsyncLoad<TextAsset>(kCharacterDataPath);
                var characterDataBytes = new ByteBuffer(loaded.bytes);

                var characterData = Proto.CharacterData.GetRootAsCharacterData(characterDataBytes);
                for (int i = 0; i < characterData.RowsLength; i++)
                {
                    var row = characterData.Rows(i);
                    m_CharacterDataDict.Add(row.Value.Id, row.Value.UnPack());
                }

                DataLoaded = true;

                LoadUtil.Release(kCharacterDataPath);
            }
        }

        public Proto.CharacterDataRowT GetData(int id)
        {
            if (m_CharacterDataDict.TryGetValue(id, out var data))
            {
                return data;
            }
            return null;
        }
    }
}