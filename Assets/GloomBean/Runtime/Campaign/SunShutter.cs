using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class SunShutter:MonoBehaviour
    {
        public GloomBean.Foundation.Lever occluder;public float period=5;public int phase;public bool reverse;float clock;Collider2D shape;SpriteRenderer view;
        void Start(){shape=GetComponent<Collider2D>();view=GetComponent<SpriteRenderer>();}
        void Update(){clock+=Time.deltaTime;bool lit=occluder ? occluder.state&&(!reverse||phase==0) : (Mathf.FloorToInt(clock/period)+phase+(reverse?1:0))%2==0;bool warn=!occluder&&Mathf.Repeat(clock,period)>period-.8f;if(shape)shape.enabled=lit;if(view){var c=view.color;c.a=lit?(warn?.5f:1):.18f;view.color=c;}}
    }
}