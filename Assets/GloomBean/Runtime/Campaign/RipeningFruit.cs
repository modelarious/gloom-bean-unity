using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class RipeningFruit:MonoBehaviour,IHittable
    {
        public float ripeAfter=9,rotAfter=20;public float age;public Rigidbody2D body;public bool fallen;public Transform stem;Vector2 attached;
        void Start(){body=GetComponent<Rigidbody2D>();attached=transform.position;if(StageSession.Current)StageSession.Current.Turned+=Ripen;}
        void OnDestroy(){if(StageSession.Current)StageSession.Current.Turned-=Ripen;}
        public void Ripen(){age=ripeAfter+.1f;Drop();}
        public void Drop(){if(fallen)return;fallen=true;body.bodyType=RigidbodyType2D.Dynamic;body.gravityScale=3.4f;}
        public void Hit(HitInfo h){Drop();body.AddForce(h.direction*4,ForceMode2D.Impulse);}
        void Update(){age+=Time.deltaTime*LocalTime.Scale(transform.position);body.mass=age>=ripeAfter?3:Mathf.Lerp(.2f,3,age/ripeAfter);var s=GetComponent<SpriteRenderer>();if(s)s.color=age>rotAfter?new Color(.28f,.31f,.15f):age>ripeAfter?new Color(.87f,.56f,.25f):new Color(.46f,.67f,.29f);if(age>rotAfter&&!fallen)Drop();}
    }
}