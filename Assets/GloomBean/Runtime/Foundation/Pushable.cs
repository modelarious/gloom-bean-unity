using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class Pushable : MonoBehaviour, IHittable
    {
        public bool metal;
        public void Hit(HitInfo hit){var rb=GetComponent<Rigidbody2D>();if(rb)rb.AddForce(new Vector2(hit.direction.x*hit.power*5,hit.downward?0:1),ForceMode2D.Impulse);}
    }
}
