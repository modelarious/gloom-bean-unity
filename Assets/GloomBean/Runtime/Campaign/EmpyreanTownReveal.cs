using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A distant image of the opening street, never a restoration of the Host or save.
    public sealed class EmpyreanTownReveal:MonoBehaviour
    {
        Transform panorama;Camera camera;public bool Revealed=>panorama&&panorama.gameObject.activeInHierarchy;
        void Start()
        {
            camera=Camera.main;panorama=new GameObject("The street that was always underneath").transform;panorama.SetParent(transform,false);
            for(int i=-8;i<=8;i++){
                float x=i*2.1f,h=2.6f+(i*i%4)*.35f;var color=new Color(.41f+(i*i%3)*.04f,.46f,.42f);
                var g=PrimitiveArt.Shape("Small Sunday facade",panorama,new Vector2(x,h*.5f),new Vector2(1.8f,h),color,PrimitiveArt.Icon.Block,-17);
                PrimitiveArt.Shape("Distant roof",panorama,new Vector2(x,h+.45f),new Vector2(2,.9f),new Color(.52f,.43f,.4f),PrimitiveArt.Icon.Spike,-17);
                for(int k=-1;k<=1;k++)PrimitiveArt.Shape("A window the size of a memory",panorama,new Vector2(x+k*.45f,h*.6f),new Vector2(.18f,.3f),new Color(.78f,.76f,.55f),PrimitiveArt.Icon.Eye,-16);
                PrimitiveArt.Shape("The old painted smile",panorama,new Vector2(x,h*.35f),new Vector2(.65f,.35f),new Color(.22f,.27f,.26f),PrimitiveArt.Icon.Arch,-16);
            }
            PrimitiveArt.Shape("The impossible distance",panorama,new Vector2(0,-1.4f),new Vector2(40,3),new Color(.16f,.18f,.2f),PrimitiveArt.Icon.Block,-17);
            StageSession.Current?.Notice("The opening town. It was below you the entire time.");
        }
        void LateUpdate(){if(camera&&panorama)panorama.position=new Vector3(camera.transform.position.x*.8f,camera.transform.position.y-camera.orthographicSize-1.2f,0);}
    }
}
