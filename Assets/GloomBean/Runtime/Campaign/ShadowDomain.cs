using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class ShadowDomain:MonoBehaviour
    {
        public static readonly List<ShadowDomain> All=new List<ShadowDomain>();public Rect area;public Collider2D[] lightPaths;
        void OnEnable(){All.Add(this);}void OnDisable(){All.Remove(this);}
        public bool Allows(Vector2 p){if(lightPaths!=null)foreach(var c in lightPaths)if(c&&c.OverlapPoint(p))return true;return false;}
    }
}