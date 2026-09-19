using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class PulseReceiver:MonoBehaviour
    {
        public Gate gate;public MotionPlatform platform;public float hold=2.4f;public int minimumStrength=1;public float Until {get;private set;}public int count;public bool alternate;
        public void Receive(int strength){if(strength<minimumStrength)return;count++;Until=Time.time+hold;if(alternate&&platform)platform.paused=!platform.paused;RuntimeEvents.Emit("pulse-arrived",name+":"+strength);}
        void FixedUpdate(){if(gate)gate.SetOpen(Time.time<Until);if(platform&&!alternate)platform.paused=Time.time>=Until;}
    }
}