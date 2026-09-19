using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class LeadingEchoZone:MonoBehaviour
    {
        void OnTriggerStay2D(Collider2D c){var h=c.GetComponent<HostController>();var f=h?h.Form<EchoForm>():null;if(f!=null&&!f.Leading){f.Synchronize();f.Leading=true;}}
    }
}