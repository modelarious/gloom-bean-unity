using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class DepthGeometry:MonoBehaviour
    {
        public int plane=1;public Vector2 canonicalSize=Vector2.one;
        public static float Factor(int plane)=>plane==0?.65f:plane==2?1.4f:1;
        public void SetPlane(int p){plane=Mathf.Clamp(p,0,2);gameObject.layer=17+plane;var c=GetComponent<BoxCollider2D>();c.size=canonicalSize*Factor(plane);var sr=GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Sliced;sr.size=c.size;sr.color=plane==0?new Color(.48f,.68f,.79f):plane==1?new Color(.77f,.66f,.53f):new Color(.92f,.48f,.4f);}
    }
}