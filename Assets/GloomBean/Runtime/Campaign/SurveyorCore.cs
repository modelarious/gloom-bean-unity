using System;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class SurveyorCore:MonoBehaviour,IHittable
    {
        public Func<bool> Exposed;public bool struck;public event Action Struck;
        public void Hit(HitInfo hit){if(struck||hit.owner==null||hit.power<1||(Exposed!=null&&!Exposed()))return;
            if((hit.owner.Shape.contactMask.value&(1<<gameObject.layer))==0)return;
            struck=true;var c=GetComponent<Collider2D>();if(c)c.enabled=false;var sprite=GetComponent<SpriteRenderer>();if(sprite)sprite.color=new Color(.53f,.87f,.73f,.5f);
            RuntimeEvents.Emit("surveyor-core",name);Struck?.Invoke();}
    }
}
