using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class EchoForm : HostForm
    {
        struct Recorded {public float time;public InputFrame frame;public Vector2 p,v;}
        public bool Leading;
        public ActorMotor Echo {get;private set;} readonly Queue<Recorded> history=new Queue<Recorded>();float clock;bool alive;
        public override HostKind Kind=>HostKind.Echo;
        public override string Help=>"Your corporeal echo replays inputs 2 seconds later. U re-synchronizes both timelines here.";
        public override string Status=>(Leading?"Body delay 2.00 s / ":"Echo delay 2.00 s / ")+history.Count+" recorded steps";
        public override void Enter(){Synchronize();}
        public void Synchronize()
        {
            if(Echo)UnityEngine.Object.Destroy(Echo.gameObject);history.Clear();clock=0;
            Echo=host.Replica(Actor.Body.position,new Color(.53f,.8f,.9f));Echo.CopyLocomotionFrom(Actor);
            // Only temporal echoes ignore their initially overlapping original. Mirror twins remain physical obstacles.
            Physics2D.IgnoreCollision(Echo.Shape,Actor.Shape,true);Echo.Body.simulated=false;alive=true;
            Echo.Died+=()=>{alive=false;};
        }
        public override void After(InputFrame f,float dt)
        {
            if(Leading)return;
            if(f.action){Synchronize();return;}clock+=dt;
            // Store input, not world positions: walls change the replay's eventual location.
            f.action=f.alternate=false;history.Enqueue(new Recorded{time=clock,frame=f,p=Actor.Body.position,v=Actor.Body.linearVelocity});
            if(!Echo||!alive)return;
            if(history.Count>0&&history.Peek().time<=clock-2f+.0001f)
            {if(!Echo.Body.simulated)Echo.Body.simulated=true;var past=history.Dequeue();
                if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-echo-trace")>=0 && (past.time<.7f||past.frame.jump||past.frame.interact))
                    RuntimeEvents.Emit("echo-witness","t="+past.time.ToString("0.000")+" expected="+past.p+" actual="+Echo.Body.position+" before="+Echo.Body.linearVelocity+" f="+past.frame.move+" j="+past.frame.jump+" E="+past.frame.interact);
                Echo.Step(past.frame,dt);
                if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-echo-trace")>=0 && past.time<.7f)RuntimeEvents.Emit("echo-velocity","expected="+past.v+" actual="+Echo.Body.linearVelocity+" ground="+Echo.Grounded);
            }
            else if(clock>2)Echo.Step(default,dt);
        }
        public InputFrame FilterLeading(InputFrame f,float dt)
        {
            if(f.action&&host.Primary==Kind)Synchronize();clock+=dt;InputFrame original=f;f.action=f.alternate=false;
            if(Echo){if(!Echo.Body.simulated)Echo.Body.simulated=true;Echo.Step(f,dt);}
            history.Enqueue(new Recorded{time=clock,frame=original});
            if(history.Count>0&&history.Peek().time<=clock-2+.0001f)return history.Dequeue().frame;
            return default;
        }
        public override void Leave(){if(Echo)UnityEngine.Object.Destroy(Echo.gameObject);history.Clear();}
    }
    public sealed class MirrorForm : HostForm
    {
        public ActorMotor Twin {get;private set;}public float Axis;bool dead;
        public override HostKind Kind=>HostKind.Mirror;public override string Help=>"Left/right drives opposite bodies. Furniture desynchronizes them. Either death loses both.";
        public override void Enter()
        {
            Axis=host.Source&&host.Source.explicitAxis?host.Source.mirrorAxis:Actor.Body.position.x+5;
            Twin=host.Replica(new Vector2(2*Axis-Actor.Body.position.x,Actor.Body.position.y),new Color(.69f,.53f,.91f));
            Twin.CopyLocomotionFrom(Actor);var v=Twin.Body.linearVelocity;v.x=-v.x;Twin.Body.linearVelocity=v;
            Twin.SetFacing(-Actor.Facing);Twin.Died+=()=>dead=true;
            if(Session&&Session.Camera)Session.Camera.secondary=Twin.transform;
        }
        public override void After(InputFrame f,float dt)
        {
            if(dead){Session?.Fail("One reflection was destroyed. Both return together.");return;}
            if(Twin){f.move.x=-f.move.x;f.action=f.alternate=false;Twin.Step(f,dt);}
        }
        public override void Leave(){if(Session&&Session.Camera)Session.Camera.secondary=null;if(Twin)UnityEngine.Object.Destroy(Twin.gameObject);}
    }
    public sealed class MarionetteForm : HostForm
    {
        public RailPath Rail {get;private set;}public DistanceJoint2D Joint {get;private set;}public float railT;
        public override HostKind Kind=>HostKind.Marionette;public override bool Locomotion=>true;
        public override string Help=>"Left/right moves the overhead anchor. Up/down reels the string. Swing below it; shears release you.";
        public override string Status=>Joint?"Thread "+Joint.distance.ToString("0.0")+" m":"No overhead rail";
        public override void Enter()
        {
            Rail=host.Source?host.Source.rail:null;
            if(!Rail){float best=999;foreach(var rail in UnityEngine.Object.FindObjectsByType<RailPath>(FindObjectsSortMode.None)){float t=rail.Nearest(Actor.Body.position);float d=(rail.Point(t)-Actor.Body.position).sqrMagnitude;if(d<best){best=d;Rail=rail;}}}
            if(!Rail){Notice("The spider has no connected overhead rail.");return;}
            railT=Rail.Nearest(Actor.Body.position);Joint=Actor.gameObject.AddComponent<DistanceJoint2D>();Joint.autoConfigureConnectedAnchor=false;Joint.autoConfigureDistance=false;
            Joint.anchor=Vector2.up*.5f;Joint.connectedAnchor=Rail.Point(railT);Joint.distance=Mathf.Clamp(Vector2.Distance(Joint.connectedAnchor,Actor.Body.position+Vector2.up*.5f),1,12);
            Joint.maxDistanceOnly=true;Joint.enableCollision=true;Actor.chargeDisabled=true;Actor.CancelActions();
        }
        public override bool Move(InputFrame f,float dt)
        {
            if(!Joint||!Rail)return false;
            if(f.action)Transfer();
            Vector2 axis=(Rail.b-Rail.a).normalized;float command=Mathf.Abs(axis.y)>.7f?f.move.y:f.move.x;
            railT=Mathf.Clamp01(railT+command*dt*5/Mathf.Max(1,Vector2.Distance(Rail.a,Rail.b)));
            Joint.connectedAnchor=Rail.Point(railT);if(Mathf.Abs(axis.y)<.7f)Joint.distance=Mathf.Clamp(Joint.distance-f.move.y*dt*3,1,12);
            Actor.Body.linearVelocity+=Vector2.down*22*dt;return true;
        }
        public bool Transfer()
        {
            if(!Joint||!Rail)return false;Vector2 anchor=Joint.connectedAnchor;RailPath best=null;float gap=2.1f;
            foreach(var r in UnityEngine.Object.FindObjectsByType<RailPath>(FindObjectsSortMode.None)){if(r==Rail)continue;float t=r.Nearest(anchor);float d=Vector2.Distance(anchor,r.Point(t));if(d<gap){best=r;gap=d;}}
            if(!best){Notice("Bring the hook to a visible rail junction first.");return false;}
            Rail=best;railT=Rail.Nearest(anchor);Joint.connectedAnchor=Rail.Point(railT);RuntimeEvents.Emit("rail-transfer",Rail.name);return true;
        }
        public override void Draw(){if(Joint)PrimitiveArt.Line("Host thread",Actor.transform,Joint.connectedAnchor,Actor.Body.position+Vector2.up*.5f,.065f,new Color(.99f,.88f,.6f),18);}
        public override void Leave(){if(Joint){Joint.enabled=false;UnityEngine.Object.Destroy(Joint);}var l=Actor.transform.Find("Host thread");if(l)UnityEngine.Object.Destroy(l.gameObject);Actor.chargeDisabled=false;}
    }
    public sealed class MoltForm : HostForm
    {
        public override HostKind Kind=>HostKind.Molt;
        public override string Help=>"U leaves a solid husk (maximum two). I or E reclaims a nearby skin. Smaller cores cannot tackle.";
        public override string Status=>"Shed skins "+host.Husks.Count+" / 2";
        public override void Enter(){Refresh();}
        public override bool Move(InputFrame f,float dt){if(f.action)Shed();if(f.alternate)Reclaim();return false;}
        public bool Shed()
        {
            host.Husks.RemoveAll(x=>!x);if(host.Husks.Count>=2){Notice("Two skins already exist. Reclaim one first.");return false;}
            var g=PrimitiveArt.Shape("Cast-off Host skin",Root,Actor.Body.position,Vector2.one,new Color(.69f,.45f,.62f),PrimitiveArt.Icon.Arch,8);
            g.layer=Layers.Prop;var b=g.AddComponent<BoxCollider2D>();b.size=Actor.Shape.size;var sr=g.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Sliced;sr.size=b.size;
            var rb=g.AddComponent<Rigidbody2D>();rb.mass=Actor.Body.mass;rb.gravityScale=3.4f;rb.freezeRotation=true;
            var h=g.AddComponent<HuskBody>();h.owner=host;host.Husks.Add(h);
            Physics2D.IgnoreCollision(b,Actor.Shape,true);h.ignoreOwnerUntil=Time.time+.6f;
            Refresh();RuntimeEvents.Emit("molt-shed",host.Husks.Count.ToString());return true;
        }
        public bool Reclaim()
        {
            for(int i=host.Husks.Count-1;i>=0;i--){var h=host.Husks[i];if(!h){host.Husks.RemoveAt(i);continue;}if(Vector2.Distance(h.transform.position,Actor.Body.position)>2)continue;
                return host.TryReclaim(h);}
            Notice("Your last skin is out of reach.");return false;
        }
        public void Refresh(){float s=host.Husks.Count==0?1:host.Husks.Count==1?.73f:.48f;Actor.SetStandingSize(new Vector2(.88f,1.5f)*s);Actor.Body.mass=s*s;Actor.chargeDisabled=host.Husks.Count>0;}
        public override void Leave(){Actor.Body.mass=1;Actor.chargeDisabled=false;Actor.RestoreShape();}
    }
    public sealed class CoffinForm : HostForm
    {
        public override HostKind Kind=>HostKind.Coffin;public override bool Locomotion=>true;
        public override string Help=>"Left/right: quarter-turn around the leading corner. No jump. Horizontal lids brace machinery.";
        public string LastBlocker {get;private set;}="none";public BoxCollider2D Hull {get;private set;} public bool Horizontal=>Mathf.Abs(Mathf.Sin(Actor.Body.rotation*Mathf.Deg2Rad))>.7f;
        LoadBearingBody brace;float cooldown,fallSpeed,inputBuffer;int bufferedDirection;bool flipping;public bool IsFlipping=>flipping;float elapsed,fromAngle,turn;Vector2 pivot,offset,pivotLocal;Rigidbody2D pivotSupport;const float duration=.30f;
        public override void Enter(){Actor.CancelActions();Actor.Shape.enabled=false;Hull=Actor.gameObject.AddComponent<BoxCollider2D>();Hull.size=new Vector2(1,2);Hull.sharedMaterial=Actor.Shape.sharedMaterial;Actor.Body.bodyType=RigidbodyType2D.Kinematic;Actor.Body.mass=4;Actor.Shape.size=new Vector2(1,2);brace=Actor.gameObject.AddComponent<LoadBearingBody>();Actor.Body.position+=Vector2.up*.3f;Actor.chargeDisabled=true;}
        bool Clear(Vector2 p,float angle)
        {
            foreach(var c in Physics2D.OverlapBoxAll(p,new Vector2(.94f,1.94f),angle,Layers.Solids))if(c&&!c.isTrigger&&c.attachedRigidbody!=Actor.Body){LastBlocker=c.name+" "+c.bounds+" candidate="+p+" angle="+angle;return false;}LastBlocker="none";
            return true;
        }
        public bool BeginFlip(int sign)
        {
            if(flipping)return false;float w=Horizontal?2:1,h=Horizontal?1:2;Vector2 center=Actor.Body.position;
            pivot=center+new Vector2(sign*w*.5f,-h*.5f);pivotSupport=Actor.GroundCollider?Actor.GroundCollider.attachedRigidbody:null;pivotLocal=pivotSupport?pivot-pivotSupport.position:Vector2.zero;offset=center-pivot;fromAngle=Actor.Body.rotation;turn=-sign*90;
            for(int i=1;i<=12;i++){float a=turn*i/12;Vector2 q=pivot+(Vector2)(Quaternion.Euler(0,0,a)*(Vector3)offset);if(!Clear(q,fromAngle+a)){Notice("The coffin's swept corner needs more room.");return false;}}
            flipping=true;elapsed=0;return true;
        }
        public override bool Move(InputFrame f,float dt)
        {
            cooldown-=dt;inputBuffer-=dt;if(Mathf.Abs(f.move.x)>.5f){bufferedDirection=f.move.x>0?1:-1;inputBuffer=.12f;}Actor.Body.linearVelocity=Vector2.zero;
            if(flipping){if(pivotSupport)pivot=pivotSupport.position+pivotLocal;elapsed+=dt;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/duration));float a=turn*t;Actor.Body.position=pivot+(Vector2)(Quaternion.Euler(0,0,a)*(Vector3)offset);Actor.Body.rotation=fromAngle+a;if(elapsed>=duration){flipping=false;cooldown=.10f;Actor.Shape.size=Horizontal?new Vector2(2,1):new Vector2(1,2);}return true;}
            float h=Horizontal?1:2;Vector2 feet=Actor.Body.position-Vector2.up*h*.5f;
            var ground=Physics2D.BoxCast(feet+Vector2.up*.06f,new Vector2((Horizontal?2:1)*.8f,.03f),0,Vector2.down,.16f,Layers.Solids);
            if(ground){fallSpeed=0;if(inputBuffer>0&&cooldown<=0&&BeginFlip(bufferedDirection))inputBuffer=0;}
            else{fallSpeed=Mathf.Min(18,fallSpeed+25*dt);var hit=Physics2D.BoxCast(Actor.Body.position,Hull.size*.96f,Actor.Body.rotation,Vector2.down,fallSpeed*dt,Layers.Solids);Actor.Body.position+=Vector2.down*(hit?Mathf.Max(0,hit.distance-.02f):fallSpeed*dt);}
            brace.bracing=Horizontal&&!flipping;return true;
        }
        public override void Leave(){if(Hull){Hull.enabled=false;UnityEngine.Object.Destroy(Hull);}if(brace)UnityEngine.Object.Destroy(brace);Actor.Shape.enabled=true;Actor.Body.bodyType=RigidbodyType2D.Dynamic;Actor.Body.mass=1;Actor.Body.rotation=0;Actor.RestoreShape();Actor.chargeDisabled=false;}
    }
}