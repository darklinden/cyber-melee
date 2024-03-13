using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    public class ToggleSwitchGO : ToggleSwitchButton
    {
        [SerializeField] protected GameObject GoOn;
        [SerializeField] protected GameObject GoOff;
        [SerializeField] protected GameObject GoDisabled;

        protected override void UpdateUI()
        {
            switch (_state)
            {
                case EState.Disabled:
                    if (GoDisabled != null) GoDisabled.SetActive(true);
                    if (GoOn != null) GoOn.SetActive(false);
                    if (GoOff != null) GoOff.SetActive(false);
                    break;
                case EState.Off:
                    if (GoDisabled != null) GoDisabled.SetActive(false);
                    if (GoOn != null) GoOn.SetActive(false);
                    if (GoOff != null) GoOff.SetActive(true);
                    break;
                case EState.On:
                    if (GoDisabled != null) GoDisabled.SetActive(false);
                    if (GoOn != null) GoOn.SetActive(true);
                    if (GoOff != null) GoOff.SetActive(false);
                    break;
            }
        }
    }
}