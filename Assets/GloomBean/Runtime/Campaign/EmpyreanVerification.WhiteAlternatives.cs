using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class EmpyreanVerification
    {
        IEnumerator MirrorInkSanctum()
        {
            yield return Walk(5);yield return Jump(7,2);yield return Jump(9,4);yield return Jump(11,6);yield return Walk(13);
            Check("the player chooses Mirror and Ink instead of Echo and Molt",host.Has(HostKind.Mirror)&&host.Has(HostKind.Ink)&&!host.Has(HostKind.Echo));if(stopped)yield break;
            yield return Walk(15,true);yield return RunArc(20,5.5f);yield return Walk(20.7f);yield return ShortArc(19.2f,5.5f);if(stopped)yield break;
            yield return InkLanding(20,6.3f,8);if(stopped)yield break;Check("the alternate route stands on its own delayed ink",actor.GroundCollider&&actor.GroundCollider.GetComponent<InkStroke>());yield return RunArc(26,8.6f);if(stopped)yield break;
            yield return Walk(28.85f);yield return Wait("the unmatched statue holds the other physical body on a scale",()=>actor.Grounded&&actor.Feet.y<.2f&&host.Form<MirrorForm>().Twin.Grounded,6);if(stopped)yield break;
            var gate=session.GetComponentsInChildren<Gate>().First(g=>g.plates.Length==2);yield return Wait("Mirror and Ink open the same first sanctum",()=>gate.opened,4);Snapshot("alternative-mirror-ink");
            yield return Walk(40);Check("the first approach does not silently require Echo",!host.Has(HostKind.Echo));
        }
        IEnumerator StitchSeasonSanctum()
        {
            yield return Walk(41);Check("seamstress is an independent seasonal choice",host.Has(HostKind.Stitch));if(stopped)yield break;
            yield return Jump(41,2);yield return Jump(43,4);yield return Jump(45,6);yield return Jump(47,8);
            yield return SanctuaryFold("season",new Vector2(1,.3f),39.93f);if(stopped)yield break;yield return Walk(54.5f);yield return RunArc(61,15);if(stopped)yield break;
            Check("folded architecture crosses the seasonal wall without a root",actor.Feet.y>14&&actor.Body.position.x>59&&!host.Has(HostKind.Root));Snapshot("alternative-stitch-season");
            yield return Walk(68);yield return Wait("leave the high route through ordinary gravity",()=>actor.Grounded&&actor.Feet.y<.3f,7);yield return Walk(69.5f);
        }
    }
}
