using System;
using App;
using Cysharp.Threading.Tasks;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wtf;

public class SkillButton : MonoBehaviour
{
    [SerializeField] internal Image BtnImg;
    [SerializeField] internal Image CdCover;
    [SerializeField] internal TextMeshProUGUI KeyText;

    private Action OnSkillClicked;
    public void Init(SkillType skillType, Action onSkillClicked)
    {
        AsyncLoadImg(skillType).Forget();
        OnSkillClicked = onSkillClicked;
        KeyText.text = Key.ToString();
    }

    private async UniTask AsyncLoadImg(SkillType skillType)
    {
        Log.D("SkillButton.AsyncLoadImg", skillType);
        var atlasLoader = Context.Inst.GetSystem<AtlasLoader>();
        var sp = await atlasLoader.AsyncLoadSprite(AtlasGroup.All, skillType.ToString());
        BtnImg.sprite = sp;
    }

    internal void SetCooldownProgress(float cd)
    {
        CdCover.fillAmount = cd;
    }

    public void OnClicked()
    {
        if (CdCover.fillAmount > 0)
        {
            // Log.D("SkillButton.OnClicked", "Skill is in cooldown");
            return;
        }

        OnSkillClicked?.Invoke();
    }

    [SerializeField] internal KeyCode Key;
    private bool m_IsKeyDown = false;
    private void Update()
    {
        var isKeyDown = Input.GetKeyDown(Key);

        if (m_IsKeyDown)
        {
            if (!isKeyDown)
            {
                m_IsKeyDown = false;
            }
        }
        else
        {
            if (isKeyDown)
            {
                m_IsKeyDown = true;
                OnClicked();
            }
        }
    }
}
