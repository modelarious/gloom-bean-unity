using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-105)]
    public sealed class BearingPress:MonoBehaviour
    {
        public Vector2 rest,low;public float speed=1.3f,holdRequired=.8f,held;public bool released,contact;Rigidbody2D body;BoxCollider2D shape;
        void Start(){body=GetComponent<Rigidbody2D>();shape=GetComponent<BoxCollider2D>();rest=body.position;}
        void FixedUpdate(){Vector2 goal=released?rest:low;Vector2 next=Vector2.MoveTowards(body.position,goal,speed*Time.fixedDeltaTime*LocalTime.Scale(body.position));
            contact=false;bool occupied=false;
            foreach(var c in Physics2D.OverlapBoxAll(next,shape.size,0,1<<Layers.Actor)){
                if(c.isTrigger)continue;occupied=true;var brace=c.GetComponent<LoadBearingBody>();
                if(brace&&brace.bracing){contact=true;held+=Time.fixedDeltaTime;if(held>=holdRequired){released=true;RuntimeEvents.Emit("bearing-released",name);}}
                else if(!released){var actor=c.GetComponent<ActorMotor>();if(actor)actor.Hit(new HitInfo(null,Vector2.left,1));}
            }
            if(!contact)held=0;
            if(occupied&&!released){body.linearVelocity=Vector2.zero;return;}
            body.MovePosition(next);
        }
    }
}
