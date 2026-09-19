using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class MagneticBody:MonoBehaviour
    {
        public static readonly List<MagneticBody> All=new List<MagneticBody>();public int polarity=1;public float strength=34;
        void OnEnable(){All.Add(this);}void OnDisable(){All.Remove(this);}
        void Update(){var s=GetComponent<SpriteRenderer>();if(s){s.color=polarity>0?new Color(.86f,.46f,.48f):new Color(.43f,.65f,.92f);}}
    }
}