using UnityEngine;
using GloomBean.Foundation;
using System.Collections.Generic;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(240)]
    public sealed class GbaWorldSign:MonoBehaviour
    {
        static readonly HashSet<string> objectCaptions=new HashSet<string>{"EMPTY FRAME","FELT","SHEARS","TAILOR BRUSH","COLD FONT","EMETIC HERBS","SALT","MATTE VELVET","FLAT SIGN","CROSSWIND","SCISSORS","GRAVE MOUTH","CERAMIC ARCH","ECLIPSE ARCH","BLOTTING SAINT"};
        public static float CaptionVisibility(string text,float distance)=>objectCaptions.Contains(text)?Mathf.Clamp01((5f-distance)/1.5f):1f;
        TextMesh original;MeshRenderer oldRenderer;SpriteRenderer image;string previous;Texture2D owned;
        public void Initialize(TextMesh text){original=text;oldRenderer=text.GetComponent<MeshRenderer>();var go=new GameObject("V6 visual / pixel sign");go.transform.SetParent(transform,false);image=go.AddComponent<SpriteRenderer>();image.sortingOrder=20;}
        void LateUpdate(){if(!original||!image)return;string text=GbaPixels.Normalize(original.text);bool visible=oldRenderer&&oldRenderer.enabled;image.enabled=visible;
            if(text!=previous){previous=text;int width=Mathf.Clamp(text.Length*6,6,192),height=text.Length>32?16:8;
                if(image.sprite)Destroy(image.sprite);if(owned)Destroy(owned);owned=GbaPixels.TextTexture(text,width,height,1,true,true);image.sprite=Sprite.Create(owned,new Rect(0,0,width,height),new Vector2(.5f,.5f),16,0,SpriteMeshType.FullRect);
            }
            if(oldRenderer)oldRenderer.forceRenderingOff=true;var color=original.color;
            var actor=StageSession.Current?StageSession.Current.player:null;if(actor)color.a*=CaptionVisibility(text,Vector2.Distance(actor.transform.position,transform.position));image.color=color;

        }
        void OnDestroy(){if(oldRenderer)oldRenderer.forceRenderingOff=false;if(image&&image.sprite)Destroy(image.sprite);if(owned)Destroy(owned);}
    }
}
