using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class EmpyreanVerification
    {
        ActorMotor LeadingControl(){var e=host.Form<EchoForm>();return e!=null&&e.Leading&&e.Echo?e.Echo:actor;}
        IEnumerator LeadWalk(float x,bool crouch=false)
        {
            if(stopped||!Live)yield break;float deadline=Time.time+18;
            input.rule=()=>{var control=LeadingControl();return new InputFrame{move=new Vector2(Mathf.Clamp((x-control.Body.position.x)*2-control.Body.linearVelocity.x*.2f,-1,1),crouch?-1:0)};};
            while(Live&&Time.time<deadline&&(Mathf.Abs(actor.Body.position.x-x)>.5f||Mathf.Abs(LeadingControl().Body.position.x-x)>.2f||Mathf.Abs(LeadingControl().Body.linearVelocity.x)>.25f))yield return Tick();
            input.rule=null;input.frame=default;yield return Pause(2.3f);Check("follow the leading body to "+x,Mathf.Abs(actor.Body.position.x-x)<.8f);
        }
        IEnumerator LeadFocus(HostKind kind)
        {
            Check("the chosen leading pair contains "+kind,host.Has(kind));if(stopped)yield break;
            for(int i=0;i<3&&host.Primary!=kind;i++){yield return Press(new InputFrame{alternate=true,move=Vector2.down});yield return Pause(2.2f);}
            Check("delayed body focuses "+kind,host.Primary==kind);
        }
        IEnumerator SanctumRoot(params Vector2[] points)
        {
            yield return Focus(HostKind.Root);if(stopped)yield break;var root=host.Form<RootForm>();
            yield return Wait("diagonal season moistens the starting seam",()=>RootSoil.All.Any(x=>x&&x.wet&&x.Near(actor.Body.position,.85f)),24);
            input.frame=new InputFrame{action=true,actionHeld=true};yield return Tick();input.frame=new InputFrame{actionHeld=true};Check("extend root through actual seasonal substrate",root.Growing);if(stopped)yield break;
            foreach(var target in points){float end=Time.time+24;input.rule=()=>new InputFrame{actionHeld=true,move=Vector2.ClampMagnitude((target-root.Tip)*3,1)};
                while(Live&&Time.time<end&&Vector2.Distance(root.Tip,target)>.13f)yield return Tick();input.rule=null;input.frame=new InputFrame{actionHeld=true};Check("steer seasonal root to "+target,Vector2.Distance(root.Tip,target)<.2f);if(stopped)yield break;}
            Vector2 endpoint=points[points.Length-1];Note("ROOT EXIT tip="+root.Tip+" clear="+host.CanStand(root.Tip));if(!host.CanStand(root.Tip))foreach(var c in Physics2D.OverlapBoxAll(root.Tip,new Vector2(.78f,1.36f),0,Layers.Solids))if(!c.isTrigger&&c.attachedRigidbody!=actor.Body)Note("ROOT EXIT BLOCKER "+c.name+" "+c.bounds);input.frame=default;yield return Tick();yield return Wait("body follows the curved seasonal root",()=>!root.Retracting&&Vector2.Distance(actor.Body.position,endpoint)<.5f,7);Snapshot("seasonal-root");
        }
        IEnumerator SanctuaryPlane(int plane)
        {
            var form=host.Form<ParallaxForm>();Check("perspective source is available",form!=null);if(stopped)yield break;
            float end=Time.time+4;input.rule=()=>new InputFrame{move=new Vector2(0,Mathf.Sign(plane-form.Plane))};while(Live&&Time.time<end&&form.Plane!=plane)yield return Tick();input.rule=null;input.frame=default;Check("align the projected body to "+plane,form.Plane==plane);
        }
        IEnumerator SanctuaryPlaneJump(float x,float top,int plane)
        {
            if(stopped||!Live)yield break;var form=host.Form<ParallaxForm>();Check("depth change has a real tenant",form!=null);if(stopped)yield break;
            float end=Time.time+7,initial=actor.Feet.y;bool sent=false;
            input.rule=()=>{bool jump=!sent&&actor.Grounded;if(jump)sent=true;return new InputFrame{jump=jump,jumpHeld=true,run=plane==0,move=new Vector2(Mathf.Clamp((x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.15f,-1,1),sent&&actor.Feet.y>initial+.45f&&form.Plane!=plane?Mathf.Sign(plane-form.Plane):0)};};
            while(Live&&Time.time<end&&!(sent&&actor.Grounded&&Mathf.Abs(actor.Feet.y-top)<.25f&&Mathf.Abs(actor.Body.position.x-x)<.25f&&form.Plane==plane))yield return Tick();input.rule=null;input.frame=default;
            Check("real plane landing "+plane+" at "+x,actor.Grounded&&Mathf.Abs(actor.Feet.y-top)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.5f&&form.Plane==plane);Snapshot("folded-perspective");
        }
        IEnumerator SanctuaryFold(string group,Vector2 aim,float angle)
        {
            yield return Focus(HostKind.Stitch);if(stopped)yield break;var f=host.Form<StitchForm>();if(f.First!=null)yield return Press(new InputFrame{alternate=true});
            yield return Press(new InputFrame{action=true,move=aim});Check("catch physical "+group+" edge",f.First&&f.First.group==group);if(stopped)yield break;
            yield return Press(new InputFrame{action=true,move=aim});Check("connect two actual structural edges",f.Active!=null);if(stopped)yield break;
            yield return Press(new InputFrame{action=true});yield return Wait("folded ramp takes its physical angle",()=>Mathf.Abs(f.Active.angle-angle)<2,8);Snapshot("folded-ramp");
        }
        IEnumerator BoardFoldedRamp()
        {
            if(stopped||!Live)yield break;yield return Walk(87,true);bool sent=false;float end=Time.time+6;
            input.rule=()=>{bool edge=!sent&&actor.Grounded;if(edge)sent=true;return new InputFrame{jump=edge,jumpHeld=true,run=true,move=new Vector2(Mathf.Clamp((91-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.35f,-1,1),0)};};
            while(Live&&Time.time<end&&!(sent&&actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.GetComponentInParent<FoldPanel>()))yield return Tick();input.rule=null;input.frame=default;
            Check("land on the actual folded plane",actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.GetComponentInParent<FoldPanel>());Snapshot("folded-plane-landing");
        }
        IEnumerator FoldedSanctum()
        {
            yield return Walk(72);yield return SanctuaryPlane(0);yield return Walk(72.7f);yield return SanctuaryPlaneJump(77,2.1625f,0);
            yield return Walk(78.2f);yield return SanctuaryPlaneJump(83,3.65f,2);yield return Walk(86);yield return Wait("spool nests alongside depth",()=>host.Has(HostKind.Stitch),3);if(stopped)yield break;
            // The seam is out of reach from the near-plane balcony. Board the real
            // horizontal flap before stitching it; do not enlarge interaction range.
            yield return BoardFoldedRamp();if(stopped)yield break;
            yield return SanctuaryFold("depth",Vector2.right,45);if(stopped)yield break;yield return Walk(95);yield return Jump(97,10);
            yield return Wait("flat sign restores the normal footprint before the painted threshold",()=>!host.Has(HostKind.Parallax),4);yield return Walk(101);Check("enter the folded fresco from its actual threshold",host.Has(HostKind.InsideOut));if(stopped)yield break;
            yield return Jump(104,11);yield return Jump(106,12);yield return Jump(108.2f,13);yield return Jump(109.6f,15);yield return Jump(111.5f,16);yield return Walk(114.25f);
            yield return Jump(116.2f,17);yield return Jump(118.2f,18);yield return Jump(119.6f,20);yield return Jump(121.5f,21);yield return Walk(126.4f);yield return Wait("empty frame returns ordinary collision",()=>!host.Has(HostKind.InsideOut),3);Snapshot("third-sanctum-exit");
        }
        IEnumerator WhiteGateRoute(bool secret)
        {
            bool alternate=Arg("-gb-white-alternatives","0")=="1";
            if(alternate){yield return MirrorInkSanctum();if(stopped)yield break;yield return StitchSeasonSanctum();}
            else {
            yield return Walk(5);yield return LeadWalk(12);
            Check("sanctum starts with a leading Echo, not a prediction",host.Form<EchoForm>()!=null&&host.Form<EchoForm>().Leading);
            Check("Molt is acquired physically beside the leading echo",host.Has(HostKind.Molt));if(stopped)yield break;
            yield return LeadWalk(18.5f,true);yield return LeadFocus(HostKind.Molt);yield return Press(new InputFrame{action=true});yield return Pause(2.3f);
            Check("leave an actual skin on the first sanctum scale",host.Husks.Count==1);yield return LeadWalk(28);
            var gate=session.GetComponentsInChildren<Gate>().First(g=>g.plates.Length==2);yield return Wait("two simultaneous bodies release the first sanctum",()=>gate.opened,4);
            yield return LeadWalk(36);if(stopped)yield break;Check("first sanctum is traversed without granting its result",actor.Body.position.x>34);
            yield return LeadWalk(40);if(stopped)yield break;yield return Walk(48);yield return Walk(47);Check("root is available after the material sanctuary sources",host.Has(HostKind.Root));
            yield return SanctumRoot(new Vector2(47,-4),new Vector2(60,-4),new Vector2(60,1.5f));if(stopped)yield break;yield return Walk(68);Check("seasonal wall was traversed through actual material",actor.Body.position.x>62);
            }
            if(stopped)yield break;
            yield return FoldedSanctum();if(stopped)yield break;yield return RisingSanctum();if(stopped)yield break;yield return BrightSanctum();if(stopped)yield break;if(secret){yield return RememberedMercy();if(stopped)yield break;}yield return WhiteCollapse();
        }
    }
}
