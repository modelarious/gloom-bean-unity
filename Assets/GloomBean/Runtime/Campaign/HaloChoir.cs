using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class HaloChoir:MonoBehaviour
    {
        public MagneticBody[] halos;public bool desynchronized;public float measure=4;float clock;int last=-1;
        void Update(){clock+=Time.deltaTime;for(int i=0;i<halos.Length;i++)if(halos[i]){int beat=Mathf.FloorToInt((clock+(desynchronized?i*.67f:0))/measure);halos[i].polarity=beat%2==0?1:-1;}last=Mathf.FloorToInt(clock/measure);}
    }
}