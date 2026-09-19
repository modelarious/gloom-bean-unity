using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // The tray carries actual terrain; no inventory flag opens a remote door.
    public sealed class TerrainServingLift:MonoBehaviour
    {
        public Vector2 lower,upper;public float speed=2;public bool returning;public EdibleChunk cargo;
        Rigidbody2D body;bool delivered;
        void Awake(){body=GetComponent<Rigidbody2D>();}
        void FixedUpdate()
        {
            if(!body)return;
            if(!cargo)foreach(var hit in Physics2D.OverlapBoxAll(body.position+Vector2.up*.65f,new Vector2(4,.9f),0,Layers.Solids))
            {var tile=hit.GetComponent<EdibleChunk>();if(tile&&tile.gameObject.activeInHierarchy){cargo=tile;tile.Remember();tile.gameObject.layer=Layers.Moving;break;}}
            bool aboard=cargo&&cargo.gameObject.activeInHierarchy&&Mathf.Abs(cargo.transform.position.x-body.position.x)<2.5f&&Mathf.Abs(cargo.transform.position.y-body.position.y)<1.3f;
            if(aboard&&!delivered){Vector2 delta=Vector2.MoveTowards(body.position,upper,speed*Time.fixedDeltaTime)-body.position;body.MovePosition(body.position+delta);cargo.transform.position+=(Vector3)delta;if(Vector2.Distance(body.position,upper)<.05f)delivered=true;}
            if(returning&&delivered){Vector2 delta=Vector2.MoveTowards(body.position,lower,speed*Time.fixedDeltaTime)-body.position;body.MovePosition(body.position+delta);if(aboard)cargo.transform.position+=(Vector3)delta;}
        }
    }
}
