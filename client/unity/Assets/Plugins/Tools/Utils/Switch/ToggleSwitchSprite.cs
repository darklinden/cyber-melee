using UnityEngine;
using UnityEngine.UI;

namespace Wtf
{
    public class ToggleSwitchSprite : ToggleSwitchButton
    {
        [SerializeField] protected Image image;
        [SerializeField] protected Sprite SpriteOn;
        [SerializeField] protected Sprite SpriteOff;
        [SerializeField] protected Sprite SpriteDisabled;

        protected override void UpdateUI()
        {
            if (image == null)
            {
                Log.E("ToggleSwitchSprite.UpdateUI: image is null", gameObject.name);
                return;
            }

            switch (_state)
            {
                case EState.Disabled:
                    {
                        if (SpriteDisabled != null)
                        {
                            image.sprite = SpriteDisabled;
                            image.color = Color.white;
                        }
                        else
                        {
                            image.color = Color.clear;
                        }
                    }
                    break;
                case EState.Off:
                    {
                        if (SpriteOff != null)
                        {
                            image.sprite = SpriteOff;
                            image.color = Color.white;
                        }
                        else
                        {
                            image.color = Color.clear;
                        }
                    }
                    break;
                case EState.On:
                    {
                        if (SpriteOn != null)
                        {
                            image.sprite = SpriteOn;
                            image.color = Color.white;
                        }
                        else
                        {
                            image.color = Color.clear;
                        }
                    }
                    break;
                default:
                    {
                        Log.E("ToggleSwitchSprite.UpdateUI: _state is not defined", gameObject.name);
                    }
                    break;
            }
        }
    }
}