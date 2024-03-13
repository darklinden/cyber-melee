using UnityEngine;

namespace Wtf
{
    public class ToggleSwitchBtnGroup : ToggleSwitch
    {
        public const float ANIM_DUR = 0.2f;

        [SerializeField] protected GameObject[] goOns;
        [SerializeField] protected GameObject[] goOffs;
        [SerializeField] protected Vector2 PosOn;
        [SerializeField] protected Vector2 PosOff;
        [SerializeField] protected TransformAnimation animGroups;

        protected override void UpdateUI()
        {
            switch (_state)
            {
                case EState.Off:
                    {
                        if (animGroups.HasTasks)
                        {
                            animGroups.stopAll();
                        }

                        var task = TransformAnimationTask.Get(
                            animGroups.Tm,
                            ANIM_DUR,
                            0,
                            TimeUtil.DeltaTimeType.UNSCALED_DELTA_TIME
                        );
                        task.DisposeByAnimation = true;
                        task.translateToAnchoredPos
                        (
                            PosOff,
                            Easing.Function.OUT_CUBIC
                        );

                        task.EndCallback = ta =>
                        {
                            foreach (var go in goOns)
                            {
                                if (go != null && go.activeSelf)
                                    go.SetActive(false);
                            }

                            foreach (var go in goOffs)
                            {
                                if (go != null && !go.activeSelf)
                                    go.SetActive(true);
                            }
                        };

                        animGroups.addTask(task);
                    }
                    break;
                case EState.On:
                    {
                        if (animGroups.HasTasks)
                        {
                            animGroups.stopAll();
                        }

                        foreach (var go in goOns)
                        {
                            if (go != null && !go.activeSelf)
                                go.SetActive(true);
                        }

                        foreach (var go in goOffs)
                        {
                            if (go != null && go.activeSelf)
                                go.SetActive(false);
                        }

                        var task = TransformAnimationTask.Get(
                            animGroups.Tm,
                            ANIM_DUR,
                            0,
                            TimeUtil.DeltaTimeType.UNSCALED_DELTA_TIME
                        );
                        task.DisposeByAnimation = true;
                        task.translateToAnchoredPos
                        (
                            PosOn,
                            Easing.Function.OUT_CUBIC
                        );

                        animGroups.addTask(task);
                    }
                    break;
                default:
                    break;
            }
        }

        private void Start()
        {
            IsOn = false;
        }

        public TimeLock AnimLock = new TimeLock("ToggleSwitchBtnGroup", 500);
        public virtual void OnButtonClicked()
        {
            if (AnimLock.IsLocked) return;
            AnimLock.Lock();

            IsOn = !IsOn;
        }
    }
}