using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class EdibleChunk:MonoBehaviour
    {
        public string stableId;public bool returnPending;public Vector2 Original {get;private set;}bool remembered;
        public Vector2 Size=>GetComponent<BoxCollider2D>().size;
        public void Remember(){if(remembered)return;Original=transform.position;remembered=true;}
        void Update()
        {
            if(!returnPending)return;bool occupied=false;
            foreach(var c in Physics2D.OverlapBoxAll(transform.position,Size*.96f,0,(1<<Layers.Actor)|(1<<Layers.Enemy)|(1<<Layers.Prop)))if(c&&!c.isTrigger)occupied=true;
            if(!occupied){GetComponent<Collider2D>().enabled=true;returnPending=false;}
        }
    }
}