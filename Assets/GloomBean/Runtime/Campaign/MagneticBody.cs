using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class MagneticBody:MonoBehaviour
    {
        public static readonly List<MagneticBody> All=new List<MagneticBody>();public int polarity=1;public float strength=34;
        // Sample the physics pose, never an interpolated render transform, for forces.
        public Vector2 Position {get {var body=GetComponent<Rigidbody2D>();return body?body.position:(Vector2)transform.position;}}
        void OnEnable(){if(!All.Contains(this))All.Add(this);}void OnDisable(){All.Remove(this);}
        void Update(){var s=GetComponent<SpriteRenderer>();if(s){s.color=polarity>0?new Color(.86f,.46f,.48f):new Color(.43f,.65f,.92f);}}
    }
}