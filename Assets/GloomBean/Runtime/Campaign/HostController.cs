using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;

namespace GloomBean.Campaign
{
    public enum HostKind { None, Echo, Marionette, Molt, Wax, Gullet, Root, Mirror, InsideOut, Parallax, Censer, Stitch, Coffin, Lodestone, Shadow, Ink }
    public abstract class HostForm
    {
        public HostController host; public ActorMotor Actor=>host.Actor; public StageSession Session=>host.Session;
        public abstract HostKind Kind {get;} public virtual bool Locomotion=>false;
        public virtual string Status=>Kind.ToString(); public abstract string Help {get;}
        public virtual void Enter(){} public virtual void Leave(){} public virtual bool Move(InputFrame f,float dt)=>false;
        public virtual void After(InputFrame f,float dt){} public virtual void Draw(){}
        protected Transform Root=>host.transform.parent;
        protected void Notice(string s)=>Session?.Notice(s);
    }
    [DisallowMultipleComponent]
    public sealed class HostController : MonoBehaviour, IActorModifier, IActorInputFilter
    {
        public ActorMotor Actor {get;private set;}
        public StageSession Session=>GetComponentInParent<StageSession>();
        public HostSource Source {get;private set;}
        public readonly List<HostForm> Forms=new List<HostForm>();
        public readonly List<HuskBody> Husks=new List<HuskBody>();
        public readonly List<WaxPlug> Plugs=new List<WaxPlug>();
        public float waxVolume=1; public int focus;
        public HostKind Primary=>Forms.Count==0?HostKind.None:Forms[Mathf.Clamp(focus,0,Forms.Count-1)].Kind;
        public event Action<HostKind> Acquired; public event Action<HostKind> Cured;
        public bool Has(HostKind kind)=>Forms.Exists(f=>f.Kind==kind);
        public T Form<T>() where T:HostForm=>Forms.Find(f=>f is T) as T;
        public static string Display(HostKind k)=>k==HostKind.InsideOut?"INSIDE-OUT":k.ToString().ToUpperInvariant();
        void Awake(){Actor=GetComponent<ActorMotor>();Actor.modifier=this;gameObject.AddComponent<HostView>();}
        public bool Acquire(HostKind kind,HostSource source=null,bool combine=false)
        {
            if(kind==HostKind.None)return Cure();
            if(Session){var ban=Session.GetComponentInChildren<HostEmbargo>();if(ban&&ban.Blocks(kind)){Session.Notice("That tenant is still held by the Host of Hosts.");return false;}}
            if(Has(kind)){focus=Forms.FindIndex(f=>f.Kind==kind);return true;}
            HostForm form=Make(kind); if(form==null)return false;
            if(!combine||Forms.Count>=2||form.Locomotion&&Forms.Exists(f=>f.Locomotion))
            { if(!Cure())return false; }
            Source=source; Actor.CancelActions(); Actor.DropCarried();form.host=this;Forms.Add(form);focus=Forms.Count-1;form.Enter();
            Acquired?.Invoke(kind);RuntimeEvents.Emit("possession",kind.ToString());
            Session?.Notice(Display(kind)+" HOST: "+form.Help,6);return true;
        }
        static HostForm Make(HostKind k)
        {
            switch(k){case HostKind.Echo:return new EchoForm();case HostKind.Marionette:return new MarionetteForm();case HostKind.Molt:return new MoltForm();case HostKind.Wax:return new WaxForm();case HostKind.Gullet:return new GulletForm();case HostKind.Root:return new RootForm();case HostKind.Mirror:return new MirrorForm();case HostKind.InsideOut:return new InsideOutForm();case HostKind.Parallax:return new ParallaxForm();case HostKind.Censer:return new CenserForm();case HostKind.Stitch:return new StitchForm();case HostKind.Coffin:return new CoffinForm();case HostKind.Lodestone:return new LodestoneForm();case HostKind.Shadow:return new ShadowForm();case HostKind.Ink:return new InkForm();default:return null;}
        }
        public bool Cure(HostKind only=HostKind.None,bool force=false)
        {
            if(Forms.Count==0||(only!=HostKind.None&&!Has(only)))return true;
            // Restoration is rejected, not clipped through a ceiling or a still-solid wall.
            if(!force&&!CanStand(Actor.Feet+Vector2.up*.75f)) {Session?.Notice("Find enough open space to return to your Open Host body.");return false;}
            for(int i=Forms.Count-1;i>=0;i--)if(only==HostKind.None||Forms[i].Kind==only)
            {var f=Forms[i];f.Leave();Forms.RemoveAt(i);Cured?.Invoke(f.Kind);RuntimeEvents.Emit("cure",f.Kind.ToString());}
            focus=Mathf.Clamp(focus,0,Mathf.Max(0,Forms.Count-1));
            if(Forms.Count==0)ResetBody();else Form<MoltForm>()?.Refresh();return true;
        }
        public bool CanStand(Vector2 p)
        {
            foreach(var c in Physics2D.OverlapBoxAll(p,new Vector2(.78f,1.36f),0,Layers.Solids))
                if(c&&!c.isTrigger&&c.attachedRigidbody!=Actor.Body)return false;
            return true;
        }
        void ResetBody()
        {
            Actor.Body.bodyType=RigidbodyType2D.Dynamic;Actor.Body.simulated=true;Actor.Body.mass=1;Actor.Body.rotation=0;
            Actor.Shape.enabled=true;Actor.Shape.excludeLayers=0;Actor.Shape.includeLayers=0;Actor.Shape.layerOverridePriority=0;
            Actor.collisionMask=Layers.Solids|(1<<18);Actor.gravityFactor=Actor.speedFactor=1;Actor.chargeDisabled=Actor.pickupDisabled=false;
            Actor.RestoreShape();Actor.CancelActions();gameObject.layer=Layers.Actor;
        }
        public InputFrame Filter(InputFrame input,float dt){var e=Form<EchoForm>();return e!=null&&e.Leading?e.FilterLeading(input,dt):input;}
        public bool BeforeMovement(ActorMotor a,InputFrame input,float dt)
        {
            if(Forms.Count>1&&input.alternate&&input.move.y<-.5f){focus=(focus+1)%Forms.Count;input.alternate=false;}
            bool custom=false;
            for(int i=0;i<Forms.Count;i++){var f=input;if(i!=focus){f.action=f.actionHeld=f.alternate=false;}custom|=Forms[i].Move(f,dt);}
            return custom;
        }
        public void AfterMovement(ActorMotor a,InputFrame input,float dt)
        {
            for(int i=0;i<Forms.Count;i++){var f=input;if(i!=focus){f.action=f.actionHeld=f.alternate=false;}Forms[i].After(f,dt);}
        }
        void Update()
        {
            if(!Actor||Actor.replica)return;
            var root=GameRoot.Instance;if(!root||root.Session!=Session)return;
            root.PossessionDisplay=Forms.Count==0?(root.IsCorrupted?"OPEN HOST":""):string.Join(" + ",Forms.ConvertAll(f=>Display(f.Kind)+" HOST"));
            root.PossessionHelp=Forms.Count==0?"Find a tenant. Sources are creatures; cures are part of the scenery.":Forms[Mathf.Clamp(focus,0,Forms.Count-1)].Status+" | "+Forms[Mathf.Clamp(focus,0,Forms.Count-1)].Help+(Forms.Count>1?" | Down+I: focus the other form":"");
            foreach(var f in Forms)f.Draw();
        }
        void OnDestroy(){foreach(var f in Forms)f.Leave();if(Actor)Actor.modifier=null;}
        public ActorMotor Replica(Vector2 p,Color color)
        {
            var go=new GameObject("Corporeal secondary Host");go.transform.SetParent(transform.parent);go.transform.position=p;
            var a=go.AddComponent<ActorMotor>();a.replica=true;a.pickupDisabled=true;a.manual=true;
            var view=go.AddComponent<ActorView>();view.corrupted=true;view.ghost=true;view.tint=color;
            return a;
        }
    }
}