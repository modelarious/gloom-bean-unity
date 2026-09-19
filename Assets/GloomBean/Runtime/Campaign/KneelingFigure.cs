using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class KneelingFigure:MonoBehaviour
    {
        public enum Pose { Warning,Falling,Kneeling,Sinking }
        public Pose pose;public float height=13,floor=0,speed=10,hold=3,delay=1.5f;public bool repeating=true;public float clock;public Rigidbody2D body;BoxCollider2D shape;Vector2 top;SpriteRenderer view;
        void Start(){top=transform.position;body=GetComponent<Rigidbody2D>();shape=GetComponent<BoxCollider2D>();view=GetComponent<SpriteRenderer>();body.bodyType=RigidbodyType2D.Kinematic;SetPose(Pose.Warning);}
        void SetPose(Pose p){pose=p;clock=0;if(shape){shape.enabled=p!=Pose.Warning;shape.size=p==Pose.Kneeling?new Vector2(3,.5f):new Vector2(.9f,2.4f);}if(view){view.drawMode=SpriteDrawMode.Sliced;view.size=shape.size;view.color=p==Pose.Warning?new Color(.9f,.62f,.47f,.35f):new Color(.61f,.62f,.68f);}}
        void FixedUpdate()
        {
            float dt=Time.fixedDeltaTime*LocalTime.Scale(transform.position);clock+=dt;
            if(pose==Pose.Warning){if(clock>=delay)SetPose(Pose.Falling);}
            else if(pose==Pose.Falling){body.MovePosition(new Vector2(top.x,Mathf.Max(floor+.25f,body.position.y-speed*dt)));if(body.position.y<=floor+.26f)SetPose(Pose.Kneeling);}
            else if(pose==Pose.Kneeling){if(clock>=hold)SetPose(Pose.Sinking);}
            else {body.MovePosition(body.position+Vector2.down*dt*2);if(clock>2.5f){if(repeating){body.position=top;SetPose(Pose.Warning);}else Destroy(gameObject);}}
        }
        void OnCollisionEnter2D(Collision2D c){if(pose!=Pose.Falling)return;var a=c.collider.GetComponent<ActorMotor>();if(a)a.Hit(new HitInfo(null,Vector2.right*(a.transform.position.x<transform.position.x?-1:1),1));}
    }
}