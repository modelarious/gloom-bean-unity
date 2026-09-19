using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class ShadowReceiver:MonoBehaviour
    {
        public Gate gate;public float radius=.75f;public bool latch=true,active;
        void FixedUpdate(){var actor=StageSession.Current?StageSession.Current.player:null;var h=actor?actor.GetComponent<HostController>():null;var s=h?h.Form<ShadowForm>():null;bool hit=s!=null&&Vector2.Distance(s.Position,transform.position)<=radius;if(hit)active=true;else if(!latch)active=false;if(gate)gate.SetOpen(active);}
    }
}