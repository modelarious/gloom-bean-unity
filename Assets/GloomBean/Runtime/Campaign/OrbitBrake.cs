using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class OrbitBrake:MonoBehaviour
    {
        public PressurePlate left,right;public RoomOrbit[] rooms;public bool released,stepOnRelease,requiresCall;public float holdRequired=.35f;
        bool applied,called;float held;
        public void Call(){if(released&&left&&right&&left.Pressed&&right.Pressed){called=true;RuntimeEvents.Emit("hotel-lift-call","both depth plates held");}}
        void FixedUpdate(){bool loaded=left&&right&&left.Pressed&&right.Pressed;held=loaded?held+Time.fixedDeltaTime:0;if(held>=holdRequired)released=true;
            if(!released||rooms==null||requiresCall&&!called)return;
            if(stepOnRelease){if(applied)return;applied=true;foreach(var r in rooms)if(r)r.RotateQuarter(-1);}
            else foreach(var r in rooms)if(r)r.running=true;}
    }
}
