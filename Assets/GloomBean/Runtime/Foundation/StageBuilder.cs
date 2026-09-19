using System;
using UnityEngine;

namespace GloomBean.Foundation
{
    // Small composable authoring API. All spawned objects are normal inspectable Unity objects.
    public sealed class StageBuilder
    {
        public readonly Transform root;
        public readonly StageSession session;
        public Color stone=new Color(.32f,.28f,.39f),accent=new Color(.85f,.38f,.48f),background=new Color(.075f,.057f,.11f);
        int serial;
        public StageBuilder(Transform parent,StageSession run){root=parent;session=run;}
        public GameObject Solid(string name,Vector2 p,Vector2 size,Color? color=null,int layer=Layers.Terrain)
        {
            var obj=PrimitiveArt.Shape(name,root,p,Vector2.one,color??stone);
            var sr=obj.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Tiled;sr.size=size;
            var box=obj.AddComponent<BoxCollider2D>();box.size=size;obj.layer=layer;return obj;
        }
        public GameObject Platform(Vector2 p,Vector2 size){return Solid("Platform",p,size,accent,Layers.Moving);}
        public GameObject Floor(float x,float y,float w){return Solid("Floor",new Vector2(x,y-.5f),new Vector2(w,1));}
        public GameObject Wall(float x,float y,float h){return Solid("Wall",new Vector2(x,y),new Vector2(1,h));}
        public GameObject Ramp(Vector2 start,Vector2 end,float depth=1)
        {
            Vector2 delta=end-start;var obj=Solid("Slope",(start+end)*.5f-Vector2.up*depth*.5f,new Vector2(Mathf.Abs(delta.x),Mathf.Abs(delta.y)+depth));
            UnityEngine.Object.Destroy(obj.GetComponent<BoxCollider2D>());
            var polygon=obj.AddComponent<PolygonCollider2D>();Vector2 origin=obj.transform.position;
            polygon.points=new[]{start-origin,end-origin,end-origin-Vector2.up*depth,start-origin-Vector2.up*depth};
            obj.GetComponent<SpriteRenderer>().enabled=false;
            var mesh=new Mesh();mesh.vertices=new[]{(Vector3)(start-origin),(Vector3)(end-origin),(Vector3)(end-origin-Vector2.up*depth),(Vector3)(start-origin-Vector2.up*depth)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.colors=new[]{stone,stone,stone*.65f,stone*.65f};
            var art=new GameObject("Slope mesh");art.transform.SetParent(obj.transform,false);art.AddComponent<MeshFilter>().mesh=mesh;var mr=art.AddComponent<MeshRenderer>();mr.sharedMaterial=new Material(Shader.Find("Sprites/Default"));mr.sortingOrder=0;
            return obj;
        }
        public ActorMotor Player(Vector2 position)
        {
            var go=new GameObject("Host");go.transform.SetParent(root);go.transform.position=position;
            go.AddComponent<Rigidbody2D>();go.AddComponent<CapsuleCollider2D>();var actor=go.AddComponent<ActorMotor>();
            var tuning=Resources.Load<MovementTuning>("MovementTuning");if(tuning)actor.tuning=tuning;
            actor.input=go.AddComponent<HumanInput>();go.AddComponent<ActorView>();session.player=actor;return actor;
        }
        public CarryableEnemy Enemy(Vector2 p,bool ledges=false,bool armored=false,int direction=-1)
        {
            var obj=Solid("Patrol "+serial++,p,new Vector2(.95f,.85f),armored?new Color(.42f,.53f,.62f):new Color(.78f,.37f,.28f),Layers.Enemy);
            obj.AddComponent<Rigidbody2D>();var enemy=obj.AddComponent<CarryableEnemy>();enemy.avoidLedges=ledges;enemy.armored=armored;enemy.direction=direction;obj.AddComponent<EnemyView>();return enemy;
        }
        public GameObject Prop(Vector2 p,Vector2 size,float mass=1,bool metal=false)
        {
            var g=Solid(metal?"Iron block":"Movable block",p,size,metal?new Color(.5f,.64f,.7f):new Color(.67f,.5f,.29f),Layers.Prop);var rb=g.AddComponent<Rigidbody2D>();rb.mass=mass;rb.gravityScale=3.4f;rb.freezeRotation=true;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;g.AddComponent<Pushable>().metal=metal;return g;
        }
        public Breakable Break(Vector2 p,Vector2 size,int power,bool down=false)
        {
            var g=Solid("Break tier "+power,p,size,power==3?new Color(.73f,.24f,.48f):power==2?new Color(.7f,.46f,.3f):new Color(.52f,.5f,.57f));
            g.GetComponent<SpriteRenderer>().sprite=PrimitiveArt.Sprite(PrimitiveArt.Icon.Stripe);var b=g.AddComponent<Breakable>();b.requiredPower=power;b.downwardOnly=down;return b;
        }
        public GameObject Trigger(string name,Vector2 p,Vector2 size,Color color,PrimitiveArt.Icon icon=PrimitiveArt.Icon.Block)
        {
            var go=PrimitiveArt.Shape(name,root,p,Vector2.one,color,icon,4);var sr=go.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Sliced;sr.size=size;
            go.AddComponent<BoxCollider2D>().size=size;go.GetComponent<BoxCollider2D>().isTrigger=true;go.layer=Layers.Sensor;return go;
        }
        public WaterVolume Water(Vector2 p,Vector2 size,Vector2 current=default)
        {
            var g=Trigger("Water",p,size,new Color(.15f,.57f,.68f,.35f));var w=g.AddComponent<WaterVolume>();w.current=current;PrimitiveArt.Line("surface",g.transform,p+new Vector2(-size.x*.5f,size.y*.5f),p+new Vector2(size.x*.5f,size.y*.5f),.08f,new Color(.47f,.83f,.86f));return w;
        }
        public Hazard Spikes(float x,float y,float width,bool instant=false)
        {
            var g=Trigger("Thorns",new Vector2(x,y+.18f),new Vector2(width,.38f),new Color(.92f,.29f,.38f),PrimitiveArt.Icon.Spike);var hazard=g.AddComponent<Hazard>();hazard.instant=instant;return hazard;
        }
        public MotionPlatform Slider(Vector2 a,Vector2 b,Vector2 size,float speed=2)
        {var obj=Platform(a,size);var p=obj.AddComponent<MotionPlatform>();p.origin=a;p.end=b;p.speed=speed;return p;}
        public Carousel Wheel(Vector2 center,float radius=3,float speed=.6f)
        {var g=PrimitiveArt.Shape("Carousel hub",root,center,Vector2.one,accent,PrimitiveArt.Icon.Round);var c=g.AddComponent<Carousel>();c.Configure(this,center,radius,speed);return c;}
        public Pickup Collect(PickupKind kind,Vector2 p,string id=null,int amount=1)
        {
            var icon=kind==PickupKind.Key?PrimitiveArt.Icon.Key:kind==PickupKind.Mercy?PrimitiveArt.Icon.Star:PrimitiveArt.Icon.Diamond;
            var c=kind==PickupKind.Mercy?new Color(.96f,.91f,.65f):kind==PickupKind.Shard?new Color(.36f,.83f,.83f):new Color(.95f,.72f,.23f);
            var go=Trigger(kind.ToString(),p,Vector2.one*(kind==PickupKind.Mercy? .6f:.45f),c,icon);var pickup=go.AddComponent<Pickup>();pickup.kind=kind;pickup.stableId=id??session.definition.id+"-"+kind+"-"+serial++;pickup.amount=amount;return pickup;
        }
        public void CoinLine(Vector2 a,Vector2 b,int count=6)
        {for(int i=0;i<count;i++)Collect(PickupKind.Coin,Vector2.Lerp(a,b,(float)i/Mathf.Max(1,count-1)));}
        public TurnSwitch Nail(Vector2 p)
        {
            var go=Solid("Return switch",p,new Vector2(1.3f,.75f),new Color(.53f,.73f,.3f));
            PrimitiveArt.Shape("switch cap",go.transform,p+new Vector2(0,.44f),new Vector2(1,.16f),new Color(1,.8f,.34f));
            PrimitiveArt.Label("TURN",root,p+new Vector2(0,1),.1f);return go.AddComponent<TurnSwitch>();
        }
        public ExitPortal Exit(Vector2 p)
        {
            var go=Trigger("Return portal",p,new Vector2(1.5f,2.2f),new Color(.33f,.83f,.72f),PrimitiveArt.Icon.Arch);return go.AddComponent<ExitPortal>();
        }
        public Checkpoint Check(Vector2 p)
        {var go=Trigger("Checkpoint",p,new Vector2(.4f,1.5f),new Color(.44f,.73f,.79f));return go.AddComponent<Checkpoint>();}
        public PressurePlate Plate(Vector2 p,float minMass=.3f)
        {var go=Trigger("Plate "+serial++,p,new Vector2(1.5f,.32f),new Color(.96f,.7f,.25f));var plate=go.AddComponent<PressurePlate>();plate.requiredMass=minMass;return plate;}
        public Gate Door(Vector2 p,Vector2 size,params PressurePlate[] plates)
        {var go=Solid("Gate",p,size,new Color(.54f,.38f,.58f));var gate=go.AddComponent<Gate>();gate.plates=plates;return gate;}
        public Lever Switch(Vector2 p,string label="LEVER")
        {var g=Trigger("Lever",p,new Vector2(.7f,.8f),accent,PrimitiveArt.Icon.Key);PrimitiveArt.Label(label,root,p+Vector2.up*.7f,.085f);return g.AddComponent<Lever>();}
        public void Tip(Vector2 p,string message,float width=3)
        {var g=Trigger("Tip",p,new Vector2(width,4),Color.clear);g.AddComponent<TipZone>().message=message;}
        public void Decor(Rect extent,string landmark,int theme=0)
        {
            UnityEngine.Random.InitState(217+theme);
            PrimitiveArt.Shape("Sky",root,extent.center,extent.size+Vector2.one*100,background,PrimitiveArt.Icon.Block,-30);
            for(int i=0;i<24;i++)
            {
                float x=extent.xMin+i*extent.width/23f,y=extent.yMin+UnityEngine.Random.Range(3f,14f);
                Color c=Color.Lerp(background,stone,.15f+UnityEngine.Random.value*.14f);
                PrimitiveArt.Shape("Distant architecture",root,new Vector2(x,y),new Vector2(3+UnityEngine.Random.value*3,15+UnityEngine.Random.value*20),c,theme%3==0?PrimitiveArt.Icon.Arch:PrimitiveArt.Icon.Block,-20);
            }
            PrimitiveArt.Label(landmark.ToUpperInvariant(),root,new Vector2(extent.xMin+13,extent.yMin+17),.26f,Color.Lerp(stone,background,.3f));
        }
        public void Bounds(Rect bounds)
        {
            var camera=session.Camera;if(camera)camera.bounds=bounds;
            var g=Trigger("Fall reset",new Vector2(bounds.center.x,bounds.yMin-6),new Vector2(bounds.width+80,4),Color.clear);g.AddComponent<KillPlane>();
        }
    }
}
