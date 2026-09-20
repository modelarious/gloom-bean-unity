using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class EmpyreanVerification
    {
        IEnumerator RiserTransfer(MotionPlatform target)
        {
            if(stopped||!Live)yield break;var body=target.GetComponent<Rigidbody2D>();float ready=Time.time+35;
            while(Live&&Time.time<ready){float dy=body.position.y+.25f-actor.Feet.y;if(dy> -2.2f&&dy<1.25f)break;yield return Tick();}
            Check("rising neighbour enters a reachable transfer window",body.position.y+.25f-actor.Feet.y> -2.3f&&body.position.y+.25f-actor.Feet.y<1.35f);if(stopped)yield break;
            bool sent=false;float end=Time.time+7;
            input.rule=()=>{bool jump=!sent&&actor.Grounded;if(jump)sent=true;return new InputFrame{jump=jump,jumpHeld=true,move=new Vector2(Mathf.Clamp((body.position.x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.25f,-1,1),0)};};
            while(Live&&Time.time<end&&!(sent&&actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.attachedRigidbody==body&&Mathf.Abs(actor.Body.position.x-body.position.x)<.45f))yield return Tick();
            input.rule=null;input.frame=default;Check("land on the actual rising nave "+target.name,actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.attachedRigidbody==body);Snapshot("ascending-nave");
        }
        IEnumerator RisingSanctum()
        {
            yield return Walk(130);Check("rising sanctum supplies its actual incense",host.Has(HostKind.Censer));if(stopped)yield break;
            yield return Walk(131.3f);var risers=session.GetComponentsInChildren<MotionPlatform>().Where(x=>x.name.StartsWith("Ascending nave ")).OrderBy(x=>x.name).ToArray();Check("four independently moving architectural chunks",risers.Length==4);if(stopped)yield break;
            foreach(var r in risers){yield return RiserTransfer(r);if(stopped)yield break;yield return Pause(1.2f);Check("stillness slows the nave supporting the Host",LocalTime.Scale(r.transform.position)<.95f);if(stopped)yield break;yield return Walk(r.transform.position.x+1.3f);}
            yield return Jump(158,20);Snapshot("fourth-sanctum-exit");
        }
        IEnumerator BrightSanctum()
        {
            yield return Walk(160,true);yield return RunArc(165,20.5f);yield return Walk(166.2f);yield return RunArc(171,21);if(stopped)yield break;Check("bright sanctum has both physical iron and detached-shadow tenants",host.Has(HostKind.Shadow)&&host.Has(HostKind.Lodestone));
            var domain=session.GetComponentsInChildren<ShadowDomain>().Single(x=>x.name=="The bright side of shadow");Check("dark space is forbidden while luminous matter admits the shadow",domain.Allows(new Vector2(171,22))&&!domain.Allows(new Vector2(171,28)));if(stopped)yield break;
            yield return Focus(HostKind.Shadow);yield return Press(new InputFrame{alternate=true});yield return ShadowTravel(new Vector2(178,24));yield return Pause(.1f);
            var receiver=session.GetComponentsInChildren<ShadowReceiver>().Single(x=>x.name=="Hand inside the light");Check("shadow inside actual luminous geometry releases the last sanctum",receiver.active);if(stopped)yield break;
            // Return around the luminous elbow. The straight diagonal to the drifting
            // body's feet crosses darkness below the right-hand stained-glass pane.
            yield return ShadowTravel(new Vector2(174,23));if(stopped)yield break;
            yield return ShadowTravel(actor.Feet);if(stopped)yield break;
            yield return Press(new InputFrame{alternate=true});Check("body and shadow reunite before physical exit",host.Form<ShadowForm>().Attached);yield return Walk(174.5f,true);yield return RunArc(179,23);yield return Walk(184.8f);Snapshot("bright-shadow-rule");
        }
        IEnumerator RememberedMercy()
        {
            if(stopped||!Live)yield break;var gate=session.GetComponentsInChildren<Gate>().Single(g=>g.name=="Remembered skipping-rhyme trapdoor");Check("optional rhyme passage begins shut",!gate.opened);if(stopped)yield break;
            float start=Time.fixedTime+.08f,end=Time.time+4;int tapped=0;float heldUntil=0;float[] offsets={0,.5f,1,2};
            input.rule=()=>{bool jump=tapped<offsets.Length&&Time.fixedTime>=start+offsets[tapped];if(jump){tapped++;heldUntil=Time.fixedTime+.12f;}return new InputFrame{jump=jump,jumpHeld=Time.fixedTime<heldUntil};};
            while(Live&&Time.time<end&&!gate.opened)yield return Tick();input.rule=null;input.frame=default;Check("the Sunday Best jump rhythm opens real terrain",gate.opened&&tapped==4);if(stopped)yield break;
            yield return Walk(188);yield return Wait("drop through the remembered rhyme into the twentieth Mercy",()=>actor.Grounded&&Mathf.Abs(actor.Feet.y-18.5f)<.3f&&session.Mercies.Count==1,6);if(stopped)yield break;
            yield return Jump(186.9f,20.1f);yield return Jump(188.8f,21.7f);yield return Jump(193,23);Snapshot("remembered-tutorial-rhythm");
        }
        IEnumerator CollapseJump(float x,float top,DescentController collapse)
        {
            if(stopped||!Live)yield break;bool sent=false;float end=Time.time+7;
            input.rule=()=>{bool jump=!sent&&actor.Grounded;if(jump)sent=true;return new InputFrame{jump=jump,jumpHeld=true,run=true,move=new Vector2(Mathf.Clamp((x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.3f,-1,1),0)};};
            while(Live&&Time.time<end&&!(sent&&actor.Grounded&&Mathf.Abs(actor.Feet.y-collapse.Offset.y-top)<.35f&&Mathf.Abs(actor.Body.position.x-x)<.35f))yield return Tick();input.rule=null;input.frame=default;
            Check("jump through the collapsing sanctum at "+x,actor.Grounded&&Mathf.Abs(actor.Feet.y-collapse.Offset.y-top)<.4f&&Mathf.Abs(actor.Body.position.x-x)<.5f);
        }
        IEnumerator WhiteCollapse()
        {
            if(stopped||!Live)yield break;yield return Walk(193);Check("final Keyling is physically collected",session.HasKey);yield return Press(new InputFrame{interact=true});var collapse=session.GetComponentInChildren<DescentController>();
            Check("final Nail starts a physical collapse, not a cute reset",session.Phase==RunPhase.Returning&&collapse.falling&&game.IsCorrupted);if(stopped)yield break;
            yield return CollapseJump(194,25,collapse);yield return CollapseJump(193,27,collapse);yield return CollapseJump(190,29,collapse);if(stopped)yield break;
            Check("ceramic and eclipse thresholds restore only the corrupted base",host.Forms.Count==0&&game.IsCorrupted);Snapshot("false-heaven-collapses");
            for(int x=182;x>=6;x-=8){yield return Walk(x+5.7f,true);yield return CollapseJump(x,29,collapse);if(stopped)yield break;}
            yield return Walk(2);yield return Wait("controlled collapse returns to the real exit",()=>session.Phase==RunPhase.Cleared,9);Check("normal return never restores the cute body",game.IsCorrupted);
        }
    }
}
