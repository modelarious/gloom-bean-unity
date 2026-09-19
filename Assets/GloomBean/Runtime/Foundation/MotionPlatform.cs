using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    [DefaultExecutionOrder(-100)]
    public sealed class MotionPlatform : MonoBehaviour
    {
        public enum Pattern { Linear, Orbit, Swing }
        public Pattern pattern;
        public Vector2 origin,end;
        public float speed=1, radius=3, phase, angle, amplitude=35;
        public bool paused, upright=true, respectBraces=true;
        public Vector2 Delta {get;private set;}
        public float timeScale=1;
        Rigidbody2D body;float clock;
        readonly Collider2D[] tests=new Collider2D[12];
        void Awake(){body=GetComponent<Rigidbody2D>();if(!body)body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.interpolation=RigidbodyInterpolation2D.Interpolate;gameObject.layer=Layers.Moving;}
        public void SetClock(float t){clock=t;}
        void FixedUpdate()
        {
            Delta=Vector2.zero;if(paused)return;
            float oldClock=clock;clock+=Time.fixedDeltaTime*timeScale;
            Vector2 next=origin;float rotation=0;
            if(pattern==Pattern.Linear){float length=Vector2.Distance(origin,end);next=Vector2.Lerp(origin,end,Mathf.PingPong(clock*speed/Mathf.Max(.01f,length)+phase,1));}
            if(pattern==Pattern.Orbit){float a=clock*speed+phase;next=origin+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;rotation=upright?0:a*Mathf.Rad2Deg;}
            if(pattern==Pattern.Swing){float a=(Mathf.Sin(clock*speed+phase)*amplitude+angle)*Mathf.Deg2Rad;next=origin+new Vector2(Mathf.Sin(a),-Mathf.Cos(a))*radius;rotation=upright?0:a*Mathf.Rad2Deg;}
            if(respectBraces)
            {
                var box=GetComponent<BoxCollider2D>();
                if(box)
                {
                    int n=Physics2D.OverlapBoxNonAlloc(next,box.size*.98f,rotation,tests,1<<Layers.Actor);
                    for(int i=0;i<n;i++){var brace=tests[i].GetComponent<LoadBearingBody>();if(brace&&brace.bracing){clock=oldClock;return;}}
                }
            }
            Delta=next-body.position;body.position=next;body.rotation=rotation;
        }
    }
}
