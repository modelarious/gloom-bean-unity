using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class WaxDrain:MonoBehaviour
    {
        public bool Sealed {get;private set;}public WaterVolume channel;public Vector2 openCurrent=new Vector2(-3,0),closedCurrent=new Vector2(3,0);public FlowEmitter emitter;public Transform diverted;
        void FixedUpdate(){Sealed=false;foreach(var c in Physics2D.OverlapBoxAll(transform.position,new Vector2(1.2f,.8f),0,1<<Layers.Prop))if(c.GetComponent<WaxPlug>())Sealed=true;if(channel)channel.current=Sealed?closedCurrent:openCurrent;if(emitter)emitter.diverted=Sealed;var s=GetComponent<SpriteRenderer>();if(s)s.color=Sealed?new Color(.94f,.82f,.45f):new Color(.21f,.16f,.19f);}
    }
}