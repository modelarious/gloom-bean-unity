using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class StolenAttack:MonoBehaviour
    {
        public HostEmbargo embargo;public AtlasBoss boss;float clock;int shot;public int Shots=>shot;
        void Update()
        {
            if(!embargo||!embargo.active||!boss||boss.defeated)return;clock+=Time.deltaTime;if(clock<5)return;clock=0;var actor=StageSession.Current.player;if(!actor)return;shot++;
            if(embargo.stolen==HostKind.Echo){SpawnLane(actor.Body.position.x,1.2f);SpawnLane(actor.Body.position.x,3.2f);}
            else if(embargo.stolen==HostKind.Wax){var g=PrimitiveArt.Shape("Stolen wax obstruction",transform.parent,new Vector2(actor.Body.position.x+3,1),Vector2.one,new Color(.88f,.78f,.4f),PrimitiveArt.Icon.Round);g.layer=Layers.Prop;var c=g.AddComponent<BoxCollider2D>();c.size=new Vector2(1.5f,2);var sr=g.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Sliced;sr.size=c.size;g.AddComponent<Rigidbody2D>().mass=.5f;Destroy(g,8);}
            else{var g=PrimitiveArt.Shape("Stolen perspective blade",transform.parent,new Vector2(actor.Body.position.x+7,actor.Body.position.y),Vector2.one,new Color(.72f,.56f,.9f),PrimitiveArt.Icon.Diamond);g.layer=17+shot%3;var c=g.AddComponent<BoxCollider2D>();c.size=Vector2.one*1.2f;var rb=g.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.linearVelocity=Vector2.left*6;g.AddComponent<HostileProjectile>();Destroy(g,5);}
        }
        void SpawnLane(float x,float delay){var g=PrimitiveArt.Shape("Delayed stolen attack",transform.parent,new Vector2(x,5),new Vector2(2.4f,18),new Color(.8f,.46f,.7f,.25f),PrimitiveArt.Icon.Stripe);g.AddComponent<BossLaneAttack>().delay=delay;}
    }
}