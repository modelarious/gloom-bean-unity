using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Bounded viscous flow: this is a material field, not a teleport or a jump upgrade.
    public sealed class WaxChannel:MonoBehaviour
    {
        static readonly List<WaxChannel> active=new List<WaxChannel>();
        public WaxDrain drain; public Vector2 normal,diverted; public float acceleration=24;
        Collider2D region;
        void OnEnable(){region=GetComponent<Collider2D>();active.Add(this);}
        void OnDisable(){active.Remove(this);}
        public static bool Flow(ActorMotor actor,Vector2 control,float dt,ref Vector2 velocity)
        {
            foreach(var channel in active)
            {
                if(!channel||!channel.region||!channel.region.OverlapPoint(actor.Body.position))continue;
                Vector2 current=channel.drain&&channel.drain.Sealed?channel.diverted:channel.normal;
                Vector2 target=current+Vector2.right*control.x*2.5f;
                velocity=Vector2.MoveTowards(velocity,target,channel.acceleration*dt);
                return true;
            }
            return false;
        }
    }
}
