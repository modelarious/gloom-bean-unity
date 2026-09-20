using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-150)]
    public sealed class DescentController:MonoBehaviour
    {
        public bool falling;public float altitude=210,speed=1.8f,fallen,exitRise=22,exitRiseRate=1.4f;public Transform exit;
        public readonly List<Transform> structures=new List<Transform>();public readonly List<MotionPlatform> movers=new List<MotionPlatform>();
        readonly List<Rigidbody2D> bodies=new List<Rigidbody2D>();readonly List<LineRenderer> lines=new List<LineRenderer>();Vector2 exitStart;
        public Vector2 Offset=>Vector2.down*fallen;public float RemainingAltitude=>Mathf.Max(0,altitude-fallen);
        public void Capture(StageBuilder b)
        {
            structures.Clear();movers.Clear();bodies.Clear();lines.Clear();
            foreach(Transform t in b.root){if(t==transform||t.GetComponent<ActorMotor>()||t.GetComponent<ImpactHoist>()||t.GetComponent<FallingLoad>())continue;
                var rb=t.GetComponent<Rigidbody2D>();if(rb&&rb.bodyType==RigidbodyType2D.Dynamic)continue;
                var mover=t.GetComponent<MotionPlatform>();if(mover){movers.Add(mover);continue;}
                if(!rb&&t.GetComponentInChildren<Collider2D>()){rb=t.gameObject.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;}
                if(rb)bodies.Add(rb);else structures.Add(t);
                foreach(var line in t.GetComponentsInChildren<LineRenderer>())if(line.useWorldSpace&&!line.GetComponentInParent<PulseBell>())lines.Add(line);
            }
            if(exit)exitStart=exit.position;
        }
        void FixedUpdate()
        {
            if(!falling)return;float dy=speed*Time.fixedDeltaTime;fallen+=dy;Vector2 delta=Vector2.down*dy;
            foreach(var rb in bodies)if(rb&&rb.transform!=exit)rb.position+=delta;
            foreach(var t in structures)if(t&&t!=exit)t.position+=(Vector3)delta;
            foreach(var mover in movers)if(mover){mover.origin+=delta;mover.end+=delta;}
            foreach(var line in lines)if(line){for(int i=0;i<line.positionCount;i++)line.SetPosition(i,line.GetPosition(i)+(Vector3)delta);}
            if(exit){Vector2 target=exitStart+Offset+Vector2.up*Mathf.Min(exitRise,fallen*exitRiseRate);var rb=exit.GetComponent<Rigidbody2D>();if(rb)rb.position=target;else exit.position=target;}
            var session=StageSession.Current;if(session&&session.Camera){var bounds=session.Camera.bounds;bounds.y-=dy;session.Camera.bounds=bounds;}
            if(fallen>=altitude)session?.Fail("The cathedral reached the bottom before you reached its moving exit.");
        }
        void OnGUI(){if(!falling)return;GUI.Label(new Rect(25,145,500,30),"ALTITUDE REMAINING  "+RemainingAltitude.ToString("0.0")+" m");}
    }
}
