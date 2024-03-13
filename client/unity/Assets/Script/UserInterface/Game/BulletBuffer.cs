using System;
using System.Collections;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Lockstep;
using UnityEngine;
using UnityEngine.UI;
using Wtf;

public class BulletBuffer : MonoBehaviour
{
    [SerializeField] private Image CellPrefab;
    private Dictionary<int, Sprite> m_BulletSpriteDict = new Dictionary<int, Sprite>();

    private Transform m_Tm = null;
    public Transform Tm => m_Tm != null ? m_Tm : (m_Tm = transform);

    // Start is called before the first frame update
    private ulong MePlayerId = 0;
    void Start()
    {
        var eventBus = Context.Inst.EventBus;
        eventBus.OnDispatchPlayBulletBufferChanged += OnDispatchPlayBulletBufferChanged;

        var serviceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();
        MePlayerId = serviceSystem.Data.PlayerInfo.PlayerId;

        AsyncLoadBulletSprite().Forget();
    }

    private AtlasLoader m_AtlasLoader = null;
    private async UniTask AsyncLoadBulletSprite()
    {
        if (m_AtlasLoader == null)
        {
            m_AtlasLoader = Context.Inst.GetSystem<AtlasLoader>();
        }
        var projectileList = Enum.GetValues(typeof(Proto.ProjectileType));
        foreach (Proto.ProjectileType projectileType in projectileList)
        {
            if (projectileType == Proto.ProjectileType.NONE) continue;
            var sprite = await m_AtlasLoader.AsyncLoadSprite(AtlasGroup.All, projectileType.ToString());
            m_BulletSpriteDict.Add((int)projectileType, sprite);
        }
    }

    private void OnDestroy()
    {
        var eventBus = Context.Inst?.EventBus;
        if (eventBus != null)
        {
            eventBus.OnDispatchPlayBulletBufferChanged -= OnDispatchPlayBulletBufferChanged;
        }
    }

    private void OnDispatchPlayBulletBufferChanged(long frame, GamePlayFrame_BulletBufferChanged gamePlayFrame_BulletBufferChanged)
    {
        if (gamePlayFrame_BulletBufferChanged.PlayerId != MePlayerId)
        {
            return;
        }

        Log.D("OnDispatchPlayBulletBufferChanged", frame, gamePlayFrame_BulletBufferChanged.BulletBuffer.Count);

        var bulletBuffer = gamePlayFrame_BulletBufferChanged.BulletBuffer;
        var childCount = Tm.childCount;

        for (int i = 0; i < MathF.Max(childCount, bulletBuffer.Count); i++)
        {
            if (i < bulletBuffer.Count)
            {
                var bullet = bulletBuffer[i];

                Transform cell = null;
                if (i < childCount)
                {
                    cell = Tm.GetChild(i);
                }
                else
                {
                    cell = Instantiate(CellPrefab, Tm).transform;
                }
                var cellImage = cell.GetComponent<Image>();
                cellImage.gameObject.SetActive(true);
                cellImage.sprite = m_BulletSpriteDict[(int)bullet];
            }
            else
            {
                var child = Tm.GetChild(i);
                child.gameObject.SetActive(false);
            }
        }
    }
}
