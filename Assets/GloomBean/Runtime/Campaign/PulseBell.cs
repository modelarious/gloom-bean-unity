using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class PulseBell:MonoBehaviour,IInteractable,IHittable
    {
        public Vector2[] rope;public PulseReceiver receiver;public float speed=6,cooldown=.35f;public bool splitOnTurn=true;float next;
        public readonly List<SoundPulse> pulses=new List<SoundPulse>();
        public void Interact(ActorMotor actor){Ring();}public void Hit(HitInfo hit){Ring();}
        public void Ring(){if(Time.time<next)return;next=Time.time+cooldown;Spawn(0,1);if(splitOnTurn&&StageSession.Current&&StageSession.Current.Phase==RunPhase.Returning)Spawn(.7f,2);RuntimeEvents.Emit("bell",name);}
        void Spawn(float delay,int strength){var g=PrimitiveArt.Shape("Traveling bell command",transform,rope!=null&&rope.Length>0?rope[0]:(Vector2)transform.position,Vector2.one*.25f,strength==1?new Color(1,.78f,.3f):new Color(.7f,.9f,1),PrimitiveArt.Icon.Round,12);var p=g.AddComponent<SoundPulse>();p.path=rope;p.receiver=receiver;p.delay=delay;p.speed=speed;p.strength=strength;pulses.Add(p);}
        void Start(){if(rope!=null)for(int i=1;i<rope.Length;i++)PrimitiveArt.Line("Bell rope "+i,transform,rope[i-1],rope[i],.045f,new Color(.7f,.64f,.49f),2);}
    }
}