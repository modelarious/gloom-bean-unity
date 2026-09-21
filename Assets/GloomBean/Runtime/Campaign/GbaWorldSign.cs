using UnityEngine;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(240)]
    public sealed class GbaWorldSign:MonoBehaviour
    {
        TextMesh original;MeshRenderer oldRenderer;SpriteRenderer image;string previous;
        public void Initialize(TextMesh text){original=text;oldRenderer=text.GetComponent<MeshRenderer>();var go=new GameObject("V6 visual / pixel sign");go.transform.SetParent(transform,false);image=go.AddComponent<SpriteRenderer>();image.sortingOrder=20;}
        void LateUpdate(){if(!original||!image)return;string text=GbaPixels.Normalize(original.text);bool visible=oldRenderer&&oldRenderer.enabled;image.enabled=visible;
            if(text!=previous){previous=text;int width=Mathf.Clamp(text.Length*6,6,192),height=text.Length>32?16:8;
                var texture=GbaPixels.TextTexture(text,width,height,1,true);if(image.sprite)Destroy(image.sprite);image.sprite=Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),16,0,SpriteMeshType.FullRect);
            }
            if(oldRenderer)oldRenderer.forceRenderingOff=true;image.color=original.color;
        }
        void OnDestroy(){if(oldRenderer)oldRenderer.forceRenderingOff=false;if(image&&image.sprite)Destroy(image.sprite);}
    }
}
