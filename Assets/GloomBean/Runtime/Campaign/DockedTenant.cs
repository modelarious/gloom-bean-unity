using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class DockedTenant:MonoBehaviour
    {
        public ApartmentLift lift;public HostSource tenant;bool shown;
        void Update(){if(shown||!lift||!tenant)return;if(lift.upperRequested&&Vector2.Distance(lift.transform.position,lift.upper)<.04f){shown=true;tenant.gameObject.SetActive(true);}}
    }
}
