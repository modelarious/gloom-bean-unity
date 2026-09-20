using UnityEngine;
namespace GloomBean.Campaign
{
    // The two suspension cables keep the island level as the support tower folds.
    [DefaultExecutionOrder(-90)]
    public sealed class FoldTipIsland:MonoBehaviour
    {
        public FoldPanel support;public Vector2 offset=new Vector2(2,.45f);Rigidbody2D body;
        void Start(){body=GetComponent<Rigidbody2D>();}
        void FixedUpdate(){if(!support||!body)return;Vector2 end=support.body.position+(Vector2)(Quaternion.Euler(0,0,support.body.rotation)*Vector2.right)*support.length;
            body.MovePosition(end+offset);body.MoveRotation(0);}
    }
}
