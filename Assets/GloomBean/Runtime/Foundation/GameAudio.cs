using System;
using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Foundation
{
    /// <summary>Original short procedural cues, no downloaded sounds or runtime services.</summary>
    public sealed class GameAudio:MonoBehaviour
    {
        public const int SampleRate=24000;
        public bool Muted {get;private set;}
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        AudioSource source;bool verification;float lastCue=-10;
        static readonly string[] kinds={"jump","tackle","pound-impact","collect","possession","cure","bell","pulse-arrived","turn","boss-phase","clear","fail","hurt"};
        public static float[] Synthesize(string kind)
        {
            float hz=170,duration=.14f,glide=0,noise=.08f;
            switch(kind){
                case "jump":hz=220;glide=230;duration=.12f;break;
                case "tackle":hz=100;glide=-55;noise=.5f;break;
                case "pound-impact":hz=72;glide=-35;noise=.6f;duration=.23f;break;
                case "collect":hz=780;glide=180;noise=0;duration=.11f;break;
                case "possession":hz=120;glide=190;duration=.42f;noise=.17f;break;
                case "cure":hz=440;glide=-150;duration=.24f;break;
                case "bell":hz=330;duration=.55f;noise=0;break;
                case "pulse-arrived":hz=540;duration=.16f;noise=0;break;
                case "turn":hz=140;glide=-100;duration=.6f;noise=.25f;break;
                case "boss-phase":hz=210;glide=-60;duration=.38f;noise=.18f;break;
                case "clear":hz=460;glide=320;duration=.55f;noise=0;break;
                case "fail":hz=190;glide=-120;duration=.48f;noise=.2f;break;
                case "hurt":hz=95;glide=-20;duration=.17f;noise=.5f;break;
            }
            var result=new float[(int)(duration*SampleRate)];double phase=0;uint seed=0x5eed1234;
            for(int i=0;i<result.Length;i++){
                float u=(float)i/result.Length;phase+=2*Math.PI*Mathf.Max(20,hz+glide*u)/SampleRate;
                seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;float grain=(seed%65536)/32768f-1;
                float envelope=Mathf.Min(1,u*40)*Mathf.Pow(1-u,2);
                float harmonic=(float)(Math.Sin(phase)+.25*Math.Sin(phase*2.76));
                result[i]=.22f*envelope*((1-noise)*harmonic+noise*grain);
            }
            return result;
        }
        public void Initialize(bool testMode)
        {
            verification=testMode;Muted=testMode||PlayerPrefs.GetInt("GloomBean.SFXMuted",0)==1;
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=.35f;
            foreach(var kind in kinds){var samples=Synthesize(kind);var clip=AudioClip.Create("Host cue "+kind,samples.Length,1,SampleRate,false);clip.SetData(samples,0);clips[kind]=clip;}
            RuntimeEvents.Event+=OnEvent;
        }
        void Update(){if(!verification&&Input.GetKeyDown(KeyCode.F4)){Muted=!Muted;PlayerPrefs.SetInt("GloomBean.SFXMuted",Muted?1:0);if(Muted)source.Stop();}}
        void OnEvent(string kind,string detail)
        {
            if(Muted||!source||Time.unscaledTime-lastCue<.025f||!clips.TryGetValue(kind,out var clip))return;
            lastCue=Time.unscaledTime;source.PlayOneShot(clip);
        }
        void OnDestroy(){RuntimeEvents.Event-=OnEvent;foreach(var clip in clips.Values)if(clip)Destroy(clip);}
    }
}
