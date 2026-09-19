using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class BraceReceiver:MonoBehaviour
    {
        public Gate gate;public float holdRequired=.65f,held;public bool latched;
        void OnTriggerStay2D(Collider2D c){var b=c.GetComponent<LoadBearingBody>();if(b&&b.bracing){held+=Time.fixedDeltaTime;if(held>=holdRequired)latched=true;}}
        void FixedUpdate(){if(gate&&latched)gate.SetOpen(true);}
    }
}