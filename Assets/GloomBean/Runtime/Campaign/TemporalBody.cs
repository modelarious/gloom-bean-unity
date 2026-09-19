using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(200)]
    public sealed class TemporalBody:MonoBehaviour
    {
        Rigidbody2D body;MotionPlatform platform;CarryableEnemy enemy;float originalGravity,lastScale=1;public float scale=1;
        void Start(){body=GetComponent<Rigidbody2D>();platform=GetComponent<MotionPlatform>();enemy=GetComponent<CarryableEnemy>();if(body)originalGravity=body.gravityScale;}
        void FixedUpdate()
        {
            scale=LocalTime.Scale(transform.position);
            if(platform)platform.timeScale=scale;
            if(body&&body.bodyType==RigidbodyType2D.Dynamic)
            {
                body.gravityScale=originalGravity*scale*scale;
                if(enemy&&enemy.state==EnemyState.Patrol)body.linearVelocity=new Vector2(enemy.direction*enemy.patrolSpeed*scale,body.linearVelocity.y*scale/lastScale);
                else if(Mathf.Abs(scale-lastScale)>.001f)body.linearVelocity*=scale/lastScale;
            }
            lastScale=Mathf.Max(.1f,scale);
        }
        void OnDisable(){if(platform)platform.timeScale=1;if(body)body.gravityScale=originalGravity;}
    }
}