using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class SeasonGrowth:MonoBehaviour
    {
        public SeasonWheel wheel;public Vector2 size=new Vector2(3,2);public float floor;BoxCollider2D shape;SpriteRenderer view;
        void Start(){shape=GetComponent<BoxCollider2D>();view=GetComponent<SpriteRenderer>();}
        void Update(){if(!wheel||!shape)return;int s=wheel.SeasonAt(transform.position);float height=s==0?size.y:s==1?size.y*.55f:s==2?.3f:size.y*.85f;var v=shape.size;v.y=Mathf.MoveTowards(v.y,height,Time.deltaTime*1.5f);shape.size=v;view.size=v;var p=transform.position;p.y=floor+v.y*.5f;transform.position=p;}
    }
}