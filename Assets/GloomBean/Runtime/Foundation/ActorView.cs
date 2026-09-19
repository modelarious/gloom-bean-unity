using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class ActorView : MonoBehaviour
    {
        public bool corrupted;
        public Color tint=new Color(.33f,.15f,.47f);
        public string costume="";
        public bool ghost;
        Transform visual,torso,eyeA,eyeB,pupilA,pupilB,leftHand,rightHand,leftFoot,rightFoot,leak;
        ActorMotor actor;
        void Start()
        {
            actor=GetComponent<ActorMotor>();visual=new GameObject("Silhouette").transform;visual.SetParent(transform,false);
            torso=Part("Torso",new Vector2(0,0),new Vector2(1.03f,1.13f),tint,PrimitiveArt.Icon.Round,10);
            Part("Curl",new Vector2(-.1f,.68f),new Vector2(.42f,.42f),tint,PrimitiveArt.Icon.Arch,10);
            leftFoot=Part("Left red shoe",new Vector2(-.3f,-.64f),new Vector2(.51f,.24f),new Color(.82f,.12f,.28f),PrimitiveArt.Icon.Round,11);
            rightFoot=Part("Right red shoe",new Vector2(.3f,-.64f),new Vector2(.51f,.24f),new Color(.82f,.12f,.28f),PrimitiveArt.Icon.Round,11);
            leftHand=Part("Left white glove",new Vector2(-.65f,-.12f),new Vector2(.36f,.36f),new Color(.95f,.92f,.86f),PrimitiveArt.Icon.Round,11);
            rightHand=Part("Right white glove",new Vector2(.65f,-.12f),new Vector2(.36f,.36f),new Color(.95f,.92f,.86f),PrimitiveArt.Icon.Round,11);
            eyeA=Part("Large eye",new Vector2(-.21f,.18f),new Vector2(.43f,.65f),new Color(1,.82f,.38f),PrimitiveArt.Icon.Eye,12);
            eyeB=Part("Small eye",new Vector2(.29f,.22f),new Vector2(.32f,.49f),new Color(1,.82f,.38f),PrimitiveArt.Icon.Eye,12);
            pupilA=Part("Pupil A",new Vector2(-.15f,.17f),new Vector2(.17f,.32f),new Color(.035f,.02f,.055f),PrimitiveArt.Icon.Eye,13);
            pupilB=Part("Pupil B",new Vector2(.34f,.23f),new Vector2(.13f,.26f),new Color(.035f,.02f,.055f),PrimitiveArt.Icon.Eye,13);
            leak=Part("Corruption leak",new Vector2(.16f,-.42f),new Vector2(.22f,.72f),new Color(.92f,.1f,.48f),PrimitiveArt.Icon.Round,14);
        }
        Transform Part(string name,Vector2 p,Vector2 size,Color c,PrimitiveArt.Icon icon,int order)
        {
            var g=PrimitiveArt.Shape(name,visual,Vector2.zero,size,c,icon,order);g.transform.localPosition=p;return g.transform;
        }
        void LateUpdate()
        {
            if(!actor||!visual)return;
            float time=Time.time, speed=actor.Body.linearVelocity.magnitude;
            float squash=actor.Crouched? .58f:1;
            float baseScale=actor.Shape.size.x/.88f;
            visual.localScale=new Vector3(baseScale,squash*baseScale,1);
            torso.GetComponent<SpriteRenderer>().color=tint;
            float stride=actor.Grounded?Mathf.Sin(time*Mathf.Clamp(speed*2,1,22))*.12f:0;
            leftFoot.localPosition=new Vector3(-.3f,-.64f+stride,0);rightFoot.localPosition=new Vector3(.3f,-.64f-stride,0);
            rightHand.localPosition=new Vector3(.65f,actor.AttackPower>0? .08f:-.12f-stride,0);
            leftHand.localPosition=new Vector3(-.65f,-.12f+stride,0);
            eyeB.localPosition=new Vector3(.29f,corrupted? .54f:.22f,0);pupilB.localPosition=eyeB.localPosition+new Vector3(actor.Facing*.05f,0,0);
            pupilA.localPosition=new Vector3(-.21f+actor.Facing*.06f,.17f,0);
            leak.gameObject.SetActive(corrupted);if(corrupted)leak.localScale=new Vector3(.22f,.65f+Mathf.Sin(time*5)*.06f,1);
            bool visible=actor.Invulnerability<=0||Mathf.Sin(time*35)>-.3f;
            foreach(var sr in visual.GetComponentsInChildren<SpriteRenderer>()){var c=sr.color;c.a=(visible?1:.2f)*(ghost? .5f:1);sr.color=c;}
            visual.localRotation=Quaternion.Euler(0,0,actor.State==MotionState.Roll?-time*650*actor.Facing:0);
        }
    }
}
