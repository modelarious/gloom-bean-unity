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
            while(Live&&Time.time<end&&!(jumped&&actor.Body.linearVelocity.y<=.2f&&actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>()&&actor.Feet.y>=low&&actor.Feet.y<=high))yield return Tick();
            input.rule=null;input.frame=default;Note("INK SUPPORT "+(actor.GroundCollider?actor.GroundCollider.name:"none"));
            Check("land on own hardened falling stroke",actor.Body.linearVelocity.y<=.2f&&actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>()&&actor.Feet.y>=low&&actor.Feet.y<=high);Snapshot("own-ink-landing");
        }
        IEnumerator RunArc(float x,float floor)
        {
            if(stopped||!Live)yield break;bool sent=false;float end=Time.time+6;
            input.rule=()=>{bool edge=!sent&&actor.Grounded;if(edge)sent=true;return new InputFrame{jump=edge,jumpHeld=true,run=true,move=new Vector2((!actor.Grounded&&actor.Feet.y>floor+.8f&&Mathf.Abs(x-actor.Body.position.x)>2.5f?Mathf.Sign(x-actor.Body.position.x):Mathf.Clamp((x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.4f,-1,1)),0)};};
            while(Live&&Time.time<end&&!(sent&&actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.35f))yield return Tick();
            input.rule=null;input.frame=default;Check("record a running arc to "+x,actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.5f);
        }
        IEnumerator SemicolonMercy()
        {
            yield return Focus(HostKind.Ink);yield return Walk(42,true);yield return RunArc(47,5.2f);yield return Walk(47.6f,true);yield return RunArc(55,3);if(stopped)yield break;yield return Pause(1.05f);
            var ink=host.Form<InkForm>();int attempts=0;
            while(Live&&!stopped&&(actor.Feet.y<6.4f||Vector2.Distance(actor.Body.position,new Vector2(58.5f,18.1f))>13.7f)&&attempts++<6){
                var points=new System.Collections.Generic.List<Vector2>();
                foreach(var st in ink.Strokes)if(st&&st.Solid&&st.age<6.4f){var edge=st.GetComponent<EdgeCollider2D>();Vector2 from=edge.transform.TransformPoint(edge.points[0]),to=edge.transform.TransformPoint(edge.points[1]);var tangent=to-from;
                    if(Mathf.Abs(tangent.x)<Mathf.Abs(tangent.y)*.44f)continue;
                    for(int j=1;j<5;j++){var q=Vector2.Lerp(from,to,j*.2f);if(q.y>actor.Feet.y+.45f&&q.y<actor.Feet.y+2.05f&&Mathf.Abs(q.x-actor.Body.position.x)<3)points.Add(q);}}
                if(points.Count==0)foreach(var st in ink.Strokes)if(st)Note("INK INVENTORY mid="+st.Midpoint+" age="+st.age+" solid="+st.Solid+" bounds="+st.GetComponent<Collider2D>().bounds);
                Check("a reachable sloping hardened arc exists",points.Count>0);if(stopped)yield break;
                var point=points.OrderBy(q=>Vector2.Distance(q+Vector2.up*.75f,new Vector2(58.5f,18.1f))).First();Note("INK TARGET "+point);yield return InkLanding(point.x,point.y-.6f,point.y+1.6f);
            }
            Check("temporary ink brings body within the semicolon tether",actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>()&&Vector2.Distance(actor.Body.position,new Vector2(58.5f,18.1f))<14f);if(stopped)yield break;
            yield return Focus(HostKind.Shadow);yield return Press(new InputFrame{alternate=true});yield return ShadowTravel(new Vector2(58.5f,18.1f));yield return Pause(.08f);
            Check("shadow takes the actual semicolon dot",session.Mercies.Count==1&&host.Form<ShadowForm>().Controlling);Check("body remains on its actual Ink while taking the dot",actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>());Snapshot("semicolon-dot");if(stopped)yield break;
            yield return ReattachWritingShadow();yield return Walk(50);yield return Wait("leave ink before it dries",()=>actor.Grounded&&Mathf.Abs(actor.Feet.y-3.3f)<.3f,8);
        }
        IEnumerator ReattachWritingShadow()
        {
            if(stopped||!Live)yield break;var shadow=host.Form<ShadowForm>();float end=Time.time+6;
            input.rule=()=>new InputFrame{move=Vector2.ClampMagnitude((actor.Feet-shadow.Position)*3,1)};
            while(Live&&Time.time<end&&shadow.Controlling&&Vector2.Distance(shadow.Position,actor.Feet)>.35f)yield return Tick();
            input.rule=null;input.frame=default;if(shadow.Controlling)yield return Press(new InputFrame{alternate=true});Check("shadow rejoins the still-physical body",!shadow.Controlling&&shadow.Attached);
        }
        IEnumerator WordStep(float x)
        {
            if(stopped||!Live)yield break;bool sent=false;float end=Time.time+6;
            input.rule=()=>{bool edge=!sent&&actor.Grounded;if(edge)sent=true;return new InputFrame{jump=edge,jumpHeld=true,move=new Vector2(Mathf.Clamp((x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.3f,-1,1),0)};};
            while(Live&&Time.time<end&&!(sent&&actor.Grounded&&actor.Body.linearVelocity.y<=.2f&&actor.Feet.y>=7&&actor.Feet.y<=10.6f&&Mathf.Abs(actor.Body.position.x-x)<.35f))yield return Tick();
            input.rule=null;input.frame=default;Check("cross the actual reflowed rows at "+x,actor.Grounded&&actor.Feet.y>=7&&actor.Feet.y<=10.6f&&Mathf.Abs(actor.Body.position.x-x)<.5f);
        }
        IEnumerator ShortArc(float x,float floor)
        {
            if(stopped||!Live)yield break;bool sent=false;float launch=-1,end=Time.time+5;
            input.rule=()=>{bool edge=!sent&&actor.Grounded;if(edge){sent=true;launch=Time.fixedTime;}return new InputFrame{jump=edge,jumpHeld=sent&&Time.fixedTime-launch<.12f,run=true,move=new Vector2(Mathf.Clamp((x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.45f,-1,1),0)};};
            while(Live&&Time.time<end&&!(sent&&Time.fixedTime-launch>.3f&&actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.35f))yield return Tick();
            input.rule=null;input.frame=default;Check("short hop authors a revisitable arc",actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.5f);yield return Pause(1.1f);
        }
        IEnumerator FreshAscent(float pad,float floor,float upper)
        {
            yield return Focus(HostKind.Ink);yield return Walk(pad,true);yield return ShortArc(pad-4,floor);if(stopped)yield break;
            yield return InkLanding(pad-2,floor+.5f,floor+2.8f);if(stopped)yield break;
            var ink=actor.GroundCollider.GetComponent<InkStroke>();Check("new return arc survives correction",ink&&!session.GetComponentInChildren<ScriptureCorrector>().Repeated(ink.Midpoint));Snapshot("fresh-return-arc");
            yield return Jump(pad-6,upper);
        }
        IEnumerator ReturnGap(float x)
        {
            if(stopped||!Live)yield break;if(actor.GroundCollider)yield return Walk(actor.GroundCollider.bounds.min.x+.7f,true);
            yield return Jump(x,9.6f);
        }
        IEnumerator CorrectingReturn()
        {
            yield return Walk(101);yield return Pause(.7f);Check("manuscript erases the actually repeated outbound path",session.GetComponentInChildren<ScriptureCorrector>().ErasedStrokes>0);if(stopped)yield break;
            yield return Jump(100,10.8f);yield return FreshAscent(98,10.8f,14);if(stopped)yield break;
            yield return Jump(86,9.6f);yield return ReturnGap(80);yield return ReturnGap(74);yield return ReturnGap(68);yield return Jump(63,9.6f);yield return Walk(50);
            yield return FreshAscent(48,9.6f,12.8f);if(stopped)yield break;
            yield return Jump(36,9.6f);yield return ReturnGap(30);yield return ReturnGap(24);yield return ReturnGap(18);yield return Walk(17,true);yield return RunArc(10,8);yield return Walk(2);
        }
        IEnumerator ScriptureNoInkControl()
        {
            Note("CAUSAL CONTROL ONLY: same physical opening, recording disabled with its real action button.");
            yield return Walk(8);yield return Press(new InputFrame{action=true});var ink=host.Form<InkForm>();Check("real input stops recording",ink!=null&&!ink.Recording);
            yield return Walk(17);yield return Wait("control reaches the original lower margin",()=>actor.Grounded&&Mathf.Abs(actor.Feet.y-1)<.3f,5);if(stopped)yield break;
            float peak=actor.Feet.y,end=Time.time+4,lastJump=-99;bool inkSupport=false;
            input.rule=()=>{bool edge=actor.Grounded&&Time.fixedTime-lastJump>.8f;if(edge)lastJump=Time.fixedTime;return new InputFrame{jump=edge,jumpHeld=true,move=new Vector2(Mathf.Clamp((15.8f-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.2f,-1,1),0)};};
            while(Live&&Time.time<end){yield return Tick();peak=Mathf.Max(peak,actor.Feet.y);inkSupport|=actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>();}
            input.rule=null;input.frame=default;Check("without the recorded fall ordinary jumps do not reach the comma",peak<4.5f&&!inkSupport&&session.Phase==RunPhase.Explore&&!session.HasKey);Note("CONTROL max feet="+peak);Snapshot("no-ink-control");
        }
        IEnumerator ScriptureRoute(bool secret)
        {
            yield return Walk(8);Check("scribe leech supplies actual Ink",host.Has(HostKind.Ink));yield return Walk(17);
            yield return Wait("land in the bottom margin",()=>actor.Grounded&&Mathf.Abs(actor.Feet.y-1)<.3f,5);yield return Pause(1.15f);
            yield return InkLanding(14.8f,2.5f,4.1f);if(stopped)yield break;yield return InkLanding(13.8f,4,7.8f);if(stopped)yield break;
            yield return Jump(17.6f,5);Check("reach punctuation desk through authored ink",actor.Feet.y>4.7f);if(stopped)yield break;
            yield return Walk(18.2f);var comma=session.GetComponentInChildren<PunctuationCart>();var beforeWords=session.GetComponentInChildren<ScriptureLayout>();Check("unmoved comma leaves every word above the desk jump",beforeWords.wrap==5&&beforeWords.words.All(w=>w.GetComponent<Collider2D>().bounds.max.y>9));yield return Press(new InputFrame{interact=true});Check("grip physical comma",comma.Holder==actor);yield return Walk(21.5f);yield return Pause(.4f);yield return Press(new InputFrame{interact=true});
            var layout=session.GetComponentInChildren<ScriptureLayout>();yield return Wait("moved comma reflows actual words",()=>comma.Slot==1&&layout.wrap==3&&layout.Reflows>0,3);Snapshot("physical-line-wrap");
            if(stopped)yield break;yield return Jump(24,5);yield return Jump(27,7.2f);yield return WordStep(31);if(!stopped&&actor.Feet.y<8)yield return Walk(32,true);yield return WordStep(35);yield return Jump(40,7.2f);
            if(stopped)yield break;Check("shadow source composes with Ink",host.Has(HostKind.Shadow)&&host.Has(HostKind.Ink));
            if(secret){yield return SemicolonMercy();if(stopped)yield break;}
            if(!secret){yield return Walk(42,true);yield return RunArc(50.5f,3.3f);}yield return Wait("lower sentence refuge",()=>actor.Grounded&&Mathf.Abs(actor.Feet.y-3.3f)<.3f,6);
            for(int k=0;k<9;k++){yield return Jump(55+k*5,k==0?3:2+k*.7f);if(stopped)yield break;}
            yield return Walk(95.9f);yield return Jump(100,9.2f);if(stopped)yield break;yield return Walk(102);Check("manuscript Keyling",session.HasKey);yield return Walk(104.5f);yield return Press(new InputFrame{interact=true});Check("imperative Turn changes path memory",session.Phase==RunPhase.Returning&&session.GetComponentInChildren<ScriptureCorrector>().imperative);
            yield return CorrectingReturn();
        }
    }
}
