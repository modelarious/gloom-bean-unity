using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class EncounterBudget:MonoBehaviour
    {
        public AtlasBoss boss;public float remaining=200;
        void Update(){if(!boss||boss.defeated)return;remaining-=Time.deltaTime;if(remaining<=0)StageSession.Current?.Fail("The combined weight reached the foundations.");}
        void OnGUI(){if(boss&&!boss.defeated)GUI.Label(new Rect(22,180,400,30),"FOUNDATION HOLDING  "+remaining.ToString("0.0")+" s");}
    }
}