using System;
using UnityEngine;
using Wtf;
using App;
using Battle;
using Lockstep;

namespace Battle
{
    internal class CharacterBody : MonoBehaviour
    {
        [SerializeField][ReadOnly] private ulong _characterId = 0;
        internal ulong CharacterId { get => _characterId; private set => _characterId = value; }

        // 
        internal Transform HeadPos { get; private set; }
        internal Transform BodyPos { get; private set; }
        internal Transform GroundPos { get; private set; }

        internal void Initialize(ulong characterId)
        {
            Reuse();
            CharacterId = characterId;
        }

        protected void Awake()
        {
            GroundPos = transform;
            BodyPos = transform.Find("BodyPos");
            HeadPos = transform.Find("HeadPos");
        }

        internal void Look(Vector3 direct)
        {
            if (Mathf.Abs(direct.x) > 0.001 || Mathf.Abs(direct.z) > 0.001)
            {
                GroundPos.rotation = Quaternion.LookRotation(direct, Vector3.up);
            }
        }

        private BattleSystem m_BattleSystem = null;
        private BattleSystem BattleSystem => m_BattleSystem != null ? m_BattleSystem : (m_BattleSystem = App.Context.Inst.GetSystem<Battle.BattleSystem>());

        private void Update()
        {
            if (!BattleSystem.IsBattleRunning) return;

            // 获得角色的数据
            var character = BattleSystem.PlayerCalc.GetPlayerById(CharacterId);
            if (character == null)
            {
                Unuse();
                Log.D("CharacterBody", CharacterId, "Character is null");
                return;
            }

            // Move
            // var frameRange = LockStepSystem.CurrentGameFrameRange;
            // var fromProp = PlayerFrameUtil.GetProp(character, frameRange.From.Frame);
            // var toProp = PlayerFrameUtil.GetProp(character, frameRange.To.Frame);
            // if (fromProp.IsDead)
            // {
            //     Unuse();
            //     return;
            // }

            // Log.D("CharacterBody", CharacterId, "Update", TimeSystem.FrameServerTickTimeMs);

        }

        internal void Reuse()
        {
        }

        internal void Unuse()
        {
            Log.D("CharacterBody Unuse", CharacterId);
            CharacterId = 0;
            // put back to pool

            gameObject.SetActive(false);
        }

        // ------------------ 占位体颜色 ------------------

#if ENABLE_LOG || UNITY_SERVER

        [SerializeField] internal GameObject m_PlaceHolder;
        internal GameObject PlaceHolder
        {
            get
            {
                if (m_PlaceHolder == null)
                {
                    m_PlaceHolder = transform.Find("PlaceHolder").gameObject;
                }
                return m_PlaceHolder;
            }
        }

        internal void SetDebugShow(Color color, bool showPlaceHolder)
        {
            if (PlaceHolder != null)
            {
                PlaceHolder.SetActive(showPlaceHolder);

                var renderers = PlaceHolder.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.material.color = color;
                }
            }
        }

#endif
    }
}
