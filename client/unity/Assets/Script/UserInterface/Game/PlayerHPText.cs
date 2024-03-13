using System;
using App;
using Cysharp.Threading.Tasks;
using Lockstep;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wtf;

public class PlayerHPText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text;

    private Transform m_Tm = null;
    public Transform Tm => m_Tm != null ? m_Tm : (m_Tm = transform);

    private CanvasGroupAlphaFading m_AlphaFading = null;
    public CanvasGroupAlphaFading AlphaFading => m_AlphaFading != null ? m_AlphaFading : (m_AlphaFading = GetComponent<CanvasGroupAlphaFading>());

    private float m_LifeTimeMax = 0.5f;
    public async UniTask AsyncShow(string text)
    {
        Text.text = text;
        AlphaFading.setTransparent(false);
        await AlphaFading.animateToTransparent(m_LifeTimeMax);
    }
}