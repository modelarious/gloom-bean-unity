using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public static class V6Art
    {
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        public static readonly string[] RequiredAssets={"background_0","background_1","background_2","background_3","background_4","background_5","background_6","boss_1_0","boss_1_1","boss_1_2","boss_2_0","boss_2_1","boss_2_2","boss_3_0","boss_3_1","boss_3_2","boss_4_0","boss_4_1","boss_4_2","boss_5_0","boss_5_1","boss_5_2","coin","corbel_0","corbel_1","corbel_2","corbel_3","corbel_4","corbel_5","fill_0","fill_1","fill_2","fill_3","fill_4","fill_5","heart","hinge","host_corrupted","host_original","key","lip_0","lip_1","lip_2","lip_3","lip_4","lip_5","mercy","nail","pear_0","pear_1","pear_2","penitent_falling","penitent_kneeling","portal"};
        public static Sprite Sprite(string name,float ppu=32)
        {
            string key=name+"@"+ppu;if(sprites.TryGetValue(key,out var s))return s;
            var t=Resources.Load<Texture2D>("VisualV6/"+name);if(!t)return null;
            t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;
            s=UnityEngine.Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,ppu,0,SpriteMeshType.FullRect);s.name="V6 original "+name;sprites[key]=s;return s;
        }
        public static SpriteRenderer Picture(Transform parent,string name,int order)
        {var go=new GameObject("V6 visual / "+name);go.transform.SetParent(parent,false);var sr=go.AddComponent<SpriteRenderer>();sr.sortingOrder=order;return sr;}
        public static void Fit(SpriteRenderer sr,Vector2 size)
        {if(!sr||!sr.sprite)return;var b=sr.sprite.bounds.size;sr.transform.localScale=new Vector3(size.x/Mathf.Max(.001f,b.x),size.y/Mathf.Max(.001f,b.y),1);}
    }
    [DefaultExecutionOrder(220)]
    public sealed class V6Surface:MonoBehaviour
    {
        SpriteRenderer source,face,lip;SpriteRenderer[] brackets;StageSession session;int world,theme=-1;bool tint;Vector2 prior;
        public void Initialize(SpriteRenderer original,int w)
        {
            source=original;world=w;session=GetComponentInParent<StageSession>();
            tint=GetComponent<MagneticBody>()||GetComponent<PressurePlate>();
            face=V6Art.Picture(transform,"material",source.sortingOrder);face.drawMode=SpriteDrawMode.Tiled;
            lip=V6Art.Picture(transform,"walking edge",source.sortingOrder+1);lip.drawMode=SpriteDrawMode.Tiled;
            LateUpdate();
        }
        void LateUpdate()
        {
            if(!source||!face)return;
            int t=session&&session.definition.course==1&&GameRoot.Instance&&!GameRoot.Instance.IsCorrupted?0:world;
            if(t!=theme){theme=t;face.sprite=V6Art.Sprite("fill_"+t,32);lip.sprite=V6Art.Sprite("lip_"+t,32);if(brackets!=null)foreach(var b in brackets)b.sprite=V6Art.Sprite("corbel_"+t,64);}
            Vector2 size=source.drawMode==SpriteDrawMode.Simple?(Vector2)source.sprite.bounds.size:source.size;
            source.forceRenderingOff=face.sprite!=null;face.size=size;face.enabled=source.enabled&&face.sprite!=null;face.color=tint?source.color:new Color(1,1,1,source.color.a);
            bool horizontal=size.x>1.7f&&size.y<size.x*.55f;
            lip.enabled=source.enabled&&horizontal;lip.size=new Vector2(size.x,.5f);lip.transform.localPosition=new Vector3(0,size.y*.5f-.25f,0);lip.color=face.color;
            if(size!=prior){prior=size;
                if(brackets!=null)foreach(var b in brackets)if(b)Destroy(b.gameObject);
                brackets=null;
                // Decorative corbels hang BEHIND the contact surface, never add collision.
                if(horizontal&&size.y<.85f&&size.x>2.4f&&!tint){int count=Mathf.Clamp(Mathf.FloorToInt(size.x/3.3f),1,8);brackets=new SpriteRenderer[count];
                    for(int i=0;i<count;i++){var b=V6Art.Picture(transform,"recessed corbel",source.sortingOrder-2);b.sprite=V6Art.Sprite("corbel_"+theme,64);b.transform.localPosition=new Vector3((i+.5f)*size.x/count-size.x*.5f,-size.y*.5f-.24f,0);b.transform.localScale=new Vector3(1.25f,.8f,1);brackets[i]=b;}}
            }
            if(brackets!=null)foreach(var b in brackets){b.enabled=source.enabled;b.color=new Color(.70f,.70f,.75f,source.color.a*.75f);}
        }
        void OnDestroy(){if(source)source.forceRenderingOff=false;}
    }
    [DefaultExecutionOrder(230)]
    public sealed class V6PropView:MonoBehaviour
    {
        SpriteRenderer source,picture;RipeningFruit fruit;KneelingFigure penitent;string asset;Vector2 size;bool followTint;
        public void Initialize(string name,Vector2 dimensions,bool tinted=false)
        {source=GetComponent<SpriteRenderer>();if(!source)return;asset=name;size=dimensions;followTint=tinted;fruit=GetComponent<RipeningFruit>();penitent=GetComponent<KneelingFigure>();picture=V6Art.Picture(transform,name,source.sortingOrder+1);}
        void LateUpdate()
        {
            if(!picture||!source)return;picture.enabled=source.enabled;
            if(fruit)asset="pear_"+(fruit.age>=fruit.rotAfter?2:fruit.age>=fruit.ripeAfter?1:0);
            if(penitent){bool kneel=penitent.pose==KneelingFigure.Pose.Kneeling||penitent.pose==KneelingFigure.Pose.Sinking;asset=kneel?"penitent_kneeling":"penitent_falling";size=kneel?new Vector2(3.6f,1.8f):new Vector2(1.8f,2.65f);picture.transform.localPosition=new Vector3(0,kneel?-.52f:0,0);picture.enabled=penitent.pose!=KneelingFigure.Pose.Warning;}
            picture.sprite=V6Art.Sprite(asset,64);source.forceRenderingOff=picture.sprite!=null;V6Art.Fit(picture,size);picture.color=followTint?source.color:new Color(1,1,1,source.color.a);
        }
        void OnDestroy(){if(source)source.forceRenderingOff=false;}
    }
    [DefaultExecutionOrder(200)]
    public sealed class V6WorldDressing:MonoBehaviour
    {
        StageDefinition definition;StageSession session;int world;SpriteRenderer presence;AtlasBoss boss;
        public int AddedColliderCount {get{int count=0;foreach(var t in GetComponentsInChildren<Transform>(true))if(t.name.StartsWith("V6 visual /"))count+=t.GetComponents<Collider2D>().Length;return count;}}
        public void Initialize(StageDefinition d)
        {
            definition=d;world=int.Parse(d.worldId.Substring(1));session=GetComponent<StageSession>();
            var renderers=GetComponentsInChildren<SpriteRenderer>(true);
            foreach(var sr in renderers)
            {
                if(sr.GetComponentInParent<ActorMotor>()||sr.GetComponentInParent<CarryableEnemy>())continue;
                var collider=sr.GetComponent<Collider2D>();if(!collider)continue;
                if(sr.GetComponent<KneelingFigure>()){sr.gameObject.AddComponent<V6PropView>().Initialize("penitent_falling",new Vector2(1.8f,2.65f));continue;}
                if(sr.GetComponent<RipeningFruit>()){sr.gameObject.AddComponent<V6PropView>().Initialize("pear_0",new Vector2(1.25f,1.9f));continue;}
                if(sr.GetComponent<ExitPortal>()){sr.gameObject.AddComponent<V6PropView>().Initialize("portal",new Vector2(1.05f,1.05f));continue;}
                if(sr.GetComponent<TurnSwitch>()){sr.gameObject.AddComponent<V6PropView>().Initialize("nail",new Vector2(1.6f,2.1f));continue;}
                var pickup=sr.GetComponent<Pickup>();if(pickup){string asset=pickup.kind==PickupKind.Coin?"coin":pickup.kind==PickupKind.Key?"key":pickup.kind==PickupKind.Mercy?"mercy":pickup.kind==PickupKind.Health?"heart":null;if(asset!=null)sr.gameObject.AddComponent<V6PropView>().Initialize(asset,Vector2.one);continue;}
                if(sr.GetComponent<HostSource>()||sr.GetComponent<HostCure>())continue;
                if(!collider.isTrigger&&collider is BoxCollider2D&&sr.sortingOrder<7&&!sr.GetComponent<ActorMotor>())
                    sr.gameObject.AddComponent<V6Surface>().Initialize(sr,world);
            }
            // Printed design labels are not foreground architecture. Keep functional signs, remove the giant duplicate stage caption.
            foreach(var text in GetComponentsInChildren<TextMesh>(true))if(string.Equals(text.text,d.title,StringComparison.OrdinalIgnoreCase))text.GetComponent<MeshRenderer>().enabled=false;
            boss=GetComponentInChildren<AtlasBoss>();
            if(boss&&world==5){presence=V6Art.Picture(transform,"distant Host manifestation",-12);presence.sprite=V6Art.Sprite("boss_5_0",128);presence.transform.localScale=new Vector3(7,9,1);}
        }
        void LateUpdate()
        {
            if(!presence||!boss)return;var c=UnityEngine.Camera.main;if(!c)return;
            bool physicalVisible=boss.body&&Mathf.Abs(boss.body.position.x-c.transform.position.x)<c.orthographicSize*c.aspect+3;
            presence.enabled=!physicalVisible&&!boss.defeated;presence.sprite=V6Art.Sprite("boss_5_"+Mathf.Clamp(boss.phase,0,2),128);presence.color=new Color(.45f,.45f,.51f,.38f);
            presence.transform.position=new Vector3(c.transform.position.x+5.1f,c.transform.position.y+1.9f,1);
        }
    }
}
