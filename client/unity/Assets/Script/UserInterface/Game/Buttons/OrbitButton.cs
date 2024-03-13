using System;
using Proto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrbitButton : MonoBehaviour
{
    [SerializeField] internal int OrbitIndex;
    [SerializeField] internal Image CdCover;
    [SerializeField] internal TextMeshProUGUI KeyText;

    private Action<int> OnOrbitClicked;
    public void Init(int orbitIndex, Action<int> onOrbitClicked)
    {
        OrbitIndex = orbitIndex;
        OnOrbitClicked = onOrbitClicked;
        KeyText.text = Key.ToString();
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

        OnOrbitClicked?.Invoke(OrbitIndex);
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
