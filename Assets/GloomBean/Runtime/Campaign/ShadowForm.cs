using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class ShadowForm:HostForm
    {
        public override HostKind Kind=>HostKind.Shadow;public override bool Locomotion=>true;
        public override string Help=>"I switches between body and shadow. Crawl only through connected cast silhouettes, within a 14 m tether. The body stays vulnerable.";
        public Vector2 Position {get;private set;}public bool Controlling {get;private set;}public float tether=14;GameObject cursor;float stranded;
        public override string Status=>Controlling?"Controlling SHADOW":"Controlling exposed BODY";
        public override void Enter(){Position=Actor.Feet;cursor=PrimitiveArt.Shape("Living shadow",Root,Position,new Vector2(.55f,.45f),new Color(.65f,.64f,.92f),PrimitiveArt.Icon.Eye,17);if(!Actor.GetComponent<ShadowCaster>())Actor.gameObject.AddComponent<ShadowCaster>();}
        public void Toggle(){Controlling=!Controlling;if(Controlling&&Vector2.Distance(Position,Actor.Body.position)>tether)Position=Actor.Feet;}
        public bool Allowed(Vector2 p)=>Vector2.Distance(p,Actor.Body.position)<=tether&&(Vector2.Distance(p,Actor.Feet)<1||ShadowSun.Contains(p));
        public bool Advance(Vector2 delta)
        {
            Vector2 next=Position+delta;for(int i=1;i<=4;i++)if(!Allowed(Vector2.Lerp(Position,next,i/4f)))return false;Position=next;return true;
        }
        public override bool Move(InputFrame f,float dt)
        {
            if(f.alternate)Toggle();if(!Controlling)return false;
            Actor.Body.linearVelocity=new Vector2(0,Mathf.Max(-18,Actor.Body.linearVelocity.y-28*dt));Advance(f.move*5*dt);
            if(!Allowed(Position)){stranded+=dt;if(stranded>.65f){Position=Actor.Feet;Controlling=false;stranded=0;Notice("The light severed your route. Your shadow snaps back.");}}else stranded=0;
            return true;
        }
        public override void After(InputFrame f,float dt)
        {
            if(Controlling||dt<=0)return;
            Vector2 radial=Actor.Body.position-Position;
            Vector2 predicted=radial+Actor.Body.linearVelocity*dt;
            if(predicted.magnitude>tether)
                Actor.Body.linearVelocity=(predicted.normalized*tether-radial)/dt;
        }
        public override void Draw(){if(cursor)cursor.transform.position=Position;PrimitiveArt.Line("Shadow tether",Actor.transform,Actor.Feet,Position,.025f,new Color(.6f,.55f,.8f,.4f),3);}
        public override void Leave(){if(cursor)UnityEngine.Object.Destroy(cursor);var l=Actor.transform.Find("Shadow tether");if(l)UnityEngine.Object.Destroy(l.gameObject);}
    }
}