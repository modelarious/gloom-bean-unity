using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A removed drain stone empties a local soil channel. Reversed pumping refills it.
    public sealed class MaterialSluice:MonoBehaviour
    {
        public EdibleChunk cover;public RootSoil[] channel;public bool reversed;
        public bool Wet=>reversed||cover&&cover.gameObject.activeInHierarchy;
        void Update(){if(channel==null)return;foreach(var soil in channel)if(soil)soil.wet=Wet;}
    }
}
