using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class AtlasBuilder
    {
        public readonly StageBuilder b;public readonly StageDefinition d;public HostController host;public ActorMotor Player=>b.session.player;
        public AtlasBuilder(StageBuilder builder,StageDefinition definition){b=builder;d=definition;}
        public void Begin(Rect bounds,Vector2 spawn)
        {
            int world=int.Parse(d.worldId.Substring(1));
            Color[] bg={new Color(.06f,.035f,.10f),new Color(.12f,.12f,.06f),new Color(.06f,.07f,.12f),new Color(.10f,.07f,.09f),new Color(.18f,.16f,.23f)};
            Color[] terrain={new Color(.35f,.24f,.4f),new Color(.36f,.39f,.21f),new Color(.28f,.35f,.47f),new Color(.40f,.30f,.31f),new Color(.63f,.59f,.55f)};
            b.background=bg[world-1];b.stone=terrain[world-1];b.accent=world==5?new Color(.86f,.75f,.44f):new Color(.75f,.46f,.51f);
            b.Bounds(bounds);b.Decor(bounds,d.title,world);b.Player(spawn);host=Player.gameObject.AddComponent<HostController>();if(d.course>1||d.boss)GameRoot.Instance?.MarkCorrupted();Player.collisionMask=Layers.Solids|(1<<18);
            for(int layer=17;layer<=19;layer++){Physics2D.IgnoreLayerCollision(Layers.Actor,layer,layer!=18);Physics2D.IgnoreLayerCollision(Layers.Enemy,layer,layer!=18);}
            var notes=b.root.gameObject.AddComponent<AtlasNotes>();notes.definition=d;
            if(world==5)for(int i=0;i<12;i++)PrimitiveArt.Shape("Distant empyrean ring",b.root,new Vector2(bounds.xMin+i*8,bounds.yMax-9),new Vector2(6,9),new Color(.8f,.73f,.5f,.13f),PrimitiveArt.Icon.Arch,-18);
            if(world==4)for(int i=0;i<15;i++)PrimitiveArt.Line("Chain "+i,b.root,new Vector2(bounds.xMin+i*9,bounds.yMin),new Vector2(bounds.xMin+i*9+15,bounds.yMax),.06f,new Color(.53f,.45f,.4f,.3f),-17);
        }
        public GameObject Floor(float left,float right,float y=0)=>b.Floor((left+right)*.5f,y,right-left);
        public GameObject Ledge(float x,float y,float w=4)=>b.Platform(new Vector2(x,y-.2f),new Vector2(w,.4f));
        public void Steps(float x,float y,int count,float dx=2.6f,float dy=1.9f,float w=3.3f){for(int i=0;i<count;i++)Ledge(x+i*dx,y+i*dy,w);}
        public HostSource Source(HostKind k,float x,float y=1,bool combine=false)
        {
            var color=k==HostKind.Root?new Color(.49f,.73f,.31f):k==HostKind.Ink?new Color(.6f,.62f,.9f):new Color(.79f,.51f,.72f);
            var g=b.Trigger("Tenant: "+k,new Vector2(x,y),new Vector2(1.1f,1.45f),color,PrimitiveArt.Icon.Round);var source=g.AddComponent<HostSource>();source.kind=k;source.combine=combine;
            Creature(k,g.transform,new Vector2(x,y),color);
            return source;
        }
        static void Creature(HostKind kind,Transform parent,Vector2 at,Color c)
        {
            void Part(string n,Vector2 p,Vector2 size,PrimitiveArt.Icon icon,Color color){PrimitiveArt.Shape(n,parent,at+p,size,color,icon,8);}
            Part("Watching pupil",new Vector2(.15f,.2f),new Vector2(.23f,.36f),PrimitiveArt.Icon.Eye,Color.black);
            switch(kind)
            {
                case HostKind.Echo:Part("Bell lip",new Vector2(0,-.45f),new Vector2(1.55f,.2f),PrimitiveArt.Icon.Block,new Color(.84f,.7f,.3f));break;
                case HostKind.Marionette:for(int i=-1;i<=1;i+=2){PrimitiveArt.Line("Spider leg "+i,parent,at,at+new Vector2(i*1.1f,-.3f),.09f,c,8);}Part("Spool",new Vector2(0,.7f),Vector2.one*.6f,PrimitiveArt.Icon.Arch,c);break;
                case HostKind.Molt:Part("Left moth wing",new Vector2(-.65f,0),new Vector2(.8f,.9f),PrimitiveArt.Icon.Eye,c);Part("Right moth wing",new Vector2(.65f,0),new Vector2(.8f,.9f),PrimitiveArt.Icon.Eye,c);break;
                case HostKind.Wax:Part("Votive wick",new Vector2(0,.9f),new Vector2(.35f,.6f),PrimitiveArt.Icon.Star,Color.yellow);break;
                case HostKind.Gullet:Part("Snail shell",new Vector2(-.45f,0),Vector2.one*1.1f,PrimitiveArt.Icon.Arch,new Color(.58f,.57f,.36f));Part("Communion mouth",new Vector2(.55f,-.2f),new Vector2(.65f,.35f),PrimitiveArt.Icon.Arch,Color.white);break;
                case HostKind.Root:Part("Burr spines",Vector2.zero,Vector2.one*1.8f,PrimitiveArt.Icon.Star,new Color(.52f,.71f,.3f,.7f));break;
                case HostKind.Mirror:Part("Mirror frame",Vector2.zero,new Vector2(1.6f,1.9f),PrimitiveArt.Icon.Arch,new Color(.85f,.77f,.5f));break;
                case HostKind.InsideOut:Part("Unstitched halo",new Vector2(0,.9f),new Vector2(1.7f,.6f),PrimitiveArt.Icon.Arch,new Color(.52f,.83f,.78f));break;
                case HostKind.Parallax:Part("Second pupil",new Vector2(-.35f,.3f),Vector2.one*.7f,PrimitiveArt.Icon.Eye,new Color(.83f,.87f,.93f));break;
                case HostKind.Censer:Part("Incense cage",new Vector2(0,-.5f),new Vector2(1.8f,.75f),PrimitiveArt.Icon.Arch,new Color(.65f,.68f,.8f));break;
                case HostKind.Stitch:Part("Needle",new Vector2(.75f,0),new Vector2(.13f,2.2f),PrimitiveArt.Icon.Diamond,Color.white);break;
                case HostKind.Coffin:Part("Crab lid",Vector2.zero,new Vector2(1.8f,1.25f),PrimitiveArt.Icon.Arch,new Color(.59f,.34f,.38f));break;
                case HostKind.Lodestone:Part("Magnetic halo",Vector2.zero,new Vector2(2,1.6f),PrimitiveArt.Icon.Arch,new Color(.75f,.67f,.35f));break;
                case HostKind.Shadow:Part("Noon lamp",new Vector2(0,.65f),Vector2.one*1.25f,PrimitiveArt.Icon.Star,new Color(.97f,.9f,.6f));break;
                case HostKind.Ink:Part("Leech nib",new Vector2(.7f,-.3f),new Vector2(.8f,1.3f),PrimitiveArt.Icon.Diamond,new Color(.14f,.1f,.27f));break;
            }
        }
        public HostCure Cure(HostKind kind,float x,float y=1,bool all=false)
        {
            string[] names={"EMPTY FRAME","FELT","SHEARS","TAILOR BRUSH","COLD FONT","EMETIC HERBS","SALT","MATTE VELVET","EMPTY FRAME","FLAT SIGN","CROSSWIND","SCISSORS","GRAVE MOUTH","CERAMIC ARCH","ECLIPSE ARCH","BLOTTING SAINT"};
            var g=b.Trigger(names[(int)kind],new Vector2(x,y),new Vector2(.7f,2.5f),new Color(.39f,.71f,.68f,.65f),PrimitiveArt.Icon.Arch);var cure=g.AddComponent<HostCure>();cure.kind=kind;cure.all=all;PrimitiveArt.Label(names[(int)kind],b.root,new Vector2(x,y+1.6f),.07f);return cure;
        }
        public void Mercy(float x,float y){b.Collect(PickupKind.Mercy,new Vector2(x,y),d.id+"-MERCY");}
        public void Key(float x,float y){b.Collect(PickupKind.Key,new Vector2(x,y),d.id+"-KEY");}
        public TurnSwitch Nail(float x,float y){var n=b.Nail(new Vector2(x,y));n.name="World Nail";n.GetComponent<SpriteRenderer>().color=new Color(.91f,.71f,.38f);return n;}
        public ExitPortal Exit(float x,float y){return b.Exit(new Vector2(x,y));}
        public void Health(float x,float y){b.Collect(PickupKind.Health,new Vector2(x,y),amount:2);}
        public RailPath Rail(Vector2 a,Vector2 z,float sag=0){var g=new GameObject("Overhead laundry line");g.transform.SetParent(b.root);var rail=g.AddComponent<RailPath>();rail.a=a;rail.b=z;rail.sag=sag;return rail;}
        public EdibleChunk Chunk(Vector2 p,Vector2 size){var g=b.Solid("Edible structural tile",p,size,new Color(.8f,.53f,.36f));var c=g.AddComponent<EdibleChunk>();c.stableId=d.id+"-tile-"+p.x+"-"+p.y;return c;}
        public TerrainSocket Socket(Vector2 p,Vector2 capacity){var g=b.Trigger("Empty terrain footprint",p,capacity,new Color(.81f,.57f,.4f,.2f),PrimitiveArt.Icon.Arch);var s=g.AddComponent<TerrainSocket>();s.capacity=capacity;return s;}
        public RootSoil Soil(Vector2 a,Vector2 z,float width=1.1f)
        {
            var size=new Vector2(Mathf.Max(width,Mathf.Abs(a.x-z.x)+width),Mathf.Max(width,Mathf.Abs(a.y-z.y)+width));var g=b.Trigger("Root seam",(a+z)*.5f,size,new Color(.34f,.6f,.32f,.45f));var soil=g.AddComponent<RootSoil>();soil.size=size;return soil;
        }
        public void SoilPath(params Vector2[] path){for(int i=1;i<path.Length;i++)Soil(path[i-1],path[i]);}
        public MagneticBody Metal(Vector2 p,Vector2 size,float mass=1,bool fixedAnchor=false,int polarity=1)
        {
            var g=fixedAnchor?b.Solid("Fixed iron altar",p,size,new Color(.64f,.67f,.73f)):b.Prop(p,size,mass,true);var m=g.AddComponent<MagneticBody>();m.polarity=polarity;g.AddComponent<ShadowCaster>();if(!fixedAnchor)g.AddComponent<TemporalBody>();return m;
        }
        public FoldPanel Hinge(Vector2 pivot,float length,float angle,string group="cloth")
        {
            var g=new GameObject("Hinged architecture");g.transform.SetParent(b.root);g.transform.position=pivot;var rb=g.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;rb.rotation=angle;
            var piece=b.Solid("Foldable cloth-stone",pivot+Vector2.right*(length*.5f),new Vector2(length,.45f),b.accent,Layers.Moving);piece.transform.SetParent(g.transform,true);piece.transform.localPosition=new Vector3(length*.5f,0,0);g.transform.rotation=Quaternion.Euler(0,0,angle);
            var panel=g.AddComponent<FoldPanel>();panel.length=length;panel.angle=panel.targetAngle=angle;panel.body=rb;
            var node=b.Trigger("Loose seam",pivot,Vector2.one*.55f,new Color(.95f,.5f,.65f),PrimitiveArt.Icon.Star);node.transform.SetParent(g.transform);node.transform.localPosition=new Vector3(length,0,0);var n=node.AddComponent<SeamNode>();n.panel=panel;n.group=group;return panel;
        }
        public SeamNode Seam(Vector2 p,string group="cloth"){var g=b.Trigger("Fixed seam",p,Vector2.one*.55f,new Color(.95f,.5f,.65f),PrimitiveArt.Icon.Star);var n=g.AddComponent<SeamNode>();n.group=group;return n;}
        public DepthGeometry Depth(Vector2 p,Vector2 size,int plane){var g=b.Solid("Perspective platform",p,size);var depth=g.AddComponent<DepthGeometry>();depth.canonicalSize=size;depth.SetPlane(plane);return depth;}
        public ProjectionOverlap Projection(Vector2 p,Vector2 size,int allowed=7){var g=b.Trigger("Shared silhouette",p,size,new Color(.75f,.85f,.85f,.08f),PrimitiveArt.Icon.Arch);var r=g.AddComponent<ProjectionOverlap>();r.size=size;r.allowed=allowed;return r;}
        public KneelingFigure Figure(float x,float top,float floor,float delay=1.5f)
        {var g=b.Solid("A falling witness",new Vector2(x,top),new Vector2(.9f,2.4f),new Color(.6f,.62f,.66f),Layers.Moving);g.AddComponent<Rigidbody2D>();var f=g.AddComponent<KneelingFigure>();f.floor=floor;f.delay=delay;f.height=top-floor;return f;}
        public PulseBell Bell(Vector2 p,Vector2[] rope,PulseReceiver receiver){var g=b.Solid("Signal bell",p,Vector2.one,new Color(.85f,.71f,.39f));var bell=g.AddComponent<PulseBell>();bell.rope=rope;bell.receiver=receiver;return bell;}
        public PulseReceiver Receiver(Vector2 p){var g=b.Trigger("Bell brake",p,Vector2.one*.5f,new Color(.86f,.68f,.4f));return g.AddComponent<PulseReceiver>();}
        public void Finish(){foreach(var m in UnityEngine.Object.FindObjectsByType<MotionPlatform>(FindObjectsSortMode.None))if(m.transform.IsChildOf(b.root)&&!m.GetComponent<TemporalBody>())m.gameObject.AddComponent<TemporalBody>();foreach(var e in UnityEngine.Object.FindObjectsByType<CarryableEnemy>(FindObjectsSortMode.None))if(e.transform.IsChildOf(b.root)&&!e.GetComponent<TemporalBody>())e.gameObject.AddComponent<TemporalBody>();}
    }
}