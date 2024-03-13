using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    public class PlayerLoadingProgressCell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI Name;
        [SerializeField] private Image ProgressBar;

        public void Setup(string name)
        {
            Name.text = name;
            ProgressBar.fillAmount = 0;
        }

        public void SetProgress(int progress)
        {
            ProgressBar.fillAmount = progress / 100f;
        }
    }
}