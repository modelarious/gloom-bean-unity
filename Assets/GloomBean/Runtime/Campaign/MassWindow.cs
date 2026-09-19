using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class MassWindow:MonoBehaviour
    {
        public PressurePlate plate;public Gate gate;public float minimum=.6f,maximum=.85f,holdSeconds;float held;
        public bool Valid=>plate&&plate.Mass>=minimum&&plate.Mass<=maximum;
        void FixedUpdate(){held=Valid?holdSeconds:Mathf.Max(0,held-Time.fixedDeltaTime);if(gate)gate.SetOpen(Valid||held>0||gate.latched);}
    }
}
