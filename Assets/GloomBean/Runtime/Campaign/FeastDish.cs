using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class FeastDish:MonoBehaviour
    {
        public int received;public AirJet steam;public RootSoil soil;public MotionPlatform conveyor;public void Receive(string kind){received++;if(kind=="grease"&&steam)steam.until=Time.time+5;if(kind=="salt"&&soil)soil.wet=false;if(conveyor)conveyor.paused=false;RuntimeEvents.Emit("dish",kind);}
    }
}