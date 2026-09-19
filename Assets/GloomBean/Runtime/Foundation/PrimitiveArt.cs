using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    // Original, deterministic stand-in art. The simulation never depends on image generation.
    public static class PrimitiveArt
    {
        public enum Icon { Block, Round, Diamond, Key, Eye, Spike, Arch, Star, Stripe }
        static readonly Dictionary<Icon,Sprite> sprites=new Dictionary<Icon,Sprite>();
        static Material lineMaterial;
        public static Sprite Sprite(Icon icon)
        {
            if(sprites.TryGetValue(icon,out var value)&&value)return value;
            int n=32;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name="Original "+icon};
            var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float dx=(x-15.5f)/15.5f,dy=(y-15.5f)/15.5f;bool fill=true;
                switch(icon)
                {
                    case Icon.Round:case Icon.Eye:fill=dx*dx+dy*dy<1;break;
                    case Icon.Diamond:fill=Mathf.Abs(dx)+Mathf.Abs(dy)<1;break;
                    case Icon.Key:fill=(dx*dx+(dy-.42f)*(dy-.42f)<.28f)||(Mathf.Abs(dx)<.13f&&dy<.4f)||(dx>0&&dx<.47f&&dy<-.45f);break;
                    case Icon.Spike:fill=dy<1-Mathf.Abs(dx)*2;break;
                    case Icon.Arch:fill=(dx*dx+dy*dy<1&&dx*dx+dy*dy>.52f)||Mathf.Abs(dx)>.72f&&dy<0;break;
                    case Icon.Star:fill=Mathf.Abs(dx)+Mathf.Abs(dy)<.8f||Mathf.Abs(dx)<.12f||Mathf.Abs(dy)<.12f;break;
                    case Icon.Stripe:fill=true;break;
                }
                if(!fill){pixels[x+y*n]=Color.clear;continue;}
                float shade=(x<2||x>29||y<2||y>29)? .64f:1f;
                if(icon==Icon.Block&&(y<4||x%16==0))shade=.7f;
                if(icon==Icon.Stripe&&(x+y)%12<5)shade=.52f;
                pixels[x+y*n]=new Color(shade,shade,shade,1);
            }
            texture.SetPixels(pixels);texture.Apply();value=UnityEngine.Sprite.Create(texture,new Rect(0,0,n,n),new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect);
            sprites[icon]=value;return value;
        }
        public static GameObject Shape(string name,Transform parent,Vector2 position,Vector2 size,Color color,Icon icon=Icon.Block,int order=0)
        {
            var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=new Vector3(size.x,size.y,1);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=Sprite(icon);sr.color=color;sr.sortingOrder=order;return go;
        }
        public static LineRenderer Line(string name,Transform parent,Vector3 a,Vector3 b,float width,Color color,int order=1)
        {
            Transform t=parent.Find(name);GameObject g=t?t.gameObject:new GameObject(name);if(!t)g.transform.SetParent(parent);
            var line=g.GetComponent<LineRenderer>();if(!line)line=g.AddComponent<LineRenderer>();
            if(!lineMaterial)lineMaterial=new Material(Shader.Find("Sprites/Default"));line.sharedMaterial=lineMaterial;
            line.positionCount=2;line.useWorldSpace=true;line.SetPosition(0,a);line.SetPosition(1,b);line.startWidth=line.endWidth=width;line.startColor=line.endColor=color;line.sortingOrder=order;return line;
        }
        public static TextMesh Label(string text,Transform parent,Vector2 position,float size=.16f,Color? color=null)
        {
            var g=new GameObject("Label "+text);g.transform.SetParent(parent);g.transform.position=new Vector3(position.x,position.y,-.1f);
            var label=g.AddComponent<TextMesh>();label.text=text;label.fontSize=48;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=color??new Color(.96f,.87f,.66f);
            label.GetComponent<MeshRenderer>().sortingOrder=20;return label;
        }
    }


}
