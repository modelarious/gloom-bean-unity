using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A seated keystone unlocks its hoist. The roof physically moves rather than vanishing.
    public sealed class BracedLiftRelease:MonoBehaviour
    {
        public BraceReceiver receiver;public MotionPlatform motion;public Vector2 destination;public float speed=2;
        void FixedUpdate()
        {
            if(!receiver||!receiver.latched)return;
            if(motion)motion.enabled=false;
            var body=GetComponent<Rigidbody2D>();
            if(body)body.position=Vector2.MoveTowards(body.position,destination,speed*Time.fixedDeltaTime);
        }
    }
}
