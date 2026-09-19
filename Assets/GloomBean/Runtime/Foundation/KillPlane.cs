using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class KillPlane : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D c){var a=c.GetComponent<ActorMotor>();if(a)a.Hit(new HitInfo(null,Vector2.up,99));}
    }
}
