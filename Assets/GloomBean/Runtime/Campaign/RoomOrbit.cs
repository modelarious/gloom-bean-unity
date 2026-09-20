using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-120)]
    public sealed class RoomOrbit:MonoBehaviour
    {
        public Vector2 center;public Vector2 radius=new Vector2(10,7),roomSize=new Vector2(10,6);
        public float phase,speed=.13f,stepSpeed=.9f;public bool running,drivenByDepth;
        public bool Settled=>Mathf.Abs(target-phase)<.015f;float target;int observedDepth=1;Rigidbody2D body;bool ready;
        void Awake(){body=GetComponent<Rigidbody2D>();if(!body)body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.interpolation=RigidbodyInterpolation2D.Interpolate;}
        void Start(){target=phase;ready=true;}
        public void RotateQuarter(int direction){if(!ready){target=phase;ready=true;}target+=Mathf.Sign(direction)*Mathf.PI*.5f;RuntimeEvents.Emit("hotel-rotation",target.ToString("0.00"));}
        void FixedUpdate()
        {
            if(!ready)return;
            if(drivenByDepth&&!running){var player=StageSession.Current?StageSession.Current.player:null;var depth=player?player.GetComponent<HostController>()?.Form<ParallaxForm>():null;
                if(depth!=null&&depth.Plane!=observedDepth){target+=(depth.Plane-observedDepth)*Mathf.PI*.5f;observedDepth=depth.Plane;}
                phase=Mathf.MoveTowards(phase,target,stepSpeed*Time.fixedDeltaTime);
            }else if(running){phase+=speed*Time.fixedDeltaTime;target=phase;}else return;
            Vector2 next=center+new Vector2(Mathf.Cos(phase)*radius.x,Mathf.Sin(phase)*radius.y);Vector2 delta=next-body.position;
            var moved=new HashSet<Rigidbody2D>();
            foreach(var c in Physics2D.OverlapBoxAll(body.position+Vector2.up*2,roomSize,0,(1<<Layers.Prop)|(1<<Layers.Enemy))){var rb=c.attachedRigidbody;
                if(rb&&rb.bodyType==RigidbodyType2D.Dynamic&&!rb.transform.IsChildOf(transform)&&moved.Add(rb))rb.position+=delta;}
            body.MovePosition(next);
        }
    }
}
