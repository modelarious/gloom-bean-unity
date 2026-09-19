using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class ForemanBoss : MonoBehaviour,IHittable
    {
        public StageBuilder builder;public int phase;public bool vulnerable;
        float clock;SpriteRenderer sprite;
        void Start(){sprite=GetComponent<SpriteRenderer>();}
        void Update()
        {
            clock+=Time.deltaTime;vulnerable=clock%6>3;
            if(sprite)sprite.color=vulnerable?new Color(.94f,.68f,.4f):new Color(.55f,.2f,.3f);
            if(clock>6){clock=0;builder.Enemy(new Vector2(30,2));RuntimeEvents.Emit("boss-attack",phase.ToString());}
        }
        public void Hit(HitInfo h)
        {
            if(!vulnerable)return;
            bool accept=phase==0?h.power>=2&&!h.downward:phase==1?h.projectile&&h.power>=2: h.downward&&h.power>=3;
            if(!accept)return;phase++;clock=0;RuntimeEvents.Emit("boss-phase",phase.ToString());
            if(phase>=3){StageSession.Current.BossClear();Destroy(gameObject);}
        }
    }
}
