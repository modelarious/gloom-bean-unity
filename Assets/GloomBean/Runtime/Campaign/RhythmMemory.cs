using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class RhythmMemory:MonoBehaviour
    {
        public bool teaching;public Gate gate;readonly List<float> taps=new List<float>();ActorMotor actor;float lastTap=-1;TextMesh label;
        void Start(){label=PrimitiveArt.Label(teaching?"skip . skip . . skip":"",transform,transform.position,.1f);}
        void OnTriggerStay2D(Collider2D c){var a=c.GetComponent<ActorMotor>();if(!a||a.replica)return;if(actor!=a){if(actor)actor.Stepped-=Step;actor=a;actor.Stepped+=Step;}}
        void OnTriggerExit2D(Collider2D c){if(actor&&c.GetComponent<ActorMotor>()==actor){actor.Stepped-=Step;actor=null;}}
        void OnDestroy(){if(actor)actor.Stepped-=Step;}
        void Step(InputFrame f,float dt){if(!f.jump)return;float now=Time.time;if(lastTap>=0)taps.Add(now-lastTap);lastTap=now;while(taps.Count>3)taps.RemoveAt(0);if(!teaching&&taps.Count==3&&Mathf.Abs(taps[0]-.5f)<.2f&&Mathf.Abs(taps[1]-.5f)<.2f&&Mathf.Abs(taps[2]-1)<.3f){if(gate)gate.SetOpen(true);StageSession.Current?.Notice("The old skipping rhyme. Something remembers.");}}
        void Update(){if(teaching&&label){float t=Mathf.Repeat(Time.time,2.5f);label.color=t<.1f||t>.5f&&t<.6f||t>1&&t<1.1f||t>2&&t<2.1f?Color.white:new Color(.64f,.59f,.68f);}}
    }
}