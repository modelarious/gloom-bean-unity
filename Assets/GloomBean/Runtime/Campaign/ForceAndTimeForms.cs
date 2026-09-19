using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public static class LocalTime
    {
        public static readonly List<CenserForm> Fields=new List<CenserForm>();
        public static float Scale(Vector2 p,bool boss=false){float s=1;foreach(var f in Fields)if(f.host&&f.Actor){float d=Vector2.Distance(f.Actor.Body.position,p);if(d<f.Radius&&f.Radius>.2f)s=Mathf.Min(s,Mathf.Lerp(.13f,1,Mathf.SmoothStep(0,1,d/f.Radius)));}return boss?Mathf.Max(.5f,s):s;}
    }
    public sealed class CenserForm:HostForm
    {
        public override HostKind Kind=>HostKind.Censer;public float Radius {get;private set;}LineRenderer ring;
        public override string Help=>"Stillness thickens a local slow-time field. Walking disperses it. You stay fast; bosses are only partly slowed.";
        public override string Status=>"Incense radius "+Radius.ToString("0.0")+" m";
        public override void Enter(){LocalTime.Fields.Add(this);}
        public override void After(InputFrame f,float dt){bool still=f.move.sqrMagnitude<.04f&&Mathf.Abs(Actor.Body.linearVelocity.x)<.6f;Radius=Mathf.MoveTowards(Radius,still?6.5f:0,dt*(still?2:4));}
        public override void Draw(){if(!ring)ring=PrimitiveArt.Line("Incense boundary",Actor.transform,Vector2.zero,Vector2.zero,.035f,new Color(.71f,.77f,.87f,.55f),4);ring.positionCount=65;for(int i=0;i<=64;i++){float a=i*Mathf.PI/32;ring.SetPosition(i,Actor.Body.position+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*Radius);}}
        public override void Leave(){LocalTime.Fields.Remove(this);if(ring)UnityEngine.Object.Destroy(ring.gameObject);}
    }
    public sealed class LodestoneForm:HostForm
    {
        public override HostKind Kind=>HostKind.Lodestone;public int Polarity {get;private set;}=1;public float range=10;
        public override string Help=>"U reverses polarity. Equal poles repel; opposite poles attract. Free metal moves too; fixed metal anchors you.";
        public override string Status=>Polarity>0?"NORTH pole":"SOUTH pole";
        public void Reverse(){Polarity=-Polarity;RuntimeEvents.Emit("polarity",Polarity.ToString());}
        public static Vector2 Force(Vector2 hostPoint,Vector2 metalPoint,int hostPole,int metalPole,float strength=34)
        {var d=metalPoint-hostPoint;float r=d.magnitude;if(r<.1f)return Vector2.zero;float sign=hostPole==metalPole?-1:1;return d/r*(strength/(1+.3f*r*r))*sign;}
        public override bool Move(InputFrame f,float dt)
        {
            if(f.action)Reverse();var shadow=host.Form<ShadowForm>();if(shadow!=null&&shadow.Controlling)return false;
            bool field=false;foreach(var m in MagneticBody.All)if(m&&Vector2.Distance(Actor.Body.position,m.transform.position)<range){field=true;break;}
            if(!field)return false;
            // Do not let the ordinary walking brake cancel every external force at each tick.
            // In a magnetic field the Host keeps inertia, with a modest steering force.
            var v=Actor.Body.linearVelocity;v.x=(v.x+f.move.x*8*dt)*Mathf.Exp(-.6f*dt);
            v.y=Actor.Grounded&&v.y<0?-.5f:Mathf.Max(-24,v.y-Actor.tuning.gravity*dt);
            if(f.jump&&Actor.Grounded)v.y=Actor.tuning.jumpSpeed;
            Actor.Body.linearVelocity=Vector2.ClampMagnitude(v,26);return true;
        }
        public override void After(InputFrame f,float dt)
        {
            foreach(var m in MagneticBody.All)
            {
                if(!m||!m.enabled||Vector2.Distance(Actor.Body.position,m.transform.position)>range)continue;
                Vector2 force=Force(Actor.Body.position,m.transform.position,Polarity,m.polarity,m.strength);
                Actor.Body.AddForce(force,ForceMode2D.Force);var rb=m.GetComponent<Rigidbody2D>();if(rb&&rb.bodyType==RigidbodyType2D.Dynamic)rb.AddForce(-force,ForceMode2D.Force);
            }
        }
    }
    public sealed class InkForm:HostForm
    {
        public override HostKind Kind=>HostKind.Ink;public bool Recording {get;private set;}=true;
        public readonly List<InkStroke> Strokes=new List<InkStroke>();Vector2 last;float sample;public float maxLength=18,Length;
        public override string Help=>"Your footpath hardens after 1 second, then dries after 8. U pauses the pen. Old ink expires beyond 18 metres.";
        public override string Status=>"Ink "+Length.ToString("0.0")+" / "+maxLength+" m  "+(Recording?"recording":"pen lifted");
        public override void Enter(){last=Actor.Feet;}
        public override bool Move(InputFrame f,float dt){if(f.action){Recording=!Recording;last=Actor.Feet;}return false;}
        public override void After(InputFrame f,float dt)
        {
            sample+=dt;if(Recording&&sample>.12f){sample=0;Vector2 now=Actor.Feet;if(Vector2.Distance(last,now)>.12f&&Vector2.Distance(last,now)<4){Add(last,now);last=now;}else if(Vector2.Distance(last,now)>=4)last=now;}
            Strokes.RemoveAll(s=>!s);Length=0;foreach(var s in Strokes)Length+=s.length;
            while(Length>maxLength&&Strokes.Count>0){var s=Strokes[0];Length-=s.length;s.Erase();Strokes.RemoveAt(0);}
        }
        public InkStroke Add(Vector2 a,Vector2 b)
        {
            var g=new GameObject("Delayed ink stroke");g.transform.SetParent(Root);var s=g.AddComponent<InkStroke>();s.Initialize(a,b);Strokes.Add(s);Length+=s.length;return s;
        }
        public override void Leave(){Recording=false;}
    }
}