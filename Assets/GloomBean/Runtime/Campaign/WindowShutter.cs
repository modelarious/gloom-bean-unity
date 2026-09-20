using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-125)]
    public sealed class WindowShutter:MonoBehaviour
    {
        public Vector2 park,lowered,closed;public float speed=3;public bool IsClosed {get;private set;}
        Rigidbody2D body;bool requested;int leg=2;
        void Awake(){body=GetComponent<Rigidbody2D>();if(!body)body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.interpolation=RigidbodyInterpolation2D.Interpolate;}
        public void SetClosed(bool value){requested=value;leg=0;RuntimeEvents.Emit("window-shutter",value?"closing":"opening");}
        void FixedUpdate(){if(leg>=2)return;Vector2 target=leg==0?lowered:requested?closed:park;body.MovePosition(Vector2.MoveTowards(body.position,target,speed*Time.fixedDeltaTime));if(Vector2.Distance(body.position,target)<.025f){leg++;if(leg>=2)IsClosed=requested;}}
    }
}
