using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public static class BroadArt
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        public static int Profile(StageSession s){if(!s)return 1;if(s.definition.boss)return 20+int.Parse(s.definition.worldId.Substring(1));if(s.definition.course==1&&GameRoot.Instance&&!GameRoot.Instance.IsCorrupted)return 0;return Mathf.Clamp(s.definition.course,1,20);}
        public static Sprite Get(string name,float ppu=16){string key=name+"@"+ppu;if(cache.TryGetValue(key,out var sprite))return sprite;var t=Resources.Load<Texture2D>("BroadVisual/"+name);if(!t)return null;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Repeat;sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),ppu,0,SpriteMeshType.FullRect);sprite.name="Broad art "+name;cache[key]=sprite;return sprite;}
        public static SpriteRenderer Picture(Transform parent,string name,int order){var go=new GameObject("Broad visual / "+name);go.transform.SetParent(parent,false);var sr=go.AddComponent<SpriteRenderer>();sr.sortingOrder=order;return sr;}
        public static void Fit(SpriteRenderer sr,Vector2 size){if(!sr.sprite)return;var b=sr.sprite.bounds.size;sr.transform.localScale=new Vector3(size.x/b.x,size.y/b.y,1);}
        public static bool Eligible(SpriteRenderer sr){if(!sr||sr.name.StartsWith("V6 visual /")||sr.name.StartsWith("Broad visual /"))return false;var c=sr.GetComponent<BoxCollider2D>();return c&&!c.isTrigger&&sr.sortingOrder<7&&!sr.GetComponentInParent<ActorMotor>()&&!sr.GetComponentInParent<CarryableEnemy>()&&!sr.GetComponent<KneelingFigure>();}
        public static int Material(SpriteRenderer sr,int profile){if(sr.GetComponent<MagneticBody>())return 17;if(sr.GetComponent<EdibleChunk>())return 6;return profile;}
    }
    [DefaultExecutionOrder(235)]
    public sealed class BroadSurface:MonoBehaviour
    {
        public SpriteRenderer Source {get;private set;}public int Profile {get;private set;}public bool Applied=>face&&face.sprite&&Source&&Source.forceRenderingOff;
        StageSession session;SpriteRenderer face,trim;readonly List<SpriteRenderer> backing=new List<SpriteRenderer>();Vector2 previous;int last=-1;bool tint;
        public void Initialize(SpriteRenderer source){Source=source;session=GetComponentInParent<StageSession>();tint=source.GetComponent<MagneticBody>();face=BroadArt.Picture(transform,"solid material",source.sortingOrder);face.drawMode=SpriteDrawMode.Tiled;trim=BroadArt.Picture(transform,"contact cornice",source.sortingOrder+1);trim.drawMode=SpriteDrawMode.Tiled;Update();}
        void Update(){if(!Source||!face||!Source.sprite)return;Profile=BroadArt.Material(Source,BroadArt.Profile(session));Vector2 size=Source.drawMode==SpriteDrawMode.Simple?(Vector2)Source.sprite.bounds.size:Source.size;size=new Vector2(Mathf.Max(.025f,size.x),Mathf.Max(.025f,size.y));bool horizontal=size.x>1.6f&&size.y<size.x*.7f;
            if(last!=Profile){last=Profile;face.sprite=BroadArt.Get("face_"+Profile);trim.sprite=BroadArt.Get("trim_"+Profile);previous=Vector2.zero;}
            face.size=size;face.enabled=Source.enabled&&face.sprite;face.color=tint?Source.color:new Color(1,1,1,Source.color.a);Source.forceRenderingOff=face.sprite!=null;
            float h=Mathf.Min(size.y,.5f);trim.size=new Vector2(size.x,h);trim.transform.localPosition=new Vector3(0,(size.y-h)*.5f,0);trim.enabled=Source.enabled&&horizontal;trim.color=face.color;
            if((size-previous).sqrMagnitude>.001f){previous=size;foreach(var sr in backing)if(sr)Destroy(sr.gameObject);backing.Clear();
                // Physical surface stays exact. Recessed feet/braces are background decoration below the contact.
                if(horizontal&&size.y<.9f&&size.x>2&&!tint){int count=Mathf.Clamp(Mathf.FloorToInt(size.x/4),1,10);for(int i=0;i<count;i++){
                    var sr=BroadArt.Picture(transform,"recessed material support",Source.sortingOrder-3);sr.sprite=BroadArt.Get("apron_"+Profile);float w=Mathf.Min(3.6f,size.x/count-.1f),height=Mathf.Min(1.7f,w*.65f);BroadArt.Fit(sr,new Vector2(w,height));sr.transform.localPosition=new Vector3((i+.5f)*size.x/count-size.x*.5f,-size.y*.5f-height*.45f,0);sr.color=new Color(.7f,.7f,.8f,.82f);backing.Add(sr);
                }}
                // Full-length ordinary floor receives architectural room bays throughout the stage.
                // This rule is object-driven, never a screenshot/location whitelist.
                if(horizontal&&size.y>=.8f&&size.x>=3.5f&&!Source.GetComponent<Rigidbody2D>()&&Source.name=="Floor"){
                    int count=Mathf.Clamp(Mathf.CeilToInt(size.x/8f),1,48);for(int i=0;i<count;i++){
                        var sr=BroadArt.Picture(transform,"level architecture bay",-26);sr.sprite=BroadArt.Get("bay_"+BroadArt.Profile(session));float width=Mathf.Min(8,size.x);BroadArt.Fit(sr,new Vector2(width,7));sr.transform.localPosition=new Vector3((i+.5f)*size.x/count-size.x*.5f,size.y*.5f+3.5f,.5f);sr.color=new Color(.82f,.82f,.90f,1);backing.Add(sr);
                    }
                }
            }
            foreach(var sr in backing)if(sr)sr.enabled=Source.enabled;
        }
        void OnDestroy(){if(Source)Source.forceRenderingOff=false;}
    }
    [DefaultExecutionOrder(242)]
    public sealed class BroadSlope:MonoBehaviour
    {
        public MeshRenderer Source;MeshRenderer view;Mesh mesh;StageSession session;int last=-1;
        public void Initialize(MeshRenderer source){Source=source;session=GetComponentInParent<StageSession>();var go=new GameObject("Broad visual / slope surface");go.transform.SetParent(source.transform,false);view=go.AddComponent<MeshRenderer>();view.sortingOrder=source.sortingOrder;mesh=Instantiate(source.GetComponent<MeshFilter>().sharedMesh);var colors=new Color[mesh.vertexCount];for(int i=0;i<colors.Length;i++)colors[i]=Color.white;mesh.colors=colors;var vertices=mesh.vertices;var uv=new Vector2[vertices.Length];for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(vertices[i].x/4,vertices[i].y/2);mesh.uv=uv;go.AddComponent<MeshFilter>().sharedMesh=mesh;view.sharedMaterial=new Material(Shader.Find("Sprites/Default"));}
        void LateUpdate(){if(!view||!Source)return;int n=BroadArt.Profile(session);if(n!=last){last=n;var image=BroadArt.Get("face_"+n);if(image)view.sharedMaterial.mainTexture=image.texture;}Source.forceRenderingOff=true;view.enabled=Source.enabled;}
        void OnDestroy(){if(Source)Source.forceRenderingOff=false;if(view&&view.sharedMaterial)Destroy(view.sharedMaterial);if(mesh)Destroy(mesh);}
    }
    [DefaultExecutionOrder(245)]
    public sealed class BroadWorldDressing:MonoBehaviour
    {
        StageSession session;float next;
        public int EligibleCount=>GetComponentsInChildren<SpriteRenderer>(true).Count(BroadArt.Eligible);
        public int AppliedCount=>GetComponentsInChildren<BroadSurface>(true).Count(v=>v.Applied);
        public int VisualColliders=>GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Broad visual /")).Sum(t=>t.GetComponents<Collider2D>().Length);
        public void Initialize(){session=GetComponent<StageSession>();Scan();}
        public void Scan(){foreach(var sr in GetComponentsInChildren<SpriteRenderer>(true))if(BroadArt.Eligible(sr)&&!sr.GetComponent<BroadSurface>()&&!sr.GetComponent<V6Surface>())sr.gameObject.AddComponent<BroadSurface>().Initialize(sr);
            foreach(var sr in GetComponentsInChildren<MeshRenderer>(true))if(sr.name=="Slope mesh"&&!sr.GetComponent<BroadSlope>())sr.gameObject.AddComponent<BroadSlope>().Initialize(sr);
            foreach(var water in GetComponentsInChildren<WaterVolume>(true))if(!water.GetComponent<BroadWaterArt>())water.gameObject.AddComponent<BroadWaterArt>();
            foreach(var rail in GetComponentsInChildren<RailPath>(true))if(!rail.GetComponent<BroadRailArt>())rail.gameObject.AddComponent<BroadRailArt>();
            foreach(var orbit in GetComponentsInChildren<Carousel>(true))if(!orbit.GetComponent<BroadCarouselArt>())orbit.gameObject.AddComponent<BroadCarouselArt>();
        }
        void Update(){if(Time.unscaledTime>=next){next=Time.unscaledTime+.75f;Scan();}}
    }
    public sealed class BroadRailArt:MonoBehaviour
    {
        RailPath rail;SpriteRenderer a,b;
        void Start(){rail=GetComponent<RailPath>();a=BroadArt.Picture(transform,"rail bearing A",1);b=BroadArt.Picture(transform,"rail bearing B",1);a.sprite=b.sprite=BroadArt.Get("pulley");}
        void LateUpdate(){if(!rail||!a)return;a.transform.position=rail.Point(0);b.transform.position=rail.Point(1);a.transform.localRotation=Quaternion.Euler(0,0,Time.time*20);b.transform.localRotation=Quaternion.Euler(0,0,-Time.time*20);}
    }
    public sealed class BroadCarouselArt:MonoBehaviour
    {
        Carousel orbit;SpriteRenderer gear;LineRenderer[] rods;
        void Start(){orbit=GetComponent<Carousel>();gear=BroadArt.Picture(transform,"carousel bearing",0);gear.sprite=BroadArt.Get("pulley");rods=new LineRenderer[orbit.arms.Length];}
        void LateUpdate(){if(!gear)return;gear.transform.localRotation=Quaternion.Euler(0,0,Time.time*orbit.angularSpeed*Mathf.Rad2Deg);
            for(int i=0;i<orbit.arms.Length;i++)if(orbit.arms[i]){var old=transform.Find("spoke"+i);if(old){var r=old.GetComponent<LineRenderer>();if(r)r.enabled=false;}rods[i]=PrimitiveArt.Line("Broad visual / structural spoke "+i,transform,transform.position,orbit.arms[i].transform.position,.17f,new Color(.42f,.40f,.49f),-2);}
        }
    }
    [DefaultExecutionOrder(250)]
    public sealed class BroadWaterArt:MonoBehaviour
    {
        WaterVolume water;SpriteRenderer source,body,surface;BoxCollider2D shape;float elapsed;
        void Start(){water=GetComponent<WaterVolume>();source=GetComponent<SpriteRenderer>();shape=GetComponent<BoxCollider2D>();body=BroadArt.Picture(transform,"water depth",source?source.sortingOrder:1);body.sprite=BroadArt.Get("water");body.drawMode=SpriteDrawMode.Tiled;surface=BroadArt.Picture(transform,"water contact",3);surface.sprite=BroadArt.Get("water");surface.drawMode=SpriteDrawMode.Tiled;}
        void LateUpdate(){if(!shape||!body)return;if(source)source.forceRenderingOff=body.sprite!=null;body.size=shape.size;body.color=new Color(.72f,1,1,1);body.enabled=shape.enabled&&(!source||source.enabled);surface.size=new Vector2(shape.size.x,.16f);surface.transform.localPosition=new Vector3(shape.offset.x,shape.offset.y+shape.size.y*.5f-.08f,0);surface.color=new Color(.8f,1,.94f,1);surface.enabled=body.enabled;}
        void OnDestroy(){if(source)source.forceRenderingOff=false;}
    }

}
