using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Campaign
{
    public sealed class MagneticBody:MonoBehaviour
    {
        public static readonly List<MagneticBody> All=new List<MagneticBody>();public int polarity=1;public float strength=34;
        [Min(0)] public float fieldRadius; // Zero keeps the existing shared Host range.
        public float Influence(Vector2 point,float hostRange){float r=fieldRadius>0?Mathf.Min(fieldRadius,hostRange):hostRange;float d=Vector2.Distance(point,Position);return !isActiveAndEnabled||d>=r?0:fieldRadius<=0?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(r-d));}
        public Vector2 ForceOn(Vector2 point,int pole,float hostRange)=>LodestoneForm.Force(point,Position,pole,polarity,strength)*Influence(point,hostRange);
        // Sample the physics pose, never an interpolated render transform, for forces.
        public Vector2 Position {get {var body=GetComponent<Rigidbody2D>();return body?body.position:(Vector2)transform.position;}}
        void OnEnable(){if(!All.Contains(this))All.Add(this);}void OnDisable(){All.Remove(this);}
        void Update(){var s=GetComponent<SpriteRenderer>();if(s){s.color=polarity>0?new Color(.86f,.46f,.48f):new Color(.43f,.65f,.92f);}}
    }
}