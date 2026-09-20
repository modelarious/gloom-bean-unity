using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-120)]
    public sealed class OutlinePainter:MonoBehaviour
    {
        public TopologyRegion region;public Vector2 start,end;public float speed=2.5f;
        Rigidbody2D body;bool drawing;public bool Requested {get;private set;}
        void Awake(){body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;}
        public void Draw(bool value){Requested=value;drawing=true;}
        void FixedUpdate(){if(!drawing)return;var target=Requested?end:start;body.MovePosition(Vector2.MoveTowards(body.position,target,speed*Time.fixedDeltaTime));if(Vector2.Distance(body.position,target)<.03f){drawing=false;region.SetSupplement(Requested);}}
    }
}
