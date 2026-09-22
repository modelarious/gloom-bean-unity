using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // All transforms written below belong to render-only children. No simulation body is moved.
    public static class QualityLivingArt
    {
        public static int Frame(float phase)=>Mathf.FloorToInt(Mathf.Repeat(phase,8));
        public static bool Visible(Behaviour owner,SpriteRenderer source,Collider2D collider=null)=>owner&&owner.isActiveAndEnabled&&(!source||source.enabled)&&(!collider||collider.enabled);
        public static SpriteRenderer Make(Transform parent,string name,int order,string asset,Vector2 size){var picture=QualityBarArt.Picture(parent,"living "+name,order);picture.sprite=QualityBarArt.Get(asset);QualityBarArt.Fit(picture,size);return picture;}
    }
    [DefaultExecutionOrder(314)]
    public sealed class QualityBarWaterDetail:MonoBehaviour
    {
        WaterVolume water;BoxCollider2D box;SpriteRenderer source,body,surface,wake;StageSession stage;float phase;int family;
        readonly List<Renderer> covered=new List<Renderer>();
        public bool Applied=>body&&body.sprite&&surface&&surface.sprite;
        public int DisplayFrame {get;private set;} public Vector2 ObservedCurrent {get;private set;}
        public void Initialize(WaterVolume owner){water=owner;box=owner.GetComponent<BoxCollider2D>();source=owner.GetComponent<SpriteRenderer>();stage=GetComponentInParent<StageSession>();family=stage&&QualityBarArt.Group(stage)==2?1:stage&&QualityBarArt.Group(stage)>=4?2:0;
            body=QualityLivingArt.Make(transform,"moving water depth",0,"living_water_"+family+"_0",Vector2.one);body.drawMode=SpriteDrawMode.Tiled;body.transform.localScale=Vector3.one;
            surface=QualityLivingArt.Make(transform,"animated actual waterline",3,"living_surface_"+family+"_0",Vector2.one);surface.drawMode=SpriteDrawMode.Tiled;surface.transform.localScale=Vector3.one;
            wake=QualityLivingArt.Make(transform,"actual swimmer wake",5,"living_wake_0",new Vector2(1.8f,.65f));Refresh();}
        public void Refresh(){if(!water||!box||!body)return;bool visible=QualityLivingArt.Visible(water,source,box);ObservedCurrent=water.current;
            if(visible)phase+=Time.deltaTime*(2+Mathf.Min(10,water.current.magnitude*2));DisplayFrame=QualityLivingArt.Frame(phase);
            body.sprite=QualityBarArt.Get("living_water_"+family+"_"+DisplayFrame);body.size=box.size;body.transform.localPosition=box.offset;body.enabled=visible;body.flipX=water.current.x<0;body.color=Color.white;
            surface.sprite=QualityBarArt.Get("living_surface_"+family+"_"+DisplayFrame);surface.size=new Vector2(box.size.x,.38f);surface.transform.localPosition=box.offset+Vector2.up*(box.size.y*.5f-.14f);surface.enabled=visible;surface.flipX=water.current.x<0;
            foreach(var old in GetComponentsInChildren<Renderer>(true))if(old.name=="Broad visual / water depth"||old.name=="Broad visual / water contact"||old.name=="surface"){if(!covered.Contains(old))covered.Add(old);old.forceRenderingOff=true;}
            if(source)source.forceRenderingOff=Applied;
            var actor=stage?stage.player:null;bool swimming=visible&&actor&&actor.Water==water&&actor.Body.linearVelocity.sqrMagnitude>.16f;wake.enabled=swimming;
            if(swimming){Vector2 at=transform.InverseTransformPoint(actor.transform.position);bool atSurface=Mathf.Abs(actor.transform.position.y-water.Surface)<.8f;at.x-=Mathf.Sign(actor.Body.linearVelocity.x)*.55f;at.x=Mathf.Clamp(at.x,box.offset.x-box.size.x*.5f+.4f,box.offset.x+box.size.x*.5f-.4f);at.y=atSurface?box.offset.y+box.size.y*.5f:Mathf.Clamp(at.y,box.offset.y-box.size.y*.5f+.2f,box.offset.y+box.size.y*.5f-.2f);wake.transform.localPosition=at;wake.sprite=QualityBarArt.Get("living_wake_"+QualityLivingArt.Frame(Time.time*9));wake.color=new Color(.85f,1,1,atSurface?.9f:.5f);}
        }
        void LateUpdate()=>Refresh();
        void OnDisable(){if(body)body.enabled=false;if(surface)surface.enabled=false;if(wake)wake.enabled=false;if(source)source.forceRenderingOff=false;foreach(var r in covered)if(r)r.forceRenderingOff=false;}
        void OnDestroy(){if(source)source.forceRenderingOff=false;foreach(var r in covered)if(r)r.forceRenderingOff=false;}
    }
    [DefaultExecutionOrder(314)]
    public sealed class QualityBarConveyorDetail:MonoBehaviour
    {
        Conveyor belt;SpriteRenderer source,tread,left,right;Collider2D shape;float phase;
        public bool Applied=>tread&&tread.sprite;public int DisplayFrame{get;private set;}
        public void Initialize(Conveyor owner){belt=owner;source=GetComponent<SpriteRenderer>();shape=GetComponent<Collider2D>();tread=QualityLivingArt.Make(transform,"actual conveyor tread",6,"living_tread_0",Vector2.one);tread.drawMode=SpriteDrawMode.Tiled;tread.transform.localScale=Vector3.one;left=QualityLivingArt.Make(transform,"left belt roller",5,"living_gear_0",new Vector2(.48f,.48f));right=QualityLivingArt.Make(transform,"right belt roller",5,"living_gear_0",new Vector2(.48f,.48f));Refresh();}
        public void Refresh(){if(!belt||!source||!tread)return;bool visible=QualityLivingArt.Visible(belt,source,shape);Vector2 size=source.drawMode==SpriteDrawMode.Simple?(Vector2)source.sprite.bounds.size:source.size;if(visible)phase+=belt.speed*Time.deltaTime*8;DisplayFrame=QualityLivingArt.Frame(phase);
            tread.sprite=QualityBarArt.Get("living_tread_"+DisplayFrame);tread.size=new Vector2(Mathf.Max(.1f,size.x),Mathf.Min(.28f,size.y));tread.transform.localPosition=new Vector3(0,size.y*.5f-tread.size.y*.5f,0);tread.enabled=visible;
            left.sprite=right.sprite=QualityBarArt.Get("living_gear_"+DisplayFrame);left.transform.localPosition=new Vector3(-size.x*.5f+.23f,size.y*.5f-.29f,0);right.transform.localPosition=new Vector3(size.x*.5f-.23f,size.y*.5f-.29f,0);left.enabled=right.enabled=visible;}
        void LateUpdate()=>Refresh();
        void OnDisable(){if(tread)tread.enabled=false;if(left)left.enabled=false;if(right)right.enabled=false;}
    }
    [DefaultExecutionOrder(314)]
    public sealed class QualityBarCarouselDetail:MonoBehaviour
    {
        Carousel wheel;SpriteRenderer source,casing,hub;readonly List<SpriteRenderer> rods=new List<SpriteRenderer>();readonly List<SpriteRenderer> pins=new List<SpriteRenderer>();
        public bool Applied=>hub&&hub.sprite;public int DisplayFrame{get;private set;}
        public void Initialize(Carousel owner){wheel=owner;source=owner.GetComponent<SpriteRenderer>();casing=QualityLivingArt.Make(transform,"fixed carousel casing",0,"living_casing",new Vector2(1.6f,1.6f));hub=QualityLivingArt.Make(transform,"carousel rotation from actual arm",2,"living_gear_0",new Vector2(1.2f,1.2f));for(int i=0;i<4;i++){rods.Add(QualityLivingArt.Make(transform,"physical arm girder "+i,-3,"construction_girder",Vector2.one));pins.Add(QualityLivingArt.Make(transform,"physical arm pin "+i,1,"living_gear_0",new Vector2(.43f,.43f)));}Refresh();}
        public void Refresh(){if(!wheel||!hub)return;bool visible=QualityLivingArt.Visible(wheel,source);casing.enabled=hub.enabled=visible;
            for(int i=0;i<4;i++){var arm=wheel.arms!=null&&i<wheel.arms.Length?wheel.arms[i]:null;bool armVisible=visible&&arm&&arm.isActiveAndEnabled;var armPicture=arm?arm.GetComponent<SpriteRenderer>():null;armVisible&=!armPicture||armPicture.enabled;rods[i].enabled=pins[i].enabled=armVisible;if(!armVisible)continue;Vector3 delta=arm.transform.position-transform.position;float angle=Mathf.Atan2(delta.y,delta.x);if(i==0)DisplayFrame=QualityLivingArt.Frame(angle*24/Mathf.PI);rods[i].transform.position=transform.position+delta*.5f;rods[i].transform.rotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg);QualityBarArt.Fit(rods[i],new Vector2(delta.magnitude,.22f));pins[i].transform.position=arm.transform.position;}
            hub.sprite=QualityBarArt.Get("living_gear_"+DisplayFrame);if(source)source.forceRenderingOff=Applied;
        }
        void LateUpdate()=>Refresh();void OnDestroy(){if(source)source.forceRenderingOff=false;}
        void OnDisable(){if(casing)casing.enabled=false;if(hub)hub.enabled=false;foreach(var r in rods)if(r)r.enabled=false;foreach(var r in pins)if(r)r.enabled=false;}
    }
    [DefaultExecutionOrder(314)]
    public sealed class QualityBarRailDrive:MonoBehaviour
    {
        RailPath rail;StageSession stage;SpriteRenderer left,right;float phase,lastT,lastString;bool engaged;
        public bool Applied=>left&&left.sprite;public int DisplayFrame{get;private set;}
        public void Initialize(RailPath owner){rail=owner;stage=GetComponentInParent<StageSession>();left=QualityLivingArt.Make(transform,"rail takeup flywheel",3,"living_gear_0",new Vector2(.86f,.86f));right=QualityLivingArt.Make(transform,"rail return flywheel",3,"living_gear_0",new Vector2(.86f,.86f));Refresh();}
        public void Refresh(){if(!rail||!left)return;var controller=stage&&stage.player?stage.player.GetComponent<HostController>():null;var form=controller?controller.Form<MarionetteForm>():null;bool active=form!=null&&form.Rail==rail&&form.Joint&&rail.isActiveAndEnabled;
            if(active){float t=form.railT*Vector2.Distance(rail.a,rail.b),length=form.Joint.distance;if(engaged)phase+=(t-lastT+length-lastString)*8;lastT=t;lastString=length;}engaged=active;DisplayFrame=QualityLivingArt.Frame(phase);
            left.sprite=right.sprite=QualityBarArt.Get("living_gear_"+DisplayFrame);left.transform.position=rail.Point(0)+Vector2.up*.04f;right.transform.position=rail.Point(1)+Vector2.up*.04f;left.transform.rotation=right.transform.rotation=Quaternion.identity;left.enabled=right.enabled=rail.isActiveAndEnabled;
        }
        void LateUpdate()=>Refresh();void OnDisable(){if(left)left.enabled=false;if(right)right.enabled=false;}
    }
}
