using App;
using UnityEngine;
using Wtf;
using Battle;
using System;

public class Game : MonoBehaviour
{
    private GameCtrl m_GameCtrl = null;
    private GameCtrl GameCtrl
    {
        get
        {
            if (m_GameCtrl == null)
            {
                m_GameCtrl = Context.Inst.GetSystem<GameCtrl>();
            }
            return m_GameCtrl;
        }
    }

    private TimeSystem m_TimeSystem = null;
    private TimeSystem TimeSystem
    {
        get
        {
            if (m_TimeSystem == null)
            {
                m_TimeSystem = Context.Inst.GetSystem<TimeSystem>();
            }
            return m_TimeSystem;
        }
    }

    [SerializeField] private Camera CharacterCamera;

    private void Start()
    {
        Log.D("Game.Start");

        Context.Inst.EventBus.OnBattleLoadingCompleted += OnBattleLoadingCompleted;
    }

    void OnDestroy()
    {
        var eventBus = Context.Inst?.EventBus;
        if (eventBus != null)
        {
            eventBus.OnBattleLoadingCompleted -= OnBattleLoadingCompleted;
        }
    }

    void OnBattleLoadingCompleted()
    {
        Log.D("Game.OnBattleLoadingCompleted");
        var serviceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();
        var battleSystem = Context.Inst.GetSystem<Battle.BattleSystem>();

        var meCharacterId = serviceSystem.Data.BattleInfo.FriendlyPlayerIds[0];
        var physicsBody = battleSystem.PlayerDisplay.GetBody(meCharacterId);

        LookAtCharacter(physicsBody);
    }

    private void LookAtCharacter(CharacterBody physicsBody)
    {
        var z = physicsBody.GroundPos.position.z;

        if (z > 0)
        {
            var pos = physicsBody.GroundPos.position;
            pos.y = 3;
            pos.z += 3;
            var rotation = Quaternion.Euler(new Vector3(19, 180, 0));
            CharacterCamera.transform.position = pos;
            CharacterCamera.transform.rotation = rotation;
        }
        else
        {
            var pos = physicsBody.GroundPos.position;
            pos.y = 3;
            pos.z -= 3;
            var rotation = Quaternion.Euler(new Vector3(19, 0, 0));
            CharacterCamera.transform.position = pos;
            CharacterCamera.transform.rotation = rotation;
        }
    }
}
