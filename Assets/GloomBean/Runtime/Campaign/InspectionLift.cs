using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A bell orders a physical inspection trip. A tall passenger really cannot fit
    // under the lower ceiling: the lift reverses rather than clipping the passenger.
    [DefaultExecutionOrder(-110)]
    public sealed class InspectionLift:MonoBehaviour
    {
        public PulseReceiver bell;public Vector2[] route;public float speed=2;public bool moving,blocked;public int trips;
        Rigidbody2D body;BoxCollider2D shape;int lastBell,index;float wait;bool returning;
        readonly HashSet<Rigidbody2D> inspected=new HashSet<Rigidbody2D>();
        void Start(){body=GetComponent<Rigidbody2D>();shape=GetComponent<BoxCollider2D>();}
        void FixedUpdate(){if(bell&&bell.count!=lastBell){lastBell=bell.count;if(!moving){index=1;moving=true;blocked=false;returning=false;}}
            if(!moving||route==null||route.Length<2){body.linearVelocity=Vector2.zero;return;}
            wait-=Time.fixedDeltaTime;if(wait>0){body.linearVelocity=Vector2.zero;return;}
            Vector2 next=Vector2.MoveTowards(body.position,route[index],speed*Time.fixedDeltaTime);Vector2 delta=next-body.position;
            inspected.Clear();bool obstructed=false;
            foreach(var c in Physics2D.OverlapBoxAll(body.position+Vector2.up*.8f,new Vector2(shape.size.x,1.8f),0,1<<Layers.Actor)){
                if(c.isTrigger||!c.attachedRigidbody||!inspected.Add(c.attachedRigidbody))continue;
                var actor=c.GetComponent<ActorMotor>();if(!actor)continue;
                Vector2 size=c.bounds.size;Vector2 center=(Vector2)c.bounds.center+delta;
                foreach(var wall in Physics2D.OverlapBoxAll(center,size-Vector2.one*.06f,0,Layers.Solids))
                    if(!wall.isTrigger&&wall.attachedRigidbody!=body&&wall.attachedRigidbody!=c.attachedRigidbody){obstructed=true;break;}
            }
            if(obstructed&&!returning){blocked=true;returning=true;index=1;body.linearVelocity=Vector2.zero;return;}
            body.MovePosition(next);
            if(Vector2.Distance(next,route[index])<.01f){if(returning){if(index==0){moving=false;trips++;}else index=0;}
                else{index++;if(index>=route.Length){moving=false;trips++;index=0;}else wait=.45f;}}
        }
    }
}
