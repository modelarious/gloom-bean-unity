using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(90)]
    [DisallowMultipleComponent]
    public sealed class HostPixelView:MonoBehaviour
    {
        ActorMotor actor;ActorView identity;HostController host;SpriteRenderer picture,badge;Transform imageRoot;
        public int VisiblePixels {get;private set;}
        void Awake(){actor=GetComponent<ActorMotor>();identity=GetComponent<ActorView>();host=GetComponent<HostController>();if(identity)identity.enabled=false;var old=GetComponent<HostView>();if(old)old.enabled=false;}
        void Start(){imageRoot=new GameObject("Original pixel Host artwork").transform;imageRoot.SetParent(transform,false);picture=imageRoot.gameObject.AddComponent<SpriteRenderer>();picture.sortingOrder=12;var g=new GameObject("Second tenant portrait");g.transform.SetParent(imageRoot,false);badge=g.AddComponent<SpriteRenderer>();badge.sortingOrder=18;}
        void LateUpdate()
        {
            if(!actor||!picture)return;var old=transform.Find("Silhouette");if(old)old.gameObject.SetActive(false);old=transform.Find("Possession silhouette");if(old)old.gameObject.SetActive(false);
            HostKind kind=host?host.Primary:HostKind.None;bool coffin=host&&host.Has(HostKind.Coffin);if(coffin)kind=HostKind.Coffin;
            bool liquid=host&&host.Form<WaxForm>()!=null&&host.Form<WaxForm>().Liquid;
            int pose=actor.State==MotionState.Hurt?4:actor.State==MotionState.Pound||actor.State==MotionState.SuperPound?3:actor.State==MotionState.Swim||actor.State==MotionState.SwimDash?2:actor.AttackPower>0?1:0;
            int frame=actor.Grounded?(int)(Time.time*Mathf.Max(2,Mathf.Abs(actor.Body.linearVelocity.x)*1.6f))%4:(int)(Time.time*4)%4;
            picture.sprite=HostPixelArt.Host(kind,identity&&identity.corrupted,frame,pose);picture.flipX=actor.Facing<0;
            float height=coffin?2:actor.Shape.size.y;float width=coffin?1.45f:Mathf.Max(.75f,height*1.15f);imageRoot.localScale=new Vector3(width*(coffin?1:1.08f),height*(coffin?1.23f:1.37f),1);
            imageRoot.localPosition=new Vector3(0,coffin?0:height*.055f,0);
            imageRoot.localRotation=Quaternion.Euler(0,0,actor.State==MotionState.Roll?-Time.time*660*actor.Facing:0);
            if(liquid){imageRoot.localPosition=Vector3.zero;}
            if(liquid)imageRoot.localScale=new Vector3(actor.Shape.size.x*1.2f,actor.Shape.size.y*1.23f,1);
            Color c=identity&&identity.ghost?Color.Lerp(Color.white,identity.tint,.35f):Color.white;c.a=(identity&&identity.ghost?.7f:1)*(actor.Invulnerability>0&&Mathf.Sin(Time.time*35)<-.3f?.3f:1);picture.color=c;
            bool paired=host&&host.Forms.Count==2;badge.enabled=paired;
            if(paired){HostKind other=host.Forms[0].Kind==kind?host.Forms[1].Kind:host.Forms[0].Kind;badge.sprite=HostPixelArt.Host(other,true,0);badge.transform.localPosition=new Vector3(-.45f,.35f,0);badge.transform.localScale=Vector3.one*.3f;badge.color=new Color(1,1,1,.85f);}
            VisiblePixels=picture.sprite.texture.width*picture.sprite.texture.height;
        }
    }
}
