using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class ShadowCaster:MonoBehaviour
    {
        public static readonly List<ShadowCaster> All=new List<ShadowCaster>();public Collider2D shape;
        void OnEnable(){All.Add(this);shape=GetComponent<Collider2D>();}void OnDisable(){All.Remove(this);}
    }
}