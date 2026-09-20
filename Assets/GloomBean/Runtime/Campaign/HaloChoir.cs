using UnityEngine;
namespace GloomBean.Campaign
{
    // Polarity belongs to the same simulation clock as rigid-body motion.
    [DefaultExecutionOrder(-200)]
    public sealed class HaloChoir:MonoBehaviour
    {
        public MagneticBody[] halos;public bool desynchronized;public float measure=4;
        double clock;
        public float Clock=>(float)clock;
        public float UntilFlip(int index)=>Mathf.Max(0,measure-Mathf.Repeat((float)clock+(desynchronized?index*.67f:0),Mathf.Max(.1f,measure)));
        void FixedUpdate()
        {
            clock+=Time.fixedDeltaTime;
            if(halos==null)return;
            for(int i=0;i<halos.Length;i++)if(halos[i])
            {int beat=Mathf.FloorToInt(((float)clock+(desynchronized?i*.67f:0))/Mathf.Max(.1f,measure));halos[i].polarity=beat%2==0?1:-1;}
        }
    }
}
