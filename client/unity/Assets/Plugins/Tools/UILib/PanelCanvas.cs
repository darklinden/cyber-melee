using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wtf
{
    public class PanelCanvas : MonoBehaviour
    {
        [SerializeField]
        bool PreventSceneDestroy = false;

        public RectTransform RtCover { get; set; }

        public static RectTransform Cover
        {
            get
            {
                if (Instances != null && Instances.Count > 0)
                    return Instances[0].RtCover;
                else
                    return null;
            }
        }

        // --- Singleton ---
        private static readonly Lazy<List<PanelCanvas>> lazy = new Lazy<List<PanelCanvas>>(() => new List<PanelCanvas>());
        private static List<PanelCanvas> Instances { get { return lazy.Value; } }
        // --- Singleton ---

        private void Awake()
        {
            if (PreventSceneDestroy) DontDestroyOnLoad(gameObject);
            RtCover = this.GetComponent<RectTransform>();
            Instances.Insert(0, this);
            for (int i = Instances.Count - 1; i >= 0; i--)
            {
                if (Instances[i] == null || Instances[i].gameObject == null)
                    Instances.RemoveAt(i);
            }
        }

        private void OnDestroy()
        {
            Instances.Remove(this);
        }
    }
}