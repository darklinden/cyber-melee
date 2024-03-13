using System;
using Lockstep;
using Proto;
using TMPro;
using UnityEngine;
using Wtf;

namespace Battle
{
    internal class ProjectileBody : MonoBehaviour
    {
        [SerializeField][ReadOnly] private int _projectileId = 0;
        internal int ProjectileId { get => _projectileId; private set => _projectileId = value; }

        [SerializeField][ReadOnly] private ProjectileType _ProjectileType;
        internal ProjectileType ProjectileType { get => _ProjectileType; private set => _ProjectileType = value; }

        internal Vector3 NormalDirection { get; private set; }

        internal bool IsPowerUp = false;

        [SerializeField] private TextMeshPro TextMeshAtk;
        [SerializeField] private TextMeshPro TextMeshDur;

        internal Transform Tm { get; private set; }
        protected void Awake()
        {
            Tm = transform;
        }

        internal void SetData(GamePlayFrame_ProjectileSpawn gamePlayFrame_ProjectileSpawn)
        {
            ProjectileId = gamePlayFrame_ProjectileSpawn.ProjectileId;
            ProjectileType = gamePlayFrame_ProjectileSpawn.ProjectileType;

            Tm.position = gamePlayFrame_ProjectileSpawn.Position;
            NormalDirection = gamePlayFrame_ProjectileSpawn.NormalDirection;

            TextMeshAtk.text = string.Empty;

            m_Duration = (int)gamePlayFrame_ProjectileSpawn.Duration;
            TextMeshDur.text = gamePlayFrame_ProjectileSpawn.Duration.ToString();
        }

        internal void Move(float deltaTime)
        {
            Tm.position += NormalDirection * deltaTime;
        }

        internal void Buff(int atkBuffCount)
        {
            if (atkBuffCount <= 0)
            {
                TextMeshAtk.text = string.Empty;
            }
            else
            {
                TextMeshAtk.text = $"Atk +{atkBuffCount}";
            }
        }

        private int m_DurBuffCount = 0;
        private int m_Duration = 0;
        internal void Duration(int durBuffCount, int duration)
        {
            m_DurBuffCount = durBuffCount;
            m_Duration = duration;
            if (m_DurBuffCount <= 0)
            {
                TextMeshDur.text = $"Dur {m_Duration}";
            }
            else
            {
                TextMeshDur.text = $"Dur +{m_DurBuffCount} {m_Duration}";
            }
        }

        private void Update()
        {
            if (m_Duration > 0)
            {
                m_Duration -= Mathf.FloorToInt(Time.deltaTime * 1000);
                if (m_DurBuffCount <= 0)
                {
                    TextMeshDur.text = IsPowerUp ? $"Pow! Dur {m_Duration}" : $"Dur {m_Duration}";
                }
                else
                {
                    TextMeshDur.text = IsPowerUp ? $"Pow! Dur +{m_DurBuffCount} {m_Duration}" : $"Dur +{m_DurBuffCount} {m_Duration}";
                }
            }
        }

        internal void HitBack()
        {
            NormalDirection = -NormalDirection;
        }

        internal void PowerUp(bool isPowerUp)
        {
            IsPowerUp = isPowerUp;
        }

        [SerializeField] private int m_BleedCount = 0;
        internal void Bleed(int eOTValue0)
        {
            m_BleedCount = eOTValue0;
        }
    }
}
