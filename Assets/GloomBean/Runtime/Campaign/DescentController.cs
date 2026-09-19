using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-150)]
    public sealed class DescentController:MonoBehaviour
    {
        public bool falling;public float altitude=210,speed=1.8f,fallen;public Transform exit;public readonly List<Transform> structures=new List<Transform>();public readonly List<MotionPlatform> movers=new List<MotionPlatform>();Vector2 exitStart;
        public float RemainingAltitude=>Mathf.Max(0,altitude-fallen);
        public void Capture(StageBuilder b)
        {
            foreach(Transform t in b.root){if(t==transform||t.GetComponent<ActorMotor>()||t.GetComponent<Rigidbody2D>()?.bodyType==RigidbodyType2D.Dynamic)continue;var m=t.GetComponent<MotionPlatform>();if(m){movers.Add(m);continue;}structures.Add(t);}
            if(exit)exitStart=exit.position;
        }
        void FixedUpdate()
        {
            if(!falling)return;float dy=speed*Time.fixedDeltaTime;fallen+=dy;
            foreach(var t in structures)if(t)t.position+=Vector3.down*dy;foreach(var m in movers)if(m){m.origin+=Vector2.down*dy;m.end+=Vector2.down*dy;}
            if(exit){var p=exit.position;p.y=exitStart.y-fallen+Mathf.Min(22,fallen*1.4f);exit.position=p;}
            var session=StageSession.Current;if(session&&session.Camera){var r=session.Camera.bounds;r.y-=dy;session.Camera.bounds=r;}if(fallen>=altitude)session?.Fail("The cathedral reached the bottom before you reached its moving exit.");
        }
        void OnGUI(){if(!falling)return;GUI.Label(new Rect(25,145,500,30),"ALTITUDE REMAINING  "+RemainingAltitude.ToString("0.0")+" m");}
    }
}