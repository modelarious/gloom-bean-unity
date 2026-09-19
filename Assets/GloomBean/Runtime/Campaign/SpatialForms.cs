using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class InsideOutForm:HostForm
    {
        public override HostKind Kind=>HostKind.InsideOut;public override bool Locomotion=>true;
        public override string Help=>"Closed stone shapes are now corridors. Empty outside space supplies no floor. An empty frame returns you.";
        public override void Enter(){Actor.Shape.excludeLayers=Layers.Solids|(7<<17);Actor.Shape.includeLayers=1<<Layers.Interior;Actor.Shape.layerOverridePriority=20;Actor.collisionMask=1<<Layers.Interior;}
        public override void Leave(){Actor.Shape.excludeLayers=Actor.Shape.includeLayers=0;Actor.collisionMask=Layers.Solids|(1<<18);}
    }
    public sealed class ParallaxForm:HostForm
    {
        public override HostKind Kind=>HostKind.Parallax;public override bool Locomotion=>true;
        public int Plane {get;private set;}=1;float cooldown;
        public override string Help=>"Up/down steps FAR / MID / NEAR only in overlapping silhouettes. Position stays fixed; scale and reach change.";
        public override string Status=>new[]{"FAR","MID","NEAR"}[Plane]+" / size x"+DepthGeometry.Factor(Plane).ToString("0.00");
        public override void Enter(){SetPlane(1);}
        void SetPlane(int p)
        {
            Plane=p;Vector2 at=Actor.Body.position;float s=DepthGeometry.Factor(p);Actor.SetStandingSize(new Vector2(.88f,1.5f)*s);Actor.Body.position=at;
            Actor.speedFactor=s;Actor.Shape.excludeLayers=(7<<17)&~(1<<(17+p));Actor.Shape.includeLayers=1<<(17+p);Actor.Shape.layerOverridePriority=15;
            Actor.collisionMask=Layers.Solids|(1<<(17+p));RuntimeEvents.Emit("depth",p.ToString());
        }
        public bool StepPlane(int direction)
        {
            int next=Mathf.Clamp(Plane+direction,0,2);if(next==Plane)return false;bool overlap=false;
            foreach(var r in UnityEngine.Object.FindObjectsByType<ProjectionOverlap>(FindObjectsSortMode.None))if(r.Contains(Actor.Body.position)&&r.Allows(Plane,next)){overlap=true;break;}
            if(!overlap){Notice("Align the two platform silhouettes before changing depth.");return false;}
            Vector2 size=new Vector2(.80f,1.38f)*DepthGeometry.Factor(next);
            foreach(var c in Physics2D.OverlapBoxAll(Actor.Body.position,size,0,Layers.Solids|(1<<(17+next))))if(c&&!c.isTrigger){Notice("The destination silhouette is occupied.");return false;}
            SetPlane(next);return true;
        }
        public override bool Move(InputFrame f,float dt){cooldown-=dt;if(Mathf.Abs(f.move.y)>.6f&&cooldown<=0){StepPlane(f.move.y>0?1:-1);cooldown=.32f;}return false;}
        public override void Leave(){Actor.Shape.excludeLayers=Actor.Shape.includeLayers=0;Actor.collisionMask=Layers.Solids|(1<<18);Actor.speedFactor=1;Actor.RestoreShape();}
    }
    public sealed class StitchForm:HostForm
    {
        public override HostKind Kind=>HostKind.Stitch;public SeamNode First {get;private set;}public SeamNode Second {get;private set;}public FoldPanel Active {get;private set;}
        public override string Help=>"U selects the nearest seam, then a second edge; U again tugs the real hinge. I cuts. Stay out of the fold.";
        public override string Status=>Active?"A live architectural stitch":First?"First seam selected":"Spool free";
        public bool Select(SeamNode n)
        {
            if(!n)return false;if(!First){First=n;return true;}
            if(n==First)return false;
            FoldPanel panel=First.panel?First.panel:n.panel;SeamNode target=First.panel?n:First;
            if(!panel||First.group!=n.group||Vector2.Distance(panel.transform.position,target.transform.position)>panel.length*1.3f){Notice("Those seams cannot make a physical fold.");return false;}
            Second=n;Active=panel;return true;
        }
        public void Tug(){if(Active&&Second){var target=First.panel?Second:First;Active.AimAt(target.transform.position);RuntimeEvents.Emit("stitch-tug",Active.name);}}
        public void Cut(){First=Second=null;Active=null;var l=Actor.transform.Find("Architectural stitch");if(l)UnityEngine.Object.Destroy(l.gameObject);}
        public override bool Move(InputFrame f,float dt)
        {
            if(f.alternate)Cut();if(f.action){if(Active)Tug();else{SeamNode best=null;float dist=2.5f;foreach(var n in UnityEngine.Object.FindObjectsByType<SeamNode>(FindObjectsSortMode.None)){float d=Vector2.Distance(Actor.Body.position,n.transform.position);if(d<dist&&n!=First){best=n;dist=d;}}Select(best);}}return false;
        }
        public override void Draw(){if(First)PrimitiveArt.Line("Architectural stitch",Actor.transform,First.transform.position,Second?Second.transform.position:Actor.transform.position,.06f,new Color(.95f,.48f,.55f),18);}
        public override void Leave(){Cut();}
    }
}