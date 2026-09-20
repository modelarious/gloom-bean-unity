using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A unilateral routed cable constraint. Pulleys are world supports; endpoint bodies
    // exchange real impulses. No positions, victory flags, or target heights are assigned.
    public sealed class CablePulley:MonoBehaviour
    {
        public Rigidbody2D deck,bell;public Vector2 deckPulley,bellPulley;
        public float length;public float Tension {get;private set;}public float Extension=>CurrentLength-length;
        public float CurrentLength=>deck&&bell?Vector2.Distance(deck.position,deckPulley)+Vector2.Distance(bell.position,bellPulley):0;
        public float MaximumExtension {get;private set;}public float PeakTension {get;private set;}
        public float InitialDeckHeight {get;private set;}public float LowestBell {get;private set;}public float HighestDeck {get;private set;}
        bool ready;LineRenderer rope;
        public void Configure(Rigidbody2D a,Rigidbody2D b,Vector2 pa,Vector2 pb)
        {deck=a;bell=b;deckPulley=pa;bellPulley=pb;length=CurrentLength;InitialDeckHeight=HighestDeck=deck.position.y;LowestBell=bell.position.y;ready=true;}
        static float EffectiveInverseMass(Rigidbody2D body,Vector2 n)
        {
            if(!body||body.bodyType!=RigidbodyType2D.Dynamic)return 0;
            var c=body.constraints;if((c&RigidbodyConstraints2D.FreezePositionX)!=0)n.x=0;if((c&RigidbodyConstraints2D.FreezePositionY)!=0)n.y=0;
            return n.sqrMagnitude/Mathf.Max(.01f,body.mass);
        }
        void FixedUpdate()
        {
            if(!ready||!deck||!bell)return;float dt=Time.fixedDeltaTime;
            Vector2 a=deck.position-deckPulley,b=bell.position-bellPulley;float al=a.magnitude,bl=b.magnitude;
            if(al<.05f||bl<.05f)return;a/=al;b/=bl;float inv=EffectiveInverseMass(deck,a)+EffectiveInverseMass(bell,b);
            float error=al+bl-length,rate=Vector2.Dot(deck.linearVelocity,a)+Vector2.Dot(bell.linearVelocity,b);
            float impulse=inv>.0001f?Mathf.Clamp((rate+error*(error>0?.2f:1)/dt)/inv,0,100):0;
            Tension=impulse/dt;MaximumExtension=Mathf.Max(MaximumExtension,error);PeakTension=Mathf.Max(PeakTension,Tension);if(impulse>0){deck.AddForce(-a*impulse,ForceMode2D.Impulse);bell.AddForce(-b*impulse,ForceMode2D.Impulse);}
            LowestBell=Mathf.Min(LowestBell,bell.position.y);HighestDeck=Mathf.Max(HighestDeck,deck.position.y);
        }
        void LateUpdate()
        {
            if(!ready||!deck||!bell)return;if(!rope)rope=PrimitiveArt.Line("Load-bearing cable",transform,Vector2.zero,Vector2.zero,.08f,new Color(.8f,.7f,.44f),6);
            rope.positionCount=4;rope.SetPosition(0,deck.position);rope.SetPosition(1,deckPulley);rope.SetPosition(2,bellPulley);rope.SetPosition(3,bell.position);
        }
    }
}
