using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class Checkpoint : MonoBehaviour
    {
        public bool activated;
        void OnTriggerEnter2D(Collider2D c){var a=c.GetComponent<ActorMotor>();if(a&&!a.replica){StageSession.Current.checkpoint=(Vector2)transform.position+Vector2.up;activated=true;}}
    }
}
