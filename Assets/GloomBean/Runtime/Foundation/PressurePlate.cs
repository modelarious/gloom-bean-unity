using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class PressurePlate : MonoBehaviour
    {
        public float requiredMass=.3f;
        public bool Pressed {get;private set;}
        public float Mass {get;private set;}
        public event Action<bool> Changed;
        readonly Collider2D[] results=new Collider2D[32];
        readonly HashSet<Rigidbody2D> bodies=new HashSet<Rigidbody2D>();
        void FixedUpdate()
        {
            var box=GetComponent<BoxCollider2D>();int n=Physics2D.OverlapBoxNonAlloc(box.bounds.center,box.bounds.size,0,results);
            bodies.Clear();Mass=0;
            for(int i=0;i<n;i++){var c=results[i];if(!c||c.isTrigger)continue;var rb=c.attachedRigidbody;if(rb&&rb.bodyType!=RigidbodyType2D.Static&&bodies.Add(rb))Mass+=rb.mass;}
            bool value=Mass>=requiredMass;
            if(value!=Pressed){Pressed=value;Changed?.Invoke(value);RuntimeEvents.Emit("plate",name+":"+value);}
            var sprite=GetComponent<SpriteRenderer>();if(sprite)sprite.color=Pressed?new Color(.4f,.92f,.6f):new Color(.93f,.67f,.24f);
        }
    }
}
