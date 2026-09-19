using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-120)]
    public sealed class RoomOrbit:MonoBehaviour
    {
        public Vector2 center;public Vector2 radius=new Vector2(10,7),roomSize=new Vector2(10,6);public float phase,speed=.13f;public bool running;Vector2 previous;
        void Start(){previous=transform.position;}
        void FixedUpdate()
        {
            if(!running){previous=transform.position;return;}
            phase+=speed*Time.fixedDeltaTime;Vector2 next=center+new Vector2(Mathf.Cos(phase)*radius.x,Mathf.Sin(phase)*radius.y);Vector2 delta=next-previous;
            // Grounded actors are carried by the foundation's contact-point support code.
            foreach(var c in Physics2D.OverlapBoxAll(previous,roomSize,0,(1<<Layers.Prop)|(1<<Layers.Enemy)))
            {var rb=c.attachedRigidbody;if(rb&&rb.bodyType==RigidbodyType2D.Dynamic)rb.position+=delta;}
            transform.position=next;previous=next;
        }
    }
}