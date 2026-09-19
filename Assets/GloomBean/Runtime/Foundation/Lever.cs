using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class Lever : MonoBehaviour, IInteractable
    {
        public bool state;public event Action<bool> Changed;
        public void Interact(ActorMotor actor){state=!state;Changed?.Invoke(state);RuntimeEvents.Emit("lever",name);}
    }
}
