using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class TurnObject : MonoBehaviour
    {
        public bool solidBefore=true,solidAfter=false;
        public Vector2 translation;
        public float rotation;
        public float duration=1.3f;
        bool turning;float t;Vector3 origin;Quaternion oldRotation;
        void Start(){origin=transform.position;oldRotation=transform.rotation;StageSession.Current.Turned+=Begin;SetCollision(solidBefore);}
        void OnDestroy(){if(StageSession.Current)StageSession.Current.Turned-=Begin;}
        void Begin(){turning=true;SetCollision(solidAfter);}
        void SetCollision(bool on){foreach(var c in GetComponents<Collider2D>())c.enabled=on;var r=GetComponent<SpriteRenderer>();if(r){var col=r.color;col.a=on?1:.2f;r.color=col;}}
        void Update(){if(!turning)return;t=Mathf.Min(1,t+Time.deltaTime/Mathf.Max(.1f,duration));transform.position=origin+(Vector3)translation*Mathf.SmoothStep(0,1,t);transform.rotation=oldRotation*Quaternion.Euler(0,0,rotation*Mathf.SmoothStep(0,1,t));}
    }
}
