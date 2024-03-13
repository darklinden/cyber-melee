using UnityEngine;

namespace Wtf
{
    public class RTSizeFitAuto : RTSizeFit
    {
        private enum FitAction
        {
            None = 0,
            AutoFit = 1
        }

        [SerializeField] private FitAction m_FitAction = FitAction.AutoFit;

        private void OnEnable()
        {
            if (m_FitAction == FitAction.AutoFit) Fit();
        }

        public void OnRectTransformDimensionsChange()
        {
            if (m_FitAction == FitAction.AutoFit) Fit();
        }
    }
}