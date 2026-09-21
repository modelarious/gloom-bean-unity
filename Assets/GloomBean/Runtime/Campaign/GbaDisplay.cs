using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Campaign-only presentation adapter. Simulation, colliders and input are not pixel-snapped.
    // The camera renders a genuine 240x160 framebuffer; the backbuffer receives integer pixels.
    [DefaultExecutionOrder(950)]
    public sealed class GbaDisplay:MonoBehaviour
    {
        public const int Width=240,Height=160;
        public static GbaDisplay Instance {get;private set;}
        public RenderTexture Frame {get;private set;}
        GameRoot game;Camera camera;float originalAspect;bool attached;
        public static int Scale=>Mathf.Max(1,Mathf.FloorToInt(Mathf.Min(Screen.width/(float)Width,Screen.height/(float)Height)));
        public static Rect Viewport=>new Rect(Mathf.Floor((Screen.width-Width*Scale)*.5f),Mathf.Floor((Screen.height-Height*Scale)*.5f),Width*Scale,Height*Scale);
        public static Matrix4x4 PixelMatrix=>Matrix4x4.TRS(new Vector3(Viewport.x,Viewport.y,0),Quaternion.identity,new Vector3(Scale,Scale,1));
        public static Matrix4x4 LegacyMatrix=>PixelMatrix*Matrix4x4.Scale(new Vector3(Width/960f,Height/600f,1));
        public static Rect PixelRect(Rect r)=>new Rect(Mathf.Round(r.x/4),Mathf.Round(r.y/3.75f),Mathf.Max(1,Mathf.Round(r.width/4)),Mathf.Max(1,Mathf.Round(r.height/3.75f)));
        void Awake(){Instance=this;game=GetComponent<GameRoot>();Frame=new RenderTexture(Width,Height,24,RenderTextureFormat.ARGB32){name="GBA 240x160 native framebuffer",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,antiAliasing=1,useMipMap=false,autoGenerateMips=false};Frame.Create();}
        void LateUpdate()
        {
            if(!camera)camera=Camera.main;if(!camera||!game)return;
            bool active=game.Session&&game.Session.gameObject.activeInHierarchy;
            if(active){if(!attached){originalAspect=camera.aspect;attached=true;}camera.targetTexture=Frame;camera.aspect=Width/(float)Height;
                var follow=camera.GetComponent<FollowCamera>();if(follow){float size=game.Session.definition.atlas?5f:7f;
                    var host=game.Session.player?game.Session.player.GetComponent<HostController>():null;if(host)foreach(var form in host.Forms)if(form.Kind==HostKind.Mirror)size=7f;
                    if(game.Session.definition.boss)size=7f;follow.baseSize=size;}
            }else if(attached){if(camera.targetTexture==Frame)camera.targetTexture=null;camera.aspect=originalAspect;attached=false;}
        }
        void OnGUI()
        {
            if(!game||Event.current.type!=EventType.Repaint)return;
            var matrix=GUI.matrix;int depth=GUI.depth;var color=GUI.color;
            GUI.depth=10000;GUI.matrix=Matrix4x4.identity;GUI.color=Color.black;GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
            GUI.color=Color.white;if(attached&&Frame)GUI.DrawTexture(Viewport,Frame,ScaleMode.StretchToFill,false);
            GUI.color=color;GUI.matrix=matrix;GUI.depth=depth;
        }
        void OnDestroy(){if(camera&&camera.targetTexture==Frame)camera.targetTexture=null;if(Frame){Frame.Release();Destroy(Frame);}if(Instance==this)Instance=null;}
    }
}
