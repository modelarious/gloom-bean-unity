using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class ChandelierImpact:MonoBehaviour
    {
        public bool struck;public Collider2D receiver;public float minimumSpeed=2.5f;public string lastContact="none";public float contactSpeed;
        void OnCollisionEnter2D(Collision2D c){lastContact=c.collider.name;contactSpeed=c.relativeVelocity.magnitude;if(receiver&&c.collider==receiver&&c.relativeVelocity.magnitude>=minimumSpeed)struck=true;}
    }
}