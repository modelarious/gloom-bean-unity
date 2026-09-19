using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-110)]
    public sealed class ProcessionCarrier:MonoBehaviour
    {
        public Vector2 a,b;public float speed=2;public PulseReceiver bell;public bool deaf;public int command;public Rigidbody2D body;int lastCount;float t,dir=1,kneel;
        void Start(){body=GetComponent<Rigidbody2D>();}
        void FixedUpdate(){if(!deaf&&bell&&bell.count!=lastCount){lastCount=bell.count;command=(command+1)%3;}if(deaf||command==0)t+=dir*Time.fixedDeltaTime*speed/Mathf.Max(1,Vector2.Distance(a,b));if(t>=1){t=1;dir=-1;}if(t<=0){t=0;dir=1;}kneel=Mathf.MoveTowards(kneel,command==1&&!deaf?1.2f:0,Time.fixedDeltaTime*2);Vector2 next=Vector2.Lerp(a,b,t)+Vector2.down*kneel;bool braced=false;foreach(var c in Physics2D.OverlapBoxAll(next,new Vector2(3,.8f),0,1<<Layers.Actor)){var h=c.GetComponent<LoadBearingBody>();if(h&&h.bracing)braced=true;}if(!braced)body.MovePosition(next);if(command==2&&!deaf)body.MoveRotation(Mathf.MoveTowardsAngle(body.rotation,90,Time.fixedDeltaTime*45));}
    }
}