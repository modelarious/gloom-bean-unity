using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Render-only: depict actual state, never add trigger/collision or change a switch.
    [DefaultExecutionOrder(225)]
    public sealed class GbaMechanismView:MonoBehaviour
    {
        SpriteRenderer original,picture;PressurePlate plate;Lever lever;string key;Vector2 size;
        public void Initialize(SpriteRenderer source,string asset,Vector2 dimensions){original=source;key=asset;size=dimensions;plate=source.GetComponent<PressurePlate>();lever=source.GetComponent<Lever>();picture=V6Art.Picture(transform,asset,source.sortingOrder+1);}
        void LateUpdate(){if(!picture||!original)return;string asset=plate?"plate_"+(plate.Pressed?1:0):lever?"lever_"+(lever.state?1:0):key;
            picture.sprite=V6Art.Sprite(asset,16);picture.enabled=original.enabled;picture.color=Color.white;V6Art.Fit(picture,size);original.forceRenderingOff=picture.sprite!=null;
        }
        void OnDestroy(){if(original)original.forceRenderingOff=false;}
    }
}
