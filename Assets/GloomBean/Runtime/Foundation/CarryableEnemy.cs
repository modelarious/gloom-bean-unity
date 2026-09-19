using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    [RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
    public sealed class CarryableEnemy : MonoBehaviour, IHittable
    {
        public float patrolSpeed=1.8f, recoverSeconds=4f;
        public bool avoidLedges, armored;
        public int direction=1;
        public EnemyState state;
        public Rigidbody2D Body {get;private set;}
        public bool CanPickUp=>state==EnemyState.Stunned;
        ActorMotor carrier,thrower;
        Collider2D shape;
        float timer,grace,collisionAge;
        bool ownerIgnored;
        void Awake()
        {
            gameObject.layer=Layers.Enemy;Body=GetComponent<Rigidbody2D>();shape=GetComponent<Collider2D>();
            Body.freezeRotation=true;Body.gravityScale=3.4f;Body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        }
        void FixedUpdate()
        {
            if(state==EnemyState.Dead)return;
            collisionAge+=Time.fixedDeltaTime;
            if(state==EnemyState.Carried){if(carrier)Follow(carrier);else RestoreDrop();return;}
            if(grace>0)grace-=Time.fixedDeltaTime;
            else if(ownerIgnored&&thrower){Physics2D.IgnoreCollision(shape,thrower.Shape,false);ownerIgnored=false;}
            if(transform.position.y<-100){Die();return;}
            if(state==EnemyState.Patrol)
            {
                var p=Body.position;
                var wall=Physics2D.Raycast(p+Vector2.right*direction*.51f,Vector2.right*direction,.28f,Layers.Solids);
                var ground=Physics2D.Raycast(p+new Vector2(direction*.65f,-.2f),Vector2.down,1.2f,Layers.Solids);
                if(wall||avoidLedges&&!ground){direction=-direction;RuntimeEvents.Emit("patrol-turn");}
                var v=Body.linearVelocity;v.x=direction*patrolSpeed;Body.linearVelocity=v;
            }
            else if(state==EnemyState.Stunned)
            {
                var v=Body.linearVelocity;v.x=Mathf.MoveTowards(v.x,0,Time.fixedDeltaTime*8);Body.linearVelocity=v;
                timer-=Time.fixedDeltaTime;if(timer<=0){state=EnemyState.Patrol;RuntimeEvents.Emit("enemy-recover");}
            }
            else if(state==EnemyState.Thrown&&collisionAge>8)Stun();
        }
        public void Hit(HitInfo hit)
        {
            if(state==EnemyState.Dead||state==EnemyState.Carried)return;
            if(hit.power>=2&&!armored||hit.power>=3){Die();return;}
            Stun();Body.linearVelocity=new Vector2(hit.direction.x*4,hit.downward?2:4);
        }
        public void Stun(){state=EnemyState.Stunned;timer=recoverSeconds;RuntimeEvents.Emit("enemy-stun");}
        public void PickUp(ActorMotor actor)
        {
            carrier=actor;state=EnemyState.Carried;Body.linearVelocity=Vector2.zero;Body.simulated=false;shape.enabled=false;Follow(actor);
        }
        public void Follow(ActorMotor actor){transform.position=actor.Body.position+new Vector2(actor.Facing*.32f,actor.Height*.5f+.53f);}
        public void Throw(ActorMotor actor,Vector2 velocity)
        {
            carrier=null;thrower=actor;state=EnemyState.Thrown;Body.simulated=true;shape.enabled=true;
            Body.position=actor.Body.position+new Vector2(actor.Facing*.9f,.45f);Body.linearVelocity=velocity;
            Physics2D.IgnoreCollision(shape,actor.Shape,true);ownerIgnored=true;grace=.28f;collisionAge=0;
        }
        void RestoreDrop(){state=EnemyState.Stunned;timer=recoverSeconds;Body.simulated=true;shape.enabled=true;}
        void OnCollisionEnter2D(Collision2D c)
        {
            if(state==EnemyState.Dead||state==EnemyState.Carried)return;
            var other=c.collider.GetComponent<CarryableEnemy>();
            if(state==EnemyState.Thrown)
            {
                if(other){other.Hit(new HitInfo(thrower,Body.linearVelocity.normalized,3));RuntimeEvents.Emit("thrown-enemy-hit");Stun();return;}
                RuntimeEvents.Hittable(c.collider)?.Hit(new HitInfo(thrower,c.relativeVelocity.normalized,2,false,true));
                if(collisionAge>.1f&&c.collider.gameObject.layer==Layers.Terrain)Stun();
            }
            var actor=c.collider.GetComponent<ActorMotor>();
            if(actor)
            {
                if(actor.AttackPower>0){Hit(new HitInfo(actor,new Vector2(actor.Facing,0),actor.AttackPower));return;}
                if(actor.Feet.y>Body.position.y+.15f&&actor.Body.linearVelocity.y<=2)
                { Stun();var v=actor.Body.linearVelocity;v.y=6;actor.Body.linearVelocity=v;return; }
                if(state==EnemyState.Patrol)actor.Hit(new HitInfo(null,new Vector2(Mathf.Sign(actor.Body.position.x-Body.position.x),0),1));
            }
        }
        public void Die(){state=EnemyState.Dead;shape.enabled=false;Body.simulated=false;RuntimeEvents.Emit("enemy-defeated");Destroy(gameObject);}
    }
}
