using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Object-based presentation. No test/camera coordinates, physics writes or gameplay predicates.
    public static class QualityBarArt
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        public static Sprite Get(string name,float ppu=16){string key=name+"@"+ppu;if(cache.TryGetValue(key,out var result))return result;var texture=Resources.Load<Texture2D>("QualityBar/"+name);if(!texture)return null;texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Repeat;result=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,ppu,0,SpriteMeshType.FullRect);result.name="QualityBar art / "+name;cache[key]=result;return result;}
        public static SpriteRenderer Picture(Transform parent,string name,int order){var go=new GameObject("QualityBar visual / "+name);go.transform.SetParent(parent,false);var sr=go.AddComponent<SpriteRenderer>();sr.sortingOrder=order;return sr;}
        public static void Fit(SpriteRenderer sr,Vector2 dimensions){if(!sr||!sr.sprite)return;var size=sr.sprite.bounds.size;sr.transform.localScale=new Vector3(dimensions.x/size.x,dimensions.y/size.y,1);}
        public static int Group(StageSession session){if(session.definition.course==1&&!GameRoot.Instance.IsCorrupted)return 0;return Mathf.Clamp(int.Parse(session.definition.worldId.Substring(1)),1,5);}
        public static string Motif(StageDefinition stage){if(stage.boss)return new[]{"bell","pump","balcony","coffin","statue"}[int.Parse(stage.worldId.Substring(1))-1];switch(stage.course){case 1:return "balcony";case 2:return "bell";case 3:return "washer";case 4:return "skin";case 5:return "pump";case 6:return "oven";case 7:return "pump";case 8:return "book";case 9:case 10:case 12:return "balcony";case 11:return "ledger";case 13:case 14:return "coffin";case 15:return "coffin";case 16:return "statue";case 17:case 18:return "halo";case 19:return "book";default:return "statue";}}
        public static bool Eligible(SpriteRenderer sr){return BroadArt.Eligible(sr)&&!sr.name.StartsWith("QualityBar visual /");}
    }
    [DefaultExecutionOrder(275)]
    public sealed class QualityBarSurface:MonoBehaviour
    {
        public SpriteRenderer Source {get;private set;}
        public bool Applied=>Source&&fascia&&fascia.sprite&&fascia.enabled==Source.enabled;
        StageSession session;int group=-1;Vector2 previous;SpriteRenderer fascia,capLeft,capRight,main,contact;readonly List<SpriteRenderer> details=new List<SpriteRenderer>();bool semanticTint;
        public void Initialize(SpriteRenderer sr){Source=sr;session=GetComponentInParent<StageSession>();semanticTint=GetComponent<MagneticBody>()||GetComponent<DepthGeometry>();main=QualityBarArt.Picture(transform,"physical material face",sr.sortingOrder);main.drawMode=SpriteDrawMode.Tiled;contact=QualityBarArt.Picture(transform,"physical contact edge",sr.sortingOrder+1);contact.drawMode=SpriteDrawMode.Tiled;fascia=QualityBarArt.Picture(transform,"recessed load housing",sr.sortingOrder-2);fascia.drawMode=SpriteDrawMode.Tiled;capLeft=QualityBarArt.Picture(transform,"left housing bracket",sr.sortingOrder-2);capRight=QualityBarArt.Picture(transform,"right housing bracket",sr.sortingOrder-2);LateUpdate();}
        void LateUpdate()
        {
            if(!Source||!Source.sprite||!session)return;int next=QualityBarArt.Group(session);bool magnetic=GetComponent<MagneticBody>();if(magnetic)next=5;
            Vector2 size=Source.drawMode==SpriteDrawMode.Simple?(Vector2)Source.sprite.bounds.size:Source.size;
            if(next!=group||(size-previous).sqrMagnitude>.001f){group=next;previous=size;Rebuild(size);}
            Color color=semanticTint?new Color(Source.color.r,Source.color.g,Source.color.b,Source.color.a):new Color(.98f,.91f,.93f,Source.color.a);
            bool visible=Source.enabled;main.enabled=visible;main.color=color;contact.enabled=visible&&size.x>1.6f&&size.y<size.x*.65f;contact.color=color;fascia.enabled=visible;fascia.color=color;capLeft.enabled=capRight.enabled=visible;capLeft.color=capRight.color=color;
            foreach(var sr in details){if(!sr)continue;sr.enabled=visible;var tint=sr.color;tint.a=Source.color.a*(sr.name.Contains("light cone")?.50f:sr.name.Contains("rear")?.72f:.88f);sr.color=tint;}
        }
        void Add(string art,string name,Vector2 at,Vector2 dimensions,int order,Color tint){var sr=QualityBarArt.Picture(transform,name,order);sr.sprite=QualityBarArt.Get(art);QualityBarArt.Fit(sr,dimensions);sr.transform.localPosition=new Vector3(at.x,at.y,.08f);sr.color=tint;details.Add(sr);}
        void Rebuild(Vector2 size)
        {
            foreach(var old in details)if(old)Destroy(old.gameObject);details.Clear();
            bool horizontal=size.x>1.6f&&size.y<size.x*.65f;bool thin=horizontal&&size.y<1.2f;
            main.sprite=QualityBarArt.Get("solid_"+group);main.size=size;contact.sprite=QualityBarArt.Get("edge_"+group);contact.size=new Vector2(size.x,.5f);float edgeHeight=Mathf.Min(size.y,.28f);contact.transform.localScale=new Vector3(1,edgeHeight/.5f,1);contact.transform.localPosition=new Vector3(0,size.y*.5f-edgeHeight*.5f,0);
            foreach(Transform child in transform){if(child.name=="Broad visual / solid material"||child.name=="Broad visual / contact cornice"||child.name=="Broad visual / left material edge"||child.name=="Broad visual / right material edge"){var sr=child.GetComponent<SpriteRenderer>();if(sr)sr.forceRenderingOff=true;}}
            // This is recessed ornament behind the collision/actor layer. The existing sharp contact trim stays at the physical top.
            if(horizontal){fascia.sprite=QualityBarArt.Get("fascia_"+group);float depth=thin?Mathf.Min(1.35f,size.x*.32f):Mathf.Min(size.y,1.7f);
                fascia.size=new Vector2(Mathf.Max(.1f,size.x),2);fascia.transform.localScale=new Vector3(1,depth/2,1);fascia.transform.localPosition=new Vector3(0,size.y*.5f-depth*.5f-.025f,0);
                capLeft.sprite=capRight.sprite=QualityBarArt.Get("bracket_"+group);float capW=Mathf.Min(.55f,size.x*.2f);float capH=Mathf.Min(1.2f,depth+.2f);QualityBarArt.Fit(capLeft,new Vector2(capW,capH));QualityBarArt.Fit(capRight,new Vector2(capW,capH));capLeft.transform.localPosition=new Vector3(-size.x*.5f+capW*.5f,size.y*.5f-capH*.5f,0);capRight.transform.localPosition=new Vector3(size.x*.5f-capW*.5f,size.y*.5f-capH*.5f,0);capRight.flipX=true;
                if(size.x>=3&&!semanticTint){int count=Mathf.Clamp(Mathf.CeilToInt(size.x/7),1,32);for(int i=0;i<count;i++){float x=(i+.5f)*size.x/count-size.x*.5f;float w=Mathf.Min(5.8f,size.x/count);Add("truss_"+group,"load-bearing rear truss",new Vector2(x,-size.y*.5f-.72f),new Vector2(w,1.8f),-8,new Color(.80f,.72f,.81f,.85f));}}
                // Room dressing follows all full floor spans; never a scene-camera or verification-coordinate whitelist.
                if(Source.name=="Floor"&&size.x>=5&&!GetComponent<Rigidbody2D>()){
                    int bays=Mathf.Clamp(Mathf.CeilToInt(size.x/12f),1,35);string motif=QualityBarArt.Motif(session.definition);
                    for(int i=0;i<bays;i++){float span=size.x/bays;float x=(i+.5f)*span-size.x*.5f;float top=size.y*.5f;
                        Add("column_"+group,"rear structural pier",new Vector2(x-span*.42f,top+3.4f),new Vector2(group==2?1.65f:1.1f,7),-21,new Color(.63f,.57f,.70f,.8f));
                        float h=motif=="washer"||motif=="oven"?4.6f:motif=="ledger"||motif=="balcony"?4.8f:4.0f;
                        float w=motif=="washer"||motif=="oven"?4.0f:3.0f;
                        bool low=motif=="washer"||motif=="oven"||motif=="pump";float y=low?top-h*.51f:top+h*.52f;
                        Add(motif,"rear "+motif,new Vector2(x+span*.12f,y),new Vector2(w,h),-18,new Color(.77f,.66f,.75f,.86f));
                        if(group!=2){float lx=x-span*.21f;Add("lamp_"+group,"rear hanging lamp",new Vector2(lx,top+5.0f),new Vector2(1.4f,1.0f),-17,new Color(1,.91f,.76f,1));Add("light_"+group,"rear light cone",new Vector2(lx,top+2.55f),new Vector2(3.6f,4.4f),-19,new Color(1,1,1,.5f));Add("chain_"+group,"rear lamp chain",new Vector2(lx,top+6.2f),new Vector2(.16f,1.8f),-20,Color.white);}
                    }
                }
                if(Source.name!="Floor"&&size.x>=2.5f&&size.y<2f&&!GetComponent<Rigidbody2D>()&&!semanticTint){
                    string motif=QualityBarArt.Motif(session.definition);float width=Mathf.Min(size.x*.95f,4f);float height=motif=="washer"?width*.8f:width*1.1f;bool above=motif=="balcony"||motif=="ledger"||motif=="statue";
                    float top=size.y*.5f;float y=above?top+height*.48f:top-height*.60f;
                    Add(motif,"rear elevated "+motif,new Vector2(0,y),new Vector2(width,height),-17,new Color(.88f,.78f,.83f,.9f));
                    if(group!=2){float lx=-width*.36f;Add("lamp_"+group,"rear elevated lamp",new Vector2(lx,top+3.6f),new Vector2(1.2f,.82f),-15,new Color(1,.96f,.86f,1));Add("light_"+group,"rear elevated light cone",new Vector2(lx,top+1.62f),new Vector2(3.0f,3.65f),-16,Color.white);}
                }
                if(GetComponent<MotionPlatform>()||GetComponent<Rigidbody2D>()&&Source.name.Contains("Halo")){
                    Add("gear_"+group,"moving axle",new Vector2(0,size.y*.5f-1.1f),new Vector2(1.2f,1.2f),-3,new Color(1,.93f,.84f,1));
                }
            }else{
                fascia.sprite=QualityBarArt.Get("column_"+group);fascia.drawMode=SpriteDrawMode.Tiled;fascia.size=new Vector2(2,6);fascia.transform.localScale=new Vector3(Mathf.Max(.1f,size.x)/2,Mathf.Max(.1f,size.y)/6,1);fascia.transform.localPosition=Vector3.zero;
                capLeft.sprite=capRight.sprite=null;
                if((Source.name.ToLowerInvariant().Contains("sheet")||Source.name.ToLowerInvariant().Contains("skin"))&&size.y>2){fascia.sprite=QualityBarArt.Get("skin");fascia.drawMode=SpriteDrawMode.Simple;QualityBarArt.Fit(fascia,size);}
            }
        }
    }
    [DefaultExecutionOrder(285)]
    public sealed class QualityBarRail:MonoBehaviour
    {
        RailPath rail;SpriteRenderer[] housings=new SpriteRenderer[2];LineRenderer cable;
        public void Initialize(RailPath path){rail=path;for(int i=0;i<2;i++){housings[i]=QualityBarArt.Picture(transform,"pulley housing "+i,-1);housings[i].sprite=QualityBarArt.Get("gear_1");QualityBarArt.Fit(housings[i],new Vector2(1.3f,1.3f));}}
        void LateUpdate(){if(!rail)return;for(int i=0;i<2;i++){housings[i].transform.position=rail.Point(i);housings[i].transform.localRotation=Quaternion.Euler(0,0,rail.loose?Mathf.Sin(Time.time*1.8f)*10:0);}}
    }
    [DefaultExecutionOrder(290)]
    public sealed class QualityBarWorld:MonoBehaviour
    {
        StageSession session;float next;readonly HashSet<SpriteRenderer> decorated=new HashSet<SpriteRenderer>();
        public int AppliedCount=>GetComponentsInChildren<QualityBarSurface>(true).Count(v=>v.Applied);
        public int EligibleCount=>GetComponentsInChildren<SpriteRenderer>(true).Count(QualityBarArt.Eligible);
        public int AddedColliders=>GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("QualityBar visual /")).Sum(t=>t.GetComponents<Collider2D>().Length);
        public void Initialize(){session=GetComponent<StageSession>();Scan();}
        public void Scan(){foreach(var sr in GetComponentsInChildren<SpriteRenderer>(true))if(QualityBarArt.Eligible(sr)&&!sr.GetComponent<QualityBarSurface>())sr.gameObject.AddComponent<QualityBarSurface>().Initialize(sr);
            foreach(var rail in GetComponentsInChildren<RailPath>(true))if(!rail.GetComponent<QualityBarRail>())rail.gameObject.AddComponent<QualityBarRail>().Initialize(rail);
            foreach(var sr in GetComponentsInChildren<SpriteRenderer>(true)){if(sr.name=="Broad visual / level architecture bay")sr.color=new Color(.48f,.43f,.58f,sr.color.a);else if(sr.name=="Broad visual / recessed material support")sr.forceRenderingOff=true;}
            var host=session.player?session.player.GetComponent<HostController>():null;if(host&&!host.GetComponent<QualityBarAnchor>())host.gameObject.AddComponent<QualityBarAnchor>();
        }
        void Update(){if(Time.unscaledTime>=next){next=Time.unscaledTime+.75f;Scan();}}

    }
    [DefaultExecutionOrder(280)]
    public sealed class QualityBarAnchor:MonoBehaviour
    {
        HostController host;SpriteRenderer trolley;
        void Start(){host=GetComponent<HostController>();trolley=QualityBarArt.Picture(transform,"actual marionette rail trolley",15);trolley.sprite=QualityBarArt.Get("gear_1");QualityBarArt.Fit(trolley,new Vector2(.82f,.82f));}
        void LateUpdate(){if(!host||!trolley)return;MarionetteForm form=null;foreach(var f in host.Forms)if(f is MarionetteForm found)form=found;trolley.enabled=form!=null&&form.Joint;if(trolley.enabled){trolley.transform.position=form.Joint.connectedAnchor;trolley.transform.rotation=Quaternion.identity;}}
    }

}
