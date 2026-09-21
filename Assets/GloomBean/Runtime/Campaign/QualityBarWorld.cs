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
        public static Vector2 PropSize(string name,float width,float height){var sprite=Get(name);if(!sprite)return new Vector2(width,height);var b=sprite.bounds.size;float scale=Mathf.Min(width/Mathf.Max(.01f,b.x),height/Mathf.Max(.01f,b.y));return new Vector2(b.x*scale,b.y*scale);}
        public static int Group(StageSession session){
            if(!session||session.definition==null)return 1;
            if(session.definition.course==1&&GameRoot.Instance&&!GameRoot.Instance.IsCorrupted)return 0;
            string id=session.definition.worldId;int world;
            return !string.IsNullOrEmpty(id)&&id.Length>1&&int.TryParse(id.Substring(1),out world)?Mathf.Clamp(world,1,5):1;
        }
        public static string Motif(StageDefinition stage){if(stage.boss)return new[]{"bell","pump","balcony","coffin","statue"}[int.Parse(stage.worldId.Substring(1))-1];switch(stage.course){case 1:return "balcony";case 2:return "bell";case 3:return "washer";case 4:return "skin";case 5:return "fruit_cart";case 6:return "oven";case 7:return "garden_well";case 8:return "season_tree";case 9:case 12:return "tenement";case 10:return "balcony";case 11:return "ledger";case 13:return "coffin";case 14:return "spindle";case 15:return "coffin";case 16:return "statue";case 17:case 18:return "statue";case 19:return "book";default:return "statue";}}
        public static string MotifVariant(StageDefinition stage,int index){
            string motif=Motif(stage);int n=Mathf.Abs(index);
            if(motif=="washer")return new[]{"washer","wringer","wash_basket"}[n%3];
            if(motif=="skin")return n%2==0?"skin":"garment_rack";
            if(motif=="book"||motif=="ledger")return n%3==0?"bound_volume":motif;
            return motif;
        }
        public static bool Eligible(SpriteRenderer sr){return BroadArt.Eligible(sr)&&!sr.name.StartsWith("QualityBar visual /");}
    }
    [DefaultExecutionOrder(275)]
    public sealed class QualityBarSurface:MonoBehaviour
    {
        public SpriteRenderer Source {get;private set;}
        public bool Applied=>Source&&fascia&&fascia.sprite&&fascia.enabled==Source.enabled;
        StageSession session;int group=-1;Vector2 previous;SpriteRenderer fascia,capLeft,capRight,main,contact;readonly List<SpriteRenderer> details=new List<SpriteRenderer>();readonly Dictionary<SpriteRenderer,float> detailAlpha=new Dictionary<SpriteRenderer,float>();bool semanticTint,food,cloth;
        public void Initialize(SpriteRenderer sr){Source=sr;session=GetComponentInParent<StageSession>();semanticTint=GetComponent<MagneticBody>()||GetComponent<DepthGeometry>();food=GetComponent<EdibleChunk>();cloth=sr.name.ToLowerInvariant().Contains("sheet")||sr.name.ToLowerInvariant().Contains("skin");main=QualityBarArt.Picture(transform,"physical material face",sr.sortingOrder);main.drawMode=SpriteDrawMode.Tiled;contact=QualityBarArt.Picture(transform,"physical contact edge",sr.sortingOrder+1);contact.drawMode=SpriteDrawMode.Tiled;fascia=QualityBarArt.Picture(transform,"recessed load housing",sr.sortingOrder-2);fascia.drawMode=SpriteDrawMode.Tiled;capLeft=QualityBarArt.Picture(transform,"left housing bracket",sr.sortingOrder-2);capRight=QualityBarArt.Picture(transform,"right housing bracket",sr.sortingOrder-2);LateUpdate();}
        void LateUpdate()
        {
            if(!Source||!Source.sprite||!session)return;int next=QualityBarArt.Group(session);bool magnetic=GetComponent<MagneticBody>();if(magnetic)next=5;
            Vector2 size=Source.drawMode==SpriteDrawMode.Simple?(Vector2)Source.sprite.bounds.size:Source.size;
            if(next!=group||(size-previous).sqrMagnitude>.001f){group=next;previous=size;Rebuild(size);}
            Color color=semanticTint?new Color(Source.color.r,Source.color.g,Source.color.b,Source.color.a):new Color(1,1,1,Source.color.a);
            bool visible=Source.enabled;main.enabled=visible;main.color=color;contact.enabled=visible&&size.x>1.6f&&size.y<size.x*.65f;contact.color=color;fascia.enabled=visible;fascia.color=color;capLeft.enabled=capRight.enabled=visible;capLeft.color=capRight.color=color;
            foreach(var sr in details){if(!sr)continue;sr.enabled=visible;var tint=sr.color;tint.a=Source.color.a*(detailAlpha.TryGetValue(sr,out var authoredAlpha)?authoredAlpha:1f);sr.color=tint;}
        }
        void Add(string art,string name,Vector2 at,Vector2 dimensions,int order,Color tint){var sr=QualityBarArt.Picture(transform,name,order);sr.sprite=QualityBarArt.Get(art);QualityBarArt.Fit(sr,dimensions);sr.transform.localPosition=new Vector3(at.x,at.y,.08f);sr.color=tint;details.Add(sr);detailAlpha[sr]=tint.a;}
        void Rebuild(Vector2 size)
        {
            foreach(var old in details)if(old)Destroy(old.gameObject);details.Clear();detailAlpha.Clear();
            bool horizontal=size.x>1.6f&&size.y<size.x*.65f;bool thin=horizontal&&size.y<1.2f;
            main.sprite=QualityBarArt.Get(food?"solid_food":cloth?"solid_cloth":"solid_"+group);main.size=size;contact.sprite=QualityBarArt.Get("edge_"+group);contact.size=new Vector2(size.x,.5f);float edgeHeight=Mathf.Min(size.y,.28f);contact.transform.localScale=new Vector3(1,edgeHeight/.5f,1);contact.transform.localPosition=new Vector3(0,size.y*.5f-edgeHeight*.5f,0);
            foreach(Transform child in transform){if(child.name=="Broad visual / solid material"||child.name=="Broad visual / contact cornice"||child.name=="Broad visual / left material edge"||child.name=="Broad visual / right material edge"){var sr=child.GetComponent<SpriteRenderer>();if(sr)sr.forceRenderingOff=true;}}
            // This is recessed ornament behind the collision/actor layer. The existing sharp contact trim stays at the physical top.
            if(horizontal){fascia.sprite=QualityBarArt.Get(food?"solid_food":"fascia_"+group);float depth=thin?Mathf.Min(1.35f,size.x*.32f):Mathf.Min(size.y,1.7f);
                fascia.size=new Vector2(Mathf.Max(.1f,size.x),2);fascia.transform.localScale=new Vector3(1,depth/2,1);fascia.transform.localPosition=new Vector3(0,size.y*.5f-depth*.5f-.025f,0);
                capLeft.sprite=capRight.sprite=QualityBarArt.Get("bracket_"+group);float capW=Mathf.Min(.55f,size.x*.2f);float capH=Mathf.Min(1.2f,depth+.2f);QualityBarArt.Fit(capLeft,new Vector2(capW,capH));QualityBarArt.Fit(capRight,new Vector2(capW,capH));capLeft.transform.localPosition=new Vector3(-size.x*.5f+capW*.5f,size.y*.5f-capH*.5f,0);capRight.transform.localPosition=new Vector3(size.x*.5f-capW*.5f,size.y*.5f-capH*.5f,0);capRight.flipX=true;
                if(size.x>=3&&!semanticTint&&!food&&!cloth){int count=Mathf.Clamp(Mathf.CeilToInt(size.x/7),1,32);for(int i=0;i<count;i++){float x=(i+.5f)*size.x/count-size.x*.5f;float w=Mathf.Min(5.8f,size.x/count);Add("truss_"+group,"load-bearing rear truss",new Vector2(x,-size.y*.5f-.72f),new Vector2(w,1.8f),-8,new Color(.80f,.72f,.81f,.85f));}}
                // Room dressing follows all full floor spans; never a scene-camera or verification-coordinate whitelist.
                if(Source.name=="Floor"&&size.x>=5&&!GetComponent<Rigidbody2D>()&&!food&&!cloth){
                    int bays=Mathf.Clamp(Mathf.CeilToInt(size.x/12f),1,35);
                    for(int i=0;i<bays;i++){string motif=QualityBarArt.MotifVariant(session.definition,i+transform.GetSiblingIndex());float span=size.x/bays;float x=(i+.5f)*span-size.x*.5f;float top=size.y*.5f;
                        Add("column_"+group,"rear structural pier",new Vector2(x-span*.42f,top+3.4f),new Vector2(group==2?1.65f:1.1f,7),-21,new Color(.82f,.78f,.86f,1));
                        float maxH=motif=="statue"||motif=="season_tree"?6.0f:motif=="tenement"?4.8f:4.0f;float maxW=motif=="tenement"||motif=="season_tree"?6.2f:4.5f;
                        Vector2 prop=QualityBarArt.PropSize(motif,maxW,maxH);float w=prop.x,h=prop.y;
                        bool low=motif=="washer"||motif=="oven";float y=low?top-h*.15f:top+h*.49f;
                        Add(motif,"rear "+motif,new Vector2(x+span*.12f,y),new Vector2(w,h),-18,new Color(1,.94f,.96f,1));
                        if(group>=3){Add(group==5?"glass":"lancet","rear lit lancet",new Vector2(x-span*.28f,top+4.0f),group==5?new Vector2(3.5f,4.6f):new Vector2(1.5f,4.5f),-24,new Color(.86f,.79f,.93f,1));
                            Add("banner_detail","rear hanging banner",new Vector2(x+span*.33f,top+4.8f),new Vector2(1.2f,2.2f),-18,new Color(.9f,.83f,.94f,1));}
                        if(group!=0&&group!=2){float lx=x-span*.21f;Add("lamp_"+group,"rear hanging lamp",new Vector2(lx,top+5.0f),new Vector2(1.4f,1.0f),-17,new Color(1,.91f,.76f,1));Add("light_"+group,"rear light cone",new Vector2(lx,top+2.55f),new Vector2(3.6f,4.4f),-10,new Color(1,1,1,.5f));Add("chain_"+group,"rear lamp chain",new Vector2(lx,top+6.2f),new Vector2(.16f,1.8f),-20,Color.white);}
                    }
                }
                if(Source.name!="Floor"&&size.x>=2.5f&&size.y<2f&&(!GetComponent<Rigidbody2D>()||GetComponent<Rigidbody2D>().bodyType==RigidbodyType2D.Kinematic)&&!semanticTint&&!food&&!cloth&&!GetComponent<ProcessionCarrier>()){
                    string motif=QualityBarArt.MotifVariant(session.definition,transform.GetSiblingIndex());Vector2 prop=QualityBarArt.PropSize(motif,Mathf.Min(size.x*1.1f,5f),motif=="statue"||motif=="season_tree"?5.5f:4.4f);float width=prop.x,height=prop.y;
                    bool above=motif!="washer"&&motif!="oven"&&motif!="bell"&&motif!="coffin";
                    float top=size.y*.5f;float y=above?top+height*.48f:top-height*.24f;
                    Add(motif,"rear elevated "+motif,new Vector2(0,y),new Vector2(width,height),-17,new Color(1,.96f,.98f,1));
                    if(group>=3){Add(group==5?"glass":"lancet","rear elevated lancet",new Vector2(-width*.28f,top+3.0f),group==5?new Vector2(2.6f,3.4f):new Vector2(1.2f,3.6f),-24,new Color(.8f,.77f,.9f,1));
                        if(width>3)Add("banner_detail","rear elevated banner",new Vector2(width*.28f,top+2.8f),new Vector2(1,1.85f),-18,Color.white);}
                    if(group!=0&&group!=2){float lx=-width*.36f;Add("lamp_"+group,"rear elevated lamp",new Vector2(lx,top+3.6f),new Vector2(1.2f,.82f),-15,new Color(1,.96f,.86f,1));Add("light_"+group,"rear elevated light cone",new Vector2(lx,top+1.62f),new Vector2(3.0f,3.65f),-10,Color.white);}
                }
                if(GetComponent<MotionPlatform>()||GetComponent<Rigidbody2D>()&&Source.name.Contains("Halo")){
                    if(group==5)Add("halo_detail","moving halo ring",new Vector2(0,size.y*.5f-.8f),new Vector2(Mathf.Max(1.4f,size.x),1.4f),-1,Color.white);
                    if(group==1&&size.x>2.5f)Add("drapery","moving tray cloth",new Vector2(size.x*.24f,size.y*.5f-.85f),new Vector2(1.5f,1.7f),-2,new Color(.85f,.73f,.78f,1));
                    Add("gear_"+group,"moving axle",new Vector2(0,size.y*.5f-1.1f),new Vector2(1.2f,1.2f),-3,new Color(1,.93f,.84f,1));
                }
            }else{
                fascia.sprite=QualityBarArt.Get(food?"solid_food":cloth?"solid_cloth":"column_"+group);fascia.drawMode=SpriteDrawMode.Tiled;fascia.size=new Vector2(2,6);fascia.transform.localScale=new Vector3(Mathf.Max(.1f,size.x)/2,Mathf.Max(.1f,size.y)/6,1);fascia.transform.localPosition=Vector3.zero;
                capLeft.sprite=capRight.sprite=null;
                if((Source.name.ToLowerInvariant().Contains("sheet")||Source.name.ToLowerInvariant().Contains("skin"))&&size.y>2){main.sprite=QualityBarArt.Get("skin");main.drawMode=SpriteDrawMode.Simple;QualityBarArt.Fit(main,size);fascia.sprite=QualityBarArt.Get("skin");fascia.drawMode=SpriteDrawMode.Simple;QualityBarArt.Fit(fascia,size);}
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
        public void Initialize(){session=GetComponentInParent<StageSession>();Scan();}
        public void Scan(){foreach(var sr in GetComponentsInChildren<SpriteRenderer>(true))if(QualityBarArt.Eligible(sr)&&!sr.GetComponent<QualityBarSurface>())sr.gameObject.AddComponent<QualityBarSurface>().Initialize(sr);
            foreach(var rail in GetComponentsInChildren<RailPath>(true))if(!rail.GetComponent<QualityBarRail>())rail.gameObject.AddComponent<QualityBarRail>().Initialize(rail);
            foreach(var sr in GetComponentsInChildren<SpriteRenderer>(true)){if(sr.name=="Broad visual / level architecture bay")sr.color=new Color(.60f,.56f,.65f,sr.color.a);else if(sr.name=="Broad visual / recessed material support")sr.forceRenderingOff=true;}
            foreach(var carrier in GetComponentsInChildren<ProcessionCarrier>(true))if(!carrier.GetComponent<QualityBarProcession>())carrier.gameObject.AddComponent<QualityBarProcession>();
            foreach(var wheel in GetComponentsInChildren<SeasonWheel>(true))if(!wheel.GetComponent<QualityBarSeason>())wheel.gameObject.AddComponent<QualityBarSeason>();
            foreach(var source in GetComponentsInChildren<HostSource>(true))if(!source.GetComponent<TenantPixelView>())source.gameObject.AddComponent<TenantPixelView>();
            foreach(var enemy in GetComponentsInChildren<CarryableEnemy>(true))if(!enemy.GetComponent<PatrolPixelView>())enemy.gameObject.AddComponent<PatrolPixelView>().world=QualityBarArt.Group(session);
            foreach(var husk in GetComponentsInChildren<HuskBody>(true))if(!husk.GetComponent<QualityBarHusk>())husk.gameObject.AddComponent<QualityBarHusk>();
            foreach(var cure in GetComponentsInChildren<HostCure>(true)){var sr=cure.GetComponent<SpriteRenderer>();if(sr&&!cure.GetComponent<GbaMechanismView>())cure.gameObject.AddComponent<GbaMechanismView>().Initialize(sr,"cure_"+(int)cure.kind,new Vector2(1.2f,2.3f));}
            var host=session&&session.player?session.player.GetComponent<HostController>():null;if(host&&!host.GetComponent<QualityBarAnchor>())host.gameObject.AddComponent<QualityBarAnchor>();
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

    [DefaultExecutionOrder(283)]
    public sealed class QualityBarHusk:MonoBehaviour
    {
        SpriteRenderer original,picture;HuskBody husk;
        void Start(){husk=GetComponent<HuskBody>();original=GetComponent<SpriteRenderer>();picture=QualityBarArt.Picture(transform,"cast-off empty Host skin",10);picture.sprite=HostPixelArt.Host(HostKind.None,true);var box=GetComponent<BoxCollider2D>();QualityBarArt.Fit(picture,box?box.size*1.1f:new Vector2(1.2f,1.6f));}
        void LateUpdate(){if(!husk||!picture)return;picture.enabled=!original||original.enabled;picture.color=new Color(.73f,.64f,.78f,.9f);if(original)original.forceRenderingOff=picture.sprite!=null;}
        void OnDestroy(){if(original)original.forceRenderingOff=false;}
    }

    [DefaultExecutionOrder(284)]
    public sealed class QualityBarProcession:MonoBehaviour
    {
        ProcessionCarrier carrier;SpriteRenderer[] people;float gait,last;bool initialized;
        void Start(){carrier=GetComponent<ProcessionCarrier>();people=new SpriteRenderer[3];for(int i=0;i<3;i++){people[i]=QualityBarArt.Picture(transform,"actual coffin pallbearer "+i,-2);people[i].sprite=QualityBarArt.Get("pallbearer_0");QualityBarArt.Fit(people[i],new Vector2(1.05f,1.58f));people[i].transform.localPosition=new Vector3(-1.35f+i*1.35f,-1.03f,0);}}
        void LateUpdate(){if(!carrier||people==null)return;float progress=carrier.Progress;if(!initialized){last=progress;initialized=true;}float delta=progress-last;gait+=Mathf.Abs(delta)*Vector2.Distance(carrier.a,carrier.b)*2;last=progress;bool walking=Mathf.Abs(delta)>.00005f;
            for(int i=0;i<people.Length;i++){int pose=walking?((int)(gait+i*.5f)%4):0;people[i].sprite=QualityBarArt.Get("pallbearer_"+pose);people[i].flipX=walking&&delta<0;var sr=carrier.GetComponent<SpriteRenderer>();people[i].enabled=!sr||sr.enabled;}
        }
    }
    [DefaultExecutionOrder(284)]
    public sealed class QualityBarSeason:MonoBehaviour
    {
        SeasonWheel wheel;SpriteRenderer picture;
        void Start(){wheel=GetComponent<SeasonWheel>();picture=QualityBarArt.Picture(transform,"actual four-season wheel",5);picture.sprite=QualityBarArt.Get("season_wheel");QualityBarArt.Fit(picture,new Vector2(2.4f,2.4f));}
        void LateUpdate(){if(!wheel||!picture)return;picture.transform.localRotation=Quaternion.Euler(0,0,-90*wheel.offset/Mathf.Max(.1f,wheel.width));}
    }

}
