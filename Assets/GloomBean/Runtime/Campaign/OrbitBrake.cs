using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class OrbitBrake:MonoBehaviour
    {
        public PressurePlate left,right;public RoomOrbit[] rooms;public bool released,stepOnRelease;bool applied;
        void FixedUpdate(){if(left&&right&&left.Pressed&&right.Pressed)released=true;if(!released||rooms==null)return;
            if(stepOnRelease){if(applied)return;applied=true;foreach(var r in rooms)if(r)r.RotateQuarter(-1);}
            else foreach(var r in rooms)if(r)r.running=true;}
    }
}
