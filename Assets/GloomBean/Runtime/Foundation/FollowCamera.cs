using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class FollowCamera : MonoBehaviour
    {
        public Transform target,secondary;
        public Rect bounds=new Rect(-10,-10,180,60);
        public float follow=.16f,lookAhead=2.2f,baseSize=7;
        Vector3 velocity;float shake;
        UnityEngine.Camera lens;
        void Awake(){lens=GetComponent<UnityEngine.Camera>();lens.orthographic=true;lens.orthographicSize=baseSize;}
        public void Kick(float force){shake=Mathf.Max(shake,force);}
        public void Snap(){if(target)transform.position=new Vector3(target.position.x,target.position.y+1,-10);velocity=Vector3.zero;}
        void LateUpdate()
        {
            if(!target)return;
            var actor=target.GetComponent<ActorMotor>();
            Vector3 dest=target.position+new Vector3(actor?actor.Facing*lookAhead:0,1.1f,-10);
            float size=baseSize;
            if(secondary)
            {dest=Vector3.Lerp(target.position,secondary.position,.5f)+new Vector3(0,1,-10);size=Mathf.Clamp(Mathf.Max(Mathf.Abs(target.position.y-secondary.position.y)*.6f,Mathf.Abs(target.position.x-secondary.position.x)/lens.aspect*.6f)+3,baseSize,13);}
            lens.orthographicSize=Mathf.Lerp(lens.orthographicSize,size,Time.deltaTime*4);
            float halfH=lens.orthographicSize,halfW=halfH*lens.aspect;
            dest.x=bounds.width<halfW*2?bounds.center.x:Mathf.Clamp(dest.x,bounds.xMin+halfW,bounds.xMax-halfW);
            dest.y=bounds.height<halfH*2?bounds.center.y:Mathf.Clamp(dest.y,bounds.yMin+halfH,bounds.yMax-halfH);
            transform.position=Vector3.SmoothDamp(transform.position,dest,ref velocity,follow);
            if(shake>0){shake=Mathf.MoveTowards(shake,0,Time.deltaTime);transform.position+=new Vector3(Mathf.Sin(Time.time*91),Mathf.Sin(Time.time*137),0)*shake;}
        }
    }
}
