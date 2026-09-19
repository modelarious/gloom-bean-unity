using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class HostView : MonoBehaviour
    {
        HostController host;Transform costume;HostKind last=(HostKind)(-1);
        void Start(){host=GetComponent<HostController>();}
        void LateUpdate()
        {
            if(!host)return;if(last==host.Primary)return;last=host.Primary;if(costume)Destroy(costume.gameObject);
            costume=new GameObject("Possession silhouette").transform;costume.SetParent(transform,false);
            var k=host.Primary;Color c=new Color(.92f,.64f,.3f);
            void Part(string n,Vector2 p,Vector2 s,PrimitiveArt.Icon icon,Color col){var g=PrimitiveArt.Shape(n,costume,Vector2.zero,s,col,icon,16);g.transform.localPosition=p;}
            switch(k)
            {
                case HostKind.Marionette:Part("Shoulder crossbar",new Vector2(0,.6f),new Vector2(1.9f,.12f),PrimitiveArt.Icon.Block,c);break;
                case HostKind.Molt:Part("Split shell",Vector2.zero,new Vector2(1.6f,1.7f),PrimitiveArt.Icon.Arch,new Color(.85f,.63f,.6f,.7f));break;
                case HostKind.Wax:Part("Dripping candle",new Vector2(0,.8f),new Vector2(.3f,.6f),PrimitiveArt.Icon.Stripe,Color.yellow);break;
                case HostKind.Gullet:Part("Unhinged jaw",new Vector2(.53f,-.3f),new Vector2(.8f,.65f),PrimitiveArt.Icon.Arch,new Color(.95f,.3f,.45f));break;
                case HostKind.Root:for(int i=-1;i<=1;i++)Part("Root toes",new Vector2(i*.3f,-.68f),new Vector2(.14f,.6f),PrimitiveArt.Icon.Stripe,new Color(.43f,.65f,.29f));break;
                case HostKind.InsideOut:Part("Peeled outline",Vector2.zero,new Vector2(1.5f,1.7f),PrimitiveArt.Icon.Arch,new Color(.65f,.9f,.88f));break;
                case HostKind.Parallax:Part("Second pupil",new Vector2(-.25f,.25f),new Vector2(.18f,.32f),PrimitiveArt.Icon.Eye,Color.white);break;
                case HostKind.Censer:Part("Censer cage",new Vector2(0,-.35f),new Vector2(1.6f,.8f),PrimitiveArt.Icon.Arch,new Color(.65f,.75f,.85f));break;
                case HostKind.Stitch:Part("Needle",new Vector2(.75f,0),new Vector2(.13f,1.8f),PrimitiveArt.Icon.Diamond,Color.white);break;
                case HostKind.Coffin:Part("Coffin",Vector2.zero,new Vector2(1,2),PrimitiveArt.Icon.Stripe,new Color(.33f,.21f,.25f,.65f));break;
                case HostKind.Lodestone:Part("Iron halo",Vector2.zero,new Vector2(1.8f,1.4f),PrimitiveArt.Icon.Arch,new Color(.63f,.68f,.79f));break;
                case HostKind.Shadow:Part("Split noon",new Vector2(0,.87f),new Vector2(.65f,.65f),PrimitiveArt.Icon.Star,Color.white);break;
                case HostKind.Ink:Part("Pen nib",new Vector2(.7f,-.35f),new Vector2(.4f,.9f),PrimitiveArt.Icon.Diamond,new Color(.1f,.1f,.23f));break;
            }
        }
    }
}