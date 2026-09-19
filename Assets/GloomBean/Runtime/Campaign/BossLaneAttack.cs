using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class BossLaneAttack:MonoBehaviour
    {
        public float delay=1.1f,floor;float age;bool hit;
        void Update(){age+=Time.deltaTime*LocalTime.Scale(transform.position,true);if(age<delay)return;if(!hit){hit=true;var s=GetComponent<SpriteRenderer>();s.color=new Color(.95f,.31f,.4f,.5f);foreach(var c in Physics2D.OverlapBoxAll(transform.position,new Vector2(2.4f,18),0,1<<Layers.Actor)){var a=c.GetComponent<ActorMotor>();if(a)a.Hit(new HitInfo(null,Vector2.right*(a.transform.position.x<transform.position.x?-1:1),1));}}if(age>delay+.25f)Destroy(gameObject);}
    }
}