using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class PerspectiveStamp:MonoBehaviour,IInteractable
    {
        public DepthGeometry[] geometry;public int plane=1;public bool reversed;
        public void Interact(ActorMotor a){plane=(plane+(reversed?-1:1)+3)%3;if(geometry!=null)foreach(var g in geometry)if(g)g.SetPlane(plane);RuntimeEvents.Emit("stamp",plane.ToString());}
    }
}