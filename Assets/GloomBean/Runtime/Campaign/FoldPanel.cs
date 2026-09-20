using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class FoldPanel:MonoBehaviour
    {
        public float length=6,angle,targetAngle,speed=55;public float minAngle=-175,maxAngle=175;public Rigidbody2D body;public bool folding;public bool HeldLoad {get;private set;}public float BracedSeconds {get;private set;}
        public void AimAt(Vector2 p){var d=p-(Vector2)transform.position;targetAngle=Mathf.Clamp(Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg,minAngle,maxAngle);folding=true;}
        void FixedUpdate()
        {
            HeldLoad=false;if(!folding){if(body)body.angularVelocity=0;return;}float next=Mathf.MoveTowardsAngle(angle,targetAngle,speed*Time.fixedDeltaTime*LocalTime.Scale(transform.position));
            // A braced coffin stops an approaching structural piece instead of clipping through it.
            Vector2 mid=(Vector2)transform.position+(Vector2)(Quaternion.Euler(0,0,next)*Vector2.right)*(length*.5f);
            foreach(var c in Physics2D.OverlapBoxAll(mid,new Vector2(length,.5f),next,1<<Layers.Actor)){var b=c.GetComponent<LoadBearingBody>();if(b&&b.bracing){HeldLoad=true;BracedSeconds+=Time.fixedDeltaTime;if(body)body.angularVelocity=0;return;}}
            angle=next;if(body)body.MoveRotation(angle);else transform.rotation=Quaternion.Euler(0,0,angle);if(Mathf.Abs(Mathf.DeltaAngle(angle,targetAngle))<.1f)folding=false;
        }
    }
}