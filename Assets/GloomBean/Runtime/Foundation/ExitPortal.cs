using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class ExitPortal : MonoBehaviour, IInteractable
    {
        public void Interact(ActorMotor actor){if(!actor.replica)StageSession.Current?.TryClear();}
        void OnTriggerEnter2D(Collider2D c){var a=c.GetComponent<ActorMotor>();if(a&&!a.replica)StageSession.Current?.TryClear();}
    }
}
