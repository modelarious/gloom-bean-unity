using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class EmpyreanVerification
    {
        IEnumerator InkLanding(float x,float low,float high)
        {
            if(stopped||!Live)yield break;float end=Time.time+6;bool jumped=false;
            input.rule=()=>{bool edge=!jumped&&actor.Grounded;if(edge)jumped=true;return new InputFrame{jump=edge,jumpHeld=true,move=new Vector2(Mathf.Clamp((x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.35f,-1,1),0)};};
            while(Live&&Time.time<end&&!(jumped&&actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>()&&actor.Feet.y>=low&&actor.Feet.y<=high))yield return Tick();
            input.rule=null;input.frame=default;Note("INK SUPPORT "+(actor.GroundCollider?actor.GroundCollider.name:"none"));
            Check("land on own hardened falling stroke",actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>()&&actor.Feet.y>=low&&actor.Feet.y<=high);Snapshot("own-ink-landing");
        }
        IEnumerator RunArc(float x,float floor)
        {
            if(stopped||!Live)yield break;bool sent=false;float end=Time.time+6;
            input.rule=()=>{bool edge=!sent&&actor.Grounded;if(edge)sent=true;return new InputFrame{jump=edge,jumpHeld=true,run=true,move=new Vector2(Mathf.Clamp((x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.4f,-1,1),0)};};
            while(Live&&Time.time<end&&!(sent&&actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.35f))yield return Tick();
            input.rule=null;input.frame=default;Check("record a running arc to "+x,actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.5f);
        }
        IEnumerator SemicolonMercy()
        {
            yield return Focus(HostKind.Ink);yield return Walk(42,true);yield return RunArc(50,1);if(stopped)yield break;yield return Pause(1.05f);
            var ink=host.Form<InkForm>();int attempts=0;
            while(Live&&!stopped&&actor.Feet.y<6.3f&&attempts++<5){
                var candidates=ink.Strokes.Where(x=>x&&x.Solid&&x.age<6.2f&&x.Midpoint.y>actor.Feet.y+.4f&&x.Midpoint.y<actor.Feet.y+2.15f&&Mathf.Abs(x.Midpoint.x-actor.Body.position.x)<3.1f).OrderByDescending(x=>x.Midpoint.y).ToArray();
                Check("a reachable hardened arc exists",candidates.Length>0);if(stopped)yield break;var point=candidates[0].Midpoint;Note("INK TARGET "+point+" age="+candidates[0].age);yield return InkLanding(point.x,point.y-.6f,point.y+1.0f);
            }
            Check("temporary ink brings body within the semicolon tether",actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>()&&Vector2.Distance(actor.Body.position,new Vector2(57.5f,14.5f))<13.5f);if(stopped)yield break;
            yield return Focus(HostKind.Shadow);yield return Press(new InputFrame{alternate=true});yield return ShadowTravel(new Vector2(57.5f,14.5f));yield return Pause(.08f);
            Check("shadow takes the actual semicolon dot",session.Mercies.Count==1&&host.Form<ShadowForm>().Controlling);Snapshot("semicolon-dot");if(stopped)yield break;
            yield return ShadowTravel(actor.Feet);yield return Press(new InputFrame{alternate=true});yield return Walk(50);yield return Wait("leave ink before it dries",()=>actor.Grounded&&Mathf.Abs(actor.Feet.y-1)<.3f,8);
        }
        IEnumerator ScriptureRoute(bool secret)
        {
            yield return Walk(8);Check("scribe leech supplies actual Ink",host.Has(HostKind.Ink));yield return Walk(17);
            yield return Wait("land in the bottom margin",()=>actor.Grounded&&Mathf.Abs(actor.Feet.y-1)<.3f,5);yield return Pause(1.15f);
            yield return InkLanding(14.8f,2.5f,4.1f);if(stopped)yield break;yield return InkLanding(13.8f,4,6.2f);if(stopped)yield break;
            yield return Jump(17.6f,5);Check("reach punctuation desk through authored ink",actor.Feet.y>4.7f);if(stopped)yield break;
            yield return Walk(18.2f);var comma=session.GetComponentInChildren<PunctuationCart>();yield return Press(new InputFrame{interact=true});Check("grip physical comma",comma.Holder==actor);yield return Walk(21.5f);yield return Pause(.4f);yield return Press(new InputFrame{interact=true});
            var layout=session.GetComponentInChildren<ScriptureLayout>();yield return Wait("moved comma reflows actual words",()=>comma.Slot==1&&layout.wrap==3&&layout.Reflows>0,3);Snapshot("physical-line-wrap");
            if(stopped)yield break;yield return Jump(24,5);yield return Jump(27,7.2f);yield return Jump(31,9.4f);yield return Jump(35,9.4f);yield return Jump(40,7.2f);
            if(stopped)yield break;Check("shadow source composes with Ink",host.Has(HostKind.Shadow)&&host.Has(HostKind.Ink));
            if(secret){yield return SemicolonMercy();if(stopped)yield break;}
            yield return Walk(50);yield return Wait("lower sentence refuge",()=>actor.Grounded&&Mathf.Abs(actor.Feet.y-1)<.3f,6);
            for(int k=0;k<9;k++){yield return Jump(55+k*5,2+k*.7f);if(stopped)yield break;}
            yield return Jump(100,9.2f);yield return Walk(102);Check("manuscript Keyling",session.HasKey);yield return Walk(104.5f);yield return Press(new InputFrame{interact=true});Check("imperative Turn changes path memory",session.Phase==RunPhase.Returning&&session.GetComponentInChildren<ScriptureCorrector>().imperative);
            yield return Jump(98,10.8f);for(int k=13;k>=0;k--){yield return Jump(14+k*6,10.8f);if(stopped)yield break;}yield return Jump(10,8);yield return Walk(2);
        }
    }
}
