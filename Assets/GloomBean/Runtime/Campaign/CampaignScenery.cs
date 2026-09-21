using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    /// <summary>Non-colliding parallax compositions; see ArtSources/Ansimuz for CC0 component credits. Simulation never reads this art.</summary>
    public static class SceneryArt
    {
        const int W=256,H=160;static readonly Dictionary<int,Sprite> cache=new Dictionary<int,Sprite>();
        public static Color Sky(int world)=>world==0?new Color(.75f,.84f,.70f):world==1?new Color(.10f,.065f,.14f):world==2?new Color(.14f,.16f,.095f):world==3?new Color(.095f,.135f,.19f):world==4?new Color(.16f,.105f,.135f):new Color(.73f,.69f,.61f);
        public static Sprite Get(int world)
        {
            var v6=V6Art.Sprite("background_"+world,12);if(v6)return v6;
            if(cache.TryGetValue(world,out var result))return result;
            Color sky=Sky(world),dark=Color.Lerp(sky,world==5?new Color(.42f,.39f,.38f):Color.black,.35f),mid=Color.Lerp(sky,world==0?Color.white:new Color(.59f,.44f,.54f),world==5?.30f:.22f),light=Color.Lerp(mid,world==5?new Color(.92f,.83f,.58f):new Color(.65f,.66f,.71f),.20f);
            var pixels=new Color32[W*H];for(int y=0;y<H;y++)for(int x=0;x<W;x++)pixels[x+y*W]=Color.Lerp(sky,dark,(1f-(float)y/H)*.25f);
            void Dot(int x,int y,Color c){if(x>=0&&x<W&&y>=0&&y<H)pixels[x+y*W]=c;}
            void Rect(int x,int y,int w,int h,Color c){for(int j=y;j<y+h;j++)for(int i=x;i<x+w;i++)Dot(i,j,c);}
            void Ellipse(int x,int y,int rx,int ry,Color c,bool ring=false){for(int j=-ry;j<=ry;j++)for(int i=-rx;i<=rx;i++){float d=(float)i*i/(rx*rx)+(float)j*j/(ry*ry);if(d<=1&&(!ring||d>.78f))Dot(x+i,y+j,c);}}
            void Line(int x,int y,int xx,int yy,Color c,int width=1){int n=Math.Max(Math.Abs(x-xx),Math.Abs(y-yy));for(int i=0;i<=n;i++){float t=n==0?0:(float)i/n;Rect(Mathf.RoundToInt(Mathf.Lerp(x,xx,t)),Mathf.RoundToInt(Mathf.Lerp(y,yy,t)),width,width,c);}}
            void Arch(int x,int y,int w,int h,Color c){Rect(x-w/2,y,w,h-w/2,c);Ellipse(x,y+h-w/2,w/2,w/2,c);}
            // Deterministic composition, no Unity random state is changed by the renderer.
            if(world==0||world==1){
                for(int i=0;i<8;i++){int x=i*36-9,h=43+(i*19)%55;Rect(x,0,27,h,mid);for(int y=8;y<h-8;y+=14){Arch(x+8,y,6,10,dark);Arch(x+20,y,6,10,dark);}for(int k=0;k<16;k++)Line(x-2+k,h+k,x+30-k,h+k,light);if(world==1){Ellipse(x+10,h+4,4,6,dark);Ellipse(x+20,h+4,4,6,dark);Line(x+16,h-1,x+15,h-9,dark);}else{Ellipse(x+14,h+2,7,3,dark);}}
                for(int i=0;i<12;i++){int x=i*24;Line(x,112,x+24,107,light);Line(x+24,107,x+48,112,light);for(int k=0;k<6;k++)Rect(x+5+k,109-k,10-k*2,1,mid);}
                if(world==0){Ellipse(198,127,13,13,new Color(.94f,.84f,.57f));for(int i=0;i<5;i++)Ellipse(30+i*51,139,19,4,Color.Lerp(sky,Color.white,.35f));}
            }else if(world==2){
                for(int i=0;i<6;i++){int x=i*49+7;Line(x,0,x+4,110,mid,4);for(int k=0;k<4;k++){int yy=45+k*18,d=k%2==0?-1:1;Line(x+4,yy,x+4+d*24,yy+18,mid,2);Line(x+d*24,yy+18,x+d*24,yy-4,light);Ellipse(x+d*24,yy-10,5,7,light);Ellipse(x+d*24-1,yy-9,1,2,dark);}for(int k=-2;k<=2;k++)Line(x+4,20,x+k*10,0,light);}
                for(int i=0;i<4;i++){Arch(34+i*64,0,38,35,dark);Ellipse(34+i*64,34,21,5,mid);}
            }else if(world==3){
                for(int i=0;i<7;i++){int x=i*43-5,h=80+(i%3)*21;Arch(x+15,0,34,h,mid);Rect(x+12,0,4,h,light);for(int k=9;k<h-8;k+=16){Arch(x+5,k,7,12,dark);Arch(x+25,k+4,8,12,dark);}Line(x+30,h-8,x+45,h-20,light);}
                Ellipse(40,136,17,17,light,true);Ellipse(214,129,11,11,light,true);for(int y=17;y<H;y+=22)Line(0,y,W,y+5,dark);
            }else if(world==4){
                for(int i=0;i<6;i++){int x=i*47+10,y=56+(i%3)*26;Ellipse(x,y+19,12,15,mid);Ellipse(x+3,y+21,4,7,dark);Line(x,y+6,x+11,y-28,mid,9);Line(x+11,y-28,x-13,y-34,mid,6);Line(x-13,y-34,x-19,y-53,light,3);Line(x-3,y,x-19,y-10,mid,4);for(int k=0;k<6;k++)Line(x+1+k,y+4-k*5,x+12+k,y+2-k*5,light);Line(x-12,160,x+22,0,dark,2);}
            }else{
                for(int i=0;i<6;i++){int x=i*51;Arch(x+17,0,30,126,mid);Arch(x+17,6,21,109,dark);Rect(x+13,8,2,100,light);Rect(x+22,8,2,100,light);for(int y=15;y<110;y+=21){Line(x+7,y,x+28,y+13,light);Line(x+28,y,x+7,y+13,light);}Ellipse(x+17,116,9,9,light,true);Ellipse(x+17,116,3,5,dark);}
                for(int i=0;i<4;i++){Ellipse(32+i*64,137,28,12,light,true);Line(32+i*64,126,32+i*64,157,mid);}
            }
            for(int x=0;x<W;x+=4)Dot(x,2,light);
            var texture=new Texture2D(W,H,TextureFormat.RGBA32,false){name="Original world painting "+world,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Repeat};texture.SetPixels32(pixels);texture.Apply(false,true);
            result=Sprite.Create(texture,new Rect(0,0,W,H),Vector2.one*.5f,8,0,SpriteMeshType.FullRect);cache[world]=result;return result;
        }
    }
    [DefaultExecutionOrder(150)]
    public sealed class CampaignScenery:MonoBehaviour
    {
        StageDefinition definition;StageSession session;SpriteRenderer[] pictures;int world,last=-1;
        public int DecorativeColliderCount=>0;
        public void Initialize(StageDefinition d)
        {
            definition=d;world=int.Parse(d.worldId.Substring(1));session=GetComponent<StageSession>();
            // These exact objects were purely decorative in the original builder.
            foreach(var sr in GetComponentsInChildren<SpriteRenderer>(true)){
                if(sr.name=="Sky"||sr.name=="Distant architecture")sr.enabled=false;
                else if((sr.name=="Floor"||sr.name=="Wall"||sr.name=="Platform")&&sr.GetComponent<Collider2D>())sr.sprite=HostPixelArt.Terrain(world);
            }
            pictures=new SpriteRenderer[3];for(int i=0;i<3;i++){var go=new GameObject("Non-colliding parallax painting "+i);go.transform.SetParent(transform,false);pictures[i]=go.AddComponent<SpriteRenderer>();pictures[i].sortingOrder=-35;}
            foreach(var s in GetComponentsInChildren<HostSource>(true))if(!s.GetComponent<TenantPixelView>())s.gameObject.AddComponent<TenantPixelView>();
            foreach(var enemy in GetComponentsInChildren<CarryableEnemy>(true))if(!enemy.GetComponent<PatrolPixelView>())enemy.gameObject.AddComponent<PatrolPixelView>().world=world;
            var boss=GetComponentInChildren<AtlasBoss>();if(boss&&boss.body){var skin=boss.body.gameObject.AddComponent<BossPixelView>();skin.boss=boss;skin.world=world;}
            gameObject.AddComponent<V6WorldDressing>().Initialize(d);
        }
        void LateUpdate()
        {
            var camera=UnityEngine.Camera.main;if(!camera||pictures==null)return;
            bool cute=definition.course==1&&!GameRoot.Instance.IsCorrupted;int theme=cute?0:definition.course==19?6:definition.course==16?7:world;
            if(theme!=last){last=theme;foreach(var p in pictures)p.sprite=SceneryArt.Get(theme);camera.backgroundColor=SceneryArt.Sky(theme);}
            Vector3 c=camera.transform.position;float scale=Mathf.Max(1,camera.orthographicSize/8f),width=32*scale;float phase=Mathf.Repeat(c.x*.25f,width);
            for(int i=0;i<3;i++){pictures[i].transform.position=new Vector3(c.x-phase+(i-1)*width,c.y,2);pictures[i].transform.localScale=new Vector3(scale,scale,1);}
        }
    }
    public sealed class TenantPixelView:MonoBehaviour
    {
        HostSource source;SpriteRenderer picture;Transform art;
        void Start(){source=GetComponent<HostSource>();foreach(var old in GetComponentsInChildren<SpriteRenderer>())old.enabled=false;foreach(var line in GetComponentsInChildren<LineRenderer>())line.enabled=false;art=new GameObject("Tenant creature pixel artwork").transform;art.SetParent(transform,false);art.localScale=Vector3.one*1.9f;picture=art.gameObject.AddComponent<SpriteRenderer>();picture.sortingOrder=9;}
        void LateUpdate(){if(!picture||!source)return;picture.sprite=HostPixelArt.Tenant(source.kind,(int)(Time.time*4)%4);picture.color=source.enabledSource?Color.white:new Color(.5f,.5f,.5f,.5f);bool floats=source.kind==HostKind.Echo||source.kind==HostKind.Molt||source.kind==HostKind.Parallax;art.localPosition=new Vector3(0,floats?Mathf.Sin(Time.time*2+transform.position.x)*.08f:0,0);}
    }
    public sealed class BossPixelView:MonoBehaviour
    {
        public AtlasBoss boss;public int world;SpriteRenderer picture;
        void Start(){picture=GetComponent<SpriteRenderer>();foreach(var old in GetComponentsInChildren<SpriteRenderer>())if(old!=picture)old.enabled=false;}
        void LateUpdate(){if(picture&&boss){picture.sprite=HostPixelArt.Boss(world,Mathf.Clamp(boss.phase,0,2));picture.color=Color.white;}}
    }
    public sealed class PatrolPixelView:MonoBehaviour
    {
        public int world;CarryableEnemy enemy;SpriteRenderer picture;Transform image;
        void Start(){enemy=GetComponent<CarryableEnemy>();foreach(var old in GetComponentsInChildren<SpriteRenderer>())old.enabled=false;image=new GameObject("Patrol mask pixel artwork").transform;image.SetParent(transform,false);image.localScale=Vector3.one*1.24f;picture=image.gameObject.AddComponent<SpriteRenderer>();picture.sortingOrder=10;}
        void LateUpdate(){if(!enemy||!picture)return;bool stunned=enemy.state==EnemyState.Stunned||enemy.state==EnemyState.Carried;picture.sprite=HostPixelArt.Enemy(world,(int)(Time.time*5)%2,stunned,enemy.armored);picture.flipX=enemy.direction<0;image.localRotation=Quaternion.Euler(0,0,enemy.state==EnemyState.Thrown?Time.time*750:0);}
    }

}
