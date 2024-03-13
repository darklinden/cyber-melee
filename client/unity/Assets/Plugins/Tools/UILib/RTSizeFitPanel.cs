using System;
using System.Collections;
using UnityEngine;

namespace Wtf
{
    public class RTSizeFitPanel : RTSizeFit
    {
        public void BlockTouch()
        {
            var btnBlocker = gameObject.GetComponent<UnityEngine.UI.Button>();
            if (btnBlocker == null)
            {
                btnBlocker = gameObject.AddComponent<UnityEngine.UI.Button>();
                btnBlocker.transition = UnityEngine.UI.Selectable.Transition.None;
            }
        }

        public void AnimEnable(Action completion)
        {
            StartCoroutine(RoutineAnimEnable(completion));
        }

        private IEnumerator RoutineAnimEnable(Action completion)
        {
            Log.D("AnimEnable: Start");

            var des_scale = Vector3.one;
            switch (_fitType)
            {
                case FitType.WidthLimit:
                    {
                        des_scale = FitWidth();
                    }
                    break;
                case FitType.HeightLimit:
                    {
                        des_scale = FitHeight();
                    }
                    break;
                default:
                    break;
            }

            Rt.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            var ta = gameObject.AddOrGetComponent<TransformAnimation>();

            var task = TransformAnimationTask.Get(Rt, 0.15f);
            task.scale(des_scale, true, Easing.Function.IN_CUBIC);
            ta.addTask(task);

            while (ta.HasTasks)
            {
                yield return null;
            }

            switch (_fitType)
            {
                case FitType.WidthLimit:
                    Rt.localScale = des_scale;
                    _widthRateOfParent = Rt.rect.width * Rt.localScale.x / ParentRt.rect.width;
                    break;
                case FitType.HeightLimit:
                    Rt.localScale = des_scale;
                    _heightRateOfParent = Rt.rect.height * Rt.localScale.y / ParentRt.rect.height;
                    break;
            }

            Log.D("AnimEnable: Ended", Rt.localScale);
            completion?.Invoke();
        }

        public void AnimDisable(Action completion)
        {
            StartCoroutine(RoutineAnimDisable(completion));
        }

        private IEnumerator RoutineAnimDisable(Action completion)
        {
            Log.D("AnimDisable: Start");
            var ta = gameObject.AddOrGetComponent<TransformAnimation>();

            var task = TransformAnimationTask.Get(Rt, 0.15f);
            task.scale(new Vector3(0.01f, 0.01f, 0.01f), true, Easing.Function.OUT_CUBIC);
            ta.addTask(task);

            while (ta.HasTasks)
            {
                yield return null;
            }

            Log.D("AnimDisable: Ended");
            completion?.Invoke();
        }
    }
}