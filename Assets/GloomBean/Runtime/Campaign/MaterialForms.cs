using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class WaxForm:HostForm
    {
        public override HostKind Kind=>HostKind.Wax;public override bool Locomotion=>true;public bool Liquid {get;private set;}
        public override string Help=>"U melts/reforms. I leaves wax; Up+I absorbs nearby plugs. Liquid flows downhill and cannot jump.";
        public override string Status=>"Body volume "+host.waxVolume.ToString("0.00")+" / 1.00";
        public override void Enter(){host.waxVolume=1;host.Plugs.RemoveAll(p=>!p);foreach(var p in host.Plugs)host.waxVolume-=p.volume;Size();}
        public void Toggle(){if(Liquid){Vector2 s=new Vector2(.88f,1.5f)*Mathf.Sqrt(Mathf.Max(.25f,host.waxVolume));if(!Actor.SetSize(s)){Notice("The wax cannot reform under this ceiling.");return;}}Liquid=!Liquid;Size();}
        void Size(){float v=Mathf.Max(.25f,host.waxVolume);Actor.Body.mass=v;Actor.SetStandingSize(Liquid?new Vector2(1.5f*v,.32f):new Vector2(.88f,1.5f)*Mathf.Sqrt(v));Actor.chargeDisabled=Liquid;}
        public bool Deposit()
        {
            if(host.waxVolume<.47f)return false;
            Vector2 p=Actor.Feet+new Vector2(-Actor.Facing*.85f,.25f);
            if(Physics2D.OverlapBox(p,new Vector2(.5f,.38f),0,1<<Layers.Terrain))return false;
            var g=PrimitiveArt.Shape("Conserved wax plug",Root,p,Vector2.one,new Color(.92f,.82f,.45f),PrimitiveArt.Icon.Round,7);g.layer=Layers.Prop;
            var c=g.AddComponent<BoxCollider2D>();c.size=new Vector2(.62f,.5f);var sr=g.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Sliced;sr.size=c.size;
            var rb=g.AddComponent<Rigidbody2D>();rb.mass=.22f;rb.gravityScale=3.4f;rb.freezeRotation=true;
            var plug=g.AddComponent<WaxPlug>();plug.owner=host;plug.volume=.22f;host.Plugs.Add(plug);host.waxVolume-=plug.volume;Size();return true;
        }
        public bool Absorb()
        {
            for(int i=host.Plugs.Count-1;i>=0;i--){var p=host.Plugs[i];if(!p){host.Plugs.RemoveAt(i);continue;}if(Vector2.Distance(p.transform.position,Actor.Body.position)>2)continue;
                host.waxVolume=Mathf.Min(1,host.waxVolume+p.volume);p.GetComponent<Collider2D>().enabled=false;UnityEngine.Object.Destroy(p.gameObject);host.Plugs.RemoveAt(i);Size();return true;}return false;
        }
        public override bool Move(InputFrame f,float dt)
        {
            if(f.action)Toggle();if(f.alternate){if(f.move.y>.3f)Absorb();else Deposit();}
            if(!Liquid)return false;
            Vector2 v=Actor.Body.linearVelocity;
            if(Actor.Grounded){var t=new Vector2(Actor.GroundNormal.y,-Actor.GroundNormal.x);float along=Vector2.Dot(v,t)+Vector2.Dot(Vector2.down*22,t)*dt+f.move.x*8*dt;along=Mathf.MoveTowards(along,0,1.5f*dt);v=t*Mathf.Clamp(along,-8,8)-Actor.GroundNormal*.8f;}
            else {v.x=Mathf.MoveTowards(v.x,f.move.x*2,4*dt);v.y=Mathf.Max(-9,v.y-18*dt);}
            Actor.Body.linearVelocity=v;return true;
        }
        public override void Leave(){Liquid=false;Actor.Body.mass=1;Actor.chargeDisabled=false;Actor.RestoreShape();}
    }
    public sealed class GulletForm:HostForm
    {
        public override HostKind Kind=>HostKind.Gullet; public EdibleChunk Stored {get;private set;}
        public override string Help=>"Aim + U bites/spits one actual terrain chunk. I returns it to its original position. Support can move.";
        public override string Status=>Stored?"Stored: "+Stored.name:"Gullet empty";
        public override bool Move(InputFrame f,float dt)
        {
            if(f.action){Vector2 dir=f.move.sqrMagnitude>.2f?f.move.normalized:Vector2.right*Actor.Facing;if(Stored)Spit(dir);else Bite(dir);}
            if(f.alternate)Return();return false;
        }
        public bool Bite(Vector2 dir)
        {
            if(Stored)return false;
            var hits=Physics2D.OverlapCircleAll(Actor.Body.position+dir*1.0f,1.0f,Layers.Solids);
            float best=999;EdibleChunk target=null;
            foreach(var c in hits){var chunk=c.GetComponent<EdibleChunk>();if(chunk){float dist=((Vector2)c.bounds.center-Actor.Body.position).sqrMagnitude;if(dist<best){best=dist;target=chunk;}}}
            if(!target){Notice("That surface is living, reinforced, or too large to swallow.");return false;}
            Stored=target;Stored.Remember();Stored.gameObject.SetActive(false);RuntimeEvents.Emit("terrain-swallowed",Stored.stableId);return true;
        }
        public bool PlaceAt(Vector2 p)
        {
            if(!Stored)return false;Vector2 size=Stored.Size;
            foreach(var c in Physics2D.OverlapBoxAll(p,size-Vector2.one*.08f,0,Layers.Solids|(1<<Layers.Actor)))if(c&&!c.isTrigger)return false;
            Stored.transform.position=p;Stored.gameObject.SetActive(true);RuntimeEvents.Emit("terrain-rehomed",Stored.stableId);Stored=null;return true;
        }
        bool Spit(Vector2 dir)
        {
            TerrainSocket closest=null;float best=3.2f;
            foreach(var s in UnityEngine.Object.FindObjectsByType<TerrainSocket>(FindObjectsSortMode.None)){float dist=Vector2.Distance(s.transform.position,Actor.Body.position);if(dist<best&&Vector2.Dot(((Vector2)s.transform.position-Actor.Body.position).normalized,dir)>.25f&&s.Accepts(Stored)){closest=s;best=dist;}}
            if(closest&&PlaceAt(closest.transform.position))return true;
            // Empty unit-grid cells are also sockets; there is no color-key gate check.
            Vector2 target=Actor.Body.position+dir*1.8f;target=new Vector2(Mathf.Round(target.x),Mathf.Round(target.y));
            if(PlaceAt(target))return true;Notice("There is no unoccupied footprint for that chunk.");return false;
        }
        public void Return(){if(!Stored)return;var chunk=Stored;Stored=null;chunk.transform.position=chunk.Original;chunk.gameObject.SetActive(true);chunk.GetComponent<Collider2D>().enabled=false;chunk.returnPending=true;}
        public override void Leave(){Return();}
    }
    public sealed class RootForm:HostForm
    {
        public override HostKind Kind=>HostKind.Root;public override bool Locomotion=>true;
        public override string Help=>"Hold U and steer the root through wet soil. Release U to pull your exposed body along the exact path. I cancels.";
        public Vector2 Tip {get;private set;} public readonly List<Vector2> Path=new List<Vector2>();
        public bool Growing {get;private set;}public bool Retracting {get;private set;}int segment;Vector2 origin;LineRenderer line;
        public override void Enter(){Tip=Actor.Body.position;}
        public bool Begin()
        {
            origin=Tip=Actor.Body.position;bool adjacent=RootSoil.All.Exists(s=>s&&s.wet&&s.Near(Tip,.85f));
            if(!adjacent){Notice("A root needs moist soil at your feet.");return false;}
            Path.Clear();Path.Add(Tip);Growing=true;Actor.Body.linearVelocity=Vector2.zero;return true;
        }
        public bool Grow(Vector2 delta)
        {
            var next=Tip+delta;if(Vector2.Distance(origin,next)>16)return false;
            for(int i=1;i<=4;i++){Vector2 q=Vector2.Lerp(Tip,next,i/4f);if(!RootSoil.All.Exists(s=>s&&s.wet&&s.Near(q,.1f))&&Vector2.Distance(q,origin)>.8f)return false;}
            Tip=next;if(Path.Count==0||Vector2.Distance(Path[Path.Count-1],Tip)>.09f)Path.Add(Tip);return true;
        }
        public void Retract()
        {
            Growing=false;if(Path.Count<2)return;
            if(!host.CanStand(Tip)){Notice("The tip has no body-sized exit pocket. The root withdraws safely.");Path.Clear();Tip=origin;return;}
            Retracting=true;segment=1;Actor.Shape.enabled=false;Actor.Body.bodyType=RigidbodyType2D.Kinematic;
        }
        public override bool Move(InputFrame f,float dt)
        {
            if(f.action&&!Growing&&!Retracting)Begin();
            if(f.alternate&&Growing){Growing=false;Path.Clear();return false;}
            if(Growing){Actor.Body.linearVelocity=Vector2.zero;Grow(f.move*4*dt);if(!f.actionHeld&&!f.action)Retract();return true;}
            if(Retracting)
            {
                float left=dt*12;
                while(left>0&&segment<Path.Count){float d=Vector2.Distance(Actor.Body.position,Path[segment]);if(d<=left){Actor.Body.position=Path[segment++];left-=d;}else{Actor.Body.position=Vector2.MoveTowards(Actor.Body.position,Path[segment],left);left=0;}}
                if(segment>=Path.Count){Retracting=false;Actor.Body.bodyType=RigidbodyType2D.Dynamic;Actor.Shape.enabled=true;Actor.Body.linearVelocity=Vector2.zero;RuntimeEvents.Emit("root-relocate");}
                return true;
            }
            return false;
        }
        public override void Draw()
        {
            if(!line){line=PrimitiveArt.Line("Root path",Actor.transform,Actor.Body.position,Tip,.09f,new Color(.64f,.78f,.35f),17);}
            line.positionCount=Path.Count;for(int i=0;i<Path.Count;i++)line.SetPosition(i,Path[i]);
        }
        public override void Leave(){Growing=Retracting=false;if(Actor){Actor.Body.bodyType=RigidbodyType2D.Dynamic;Actor.Shape.enabled=true;}if(line)UnityEngine.Object.Destroy(line.gameObject);}
    }
}