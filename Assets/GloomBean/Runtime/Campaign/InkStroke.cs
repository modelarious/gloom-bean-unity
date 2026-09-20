using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class InkStroke:MonoBehaviour
    {
        public float length;public float age;public float harden=1,dry=8;EdgeCollider2D edge;LineRenderer line;public bool Solid=>edge&&edge.enabled;public Vector2 Midpoint {get;private set;}
        public void Initialize(Vector2 a,Vector2 b)
        {
            transform.position=a;Midpoint=(a+b)*.5f;gameObject.layer=Layers.Moving;length=Vector2.Distance(a,b);
            edge=gameObject.AddComponent<EdgeCollider2D>();edge.points=new[]{Vector2.zero,b-a};edge.edgeRadius=.035f;edge.enabled=false;
            var effect=gameObject.AddComponent<PlatformEffector2D>();effect.useOneWay=true;effect.surfaceArc=150;edge.usedByEffector=true;
            line=PrimitiveArt.Line("Visible stroke",transform,a,b,.07f,new Color(.46f,.5f,.8f,.5f),5);
        }
        void FixedUpdate(){age+=Time.fixedDeltaTime;if(edge)edge.enabled=age>=harden;if(line){Color c=age>=harden?new Color(.8f,.75f,.95f):new Color(.43f,.48f,.75f,.5f);line.startColor=line.endColor=c;}if(age>=dry)Erase();}
        public void Erase(){if(edge)edge.enabled=false;Destroy(gameObject);}
    }
}