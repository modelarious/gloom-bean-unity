using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Hand-authored pixel primitives and palettes. Art never defines collision,
    // saves, generated rules or puzzle validity. No models or external assets.
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
                Shoe(p,23,8+foot+lift);Shoe(p,44,8-foot+lift);
                p.Line(26,43,24,54,4,Ink);p.Line(24,54,30,59,4,Ink);p.Line(30,59,34,56,4,Ink);p.Line(26,44,25,54,2,Mid);p.Line(25,54,30,57,2,High);
                p.Ellipse(33,29+bob,21,21,Ink);p.Ellipse(33,29+bob,18,18,body);p.Ellipse(29,34+bob,15,15,Mid);p.Ellipse(25,38+bob,9,8,High);p.Ellipse(38,23+bob,13,12,body);p.Ellipse(38,19+bob,12,7,Dark);
                Glove(p,8,23-foot+(pose==1?2:0));Glove(p,55,pose==1?32:23+foot);
                Eye(p,28,33+bob,10,15,2,corrupt);Eye(p,47,corrupt?47:37+bob,7,10,1,corrupt);
                if(corrupt){p.Line(40,37,46,42,3,Pink);p.Line(24,22,22,5+frame,2,Pink);p.Line(35,18,33,4,3,Pink);p.Ellipse(47,23,10,8,Pink);p.Ellipse(49,23,6,5,Ink);p.Line(51,19,56,12,2,Pink);p.Line(56,12,60,14,2,Rose);p.Rect(45,25,3,3,Cream);p.Rect(50,27,2,2,White);p.Ellipse(31,6,3,2,Rose);}
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
