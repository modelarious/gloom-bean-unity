using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class FlowEmitter:MonoBehaviour
    {
        public bool diverted;public Vector2 normalVelocity=new Vector2(4,0),divertedVelocity=new Vector2(0,5);public string ingredient="grease";public float interval=1.2f;float clock;public bool reversed;
        void Update(){clock+=Time.deltaTime*LocalTime.Scale(transform.position);if(clock<interval)return;clock=0;var g=PrimitiveArt.Shape("Flowing "+ingredient,transform.parent,transform.position,Vector2.one*.25f,new Color(.88f,.69f,.39f),PrimitiveArt.Icon.Round,6);g.layer=Layers.Prop;var c=g.AddComponent<CircleCollider2D>();c.radius=.5f;var rb=g.AddComponent<Rigidbody2D>();rb.mass=.08f;rb.gravityScale=1;rb.linearVelocity=(diverted?divertedVelocity:normalVelocity)*(reversed?-1:1);var i=g.AddComponent<IngredientDrop>();i.kind=ingredient;Destroy(g,8);}
    }
}