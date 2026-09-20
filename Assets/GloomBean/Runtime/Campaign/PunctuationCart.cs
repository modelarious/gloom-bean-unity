using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Ordinary physical punctuation: E grips/releases, movement pulls through forces.
    // Its position controls typography; it is never consumed as a matching key.
    [DefaultExecutionOrder(50)]
    public sealed class PunctuationCart:MonoBehaviour,IInteractable
    {
        public float firstSlot,slotWidth=3;public int[] lineLengths={5,3,4};
        public Rigidbody2D Body {get;private set;}public ActorMotor Holder {get;private set;}
        float offset;
        public int Slot=>Mathf.Clamp(Mathf.RoundToInt((Body.position.x-firstSlot)/slotWidth),0,lineLengths.Length-1);
        public int Columns=>lineLengths[Slot];
        void Awake(){Body=GetComponent<Rigidbody2D>();}
        public void Interact(ActorMotor actor){if(Holder==actor){Holder=null;return;}Holder=actor;offset=Mathf.Clamp(Body.position.x-actor.Body.position.x,-1.4f,1.4f);RuntimeEvents.Emit("punctuation-grip",name);}
        void FixedUpdate()
        {
            if(!Holder)return;if(Vector2.Distance(Holder.Body.position,Body.position)>3.5f||Holder.Health<=0){Holder=null;return;}
            float error=Holder.Body.position.x+offset-Body.position.x;float force=Mathf.Clamp(error*80-Body.linearVelocity.x*12,-70,70);
            Body.AddForce(Vector2.right*force);Holder.Body.AddForce(Vector2.left*force*.15f);
        }
    }
}
