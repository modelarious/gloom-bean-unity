using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class AirJet:MonoBehaviour
    {
        public float until;public bool always;public Vector2 force=new Vector2(0,30);
        void OnTriggerStay2D(Collider2D c){if(!always&&Time.time>until)return;var rb=c.attachedRigidbody;if(rb&&rb.bodyType==RigidbodyType2D.Dynamic)rb.AddForce(force*rb.mass,ForceMode2D.Force);}
    }
}