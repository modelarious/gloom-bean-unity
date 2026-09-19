using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class KeyFollower : MonoBehaviour
    {
        public ActorMotor target;
        readonly Queue<Vector3> trail=new Queue<Vector3>();
        void FixedUpdate()
        {if(!target)return;trail.Enqueue(target.transform.position+new Vector3(0,.4f,0));if(trail.Count>20)transform.position=Vector3.Lerp(transform.position,trail.Dequeue(),.4f);}
    }
}
