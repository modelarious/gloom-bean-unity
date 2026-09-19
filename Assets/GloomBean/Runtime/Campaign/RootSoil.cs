using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class RootSoil:MonoBehaviour
    {
        public static readonly List<RootSoil> All=new List<RootSoil>();public bool wet=true;public Vector2 size=Vector2.one;public float moisture=1;
        void OnEnable(){All.Add(this);}void OnDisable(){All.Remove(this);}
        public bool Near(Vector2 p,float margin){var r=new Rect((Vector2)transform.position-size*.5f-Vector2.one*margin,size+Vector2.one*margin*2);return r.Contains(p);}
        void Update(){var s=GetComponent<SpriteRenderer>();if(s)s.color=wet?new Color(.32f,.55f,.35f,.55f):new Color(.49f,.37f,.3f,.5f);}
    }
}