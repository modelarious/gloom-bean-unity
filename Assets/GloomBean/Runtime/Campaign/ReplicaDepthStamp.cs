using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class ReplicaDepthStamp:MonoBehaviour
    {
        public int plane=2;public ActorMotor Stamped {get;private set;}
        void OnTriggerStay2D(Collider2D c){var a=c.GetComponentInParent<ActorMotor>();if(!a||!a.replica||a==Stamped)return;float scale=DepthGeometry.Factor(plane);
            if(!a.TrySetStandingSizeCentered(new Vector2(.88f,1.5f)*scale,Layers.Solids|(1<<(17+plane))))return;
            a.Shape.excludeLayers=(7<<17)&~(1<<(17+plane));a.Shape.includeLayers=1<<(17+plane);a.Shape.layerOverridePriority=15;a.collisionMask=Layers.Solids|(1<<(17+plane));a.speedFactor=scale;Stamped=a;RuntimeEvents.Emit("replica-depth",plane.ToString());}
    }
}
