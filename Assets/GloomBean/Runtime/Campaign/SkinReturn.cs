using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class SkinReturn:MonoBehaviour
    {
        public Vector2 home;public bool crawling;public float speed=1.8f;public bool Pinned {get;private set;}
        void FixedUpdate(){if(!crawling)return;Pinned=false;foreach(var c in Physics2D.OverlapCircleAll(transform.position,1.2f,1<<Layers.Prop))if(c.GetComponent<HuskBody>())Pinned=true;if(!Pinned)transform.position=Vector2.MoveTowards(transform.position,home,Time.fixedDeltaTime*speed);}
    }
}