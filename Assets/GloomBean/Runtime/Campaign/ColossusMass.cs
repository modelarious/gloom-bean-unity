using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // This boss is a real dynamic mass. Its displayed altitude comes from its pose,
    // not an elapsed-time health substitute; ordinary attacks never subtract HP.
    public sealed class ColossusMass:MonoBehaviour
    {
        public Rigidbody2D Body {get;private set;}public float bottom=-6,initialAltitude;public string LastContact="none";float originalGravity,lastScale=1;
        public float Altitude=>Mathf.Max(0,Body.position.y-bottom);
        public bool AtBottom=>Body.position.x>43&&Body.position.y<bottom+.5f&&Mathf.Abs(Body.linearVelocity.y)<.6f;
        void Awake(){Body=GetComponent<Rigidbody2D>();originalGravity=Body.gravityScale;initialAltitude=Body.position.y-bottom;}
        void FixedUpdate(){float scale=LocalTime.Scale(Body.position,true);Body.gravityScale=originalGravity*scale*scale;
            if(Mathf.Abs(scale-lastScale)>.001f)Body.linearVelocity*=scale/lastScale;lastScale=scale;}
        void OnCollisionEnter2D(Collision2D hit){LastContact=hit.collider.name;var actor=hit.collider.GetComponent<ActorMotor>();if(!actor)return;
            var brace=actor.GetComponent<LoadBearingBody>();if(!brace||!brace.bracing)actor.Hit(new HitInfo(null,(actor.Body.position-Body.position).normalized,2));}
        void OnGUI(){if(GameRoot.Instance&&GameRoot.Instance.GameplayHud!=null)return;if(!StageSession.Current||StageSession.Current.Phase==RunPhase.Cleared)return;GUI.Label(new Rect(28,168,540,32),"THE WEIGHT OF EVERYONE  /  ALTITUDE  "+Altitude.ToString("0.0")+" m");}
    }
}
