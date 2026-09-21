using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace GloomBean.Campaign
{
    // Original 5x7 bitmap lettering. No external font dependency or smoothed dynamic glyphs.
    public static class GbaPixels
    {
        static readonly Dictionary<char,string> letters=new Dictionary<char,string>{
        {'A',"01110/10001/10001/11111/10001/10001/10001"},{'B',"11110/10001/10001/11110/10001/10001/11110"},
        {'C',"01111/10000/10000/10000/10000/10000/01111"},{'D',"11110/10001/10001/10001/10001/10001/11110"},
        {'E',"11111/10000/10000/11110/10000/10000/11111"},{'F',"11111/10000/10000/11110/10000/10000/10000"},
        {'G',"01111/10000/10000/10111/10001/10001/01111"},{'H',"10001/10001/10001/11111/10001/10001/10001"},
        {'I',"11111/00100/00100/00100/00100/00100/11111"},{'J',"00111/00010/00010/00010/10010/10010/01100"},
        {'K',"10001/10010/10100/11000/10100/10010/10001"},{'L',"10000/10000/10000/10000/10000/10000/11111"},
        {'M',"10001/11011/10101/10101/10001/10001/10001"},{'N',"10001/11001/11001/10101/10011/10011/10001"},
        {'O',"01110/10001/10001/10001/10001/10001/01110"},{'P',"11110/10001/10001/11110/10000/10000/10000"},
        {'Q',"01110/10001/10001/10001/10101/10010/01101"},{'R',"11110/10001/10001/11110/10100/10010/10001"},
        {'S',"01111/10000/10000/01110/00001/00001/11110"},{'T',"11111/00100/00100/00100/00100/00100/00100"},
        {'U',"10001/10001/10001/10001/10001/10001/01110"},{'V',"10001/10001/10001/10001/10001/01010/00100"},
        {'W',"10001/10001/10001/10101/10101/10101/01010"},{'X',"10001/10001/01010/00100/01010/10001/10001"},
        {'Y',"10001/10001/01010/00100/00100/00100/00100"},{'Z',"11111/00001/00010/00100/01000/10000/11111"},
        {'0',"01110/10001/10011/10101/11001/10001/01110"},{'1',"00100/01100/00100/00100/00100/00100/01110"},
        {'2',"01110/10001/00001/00010/00100/01000/11111"},{'3',"11110/00001/00001/01110/00001/00001/11110"},
        {'4',"00010/00110/01010/10010/11111/00010/00010"},{'5',"11111/10000/10000/11110/00001/00001/11110"},
        {'6',"01110/10000/10000/11110/10001/10001/01110"},{'7',"11111/00001/00010/00100/01000/01000/01000"},
        {'8',"01110/10001/10001/01110/10001/10001/01110"},{'9',"01110/10001/10001/01111/00001/00001/01110"},
        {'-',"00000/00000/00000/11111/00000/00000/00000"},{'+',"00000/00100/00100/11111/00100/00100/00000"},
        {':',"00000/00100/00100/00000/00100/00100/00000"},{'.',"00000/00000/00000/00000/00000/00110/00110"},
        {',',"00000/00000/00000/00000/00110/00100/01000"},{'/',"00001/00001/00010/00100/01000/10000/10000"},
        {'!',"00100/00100/00100/00100/00100/00000/00100"},{'?',"01110/10001/00001/00010/00100/00000/00100"},
        {'>',"10000/01000/00100/00010/00100/01000/10000"},{'<',"00001/00010/00100/01000/00100/00010/00001"},
        {'[',"01110/01000/01000/01000/01000/01000/01110"},{']',"01110/00010/00010/00010/00010/00010/01110"},
        {'(',"00010/00100/01000/01000/01000/00100/00010"},{')',"01000/00100/00010/00010/00010/00100/01000"},
        {'\'',"00100/00100/00000/00000/00000/00000/00000"},{'%',"11001/11010/00100/00100/01000/10110/00110"},
        {'=',"00000/00000/11111/00000/11111/00000/00000"},{'|',"00100/00100/00100/00100/00100/00100/00100"}
        };
        static readonly Dictionary<string,Texture2D> textCache=new Dictionary<string,Texture2D>();
        static readonly Dictionary<(UnityEngine.Sprite,int,int),Texture2D> imageCache=new Dictionary<(UnityEngine.Sprite,int,int),Texture2D>();
        public static string Normalize(string s)=>(s??"").ToUpperInvariant().Replace('—','-').Replace('–','-').Replace('’','\'').Replace('·','/').Replace("→",">").Replace("×","X").Replace("…","...");
        public static Texture2D TextTexture(string raw,int width,int height,int scale=1,bool shadow=true)
        {
            width=Mathf.Clamp(width,1,480);height=Mathf.Clamp(height,1,200);scale=Mathf.Clamp(scale,1,3);string value=Normalize(raw);
            string key=value+"/"+width+"/"+height+"/"+scale+"/"+shadow;if(textCache.TryGetValue(key,out var result))return result;
            int cols=Math.Max(1,width/(6*scale));var lines=new List<string>();
            foreach(string paragraph in value.Split('\n')){string line="";foreach(string word in paragraph.Split(' ')){
                if(line.Length>0&&line.Length+word.Length+1>cols){lines.Add(line);line="";}
                if(line.Length>0)line+=" ";line+=word;
                while(line.Length>cols){lines.Add(line.Substring(0,cols));line=line.Substring(cols);}
            }lines.Add(line);}
            var pixels=new Color32[width*height];
            void Dot(int x,int y,Color32 c){if(x>=0&&y>=0&&x<width&&y<height)pixels[x+(height-1-y)*width]=c;}
            for(int pass=shadow?0:1;pass<2;pass++)for(int row=0;row<lines.Count;row++){
                int yy=row*8*scale;if(yy+7*scale>height)break;
                for(int col=0;col<lines[row].Length;col++){if(!letters.TryGetValue(lines[row][col],out var pattern))continue;int n=0;
                    foreach(char bit in pattern){if(bit=='/')continue;if(bit=='1')for(int dy=0;dy<scale;dy++)for(int dx=0;dx<scale;dx++)Dot(col*6*scale+(n%5)*scale+dx+(pass==0?1:0),yy+(n/5)*scale+dy+(pass==0?1:0),pass==0?new Color32(0,0,0,230):new Color32(255,255,255,255));n++;}
                }
            }
            result=new Texture2D(width,height,TextureFormat.RGBA32,false){name="Original GBA glyphs",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};result.SetPixels32(pixels);result.Apply(false,true);
            if(textCache.Count>384){foreach(var t in textCache.Values)UnityEngine.Object.Destroy(t);textCache.Clear();}textCache[key]=result;return result;
        }
        public static void Fill(Rect rect,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        public static void Text(Rect rect,string value,Color tint,int scale=1,bool shadow=true){var old=GUI.color;GUI.color=tint;GUI.DrawTexture(rect,TextTexture(value,Mathf.RoundToInt(rect.width),Mathf.RoundToInt(rect.height),scale,shadow),ScaleMode.StretchToFill,true);GUI.color=old;}
        public static void LegacyLabel(Rect rect,string value,int fontSize,Color color){var m=GUI.matrix;GUI.matrix=GbaDisplay.PixelMatrix;Text(GbaDisplay.PixelRect(rect),value,color,fontSize>=35?2:1,false);GUI.matrix=m;}
        public static void LegacyPanel(Rect rect,Color color){var m=GUI.matrix;GUI.matrix=GbaDisplay.PixelMatrix;Fill(GbaDisplay.PixelRect(rect),color);GUI.matrix=m;}
        public static void LegacyButton(Rect rect,string value,bool selected,bool enabled){var m=GUI.matrix;GUI.matrix=GbaDisplay.PixelMatrix;var r=GbaDisplay.PixelRect(rect);
            Fill(r,new Color(.08f,.055f,.15f,1));Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),selected?new Color(.45f,.19f,.39f):new Color(.19f,.14f,.28f));
            if(selected)Fill(new Rect(r.x+1,r.y+1,2,r.height-2),new Color(1,.78f,.40f));
            Text(new Rect(r.x+5,r.y+3,r.width-8,r.height-4),(selected?"> ":"  ")+value,enabled?new Color(1,.94f,.76f):new Color(.49f,.45f,.49f),1,false);GUI.matrix=m;}
        public static void Sprite(Rect rect,UnityEngine.Sprite sprite,Color tint)
        {
            if(!sprite)return;int width=Mathf.Max(1,Mathf.RoundToInt(rect.width)),height=Mathf.Max(1,Mathf.RoundToInt(rect.height));var key=(sprite,width,height);
            if(!imageCache.TryGetValue(key,out var texture)){
                Texture2D source=sprite.texture;Color32[] pixels;
                if(source.isReadable)pixels=source.GetPixels32();else{var prior=RenderTexture.active;var rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32);Graphics.Blit(source,rt);RenderTexture.active=rt;var copy=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);copy.ReadPixels(new Rect(0,0,source.width,source.height),0,0);copy.Apply();pixels=copy.GetPixels32();RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(copy);}
                var dst=new Color32[width*height];
                float fit=Mathf.Min(width/(float)source.width,height/(float)source.height);int w=Mathf.Max(1,Mathf.RoundToInt(source.width*fit)),h=Mathf.Max(1,Mathf.RoundToInt(source.height*fit));
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)dst[x+(width-w)/2+(y+(height-h)/2)*width]=pixels[Mathf.Min(source.width-1,(int)((x+.5f)*source.width/w))+Mathf.Min(source.height-1,(int)((y+.5f)*source.height/h))*source.width];
                texture=new Texture2D(width,height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};texture.SetPixels32(dst);texture.Apply(false,true);
                if(imageCache.Count>128){foreach(var t in imageCache.Values)UnityEngine.Object.Destroy(t);imageCache.Clear();}imageCache[key]=texture;
            }
            var old=GUI.color;GUI.color=tint;GUI.DrawTexture(rect,texture,ScaleMode.StretchToFill,true);GUI.color=old;
        }
        public static void LegacySprite(Rect rect,UnityEngine.Sprite sprite,Color tint){var m=GUI.matrix;GUI.matrix=GbaDisplay.PixelMatrix;Sprite(GbaDisplay.PixelRect(rect),sprite,tint);GUI.matrix=m;}
    }
}
