using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class SurveyorCounterstroke:MonoBehaviour
    {
        public AtlasBoss boss;public ActorMotor actor;public float axis=22;float ready;
        void Start(){if(actor)actor.Stepped+=Input;}
        void OnDestroy(){if(actor)actor.Stepped-=Input;}
        void Input(InputFrame frame,float dt){if(!boss||boss.phase!=0||!frame.attack||Time.time<ready)return;ready=Time.time+1;
            float x=actor.Body.position.x;foreach(float at in new[]{x,2*axis-x}){
                var g=PrimitiveArt.Shape("The Surveyor copies your attack",transform.parent,new Vector2(at,7),Vector2.one,new Color(.53f,.71f,.93f,.2f),PrimitiveArt.Icon.Stripe,2);
                var sr=g.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Sliced;sr.size=new Vector2(2.4f,18);var attack=g.AddComponent<BossLaneAttack>();attack.delay=1.5f;attack.floor=-1;}}
    }
}
