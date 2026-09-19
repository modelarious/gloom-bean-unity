using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class Carousel : MonoBehaviour
    {
        public float radius=3,angularSpeed=.6f;
        public bool keepUpright=true;
        public MotionPlatform[] arms=new MotionPlatform[4];
        public void Configure(StageBuilder b,Vector2 center,float r,float speed)
        {
            radius=r;angularSpeed=speed;
            for(int i=0;i<4;i++)
            {
                float a=i*Mathf.PI*.5f;
                var obj=b.Platform(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,new Vector2(2.3f,.35f));
                var m=obj.AddComponent<MotionPlatform>();m.pattern=MotionPlatform.Pattern.Orbit;m.origin=center;m.radius=r;m.speed=speed;m.phase=a;m.upright=keepUpright;arms[i]=m;
            }
        }
        void LateUpdate()
        {for(int i=0;i<arms.Length;i++)if(arms[i])PrimitiveArt.Line("spoke"+i,transform,transform.position,arms[i].transform.position,.045f,new Color(.4f,.4f,.52f),-1);}
    }
}
