using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class Hazard : MonoBehaviour
    {
        public int damage=1;
        public bool instant;
        public float activeDuration=0,period=0;
        public bool Active=>period<=0||Time.time%period<activeDuration;
        void OnTriggerStay2D(Collider2D c)
        {if(!Active)return;var a=c.GetComponent<ActorMotor>();if(a)a.Hit(new HitInfo(null,new Vector2(Mathf.Sign(a.transform.position.x-transform.position.x),0),instant?99:damage));}
    }
}
