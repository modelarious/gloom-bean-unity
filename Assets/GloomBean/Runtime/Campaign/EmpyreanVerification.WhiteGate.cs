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
        IEnumerator LeadWalk(float x)
        {
            if(stopped||!Live)yield break;float deadline=Time.time+18;
            input.rule=()=>{var control=LeadingControl();return new InputFrame{move=new Vector2(Mathf.Clamp((x-control.Body.position.x)*2-control.Body.linearVelocity.x*.2f,-1,1),0)};};
            while(Live&&Time.time<deadline&&(Mathf.Abs(LeadingControl().Body.position.x-x)>.2f||Mathf.Abs(LeadingControl().Body.linearVelocity.x)>.25f))yield return Tick();
            input.rule=null;input.frame=default;yield return Pause(2.3f);Check("follow the leading body to "+x,Mathf.Abs(actor.Body.position.x-x)<.8f);
        }
        IEnumerator LeadFocus(HostKind kind)
        {
            Check("the chosen leading pair contains "+kind,host.Has(kind));if(stopped)yield break;
            for(int i=0;i<3&&host.Primary!=kind;i++){yield return Press(new InputFrame{alternate=true,move=Vector2.down});yield return Pause(2.2f);}
            Check("delayed body focuses "+kind,host.Primary==kind);
        }
        IEnumerator WhiteGateRoute(bool secret)
        {
            yield return Walk(5);yield return LeadWalk(12);
            Check("sanctum starts with a leading Echo, not a prediction",host.Form<EchoForm>()!=null&&host.Form<EchoForm>().Leading);
            Check("Molt is acquired physically beside the leading echo",host.Has(HostKind.Molt));if(stopped)yield break;
            yield return LeadWalk(18);yield return LeadFocus(HostKind.Molt);yield return Press(new InputFrame{action=true});yield return Pause(2.3f);
            Check("leave an actual skin on the first sanctum scale",host.Husks.Count==1);yield return LeadWalk(28);
            var gate=session.GetComponentsInChildren<Gate>().First(g=>g.plates.Length==2);yield return Wait("two simultaneous bodies release the first sanctum",()=>gate.opened,4);
            yield return LeadWalk(36);if(stopped)yield break;Check("first sanctum is traversed without granting its result",actor.Body.position.x>34);
            Check("remaining White Gate route requires further authored input proof",false);
        }
    }
}
