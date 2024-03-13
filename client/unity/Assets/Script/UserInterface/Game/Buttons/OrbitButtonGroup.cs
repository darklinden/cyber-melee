using System;
using System.Collections;
using System.Collections.Generic;
using App;
using Lockstep;
using Proto;
using UnityEngine;
using Wtf;

public class OrbitButtonGroup : MonoBehaviour
{
    [SerializeField] private OrbitButton[] _OrbitButtons;
    [SerializeField] private SkillButton _SkillButton;

    private List<OrbitButton> OrbitButtons = new List<OrbitButton>();

    private SkillType CastSkill = SkillType.NONE;
    private long CastSkillCdFull = -1;
    private long CastSkillCdProc = 0;


    private SkillType SpecialSkill = SkillType.NONE;
    private long SpecialSkillCdFull = -1;
    private long SpecialSkillCdProc = 0;

    private long SkillGlobalCdFull = -1;

    private int BulletCount = 0;

    void Start()
    {
        Log.D("SkillButtonGroup.Start");
        var eventBus = Context.Inst?.EventBus;
        eventBus.OnSkillActionBroadcast += OnSkillActionBroadcast;
        eventBus.OnDispatchPlayBulletBufferChanged += OnDispatchPlayBulletBufferChanged;

        // 初始化技能按钮
        var serviceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();
        var campIds = serviceSystem.Data.BattleInfo.CampIds;
        var reverse = campIds.IndexOf(serviceSystem.Data.BattleInfo.FriendlyCampId) != 0;
        var baseMap = Configs.Instance.OrbitMapData.GetData(Constants.MAP_ID);
        Log.AssertIsTrue(baseMap.Count == _OrbitButtons.Length, "轨道与按钮数量不匹配");

        for (var i = 0; i < _OrbitButtons.Length; i++)
        {
            var index = reverse ? _OrbitButtons.Length - 1 - i : i;

            var orbitButton = _OrbitButtons[i];
            OrbitButtons.Add(orbitButton);
            orbitButton.Init(index, OnOrbitClicked);
        }

        var characterId = serviceSystem.Data.PlayerInfo.CharacterId;
        var characterData = Configs.Instance.CharacterData.GetData(characterId);
        CastSkill = characterData.CastSkill;
        SpecialSkill = characterData.SpecialSkill;

        _SkillButton.Init(SpecialSkill, OnSkillClicked);

        var castSkillData = Configs.Instance.SkillData.GetData((int)CastSkill);
        var specialSkillData = Configs.Instance.SkillData.GetData((int)SpecialSkill);

        CastSkillCdFull = castSkillData.Cooldown;
        SpecialSkillCdFull = specialSkillData.Cooldown;

        Log.AssertIsTrue(castSkillData.GlobalCd == specialSkillData.GlobalCd, "全局冷却时间不一致");
        SkillGlobalCdFull = castSkillData.GlobalCd;
    }

    void OnDestroy()
    {
        var eventBus = Context.Inst?.EventBus;
        if (eventBus == null) return;

        eventBus.OnSkillActionBroadcast -= OnSkillActionBroadcast;
        eventBus.OnDispatchPlayBulletBufferChanged -= OnDispatchPlayBulletBufferChanged;
    }

    private void OnDispatchPlayBulletBufferChanged(long frame, GamePlayFrame_BulletBufferChanged gamePlayFrame_BulletBufferChanged)
    {
        BulletCount = gamePlayFrame_BulletBufferChanged.BulletBuffer.Count;
    }

    private void OnOrbitClicked(int orbitIndex)
    {
        if (CastSkillCdProc > 0)
        {
            Log.D("OrbitButton", "Skill is in cooldown");
            return;
        }

        if (BulletCount <= 0)
        {
            Log.D("OrbitButton", "Bullet is not enough");
            return;
        }

        GameCtrl.PlayerUseSkill(CastSkill, orbitIndex);
    }

    private void OnSkillClicked()
    {
        if (SpecialSkillCdProc > 0)
        {
            Log.D("SkillButton", "Skill is in cooldown");
            return;
        }
        switch (SpecialSkill)
        {
            case SkillType.Swap:
                {
                    if (BulletCount < 2)
                    {
                        Log.D("SkillButton", SpecialSkill, "Bullet is not enough");
                        return;
                    }
                }
                break;
            case SkillType.Random:
            case SkillType.Remove:
                {
                    if (BulletCount < 1)
                    {
                        Log.D("SkillButton", SpecialSkill, "Bullet is not enough");
                        return;
                    }
                }
                break;
            case SkillType.PowerUp:
                {
                    // pass
                }
                break;
            default:
                Log.E("Skill Not Support", SpecialSkill);
                break;
        }
        GameCtrl.PlayerUseSkill(SpecialSkill, -1);
    }

