using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class Pickup : MonoBehaviour
    {
        public PickupKind kind;
        public string stableId;
        public int amount=1;
        bool collected;
        void OnTriggerEnter2D(Collider2D c)
        {
            var a=c.GetComponent<ActorMotor>();if(!a||a.replica||collected)return;
            collected=true;StageSession.Current?.Collect(this,a);GetComponent<Collider2D>().enabled=false;Destroy(gameObject);
        }
    }
}
