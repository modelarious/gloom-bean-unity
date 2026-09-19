using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class SoundPulse:MonoBehaviour
    {
        public Vector2[] path;public PulseReceiver receiver;public float speed=6,delay;public int strength=1;int i=1;
        void Update(){delay-=Time.deltaTime;if(delay>0)return;if(path==null||path.Length<2){Destroy(gameObject);return;}float left=Time.deltaTime*speed;while(left>0&&i<path.Length){float d=Vector2.Distance(transform.position,path[i]);if(d<=left){transform.position=path[i++];left-=d;}else{transform.position=Vector2.MoveTowards(transform.position,path[i],left);left=0;}}if(i>=path.Length){if(receiver)receiver.Receive(strength);Destroy(gameObject);}}
    }
}