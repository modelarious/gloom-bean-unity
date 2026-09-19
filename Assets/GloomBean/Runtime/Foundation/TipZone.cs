using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class TipZone : MonoBehaviour
    {
        [TextArea]public string message;public float seconds=5;
        void OnTriggerEnter2D(Collider2D c){var a=c.GetComponent<ActorMotor>();if(a&&!a.replica)StageSession.Current?.Notice(message,seconds);}
    }
}
