using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class RipeningFruit:MonoBehaviour,IHittable
    {
        public float ripeAfter=9,rotAfter=20;public float age;public Rigidbody2D body;public bool fallen;public Transform stem;Vector2 attached;float fallenAge;bool burst;
        void Start(){body=GetComponent<Rigidbody2D>();attached=transform.position;if(StageSession.Current)StageSession.Current.Turned+=Ripen;}
        void OnDestroy(){if(StageSession.Current)StageSession.Current.Turned-=Ripen;}
        public void Ripen(){age=ripeAfter+.1f;Drop();}
        public void Drop(){if(fallen)return;fallen=true;body.bodyType=RigidbodyType2D.Dynamic;body.gravityScale=3.4f;}
        public void Hit(HitInfo h){if(fallen){Burst();return;}Drop();body.AddForce(h.direction*4,ForceMode2D.Impulse);}
        void Burst(){if(burst)return;burst=true;GetComponent<Collider2D>().enabled=false;
            for(int i=0;i<2;i++){var bug=PrimitiveArt.Shape("Fruit insect",transform.parent,(Vector2)transform.position,Vector2.one,new Color(.46f,.55f,.23f),PrimitiveArt.Icon.Eye,6);bug.layer=Layers.Prop;
                var view=bug.GetComponent<SpriteRenderer>();view.drawMode=SpriteDrawMode.Sliced;view.size=new Vector2(.35f,.25f);bug.AddComponent<BoxCollider2D>().size=view.size;var rb=bug.AddComponent<Rigidbody2D>();rb.mass=.08f;rb.gravityScale=3.4f;rb.AddForce(new Vector2(i==0?-2:2,3)*.08f,ForceMode2D.Impulse);Destroy(bug,6);}
            RuntimeEvents.Emit("fruit-burst",name);Destroy(gameObject);
        }
        void Update(){if(fallen){fallenAge+=Time.deltaTime;if(fallenAge>12){Burst();return;}}age+=Time.deltaTime*LocalTime.Scale(transform.position);body.mass=age>=ripeAfter?3:Mathf.Lerp(.2f,3,age/ripeAfter);var s=GetComponent<SpriteRenderer>();if(s)s.color=age>rotAfter?new Color(.28f,.31f,.15f):age>ripeAfter?new Color(.87f,.56f,.25f):new Color(.46f,.67f,.29f);if(age>rotAfter&&!fallen)Drop();}
    }
}