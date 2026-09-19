using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class WaterVolume : MonoBehaviour
    {
        public Vector2 current;
        public float Surface=>GetComponent<BoxCollider2D>().bounds.max.y;
        void Awake(){GetComponent<BoxCollider2D>().isTrigger=true;gameObject.layer=Layers.Sensor;}
        public bool Contains(Vector2 p)=>GetComponent<BoxCollider2D>().bounds.Contains(p);
        void OnTriggerStay2D(Collider2D c){var actor=c.GetComponent<ActorMotor>();if(actor)actor.EnterWater(this);}
        void OnTriggerEnter2D(Collider2D c){var actor=c.GetComponent<ActorMotor>();if(actor)actor.EnterWater(this);}
    }
}
