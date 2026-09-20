using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-110)]
    public sealed class ProcessionCarrier:MonoBehaviour
    {
        public Vector2 a,b;public float speed=2,endPause=1.2f;public PulseReceiver bell;public bool deaf;public int command;public Rigidbody2D body;
        public bool AtEnd {get;private set;}public float Progress=>t;int lastCount;float t,dir=1,kneel,pause;
        void Start(){body=GetComponent<Rigidbody2D>();}
        void FixedUpdate(){
            if(!deaf&&bell&&bell.count!=lastCount){lastCount=bell.count;command=(command+1)%4;if(command==3)dir=-dir;RuntimeEvents.Emit("procession-command",name+":"+command);}
            float dt=Time.fixedDeltaTime*LocalTime.Scale(body.position);pause-=dt;
            if(deaf||command==0||command==3){if(pause<=0)t+=dir*dt*speed/Mathf.Max(1,Vector2.Distance(a,b));}
            if(t>1){t=1;dir=-1;pause=endPause;}else if(t<0){t=0;dir=1;pause=endPause;}
            AtEnd=t<.015f||t>.985f;kneel=Mathf.MoveTowards(kneel,command==2&&!deaf?1.2f:0,dt*2);
            Vector2 next=Vector2.Lerp(a,b,t)+Vector2.down*kneel;
            body.MovePosition(next);body.angularVelocity=0;
        }
    }
}
