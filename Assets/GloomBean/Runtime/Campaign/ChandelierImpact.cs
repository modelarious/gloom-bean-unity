using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class ChandelierImpact:MonoBehaviour
    {
        public bool struck;public Collider2D receiver;public float minimumSpeed=2.5f;
        void OnCollisionEnter2D(Collision2D c){if(receiver&&c.collider==receiver&&c.relativeVelocity.magnitude>=minimumSpeed)struck=true;}
    }
}