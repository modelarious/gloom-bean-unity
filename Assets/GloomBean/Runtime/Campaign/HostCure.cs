using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class HostCure : MonoBehaviour,IInteractable
    {
        public HostKind kind;public bool all; public void Interact(ActorMotor a){if(!a.replica)a.GetComponent<HostController>()?.Cure(all?HostKind.None:kind);}
        void OnTriggerStay2D(Collider2D c){var a=c.GetComponentInParent<ActorMotor>();if(a)Interact(a);}
    }
}