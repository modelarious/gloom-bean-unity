using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class CorruptionTrigger:MonoBehaviour
    {
        public GameObject[] pretty;public GameObject[] revealed;public bool applied;
        void OnTriggerEnter2D(Collider2D c){var a=c.GetComponent<ActorMotor>();if(!a||a.replica||applied)return;Apply();}
        public void Apply(){applied=true;GameRoot.Instance?.MarkCorrupted();if(pretty!=null)foreach(var p in pretty)if(p)p.SetActive(false);if(revealed!=null)foreach(var r in revealed)if(r)r.SetActive(true);StageSession.Current?.Camera?.Kick(.18f);StageSession.Current?.Notice("Something got in. Your old body does not come back when a tenant leaves.",6);RuntimeEvents.Emit("permanent-corruption");}
    }
}