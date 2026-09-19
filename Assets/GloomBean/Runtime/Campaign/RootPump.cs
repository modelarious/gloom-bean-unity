using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class RootPump:MonoBehaviour,IInteractable
    {
        public RootSoil[] channels;public int state;public bool automatic,reversed;public float interval=7;float clock;
        public void Interact(ActorMotor a){Advance();}public void Advance(){if(channels==null||channels.Length==0)return;state=(state+(reversed?-1:1)+channels.Length)%channels.Length;for(int i=0;i<channels.Length;i++)if(channels[i])channels[i].wet=i==state;RuntimeEvents.Emit("pump",state.ToString());}
        void Update(){if(!automatic)return;clock+=Time.deltaTime;if(clock>=interval){clock=0;Advance();}}
    }
}