using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class HostEmbargo:MonoBehaviour
    {
        public HostKind stolen;public bool active;public bool Blocks(HostKind k)=>active&&stolen==k;
        public void Steal(HostController h,HostKind k){stolen=k;active=true;if(h.Has(k))h.Cure(k,true);GloomBean.Foundation.StageSession.Current?.Notice("The Host of Hosts took "+HostController.Display(k)+". Solve the next act without it.",6);}
    }
}