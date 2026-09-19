using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class TurnSwitch : MonoBehaviour, IInteractable, IHittable
    {
        public void Interact(ActorMotor actor){if(!actor.replica)StageSession.Current?.Turn();}
        public void Hit(HitInfo hit){if(hit.owner&&!hit.owner.replica&&hit.downward)StageSession.Current?.Turn();}
        void OnCollisionEnter2D(Collision2D c){var a=c.collider.GetComponent<ActorMotor>();if(a&&!a.replica&&a.Feet.y>transform.position.y)StageSession.Current?.Turn();}
    }
}
