using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    [DefaultExecutionOrder(0)]
    [RequireComponent(typeof(Rigidbody2D),typeof(CapsuleCollider2D))]
    public sealed class ActorMotor : MonoBehaviour, IHittable
    {
        public MovementTuning tuning;
        public IActorInput input;
        public IActorModifier modifier;
        public bool replica, manual, disabled, chargeDisabled, pickupDisabled;
        public int collisionMask=Layers.Solids;
        public float gravityFactor=1f, speedFactor=1f;
        public Rigidbody2D Body {get; private set;}
        public CapsuleCollider2D Shape {get; private set;}
        public MotionState State {get; private set;}
        public bool Grounded {get; private set;}
        public Vector2 GroundNormal {get; private set;}=Vector2.up;
        public Collider2D GroundCollider {get; private set;}
        public int Facing {get; private set;}=1;
        public int Health {get; private set;}
        public bool Crouched {get; private set;}
        public float Invulnerability {get; private set;}
        public WaterVolume Water {get; private set;}
        public CarryableEnemy Carried {get; private set;}
        public InputFrame LastInput {get; private set;}
        public event Action<InputFrame,float> Stepped;
        public event Action<int> LandedPound;
        public event Action Died;
        public float Height => Shape.size.y;
        public Vector2 Feet => Body.position + Shape.offset - Vector2.up*Height*.5f;
        public int AttackPower => State==MotionState.SuperPound?3:State==MotionState.Pound?2:
            State==MotionState.RunTackle||State==MotionState.AirTackle||State==MotionState.Roll?2:State==MotionState.Tackle?1:0;
        float coyote,buffer,runUp,attackTime,windup,poundTop,hurtTime,dashTime,dashCooldown;
        bool pounding,rolling,jumpWasHeld;
        Vector2 dashDirection;
        Vector2 standingSize=new Vector2(.88f,1.5f);
        Vector2 baseOffset;
        float groundIgnore;
        readonly RaycastHit2D[] groundHits=new RaycastHit2D[16];
        readonly Collider2D[] overlaps=new Collider2D[32];
        readonly HashSet<Collider2D> struck=new HashSet<Collider2D>();
        Collider2D previousSupport;
        Vector2 supportLocal;
        Vector2 previousSupportPoint;

        void Awake()
        {
            tuning=tuning?tuning:MovementTuning.Default();
            Body=GetComponent<Rigidbody2D>(); Shape=GetComponent<CapsuleCollider2D>();
            Body.gravityScale=0; Body.freezeRotation=true; Body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            Body.interpolation=RigidbodyInterpolation2D.Interpolate;
            Shape.size=standingSize; baseOffset=Shape.offset;
            var material=new PhysicsMaterial2D("Host frictionless"){friction=0,bounciness=0}; Shape.sharedMaterial=material;
            gameObject.layer=Layers.Actor; Health=tuning.maximumHealth;
        }
        void FixedUpdate() { if(!manual) Step(input?.Consume()??default,Time.fixedDeltaTime); }
        public void Step(InputFrame f,float dt)
        {
            if(disabled||State==MotionState.Dead||Time.timeScale==0) return;
            if(modifier is IActorInputFilter filter)f=filter.Filter(f,dt);
            LastInput=f; Invulnerability=Mathf.Max(0,Invulnerability-dt); hurtTime-=dt; groundIgnore-=dt;
            if(Mathf.Abs(f.move.x)>.1f && attackTime<=0 && !rolling)Facing=f.move.x>0?1:-1;
            CarryWithSupport(); ProbeGround();
            if(Grounded)coyote=tuning.coyote;else coyote-=dt;
            if(f.jump) buffer=tuning.jumpBuffer;else buffer-=dt;
            if(f.grab&&!pickupDisabled) GrabOrThrow(f.move);
            if(f.interact) Interact();
            if(hurtTime>0) { State=MotionState.Hurt; ApplyGravity(dt); Record(f,dt); return; }
            if(modifier!=null && modifier.BeforeMovement(this,f,dt))
            { State=MotionState.Custom; modifier.AfterMovement(this,f,dt); Record(f,dt); return; }
            if(Water && Water.Contains(Body.position)) Swim(f,dt); else { Water=null; GroundMove(f,dt); }
            if(AttackPower>0) AttackContact();
            modifier?.AfterMovement(this,f,dt);
            if(Carried) Carried.Follow(this);
            Record(f,dt);
        }
        void Record(InputFrame f,float dt)
        {
            jumpWasHeld=f.jumpHeld; Stepped?.Invoke(f,dt);
            if(Grounded && GroundCollider && (GroundCollider.gameObject.layer==Layers.Moving || (GroundCollider.attachedRigidbody && GroundCollider.attachedRigidbody.bodyType==RigidbodyType2D.Kinematic)))
            {
                previousSupport=GroundCollider;
                previousSupportPoint=Feet;
                supportLocal=GroundCollider.transform.InverseTransformPoint(previousSupportPoint);
            } else previousSupport=null;
        }
        void CarryWithSupport()
        {
            if(!previousSupport || groundIgnore>0)return;
            Vector2 now=previousSupport.transform.TransformPoint(supportLocal);
            var delta=now-previousSupportPoint;
            // Platform motion is applied once; actor velocity remains relative to support.
            if(delta.sqrMagnitude<9f) Body.position+=delta;
            previousSupportPoint=now;
        }
        public void ProbeGround()
        {
            Grounded=false; GroundCollider=null; GroundNormal=Vector2.up;
            if(groundIgnore>0||Body.linearVelocity.y>2.5f)return;
            var origin=Feet+Vector2.up*.09f;
            int n=Physics2D.BoxCastNonAlloc(origin,new Vector2(Shape.size.x*.78f,.06f),0,Vector2.down,groundHits,.18f,collisionMask);
            float closest=999;
            for(int i=0;i<n;i++)
            {
                var h=groundHits[i]; if(!h.collider||h.collider.isTrigger||h.collider==Shape||h.normal.y<.4f)continue;
                if(h.collider.GetComponent<OneWaySurface>()&&Feet.y<h.collider.bounds.max.y-.12f)continue;
                if(h.distance<closest){closest=h.distance;Grounded=true;GroundCollider=h.collider;GroundNormal=h.normal;}
            }
        }
        public bool SetSize(Vector2 size)
        {
            var feet=Feet;
            if(size.y>Shape.size.y || size.x>Shape.size.x)
            {
                int n=Physics2D.OverlapBoxNonAlloc(feet+Vector2.up*(size.y*.5f+.04f),size-new Vector2(.08f,.08f),0,overlaps,collisionMask);
                for(int i=0;i<n;i++) if(overlaps[i]&&!overlaps[i].isTrigger&&overlaps[i]!=Shape)return false;
            }
            Shape.direction=size.x>size.y?CapsuleDirection2D.Horizontal:CapsuleDirection2D.Vertical;
            Shape.size=size; Shape.offset=baseOffset;
            Body.position=feet-Shape.offset+Vector2.up*size.y*.5f;
            return true;
        }
        // Projection changes preserve the actor centre. Validate the exact destination
        // capsule before mutating either its standing size or current collision shape.
        public bool TrySetStandingSizeCentered(Vector2 size,int destinationMask)
        {
            if(size.x<.1f||size.y<.1f||float.IsNaN(size.x)||float.IsNaN(size.y)||float.IsInfinity(size.x)||float.IsInfinity(size.y))return false;
            var direction=size.x>size.y?CapsuleDirection2D.Horizontal:CapsuleDirection2D.Vertical;
            foreach(var c in Physics2D.OverlapCapsuleAll(Body.position+baseOffset,size-Vector2.one*.025f,direction,Body.rotation,destinationMask))
                if(c&&!c.isTrigger&&c.attachedRigidbody!=Body)return false;
            standingSize=size;Shape.direction=direction;Shape.size=size;Shape.offset=baseOffset;Crouched=false;
            return true;
        }
        public void SetStandingSize(Vector2 size)
        { standingSize=size; if(!Crouched)SetSize(size); }
        public void RestoreShape(){standingSize=new Vector2(.88f,1.5f); SetSize(standingSize);Crouched=false;}
        void SetCrouch(bool wants)
        {
            if(wants&&!Crouched){SetSize(new Vector2(standingSize.x,standingSize.y*.53f));Crouched=true;}
            else if(!wants&&Crouched&&SetSize(standingSize))Crouched=false;
        }
        void GroundMove(InputFrame f,float dt)
        {
            Vector2 v=Body.linearVelocity;
            if(pounding)
            {
                windup-=dt;
                if(windup>0){v=new Vector2(v.x*.85f,0);State=MotionState.PoundWindup;}
                else
                {
                    bool strong=poundTop-Feet.y>=tuning.superPoundDistance;
                    State=strong?MotionState.SuperPound:MotionState.Pound;v=new Vector2(0,-tuning.poundSpeed*(strong?1.2f:1));
                    if(Grounded){FinishPound(strong?3:2);v=Vector2.zero;}
                }
                Body.linearVelocity=v; return;
            }
            if((f.pound || f.attack&&f.move.y<-.5f)&&!Grounded)
            {
                pounding=true;windup=tuning.poundWindup;poundTop=Feet.y;rolling=false;attackTime=0;struck.Clear();
                SetCrouch(false);Body.linearVelocity=Vector2.zero;State=MotionState.PoundWindup;RuntimeEvents.Emit("pound-start");return;
            }
            bool onSlope=Grounded&&Mathf.Abs(GroundNormal.x)>.12f;
            if(Grounded&&f.move.y<-.5f && onSlope && !Carried){rolling=true;attackTime=0;struck.Clear();}
            if(rolling)
            {
                SetCrouch(true);
                var tangent=new Vector2(GroundNormal.y,-GroundNormal.x);
                float along=Vector2.Dot(v,tangent);
                if(Grounded)along+=Vector2.Dot(Vector2.down*tuning.gravity,tangent)*dt;
                along=Mathf.MoveTowards(along,0,tuning.rollFriction*dt);
                along=Mathf.Clamp(along,-tuning.rollMax,tuning.rollMax);
                if(Grounded){v=tangent*along-GroundNormal*1.5f;} else v.y=Mathf.Max(-tuning.terminalSpeed,v.y-tuning.gravity*dt);
                if(buffer>0&&coyote>0){v.y=tuning.jumpSpeed*.82f;buffer=coyote=0;groundIgnore=.15f;}
                if(Mathf.Abs(v.x)<1.8f&&Grounded&&!onSlope){rolling=false;SetCrouch(f.move.y<-.5f);}
                State=MotionState.Roll; if(Mathf.Abs(v.x)>.2f)Facing=v.x>0?1:-1;
                Body.linearVelocity=v;return;
            }
            SetCrouch(Grounded&&f.move.y<-.5f);
            if(f.run&&Mathf.Abs(v.x)>tuning.walkSpeed*.8f&&Mathf.Abs(f.move.x)>.5f)runUp+=dt;else if(Grounded)runUp=0;
            if(f.attack&&!chargeDisabled&&!Crouched)
            {
                bool running=runUp>=tuning.runUp; attackTime=running? .65f:tuning.tackleDuration;
                State=running?(Grounded?MotionState.RunTackle:MotionState.AirTackle):MotionState.Tackle;
                struck.Clear();RuntimeEvents.Emit("tackle",State.ToString());
            }
            if(attackTime>0)
            {
                attackTime-=dt;
                bool running=State==MotionState.RunTackle||State==MotionState.AirTackle;
                v.x=Facing*(running?tuning.runTackleSpeed:tuning.tackleSpeed);
                if(!Grounded&&running)State=MotionState.AirTackle;
            }
            else
            {
                float beltSpeed=0;
                if(Grounded&&GroundCollider){var belt=GroundCollider.GetComponent<Conveyor>();if(belt)beltSpeed=belt.speed;}
                // A belt sets a support-relative target. Adding its speed every tick causes unbounded acceleration.
                float target=f.move.x*(Crouched?tuning.crawlSpeed:f.run?tuning.runSpeed:tuning.walkSpeed)*speedFactor*(Carried? .8f:1f)+beltSpeed;
                v.x=Mathf.MoveTowards(v.x,target,(Grounded?(Mathf.Abs(target)<.1f?tuning.braking:tuning.acceleration):tuning.airAcceleration)*dt);
                State=!Grounded?MotionState.Air:Crouched?(Mathf.Abs(f.move.x)>.1f?MotionState.Crawl:MotionState.Crouch):Mathf.Abs(v.x)<.2f?MotionState.Idle:f.run?MotionState.Run:MotionState.Walk;
            }
            if(buffer>0&&coyote>0&&!Crouched)
            {
                v.y=tuning.jumpSpeed*(Carried? .92f:1);buffer=coyote=0;groundIgnore=.16f;Grounded=false;
                if(State==MotionState.RunTackle)State=MotionState.AirTackle;
                RuntimeEvents.Emit("jump");
            }
            else if(Grounded&&v.y<=2.5f)
            {
                // Match the slope tangent, avoiding the uphill-stall of horizontal-only controllers.
                v.y=-GroundNormal.x/Mathf.Max(.4f,GroundNormal.y)*v.x-1.0f;
                
            }
            else v.y=Mathf.Max(-tuning.terminalSpeed,v.y-tuning.gravity*gravityFactor*dt);
            if(!f.jumpHeld&&jumpWasHeld&&v.y>0)v.y*=tuning.jumpCut;
            Body.linearVelocity=v;
        }
        void Swim(InputFrame f,float dt)
        {
            pounding=rolling=false; attackTime=0;SetCrouch(false);dashCooldown-=dt;dashTime-=dt;
            if((f.attack||f.run&&f.jump)&&dashCooldown<=0)
            {
                dashDirection=f.move.sqrMagnitude>.1f?f.move.normalized:new Vector2(Facing,0);
                dashTime=tuning.swimDashTime;dashCooldown=tuning.swimDashCooldown+tuning.swimDashTime;RuntimeEvents.Emit("swim-dash");
            }
            Vector2 target=f.move*tuning.swimSpeed+Water.current;
            if(dashTime>0){State=MotionState.SwimDash;Body.linearVelocity=dashDirection*tuning.swimDashSpeed+Water.current;}
            else {State=MotionState.Swim;Body.linearVelocity=Vector2.MoveTowards(Body.linearVelocity,target,tuning.swimAcceleration*dt);}
            if(f.jump&&Body.position.y>=Water.Surface-.6f){Body.linearVelocity=new Vector2(Body.linearVelocity.x,tuning.jumpSpeed*.83f);Water=null;groundIgnore=.15f;}
        }
        void ApplyGravity(float dt){var v=Body.linearVelocity;v.y=Mathf.Max(-tuning.terminalSpeed,v.y-tuning.gravity*dt);Body.linearVelocity=v;}
        void FinishPound(int power)
        {
            pounding=false;State=MotionState.Idle;RuntimeEvents.Emit("pound-impact",power.ToString());LandedPound?.Invoke(power);
            int n=Physics2D.OverlapBoxNonAlloc(Feet+Vector2.down*.15f,new Vector2(power==3?4f:1.5f,.7f),0,overlaps);
            for(int i=0;i<n;i++) if(overlaps[i]&&overlaps[i]!=Shape)RuntimeEvents.Hittable(overlaps[i])?.Hit(new HitInfo(this,Vector2.down,power,true));
            StageSession.Current?.Camera?.Kick(power==3? .18f:.08f);
        }
        void AttackContact()
        {
            Vector2 center=pounding?Feet+Vector2.down*.12f:Body.position+Vector2.right*Facing*.58f;
            int n=Physics2D.OverlapBoxNonAlloc(center,pounding?new Vector2(.8f,.45f):new Vector2(.65f,Height*.72f),0,overlaps);
            for(int i=0;i<n;i++)
            {
                var c=overlaps[i]; if(!c||c==Shape||c.isTrigger||c.GetComponentInParent<ActorMotor>())continue;
                var h=RuntimeEvents.Hittable(c); if(h==null||!struck.Add(c))continue;
                h.Hit(new HitInfo(this,pounding?Vector2.down:Vector2.right*Facing,AttackPower,pounding));
            }
        }
        public void GrabOrThrow(Vector2 aim)
        {
            if(Carried)
            {
                Vector2 v=aim.y>.5f?new Vector2(Facing*4,tuning.throwSpeed):aim.y<-.5f?new Vector2(Facing*3,-tuning.throwSpeed):new Vector2(Facing*tuning.throwSpeed,tuning.throwLift);
                var e=Carried;Carried=null;e.Throw(this,v);RuntimeEvents.Emit("throw");return;
            }
            int n=Physics2D.OverlapCircleNonAlloc(Body.position+Vector2.right*Facing*.55f,1.1f,overlaps,1<<Layers.Enemy);
            CarryableEnemy best=null;float dist=999;
            for(int i=0;i<n;i++){var e=overlaps[i].GetComponent<CarryableEnemy>();if(e&&e.CanPickUp){float d=((Vector2)e.transform.position-Body.position).sqrMagnitude;if(d<dist){best=e;dist=d;}}}
            if(best){Carried=best;best.PickUp(this);RuntimeEvents.Emit("pickup-enemy");}
        }
        public void DropCarried(){if(Carried){var e=Carried;Carried=null;e.Throw(this,new Vector2(Facing*2,1));}}
        void Interact()
        {
            int n=Physics2D.OverlapCircleNonAlloc(Body.position,1.6f,overlaps);
            IInteractable best=null;float dist=999;
            for(int i=0;i<n;i++)foreach(var b in overlaps[i].GetComponents<MonoBehaviour>())if(b is IInteractable v){float d=((Vector2)b.transform.position-Body.position).sqrMagnitude;if(d<dist){dist=d;best=v;}}
            best?.Interact(this);
        }
        // A temporal replica starts from the actual movement state, not a stationary approximation.
        // World geometry still acts independently on it after the replay begins.
        public void CopyLocomotionFrom(ActorMotor original)
        {
            if(!replica)throw new InvalidOperationException("Only a replica may copy initial locomotion.");
            tuning=original.tuning;Body.linearVelocity=original.Body.linearVelocity;Facing=original.Facing;
            // An echo of a small core must not become a full-size, full-mass substitute.
            standingSize=original.standingSize;Shape.size=original.Shape.size;Shape.offset=original.Shape.offset;
            Crouched=original.Crouched;Body.mass=original.Body.mass;chargeDisabled=original.chargeDisabled;
            collisionMask=original.collisionMask;Shape.excludeLayers=original.Shape.excludeLayers;Shape.includeLayers=original.Shape.includeLayers;
            coyote=original.coyote;buffer=original.buffer;runUp=original.runUp;
            groundIgnore=original.groundIgnore;jumpWasHeld=original.jumpWasHeld;
            State=original.State;Water=original.Water;gravityFactor=original.gravityFactor;speedFactor=original.speedFactor;
        }
        public void EnterWater(WaterVolume water){Water=water;}
        public void AddVelocity(Vector2 impulse){Body.linearVelocity+=impulse;}
        public void SetFacing(int sign){Facing=sign<0?-1:1;}
        public void SetCustomState(){State=MotionState.Custom;pounding=rolling=false;attackTime=0;}
        public void CancelActions(){pounding=rolling=false;attackTime=0;dashTime=0;struck.Clear();SetCrouch(false);}
        public void Heal(int count){Health=Mathf.Min(tuning.maximumHealth,Health+count);}
        public void Hit(HitInfo h)
        {
            if(Invulnerability>0||State==MotionState.Dead)return;
            Health=Mathf.Max(0,Health-Mathf.Max(1,h.power));Invulnerability=tuning.hurtInvulnerability;hurtTime=.25f;
            CancelActions();DropCarried();Body.linearVelocity=new Vector2(h.direction.x*7,7);State=MotionState.Hurt;
            RuntimeEvents.Emit("damage",Health.ToString());
            if(Health<=0){State=MotionState.Dead;Died?.Invoke();}
        }
        public void Revive(Vector2 point){Health=tuning.maximumHealth;Invulnerability=1;Reposition(point);}
        // Authored arena transitions reset motion, not the player's accumulated damage.
        public void Reposition(Vector2 point){hurtTime=0;disabled=false;State=MotionState.Idle;CancelActions();Body.position=point;Body.linearVelocity=Vector2.zero;previousSupport=null;coyote=buffer=runUp=0;groundIgnore=0;}
        void OnCollisionEnter2D(Collision2D c)
        {
            foreach(var contact in c.contacts)
            {
                if(contact.normal.y>.4f&&pounding&&windup<=0){int p=poundTop-Feet.y>=tuning.superPoundDistance?3:2;FinishPound(p);}
                if(Mathf.Abs(contact.normal.x)>.65f&&(attackTime>0||rolling))
                {
                    RuntimeEvents.Hittable(c.collider)?.Hit(new HitInfo(this,Vector2.right*Facing,AttackPower));
                    if(c.collider.gameObject.layer==Layers.Terrain||c.collider.gameObject.layer==Layers.Moving){attackTime=0;rolling=false;Body.linearVelocity=new Vector2(-Facing*2,2);}
                }
            }
        }
    }
}
