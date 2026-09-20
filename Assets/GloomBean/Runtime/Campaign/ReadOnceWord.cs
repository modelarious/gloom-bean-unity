using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Leaving a word in the imperative paragraph consumes its physical support.
    public sealed class ReadOnceWord:MonoBehaviour
    {
        public bool Read {get;private set;}public bool Erased {get;private set;}float fading;Collider2D shape;SpriteRenderer art;
        void Awake(){shape=GetComponent<Collider2D>();art=GetComponent<SpriteRenderer>();}
        void FixedUpdate(){var s=StageSession.Current;if(!s||s.Phase!=RunPhase.Returning||!s.player||Erased)return;
            bool supported=s.player.GroundCollider==shape&&s.player.Grounded;if(supported){Read=true;return;}
            if(!Read)return;fading+=Time.fixedDeltaTime;if(art){var c=art.color;c.a=Mathf.Max(.15f,1-fading*2);art.color=c;}
            if(fading>.45f){shape.enabled=false;Erased=true;RuntimeEvents.Emit("word-erased",name);}}
    }
}
