using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class ShadowDomain:MonoBehaviour
    {
        public static readonly List<ShadowDomain> All=new List<ShadowDomain>();public Rect area;public Collider2D[] lightPaths;public ShadowSun onlySun;
        void OnEnable(){All.Add(this);}void OnDisable(){All.Remove(this);}
        public bool Allows(Vector2 p){if(onlySun){foreach(var poly in onlySun.Polygons)if(ShadowSun.Inside(p,poly))return true;return false;}if(lightPaths!=null)foreach(var c in lightPaths)if(c&&c.OverlapPoint(p))return true;return false;}
    }
}