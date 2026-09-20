using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A real collectible accessible in cast-shadow space. The physical Host remains exposed.
    public sealed class ShadowMercy:MonoBehaviour
    {
        bool collected;
        void FixedUpdate()
        {
            if(collected)return;var s=StageSession.Current;if(!s||!s.player)return;
            var h=s.player.GetComponent<HostController>();var shadow=h?h.Form<ShadowForm>():null;
            if(shadow==null||!shadow.Controlling||!shadow.Allowed(shadow.Position)||Vector2.Distance(shadow.Position,transform.position)>.5f)return;
            var pickup=GetComponent<Pickup>();if(!pickup)return;collected=true;GetComponent<Collider2D>().enabled=false;
            s.Collect(pickup,s.player);RuntimeEvents.Emit("shadow-mercy",pickup.stableId);Destroy(gameObject);
        }
    }
}
