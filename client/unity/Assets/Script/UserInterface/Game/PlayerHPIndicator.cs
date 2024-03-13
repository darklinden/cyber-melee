using System;
using System.Text;
using App;
using Battle;
using Cysharp.Threading.Tasks;
using Lockstep;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wtf;

public class PlayerHPIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerName;
    [SerializeField] private Image HpBar;
    [SerializeField] private Image DeadIcon;

    [SerializeField] private bool isMe;

    [SerializeField] private PlayerHPText m_TextPrefab = null;
    private GameObjectPool m_TextPool = null;

    private void Start()
    {
        PlayerName.text = "";
        HpBar.fillAmount = 1;
        DeadIcon.gameObject.SetActive(false);

        AsyncInitTextPool().Forget();

        var eventBus = Context.Inst.EventBus;
        eventBus.OnDispatchPlayPlayerEnter += OnDispatchPlayCharacterEnter;
        eventBus.OnDispatchPlayPlayerHPChanged += OnDispatchPlayPlayerHPChanged;
        eventBus.OnDispatchPlayPlayerDefeat += OnDispatchPlayCharacterDead;
    }

    private async UniTask AsyncInitTextPool()
    {
        m_TextPool = GameObjectPool.CreateInstance(transform);
        m_TextPool.Initialize(m_TextPrefab);
        await m_TextPool.PrewarmAsync(10);
    }

    private void OnDestroy()
    {
        if (m_TextPool != null)
        {
            m_TextPool.Deinitialize();
            m_TextPool = null;
        }
        var eventBus = Context.Inst?.EventBus;
        if (eventBus == null) return;
        eventBus.OnDispatchPlayPlayerEnter -= OnDispatchPlayCharacterEnter;
        eventBus.OnDispatchPlayPlayerHPChanged -= OnDispatchPlayPlayerHPChanged;
        eventBus.OnDispatchPlayPlayerDefeat -= OnDispatchPlayCharacterDead;
    }

    // 每阵营只有一个玩家一个角色
    private int CampId = 0;
    private ulong PlayerId = 0;
    private ulong CharacterId = 0;
    private string strPlayerName = null;
    private void OnDispatchPlayCharacterEnter(long frame, GamePlayFrame_PlayerEnter gamePlayFrame_CharacterEnter)
    {
        Log.D("OnDispatchPlayCharacterEnter", frame, gamePlayFrame_CharacterEnter);
        var serviceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();
        var battleSystem = Context.Inst.GetSystem<Battle.BattleSystem>();
        if (isMe)
        {
            CampId = serviceSystem.Data.BattleInfo.FriendlyCampId;
            var players = serviceSystem.Data.BattleInfo.FriendlyPlayerIds;
            if (players.Count > 0)
            {
                PlayerId = players[0];
                var player = serviceSystem.Data.BattleInfo.PlayerInfoDict[PlayerId];
                Log.D("PlayerName", player.Name);
                strPlayerName = player.Name;
            }
            var characters = battleSystem.PlayerCalc.GetPlayerIdsByCampId(CampId);
            if (characters.Count > 0)
            {
                CharacterId = characters[0];
                var player = battleSystem.PlayerCalc.GetPlayerById(CharacterId);
                var prop = PlayerFrameUtil.GetProp(player, frame);
                PlayerName.text = $"{strPlayerName} ({prop.Hp})";
            }
        }
        else
        {
            CampId = serviceSystem.Data.BattleInfo.HostileCampId;
            var players = serviceSystem.Data.BattleInfo.HostilePlayerIds;
            if (players.Count > 0)
            {
                PlayerId = players[0];
                var player = serviceSystem.Data.BattleInfo.PlayerInfoDict[PlayerId];
                Log.D("PlayerName", player.Name);
                strPlayerName = player.Name;
            }
            var characters = battleSystem.PlayerCalc.GetPlayerIdsByCampId(CampId);
            if (characters.Count > 0)
            {
                CharacterId = characters[0];
                var player = battleSystem.PlayerCalc.GetPlayerById(CharacterId);
                var prop = PlayerFrameUtil.GetProp(player, frame);
                PlayerName.text = $"{strPlayerName} ({prop.Hp})";
            }
        }

        if (gamePlayFrame_CharacterEnter.CampId == CampId)
        {
            HpBar.fillAmount = 1;
        }
    }

    private StringBuilder m_StringBuilder = new StringBuilder();
    private void OnDispatchPlayPlayerHPChanged(long frame, GamePlayFrame_PlayerHPChanged gamePlayFrame_CharacterHurt)
    {
        Log.D("OnDispatchPlayCharacterHurt", frame, gamePlayFrame_CharacterHurt);
        if (gamePlayFrame_CharacterHurt.CharacterTarget == CharacterId)
        {
            HpBar.fillAmount = gamePlayFrame_CharacterHurt.Hp / (float)gamePlayFrame_CharacterHurt.MaxHp;
            PlayerName.text = $"{strPlayerName} ({gamePlayFrame_CharacterHurt.Hp})";
            if (gamePlayFrame_CharacterHurt.Damage != 0)
            {
                m_StringBuilder.Clear();
                m_StringBuilder.Append(gamePlayFrame_CharacterHurt.Damage > 0 ? "+" : "");
                m_StringBuilder.Append(gamePlayFrame_CharacterHurt.Damage);
                if (gamePlayFrame_CharacterHurt.SkillType != Proto.SkillType.NONE)
                {
                    m_StringBuilder.Append(" ");
                    m_StringBuilder.Append(gamePlayFrame_CharacterHurt.SkillType);
                }
                if (gamePlayFrame_CharacterHurt.ProjectileType != Proto.ProjectileType.NONE)
                {
                    m_StringBuilder.Append(" ");
                    m_StringBuilder.Append(gamePlayFrame_CharacterHurt.ProjectileType);
                }
                var text = m_StringBuilder.ToString();
                AsyncShowText(text).Forget();
            }
        }
    }

    private async UniTask AsyncShowText(string text)
    {
        var txGo = m_TextPool?.Get();
        if (txGo == null) return;
        txGo.transform.position = transform.position;
        var tx = txGo.GetComponent<PlayerHPText>();
        await tx.AsyncShow(text);
        m_TextPool.Return(txGo);
    }

    private void OnDispatchPlayCharacterDead(long frame, GamePlayFrame_PlayerDefeat gamePlayFrame_CharacterDead)
    {
        Log.D("OnDispatchPlayCharacterDead", frame, gamePlayFrame_CharacterDead);
        if (gamePlayFrame_CharacterDead.CharacterId == CharacterId)
        {
            HpBar.fillAmount = 0;
            DeadIcon.gameObject.SetActive(true);
        }
    }
}