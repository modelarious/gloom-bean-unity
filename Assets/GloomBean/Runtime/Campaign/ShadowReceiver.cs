using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class ShadowReceiver:MonoBehaviour
    {
        public Gate gate;public float radius=.75f;public bool latch=true,active;public bool requireControl=true;public Collider2D requiredCaster;public ShadowSun requiredSun;
        void FixedUpdate(){var actor=StageSession.Current?StageSession.Current.player:null;var h=actor?actor.GetComponent<HostController>():null;var s=h?h.Form<ShadowForm>():null;bool hit=s!=null&&(!requireControl||s.Controlling)&&s.Allowed(s.Position)&&Vector2.Distance(s.Position,transform.position)<=radius&&(!requiredCaster||requiredSun&&requiredSun.CastBy(s.Position,requiredCaster));if(hit)active=true;else if(!latch)active=false;if(gate)gate.SetOpen(active);}
    }
}