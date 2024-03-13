using Wtf;
using TMPro;
using System;
using UnityEngine;

namespace UserInterface
{
    public class PanelTips : CommonPanelWithData<PanelTips.Data>
    {
        public static new string PanelPath => $"Assets/Addrs/{i18n.Locale}/UI/Panels/PanelTips.prefab";

        public TextMeshProUGUI Title;
        public TextMeshProUGUI Content;
        public TextMeshProUGUI TitleBtnCancel;
        public TextMeshProUGUI TitleBtnOK;
        public GameObject BtnClose;
        public GameObject BtnCancel;
        public GameObject BtnOK;

        public enum Choice
        {
            NoChoice,
            Cancel,
            OK,
        }

        private Choice m_Choice = Choice.NoChoice;

        public enum CloseType
        {
            ClickOutsideClose, // 点击外部关闭
            CloseEnabled, // 显示关闭按钮
            MustChoose, // 必须选择 才能关闭
        }

        public class Data
        {
            public string Title;
            public string Content;
            public string TitleBtnCancel;
            public string TitleBtnOK;
            public CloseType CloseType = CloseType.ClickOutsideClose;
            public Choice OutChoice = Choice.NoChoice;
            public Action<Choice> OnPanelClosed;
        }

        public Data InData { get; private set; }

        public override void SetData(Data data)
        {
            InData = data;
            m_Choice = Choice.NoChoice;
            switch (InData.CloseType)
            {
                case CloseType.ClickOutsideClose:
                    _clickBgClose = true;
                    BtnClose.SetActive(true);
                    break;
                case CloseType.CloseEnabled:
                    _clickBgClose = false;
                    BtnClose.SetActive(true);
                    break;
                case CloseType.MustChoose:
                    _clickBgClose = false;
                    BtnClose.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        protected override void OnOpenComplete()
        {
            Log.D("PanelTips.OnOpenComplete");
            RefreshUI();
        }

        protected override void OnCloseStart()
        {
            Log.D("PanelTips.OnCloseComplete");
            InData.OutChoice = m_Choice;
            InData.OnPanelClosed?.Invoke(m_Choice);
        }

        private void RefreshUI()
        {
            if (InData == null)
            {
                Log.E("PanelTips.RefreshUI", "InData is null");
                return;
            }

            if (!string.IsNullOrEmpty(InData.Title)) Title.text = InData.Title;
            if (!string.IsNullOrEmpty(InData.Content)) Content.text = InData.Content;
            if (!string.IsNullOrEmpty(InData.TitleBtnCancel)) TitleBtnCancel.text = InData.TitleBtnCancel;
            if (!string.IsNullOrEmpty(InData.TitleBtnOK)) TitleBtnOK.text = InData.TitleBtnOK;
            BtnCancel.SetActive(!string.IsNullOrEmpty(InData.TitleBtnCancel));
        }

        public void OnBtnCancel()
        {
            m_Choice = Choice.Cancel;
            AClose();
        }

        public void OnBtnOK()
        {
            m_Choice = Choice.OK;
            AClose();
        }
    }
}