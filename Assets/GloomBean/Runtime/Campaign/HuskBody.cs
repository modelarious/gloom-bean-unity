using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class HuskBody:MonoBehaviour,IHittable,IInteractable
    {
        public HostController owner;public float ignoreOwnerUntil;
        void Update(){if(owner&&ignoreOwnerUntil>0&&Time.time>ignoreOwnerUntil){Physics2D.IgnoreCollision(GetComponent<Collider2D>(),owner.Actor.Shape,false);ignoreOwnerUntil=0;}}
        public void Interact(ActorMotor actor){if(owner&&actor==owner.Actor)owner.TryReclaim(this);}
        public void Hit(HitInfo h){GetComponent<Rigidbody2D>().AddForce(h.direction*h.power*2,ForceMode2D.Impulse);}
    }
}