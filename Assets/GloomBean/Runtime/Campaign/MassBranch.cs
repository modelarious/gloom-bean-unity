using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class MassBranch:MonoBehaviour
    {
        public Rigidbody2D body;public float rest=5,stiffness=3,maxDeflection=30;public float Load {get;private set;}
        readonly Dictionary<Rigidbody2D,float> contacts=new Dictionary<Rigidbody2D,float>();
        void OnCollisionStay2D(Collision2D c){if(c.rigidbody&&c.rigidbody.bodyType==RigidbodyType2D.Dynamic)contacts[c.rigidbody]=Time.time;}
        void FixedUpdate(){Load=0;foreach(var kv in contacts)if(kv.Key&&Time.time-kv.Value<.12f)Load+=kv.Key.mass*Mathf.Clamp(kv.Key.position.x-body.position.x,-8,8);float target=rest-Mathf.Clamp(Load*stiffness,-maxDeflection,maxDeflection);body.MoveRotation(Mathf.MoveTowardsAngle(body.rotation,target,25*Time.fixedDeltaTime));}
    }
}