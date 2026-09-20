using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-120)]
    public sealed class ApartmentLift:MonoBehaviour,IInteractable
    {
        public Vector2 lower,upper;public float speed=3;public Gate access;public bool upperRequested;
        Rigidbody2D body;
        void Awake(){body=GetComponent<Rigidbody2D>();if(!body)body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.interpolation=RigidbodyInterpolation2D.Interpolate;}
        public void Interact(ActorMotor actor){if(actor.replica)return;if(access&&!access.opened){StageSession.Current?.Notice("Both apartment scales must release the lift first.");return;}upperRequested=!upperRequested;RuntimeEvents.Emit("lift-call",upperRequested?"upper":"lower");}
        void FixedUpdate(){body.MovePosition(Vector2.MoveTowards(body.position,upperRequested?upper:lower,speed*Time.fixedDeltaTime));}
    }
}
