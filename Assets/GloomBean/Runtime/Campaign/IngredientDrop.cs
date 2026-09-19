using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class IngredientDrop:MonoBehaviour
    {
        public string kind="grease";
        static PhysicsMaterial2D grease;
        void Start(){if(kind=="grease"){if(!grease)grease=new PhysicsMaterial2D("Flowing kitchen grease"){friction=0,bounciness=0};GetComponent<Collider2D>().sharedMaterial=grease;}}
        void OnCollisionEnter2D(Collision2D c){var dish=c.collider.GetComponent<FeastDish>();if(dish){dish.Receive(kind);Destroy(gameObject);}}
    }
}
