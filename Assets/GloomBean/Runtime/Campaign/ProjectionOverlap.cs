using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class ProjectionOverlap:MonoBehaviour
    {
        public Vector2 size=new Vector2(3,4);public int allowed=7;
        public bool Contains(Vector2 p)=>new Rect((Vector2)transform.position-size*.5f,size).Contains(p);
        public bool Allows(int a,int b)=>(allowed&(1<<a))!=0&&(allowed&(1<<b))!=0;
    }
}