using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class HostSource : MonoBehaviour,IInteractable
    {
        public HostKind kind;public bool combine;public RailPath rail;public float mirrorAxis;public bool explicitAxis;
        public bool enabledSource=true;
        public void Interact(ActorMotor a){if(enabledSource&&!a.replica)a.GetComponent<HostController>()?.Acquire(kind,this,combine);}
        void OnTriggerEnter2D(Collider2D c){var a=c.GetComponentInParent<ActorMotor>();if(a)Interact(a);}
    }
}