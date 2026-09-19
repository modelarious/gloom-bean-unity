using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class Breakable : MonoBehaviour, IHittable
    {
        public int requiredPower=1;
        public bool downwardOnly;
        public void Hit(HitInfo h)
        {
            if(h.power<requiredPower||downwardOnly&&!h.downward)return;
            GetComponent<Collider2D>().enabled=false;RuntimeEvents.Emit("break",requiredPower.ToString());Destroy(gameObject);
        }
    }
}
