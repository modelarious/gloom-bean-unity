using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Hand-authored pixel primitives and palettes. Art never defines collision,
    // saves, generated rules or puzzle validity. V6 portraits use the user-supplied reference; no runtime model.
    public static class HostPixelArt
    {
        static readonly Dictionary<string,Sprite> bank=new Dictionary<string,Sprite>();
        static Color32 Hex(uint value)=>new Color32((byte)(value>>16),(byte)(value>>8),(byte)value,255);
        static readonly Color32 Ink=Hex(0x100d21),Dark=Hex(0x281237),Purple=Hex(0x492164),Mid=Hex(0x713e96),High=Hex(0xa163bf),Pink=Hex(0xe92978),Rose=Hex(0xff665f),Gold=Hex(0xeab552),Cream=Hex(0xffeeab),White=Hex(0xfff9e7),Lavender=Hex(0xc4b4d5);
        sealed class Pixel
        {
            public readonly int n;public readonly Color32[] p;public Pixel(int size){n=size;p=new Color32[n*n];}
            public void Dot(int x,int y,Color32 c){if(x>=0&&x<n&&y>=0&&y<n)p[x+y*n]=c;}
            public void Rect(int x,int y,int w,int h,Color32 c){for(int j=y;j<y+h;j++)for(int i=x;i<x+w;i++)Dot(i,j,c);}
            public void Ellipse(int x,int y,int rx,int ry,Color32 c){if(rx<1||ry<1)return;for(int j=-ry;j<=ry;j++)for(int i=-rx;i<=rx;i++)if((float)i*i/(rx*rx)+(float)j*j/(ry*ry)<=1)Dot(x+i,y+j,c);}
            public void Ring(int x,int y,int rx,int ry,int thick,Color32 c){for(int j=-ry;j<=ry;j++)for(int i=-rx;i<=rx;i++){float outer=(float)i*i/(rx*rx)+(float)j*j/(ry*ry),inner=(float)i*i/Mathf.Max(1,(rx-thick)*(rx-thick))+(float)j*j/Mathf.Max(1,(ry-thick)*(ry-thick));if(outer<=1&&inner>=1)Dot(x+i,y+j,c);}}
            public void Line(int x,int y,int z,int w,int thick,Color32 c){int steps=Math.Max(Math.Abs(z-x),Math.Abs(w-y));for(int i=0;i<=steps;i++){float t=steps==0?0:(float)i/steps;Ellipse(Mathf.RoundToInt(Mathf.Lerp(x,z,t)),Mathf.RoundToInt(Mathf.Lerp(y,w,t)),thick,thick,c);}}
            public bool Reference(bool corrupt,HostKind form,int frame,int pose)
            {
                var texture=Resources.Load<Texture2D>("VisualV6/"+(corrupt?"host_corrupted":"host_original"));
                if(!texture||texture.width!=64||texture.height!=64)return false;
                var reference=texture.GetPixels32();
                int step=frame==1?1:frame==3?-1:0;
                for(int y=0;y<64;y++)for(int x=0;x<64;x++){
                    int sy=y;if(y<15)sy=y+(x<33?step:-step);else if(pose==2)sy=y-1;
                    if(sy<0||sy>=64)continue;var c=reference[x+sy*64];
                    if(c.a>0&&c.r<150&&c.g<95&&c.b>c.g*1.3f&&c.b>c.r){
                        if(form==HostKind.Wax)c=(Color32)Color.Lerp(new Color(.27f,.17f,.17f),new Color(.95f,.82f,.48f),Mathf.Clamp01(c.r/110f));
                        else if(form==HostKind.Root)c=(Color32)Color.Lerp(new Color(.11f,.19f,.17f),new Color(.59f,.71f,.35f),Mathf.Clamp01(c.r/160f));
                    }
                    Dot(x,y,c);
                }
                return true;
            }
            public Sprite Finish(string name,float pixelsPerUnit=64)
            {var t=new Texture2D(n,n,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};t.SetPixels32(p);t.Apply(false,true);return Sprite.Create(t,new Rect(0,0,n,n),Vector2.one*.5f,pixelsPerUnit,0,SpriteMeshType.FullRect);}
        }
        static void Eye(Pixel p,int x,int y,int rx,int ry,int look,bool torn=false)
        {p.Ellipse(x,y,rx+2,ry+2,Ink);if(torn)p.Ellipse(x,y,rx+1,ry+1,Pink);p.Ellipse(x,y,rx,ry,Gold);p.Ellipse(x-1,y+1,rx-1,ry-1,Cream);p.Ellipse(x+look,y,Math.Max(2,rx/3),Math.Max(3,ry/2),Ink);p.Rect(x+look-1,y+ry/2-2,2,2,White);}
        static void Shoe(Pixel p,int x,int y){p.Ellipse(x,y,10,6,Ink);p.Ellipse(x,y+1,8,4,Hex(0xb71858));p.Ellipse(x-2,y+2,5,3,Pink);p.Rect(x-5,y+3,5,1,Rose);}
        static void Glove(Pixel p,int x,int y){p.Ellipse(x,y,7,7,Ink);p.Ellipse(x,y+1,5,5,Lavender);p.Ellipse(x-1,y+2,4,4,White);p.Line(x+1,y-2,x+3,y-1,1,Hex(0x746080));}
        public static Sprite Host(HostKind form,bool corrupt,int frame=0,int pose=0)
        {
            frame&=3;string key="host-"+form+"-"+corrupt+"-"+frame+"-"+pose;if(bank.TryGetValue(key,out var cached))return cached;
            var p=new Pixel(64);int lift=pose==2?2:0,bob=frame==1||frame==3?1:0,foot=frame==1?2:frame==3?-2:0;
            Color32 body=form==HostKind.Wax?Hex(0xddb971):form==HostKind.Root?Hex(0x556c3b):form==HostKind.Ink?Hex(0x2f2457):Purple;
            if(form==HostKind.Coffin){
                p.Rect(18,5,29,50,Ink);p.Rect(15,13,35,32,Ink);p.Rect(20,8,25,45,Hex(0x7d4856));p.Rect(18,15,29,28,Hex(0x482e40));p.Rect(22,10,2,41,Hex(0xad726a));p.Rect(42,10,2,41,Hex(0xad726a));
                p.Rect(25,41,15,2,Gold);p.Rect(31,35,2,14,Gold);Eye(p,29,29,5,8,0);Eye(p,39,30,4,6,0);p.Line(33,21,32,10,1,Pink);foreach(int y in new[]{12,23,39,49}){p.Rect(20,y,2,2,Cream);p.Rect(43,y,2,2,Cream);}Glove(p,11,28);Glove(p,53,28);
            }else{
                if(!p.Reference(corrupt,form,frame,pose)){
                Shoe(p,23,8+foot+lift);Shoe(p,44,8-foot+lift);
                p.Line(26,43,24,54,4,Ink);p.Line(24,54,30,59,4,Ink);p.Line(30,59,34,56,4,Ink);p.Line(26,44,25,54,2,Mid);p.Line(25,54,30,57,2,High);
                p.Ellipse(33,29+bob,21,21,Ink);p.Ellipse(33,29+bob,18,18,body);p.Ellipse(29,34+bob,15,15,Mid);p.Ellipse(25,38+bob,9,8,High);p.Ellipse(38,23+bob,13,12,body);p.Ellipse(38,19+bob,12,7,Dark);
                Glove(p,8,23-foot+(pose==1?2:0));Glove(p,55,pose==1?32:23+foot);
                Eye(p,28,33+bob,10,15,2,corrupt);Eye(p,47,corrupt?47:37+bob,7,10,1,corrupt);
                if(corrupt){p.Line(40,37,46,42,3,Pink);p.Line(24,22,22,5+frame,2,Pink);p.Line(35,18,33,4,3,Pink);p.Ellipse(47,23,10,8,Pink);p.Ellipse(49,23,6,5,Ink);p.Line(51,19,56,12,2,Pink);p.Line(56,12,60,14,2,Rose);p.Rect(45,25,3,3,Cream);p.Rect(50,27,2,2,White);p.Ellipse(31,6,3,2,Rose);}
                }
                if(pose==4){p.Line(23,39,32,29,1,White);p.Line(32,39,23,29,1,White);}
                switch(form){
                    case HostKind.Echo:p.Ring(32,31,26,25,1,Hex(0x9ee4dd));p.Line(6,41,13,41,1,Hex(0x76b8cf));p.Line(4,35,10,35,1,Hex(0x76b8cf));break;
                    case HostKind.Marionette:p.Rect(12,50,42,4,Ink);p.Rect(13,51,39,2,Gold);p.Line(32,53,32,63,1,Cream);p.Line(15,50,10,28,1,Cream);p.Line(51,50,55,28,1,Cream);break;
                    case HostKind.Molt:p.Ring(32,29,24,24,3,Hex(0xb27899));p.Line(12,15,20,7,2,Hex(0xe6adbd));p.Line(47,47,53,57,2,Hex(0xe6adbd));break;
                    case HostKind.Wax:p.Rect(18,41,7,13,Gold);p.Ellipse(20,53,5,4,Cream);p.Line(20,57,21,60,1,Ink);p.Ellipse(21,61,2,2,Rose);p.Line(15,32,13,19,2,Gold);p.Line(44,38,43,22,2,Cream);break;
                    case HostKind.Gullet:p.Ellipse(47,24,14,11,Ink);p.Ellipse(47,25,12,9,Pink);p.Ellipse(48,25,9,6,Ink);for(int x=41;x<56;x+=4){p.Rect(x,29,2,3,Cream);p.Rect(x+1,19,2,3,Cream);}break;
                    case HostKind.Root:for(int i=0;i<5;i++){p.Line(22+i*5,13,13+i*9,3,1,Hex(0xa1b56a));p.Line(13+i*9,3,10+i*10,1,1,Hex(0x637542));}p.Line(17,40,12,47,1,Hex(0xb9c789));break;
                    case HostKind.Mirror:p.Ring(33,31,23,25,2,Hex(0xc7ddeb));p.Line(18,47,41,20,1,White);p.Line(21,52,30,43,1,Hex(0x93a6c8));break;
                    case HostKind.InsideOut:p.Ring(33,30,21,21,2,Hex(0xd5fff3));p.Line(13,40,8,48,1,Hex(0xb4e7dc));p.Line(52,40,58,45,1,White);for(int y=19;y<43;y+=5)p.Rect(13,y,3,1,Pink);break;
                    case HostKind.Parallax:p.Ring(47,37,10,13,1,Hex(0xc8d6ef));p.Ellipse(49,37,2,4,Hex(0x6bb5cd));p.Line(44,53,56,57,1,Gold);break;
                    case HostKind.Censer:p.Ellipse(33,15,23,10,Ink);p.Ring(33,17,20,7,2,Gold);p.Line(16,18,25,6,1,Cream);p.Line(50,18,41,6,1,Cream);p.Line(25,6,41,6,1,Gold);for(int i=0;i<3;i++)p.Ring(15+i*14,49+(frame+i)%3*3,5,4,1,Hex(0x91a3c2));break;
                    case HostKind.Stitch:p.Line(54,5,58,57,2,Ink);p.Line(55,6,58,56,1,White);p.Ring(58,55,2,3,1,Gold);p.Line(58,57,44,60,1,Pink);p.Ring(38,17,7,4,2,Hex(0xc791b5));break;
                    case HostKind.Lodestone:p.Ring(32,31,29,19,3,Ink);p.Ring(32,32,27,17,2,Hex(0xc0a470));p.Rect(5,29,5,7,Hex(0xde656e));p.Rect(54,29,5,7,Hex(0x7dacc7));break;
                    case HostKind.Shadow:p.Ring(34,56,7,6,2,Gold);p.Line(34,47,34,63,1,Cream);p.Line(24,56,44,56,1,Cream);p.Line(15,6,6+frame,2,2,Hex(0x080715));break;
                    case HostKind.Ink:p.Line(52,27,59,10,3,Ink);p.Line(55,19,60,9,1,Cream);p.Ellipse(60,5,2,3,Mid);p.Ellipse(18,5,9,2,Ink);break;
                }
            }
            if(pose==3){p.Line(7,51,4,59,1,Cream);p.Line(57,48,60,57,1,Cream);}var sprite=p.Finish(key);bank[key]=sprite;return sprite;
        }

        public static Sprite Enemy(int world,int frame,bool stunned,bool armor)
        {
            frame&=1;string key="patrol-"+world+"-"+frame+"-"+stunned+"-"+armor;if(bank.TryGetValue(key,out var cached))return cached;
            var p=new Pixel(32);Color32 coat=world==1?Hex(0x785379):world==2?Hex(0x66754e):world==3?Hex(0x567184):world==4?Hex(0x836878):Hex(0xb89d73);
            p.Ellipse(16,14,12,12,Ink);p.Ellipse(16,15,10,9,coat);p.Rect(7,2+frame,7,4,Ink);p.Rect(20,3-frame,7,4,Ink);p.Rect(8,4+frame,5,2,High);p.Rect(21,5-frame,5,2,High);
            p.Ellipse(16,20,9,10,Ink);p.Ellipse(16,21,7,8,Cream);p.Ellipse(17,15,3,2,Ink);p.Line(14,25,13,19,1,Ink);p.Line(21,25,20,19,1,Ink);
            if(stunned){p.Line(10,24,15,20,1,Ink);p.Line(10,20,15,24,1,Ink);p.Line(18,24,23,20,1,Ink);p.Line(18,20,23,24,1,Ink);p.Rect(2,27,3,2,Gold);p.Rect(26,29,3,2,Gold);}
            if(armor){p.Rect(5,24,24,4,Dark);p.Ellipse(16,27,10,5,Ink);p.Ellipse(16,28,8,3,Hex(0x879ba9));p.Rect(15,24,2,7,White);}
            else if(world==2){p.Line(9,27,4,31,1,Gold);p.Line(23,27,28,31,1,Gold);p.Rect(5,29,3,2,High);}
            else if(world==3){p.Rect(7,28,21,2,Dark);p.Rect(10,29,15,3,coat);}
            else if(world==4){p.Line(8,27,3,21,2,coat);p.Line(24,27,29,21,2,coat);}
            else if(world==5)p.Ring(16,30,12,2,1,Gold);
            p.Rect(2,10,4,6,Ink);p.Rect(3,12,3,4,White);p.Rect(27,10,4,6,Ink);p.Rect(27,12,3,4,White);
            var sprite=p.Finish(key,32);bank[key]=sprite;return sprite;
        }
        public static Sprite Tenant(HostKind kind,int frame=0)
        {
            frame&=3;string key="tenant-"+kind+"-"+frame;if(bank.TryGetValue(key,out var cached))return cached;
            var p=new Pixel(64);int bob=frame==1||frame==3?1:0;
            void Body(int x,int y,int rx,int ry,Color32 c){p.Ellipse(x,y,rx+2,ry+2,Ink);p.Ellipse(x,y,rx,ry,c);}
            switch(kind){
                case HostKind.Echo:
                    Body(32,31,16,18,Hex(0x779db7));p.Rect(13,14,39,5,Ink);p.Rect(15,17,35,4,Gold);p.Ring(32,48,6,7,2,Gold);Eye(p,33,33,6,9,1);p.Line(9,36,4,36,1,Lavender);p.Line(57,27,61,27,1,Lavender);break;
                case HostKind.Marionette:
                    for(int k=0;k<4;k++){int y=18+k*8;for(int d=-1;d<=1;d+=2){p.Line(32,y,32+d*24,y+7-k*3,2,Ink);p.Line(32+d*24,y+7-k*3,32+d*27,y-5+bob,1,High);}}
                    Body(32,33,13,17,Purple);p.Line(32,46,32,63,1,Cream);p.Ring(32,44,7,7,2,Gold);Eye(p,28,33,4,6,0);Eye(p,38,33,4,6,0);break;
                case HostKind.Molt:
                    Body(16,34,13,21,Hex(0xa56c87));Body(48,34,13,21,Hex(0xa56c87));p.Ring(15,37,8,12,2,Cream);p.Ring(48,37,8,12,2,Cream);Body(32,27,6,18,Dark);p.Line(29,41,23,57,1,Gold);p.Line(35,41,42,57,1,Gold);Eye(p,30,42,3,4,0);Eye(p,36,42,3,4,0);break;
                case HostKind.Wax:
                    for(int d=-1;d<=1;d+=2){Body(32+d*14,35,12,16,Cream);p.Line(32+d*12,27,32+d*23,39,1,Gold);}
                    p.Rect(24,12,20,33,Ink);p.Rect(27,14,14,30,Hex(0xb89b74));Body(33,46,10,10,Cream);Eye(p,34,46,4,5,1);p.Ring(33,59,12,3,1,Gold);p.Line(44,32,53,30,2,Gold);p.Rect(49,27,7,16,Cream);p.Ellipse(54,48+bob,3,5,Rose);p.Line(51,27,51,16,2,Gold);break;
                case HostKind.Gullet:
                    Body(23,28,20,21,Hex(0x81946c));p.Ring(22,29,15,15,2,Gold);p.Ring(22,29,9,9,2,Dark);p.Ring(22,29,3,4,1,Cream);Body(39,14,20,8,Hex(0xbfa781));p.Line(44,22,43,43,2,Ink);p.Line(56,22,58,45,2,Ink);Eye(p,43,44,4,6,0);Eye(p,57,45,4,6,0);p.Ellipse(51,20,10,8,Ink);for(int x=44;x<=59;x+=4)p.Rect(x,22,2,4,Cream);break;
                case HostKind.Root:
                    for(int i=0;i<12;i++){float a=i*Mathf.PI/6;p.Line(32,31,32+(int)(Mathf.Cos(a)*27),31+(int)(Mathf.Sin(a)*27),2,Hex(0x647d45));}
                    Body(32,31,18,18,Hex(0x445a32));for(int i=0;i<8;i++)p.Ellipse(23+(i%3)*9,18+(i/3)*10,3,3,Gold);Eye(p,32,34,7,10,0);break;
                case HostKind.Mirror:
                    p.Rect(13,7,38,42,Ink);p.Ellipse(32,42,20,18,Ink);p.Ellipse(32,45,10,11,Hex(0xb297a7));p.Line(17,10,25,48,1,High);p.Line(46,10,41,46,1,High);Eye(p,30,47,4,5,0);Body(36,29,14,19,Gold);p.Ellipse(36,30,11,16,Hex(0x698bab));p.Line(29,27,42,42,1,White);p.Line(33,17,44,30,1,Lavender);break;
                case HostKind.InsideOut:
                    Body(14,38,11,15,White);Body(50,38,11,15,White);Body(32,36,12,16,Pink);p.Ring(32,55,12,4,1,Gold);p.Ellipse(32,38,8,11,Cream);p.Line(22,37,38,25,2,Ink);p.Line(21,34,11,16,2,Pink);p.Line(42,34,56,13,2,Pink);p.Line(11,16,19,8,1,White);Eye(p,31,41,4,5,0);break;
                case HostKind.Parallax:
                    Body(17,41+bob,14,14,Lavender);Body(47,41-bob,14,14,Lavender);Body(32,26,13,16,Dark);Eye(p,23,35,8,10,1);Eye(p,42,38,8,11,-1);p.Line(27,15,20,5,1,Gold);p.Line(38,15,43,5,1,Gold);p.Ellipse(26,37,2,4,Hex(0x739fbb));break;
                case HostKind.Censer:
                    p.Line(32,59,12,24,1,Gold);p.Line(32,59,52,24,1,Gold);p.Ring(32,60,4,4,1,Cream);Body(32,23,22,13,Hex(0x55747e));p.Rect(12,26,40,3,Gold);for(int x=16;x<=49;x+=8)p.Rect(x,14,2,12,Gold);for(int k=0;k<3;k++)p.Ring(20+k*12,40+(k+frame)%3*4,6,7,1,Hex(0xa5c3c6));Eye(p,32,23,5,7,0);break;
                case HostKind.Stitch:
                    for(int k=0;k<3;k++){p.Line(30,20+k*8,7,12+k*15,1,Gold);p.Line(35,20+k*8,51,12+k*15,1,Gold);}
                    Body(30,28,12,18,Hex(0x985571));p.Ring(29,22,8,7,3,Cream);Eye(p,30,40,5,7,1);p.Line(54,5,57,57,2,Ink);p.Line(55,6,58,57,1,White);p.Ring(57,58,3,4,1,Gold);p.Line(55,60,34,58,1,Pink);break;
                case HostKind.Coffin:
                    for(int d=-1;d<=1;d+=2){p.Line(32,23,32+d*25,14,2,Ink);p.Line(32+d*25,14,32+d*28,7+bob,1,Rose);Body(32+d*22,29,7,10,Hex(0xa05a60));}
                    p.Rect(14,15,35,31,Ink);p.Rect(18,12,27,38,Ink);p.Rect(20,16,23,30,Hex(0x85545f));p.Rect(24,22,15,2,Gold);p.Rect(30,19,2,10,Gold);Eye(p,26,48,4,6,0);Eye(p,38,48,4,6,0);break;
                case HostKind.Lodestone:
                    p.Rect(13,5,38,8,Ink);p.Rect(16,10,32,5,Gold);p.Rect(22,14,22,23,Hex(0x87969c));Body(33,41,12,13,Hex(0xa5b2b6));Eye(p,34,41,4,6,1);p.Ring(32,40,27,20,3,Ink);p.Ring(32,40,25,18,2,Gold);p.Rect(6,32,6,12,Hex(0xcf616c));p.Rect(52,32,6,12,Hex(0x6596c4));break;
                case HostKind.Shadow:
                    p.Rect(29,5,6,28,Ink);p.Rect(18,4,29,4,Gold);p.Line(32,32,32,59,2,Gold);Body(32,40,20,19,Gold);p.Ellipse(32,40,16,15,Cream);p.Ellipse(34,39,8,11,Ink);p.Line(5,41,13,41,1,White);p.Line(52,41,60,41,1,White);p.Line(32,59,32,63,1,White);break;
                default:
                    for(int i=0;i<6;i++){int x=12+i*7,y=20+(int)(Mathf.Sin(i*.65f+frame*.35f)*9);Body(x,y,8,10,Dark);p.Line(x-3,y-3,x+3,y+3,1,Mid);}
                    Body(51,36,10,15,Hex(0x6f5486));Eye(p,51,41,4,6,0);p.Line(53,23,57,12,3,Ink);p.Line(55,20,57,12,1,Cream);p.Ellipse(57,6,3,3,Mid);break;
            }
            var sprite=p.Finish(key);bank[key]=sprite;return sprite;
        }
        public static Sprite Boss(int world,int act=0)
        {
            var authored=BroadArt.Get("boss_"+world+"_"+Mathf.Clamp(act,0,2),128)??V6Art.Sprite("boss_"+world+"_"+Mathf.Clamp(act,0,2),128);if(authored)return authored;
            string key="boss-"+world+"-"+act;if(bank.TryGetValue(key,out var old))return old;
            var p=new Pixel(128);Color32 flesh=world==2?Hex(0x9d9b60):world==3?Hex(0x8ca6ac):world==5?Hex(0xd9bd9f):Hex(0x927084);
            p.Ellipse(64,57,46,54,Ink);p.Ellipse(64,57,42,49,Dark);
            for(int d=-1;d<=1;d+=2){p.Line(64+d*31,49,64+d*54,10,5,Ink);p.Line(64+d*31,49,64+d*54,10,3,flesh);}
            p.Ellipse(64,81,36,36,Ink);p.Ellipse(64,81,32,32,flesh);p.Ellipse(57,85,25,28,Hex(0xb99c99));
            for(int y=15;y<65;y+=8){p.Line(40,y,58,y+4,1,flesh);p.Line(70,y+4,90,y,1,flesh);}
            Eye(p,49,85,9,13,act==1?-2:1,true);Eye(p,79,88,9,13,act==1?2:-1,true);
            p.Ellipse(65,58,18,act==2?17:10,Ink);for(int x=53;x<=79;x+=6)p.Rect(x,act==2?68:62,3,5,Cream);
            if(world==1){p.Rect(23,110,81,5,Ink);p.Rect(34,116,57,8,Gold);p.Line(33,105,27,74,2,Gold);p.Line(97,105,101,74,2,Gold);}
            if(world==2){for(int x=18;x<115;x+=18){p.Line(64,109,x,122,2,Hex(0x69804c));p.Ellipse(x,116,7,9,Gold);Eye(p,x,117,2,3,0);}}
            if(world==3){p.Rect(26,115,74,4,Gold);p.Line(29,120,10,62,1,Lavender);p.Line(99,120,117,62,1,Lavender);p.Ring(65,88,31,31,1,Cream);}
            if(world==4){for(int i=0;i<4;i++){Eye(p,25+i*25,37+(i%2)*12,5,7,0);p.Line(22+i*25,25,18+i*26,5,2,flesh);}}
            if(world==5){p.Ring(64,88,49,35,2,Gold);for(int i=0;i<5;i++)Eye(p,26+i*19,115-(i%2)*10,4,6,0);p.Line(60,59,59,19,3,Pink);p.Line(72,53,80,12,2,Rose);}
            old=p.Finish(key,128);bank[key]=old;return old;
        }

        public static Sprite Terrain(int world)
        {
            string key="terrain-"+world;if(bank.TryGetValue(key,out var s))return s;var p=new Pixel(32);var shade=Hex(0xcbbfd1);p.Rect(0,0,32,32,shade);p.Rect(0,29,32,3,White);p.Rect(0,0,32,3,Hex(0x796b84));
            switch(world){case 1:case 3:for(int y=0;y<32;y+=8){p.Rect(0,y,32,1,Hex(0x978499));for(int x=(y/8%2)*8;x<32;x+=16)p.Rect(x,y,1,8,Hex(0x978499));}break;
                case 2:for(int x=3;x<32;x+=7){p.Line(x,2,x+2,14,1,Hex(0x9d9a83));p.Line(x+2,14,x-1,27,1,Hex(0x9d9a83));}break;
                case 4:for(int x=4;x<32;x+=8){p.Rect(x,4,1,22,Hex(0x9e8693));p.Rect(x+2,6,1,16,Hex(0xebe0dd));}break;
                default:for(int x=4;x<32;x+=8){p.Line(x,7,x+4,15,1,Hex(0x9e9485));p.Line(x+4,15,x,23,1,Hex(0x9e9485));}break;}
            s=p.Finish(key,32);bank[key]=s;return s;
        }
    }
}
