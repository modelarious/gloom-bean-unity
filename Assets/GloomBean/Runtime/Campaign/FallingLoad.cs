using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class FallingLoad:MonoBehaviour
    {
        public DescentController frame;public PulseReceiver releaseBell;public ImpactHoist hoist;public bool released,impacted;public float impactSpeed;
        Rigidbody2D body;Vector2 origin,caught;float liftClearance;bool caughtBody;
        void Start(){body=GetComponent<Rigidbody2D>();origin=body.position-(frame?frame.Offset:Vector2.zero);body.simulated=false;}
        void FixedUpdate(){Vector2 offset=frame?frame.Offset:Vector2.zero;
            if(!released){body.position=origin+offset;if(releaseBell&&releaseBell.count>0){released=true;body.simulated=true;RuntimeEvents.Emit("nave-released",name);}return;}
            if(impacted){if(!caughtBody){caughtBody=true;body.bodyType=RigidbodyType2D.Kinematic;body.linearVelocity=Vector2.zero;body.angularVelocity=0;}
                liftClearance=Mathf.MoveTowards(liftClearance,4,3*Time.fixedDeltaTime);body.MovePosition(caught+offset+Vector2.up*((hoist?hoist.lifted:0)+liftClearance));}
        }
        void OnCollisionEnter2D(Collision2D c){if(impacted||!released)return;var bearing=c.collider.GetComponent<LoadBearingBody>();
            if(bearing&&bearing.bracing&&c.relativeVelocity.magnitude>=2.5f){impacted=true;impactSpeed=c.relativeVelocity.magnitude;caught=body.position-(frame?frame.Offset:Vector2.zero);RuntimeEvents.Emit("nave-bearing-impact",impactSpeed.ToString("0.00"));}}
    }
}
