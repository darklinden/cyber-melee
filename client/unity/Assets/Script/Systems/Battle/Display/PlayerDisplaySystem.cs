using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wtf;

namespace Battle
{
    public class PlayerDisplaySystem : MonoBehaviour, ISystemBase
    {
        private Dictionary<ulong, CharacterBody> PlayerBodyDict
            = new Dictionary<ulong, CharacterBody>();

        private GameObject CharacterBodyPrefab { get; set; } = null;

        private Service.ServiceSystem m_ServiceSystem = null;
        private Service.ServiceSystem ServiceSystem => m_ServiceSystem != null ? m_ServiceSystem : (m_ServiceSystem = Context.Inst.GetSystem<Service.ServiceSystem>());

        internal void InitPlayerPos(Player player)
        {
            Log.D("InitCharacter CharacterBody", player.PlayerId, "Start");
            var prop = PlayerFrameUtil.GetProp(player, 0);
            var characterBody = GetBody(player.PlayerId);
            if (characterBody == null)
            {
                var go = Instantiate(CharacterBodyPrefab, Tm);
                characterBody = go.GetComponent<CharacterBody>();
                if (characterBody == null) characterBody = go.AddComponent<CharacterBody>();
                characterBody.GroundPos.position = player.OrbitCenter.ToVector3();
                characterBody.Initialize(player.PlayerId);
                SetBody(player.PlayerId, characterBody);
            }
#if ENABLE_LOG || UNITY_SERVER
            characterBody.SetDebugShow(
                Color.red,
                player.PlayerId != ServiceSystem.Data.PlayerInfo.PlayerId);
#endif
            characterBody.gameObject.SetActive(true);
            Log.D("InitCharacter CharacterBody", player.PlayerId, "End");
        }


        internal CharacterBody GetBody(ulong characterId)
        {
            if (PlayerBodyDict.TryGetValue(characterId, out var physicsBody))
            {
                return physicsBody;
            }
            return null;
        }

        internal void SetBody(ulong characterId, CharacterBody physicsBody)
        {
            if (PlayerBodyDict.ContainsKey(characterId))
            {
                Log.E(
                    "CharacterBodySystem",
                    "SetBody",
                    "CharacterBodyDict already contains key",
                    characterId);

                var oldBody = PlayerBodyDict[characterId];
                if (oldBody != null && oldBody.gameObject != physicsBody.gameObject)
                {
                    Destroy(oldBody.gameObject);
                }
                PlayerBodyDict[characterId] = physicsBody;
            }
            else
            {
                PlayerBodyDict.Add(characterId, physicsBody);
            }
        }


        public Dictionary<string, ISystemBase> SubSystems => null;

        private Transform m_Tm = null;
        public Transform Tm => m_Tm != null ? m_Tm : (m_Tm = transform);

        public bool IsInitialized { get; private set; } = false;

        public void Initialize() { }

        public async UniTask AsyncInitialize()
        {
            if (CharacterBodyPrefab == null)
            {
                CharacterBodyPrefab = await LoadUtil.AsyncLoad<GameObject>($"Assets/Addrs/{i18n.Locale}/Prefabs/CharacterBody.prefab");
            }

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            LoadUtil.Release($"Assets/Addrs/{i18n.Locale}/Prefabs/CharacterBody.prefab");
            IsInitialized = false;
        }
    }
}