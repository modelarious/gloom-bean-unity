using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class Gate : MonoBehaviour
    {
        public PressurePlate[] plates=Array.Empty<PressurePlate>();
        public bool requireAll=true,latched,opened;
        public float holdSeconds;
        float latchTimer;
        Collider2D shape;SpriteRenderer sprite;
        void Awake(){shape=GetComponent<Collider2D>();sprite=GetComponent<SpriteRenderer>();}
        void FixedUpdate()
        {
            if(plates.Length==0)return;
            bool condition=requireAll;foreach(var p in plates){if(requireAll)condition&=p&&p.Pressed;else condition|=p&&p.Pressed;}
            if(condition)latchTimer=holdSeconds;else latchTimer-=Time.fixedDeltaTime;
            SetOpen(condition||latchTimer>0||latched&&opened);
        }
        public void SetOpen(bool value)
        {
            opened=value;if(!shape)shape=GetComponent<Collider2D>();if(!sprite)sprite=GetComponent<SpriteRenderer>();
            shape.enabled=!value;if(sprite){var c=sprite.color;c.a=value? .12f:1;sprite.color=c;}
        }
    }
}
