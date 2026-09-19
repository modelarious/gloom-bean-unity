using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class CounterweightPair:MonoBehaviour
    {
        public MotionPlatform left,right;float t=.5f;
        float Mass(MotionPlatform p){float m=0;foreach(var c in Physics2D.OverlapBoxAll((Vector2)p.transform.position+Vector2.up*.65f,new Vector2(4,1.3f),0,(1<<Layers.Actor)|(1<<Layers.Prop)|(1<<Layers.Enemy)))if(c.attachedRigidbody&&!c.isTrigger)m+=c.attachedRigidbody.mass;return m;}
        void FixedUpdate(){if(!left||!right)return;left.paused=right.paused=true;t=Mathf.Clamp01(t+(Mass(left)-Mass(right))*Time.fixedDeltaTime*.15f);left.GetComponent<Rigidbody2D>().position=Vector2.Lerp(left.end,left.origin,t);right.GetComponent<Rigidbody2D>().position=Vector2.Lerp(right.origin,right.end,t);}
    }
}