using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-120)]
    public sealed class ApartmentLift:MonoBehaviour,IInteractable
    {
        public Vector2 lower,upper,via;public bool useVia;public float speed=3;public Gate access;public bool upperRequested;
        Rigidbody2D body;int leg=1;bool lastRequest;
        void Awake(){body=GetComponent<Rigidbody2D>();if(!body)body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.interpolation=RigidbodyInterpolation2D.Interpolate;}
        public void Interact(ActorMotor actor){if(actor.replica)return;if(access&&!access.opened){StageSession.Current?.Notice("Both apartment scales must release the lift first.");return;}upperRequested=!upperRequested;RuntimeEvents.Emit("lift-call",upperRequested?"upper":"lower");}
        void FixedUpdate(){if(upperRequested!=lastRequest){lastRequest=upperRequested;leg=0;}Vector2 target=useVia&&leg==0?via:upperRequested?upper:lower;
            body.MovePosition(Vector2.MoveTowards(body.position,target,speed*Time.fixedDeltaTime));if(useVia&&leg==0&&Vector2.Distance(body.position,target)<.025f)leg=1;}
    }
}