    private Service.ServiceSystem m_ServiceSystem = null;
    private Service.ServiceSystem ServiceSystem
    {
        get
        {
            if (m_ServiceSystem == null)
            {
                m_ServiceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();
            }
            return m_ServiceSystem;
        }
    }

    private void OnSkillActionBroadcast(SkillType selectType, ulong playerId, long tick)
    {
        Log.D("SkillButtonGroup.OnSkillActionBroadcast", selectType, playerId, tick);
        if (playerId != ServiceSystem.Data.PlayerInfo.PlayerId)
        {
            return;
        }

        if (selectType == CastSkill)
        {
            CastSkillCdProc = CastSkillCdFull;
            if (SpecialSkillCdProc <= 0)
                SpecialSkillCdProc = SkillGlobalCdFull;
        }
        else if (selectType == SpecialSkill)
        {
            SpecialSkillCdProc = SpecialSkillCdFull;
            if (CastSkillCdProc <= 0)
                CastSkillCdProc = SkillGlobalCdFull;
        }
        else
        {
            Log.E("未知技能类型", selectType);
        }
    }

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

    // Update is called once per frame
    void Update()
    {
        var deltaTimeLong = (long)(Time.deltaTime * 1000);

        // 轨道施放技能
        {
            var cd_full = CastSkillCdFull;
            var cd_proc = CastSkillCdProc;

            if (cd_full < 0)
            {
                // 目前不知道冷却时间总长
                if (cd_proc >= 0)
                {
                    // 如果当前冷却时间 >=0, 设置展示冷却时间为 0
                    CastSkillCdProc = -1;
                    foreach (var button in OrbitButtons)
                    {
                        button.SetCooldownProgress(0);
                    }
                }
                else
                {
                    // 否则, 不做任何操作
                }
            }
            else if (cd_full > 0)
            {
                // 目前已经在技能冷却过程中
                if (cd_proc >= 0)
                {
                    // 如果尚未冷却结束 更新冷却时间展示
                    cd_proc -= deltaTimeLong;
                    float f_cd;
                    if (cd_proc >= 0)
                    {
                        f_cd = cd_proc / (float)cd_full;
                    }
                    else
                    {
                        f_cd = 0;
                        cd_proc = -1;
                    }
                    CastSkillCdProc = cd_proc;
                    foreach (var button in OrbitButtons)
                    {
                        button.SetCooldownProgress(f_cd);
                    }
                }
                else
                {
                    // 否则, 不做任何操作
                }
            }
            else
            {
                // 技能冷却时长为 0, 不可能出现
                Log.E("技能冷却时长为 0", CastSkill);
            }
        }

        // 特殊技能
        {
            var cd_full = SpecialSkillCdFull;
            var cd_proc = SpecialSkillCdProc;

            if (cd_full < 0)
            {
                // 目前不知道冷却时间总长
                if (cd_proc >= 0)
                {
                    // 如果当前冷却时间 >=0, 设置展示冷却时间为 0
                    SpecialSkillCdProc = -1;
                    _SkillButton.SetCooldownProgress(0);
                }
                else
                {
                    // 否则, 不做任何操作
                }
            }
            else if (cd_full > 0)
            {
                // 目前已经在技能冷却过程中
                if (cd_proc >= 0)
                {
                    // 如果尚未冷却结束 更新冷却时间展示
                    cd_proc -= deltaTimeLong;
                    float f_cd;
                    if (cd_proc >= 0)
                    {
                        f_cd = cd_proc / (float)cd_full;
                    }
                    else
                    {
                        f_cd = 0;
                        cd_proc = -1;
                    }
                    SpecialSkillCdProc = cd_proc;
                    _SkillButton.SetCooldownProgress(f_cd);
                }
                else
                {
                    // 否则, 不做任何操作
                }
            }
            else
            {
                // 技能冷却时长为 0, 不可能出现
                Log.E("技能冷却时长为 0", SpecialSkill);
            }
        }
    }
}
