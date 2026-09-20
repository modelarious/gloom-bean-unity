using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // These signals describe ordinary colliders, support and motion. A circuit
    // never asks for a particular form's name or accepts an inventory key.
    public sealed class FinalCircuit:MonoBehaviour
    {
        public AtlasBoss boss;public int act=2;public float commitment=.2f;
        public Func<bool> Evaluate;public Action Completed;public bool Latched {get;private set;}
        public float Held {get;private set;}public string circuitId;
        void FixedUpdate()
        {
            if(Latched||!boss||boss.defeated||boss.phase!=act)return;
            Held=Evaluate!=null&&Evaluate()?Held+Time.fixedDeltaTime:0;
            if(Held<commitment)return;Latched=true;RuntimeEvents.Emit("final-circuit",circuitId);Completed?.Invoke();
        }
    }
    public sealed class FinalHeartAnchor:MonoBehaviour
    {
        public Rigidbody2D heart;public bool Released {get;private set;}public string ReleasedBy {get;private set;}
        public float StartHeight {get;private set;}public float LowestHeight {get;private set;}
        public readonly List<LineRenderer> cables=new List<LineRenderer>();
        void Start(){StartHeight=LowestHeight=heart?heart.position.y:0;}
        public void Release(string circuit)
        {
            if(Released||!heart)return;Released=true;ReleasedBy=circuit;
            heart.bodyType=RigidbodyType2D.Dynamic;heart.gravityScale=1.8f;
            foreach(var cable in cables)if(cable)cable.enabled=false;
            RuntimeEvents.Emit("heart-cable-released",circuit);
        }
        void FixedUpdate(){if(heart)LowestHeight=Mathf.Min(LowestHeight,heart.position.y);}
    }
    [DefaultExecutionOrder(-90)]
    public sealed class FinalCargoHoist:MonoBehaviour
    {
        public Vector2 lower,upper;public float speed=2.4f;
        public EdibleChunk Cargo {get;private set;}public float RiderMass {get;private set;}
        public bool Balanced {get;private set;}public bool Delivered {get;private set;}
        public Rigidbody2D counterweight;Rigidbody2D body;Vector2 counterOrigin;
        void Awake(){body=GetComponent<Rigidbody2D>();}
        void Start(){if(counterweight)counterOrigin=counterweight.position;}
        void FixedUpdate()
        {
            if(!body)return;
            if(!Cargo)foreach(var c in Physics2D.OverlapBoxAll(body.position+Vector2.up*.7f,new Vector2(3.8f,1.2f),0,Layers.Solids))
            {var tile=c.GetComponent<EdibleChunk>();if(tile&&tile.gameObject.activeInHierarchy&&Mathf.Abs(tile.transform.position.y-body.position.y-.7f)<.25f){Cargo=tile;tile.Remember();tile.gameObject.layer=Layers.Moving;break;}}
            bool aboard=Cargo&&Cargo.gameObject.activeInHierarchy&&Mathf.Abs(Cargo.transform.position.x-body.position.x)<1&&Mathf.Abs(Cargo.transform.position.y-body.position.y-.7f)<.3f;
            RiderMass=0;
            if(aboard){var shape=Cargo.GetComponent<Collider2D>();var ids=new HashSet<Rigidbody2D>();
                foreach(var c in Physics2D.OverlapBoxAll((Vector2)shape.bounds.center+Vector2.up*(shape.bounds.extents.y+.6f),new Vector2(shape.bounds.size.x,1.2f),0,1<<Layers.Actor))
                {var actor=c.GetComponentInParent<ActorMotor>();if(actor&&actor.Grounded&&actor.GroundCollider==shape&&ids.Add(actor.Body))RiderMass+=actor.Body.mass;}}
            Balanced=aboard&&RiderMass>.65f&&RiderMass<.86f;
            if(!Delivered&&Balanced){Vector2 delta=Vector2.MoveTowards(body.position,upper,speed*Time.fixedDeltaTime)-body.position;
                body.position+=delta;Cargo.transform.position+=(Vector3)delta;
                if(counterweight)counterweight.position=counterOrigin-Vector2.up*(body.position.y-lower.y);
                if(Vector2.Distance(body.position,upper)<.03f){Delivered=true;RuntimeEvents.Emit("conserved-cargo-delivered",Cargo.stableId);}}
        }
    }
    public sealed class FinalChoiceLedger:MonoBehaviour
    {
        public HostKind Chosen {get;private set;}public AtlasBoss boss;public HostEmbargo embargo;
        public int Thefts {get;private set;}public int HealthBeforeTheft {get;private set;}public int HealthAfterTheft {get;private set;}
        public void Observe(HostKind k){if(boss&&boss.phase==0&&(k==HostKind.Echo||k==HostKind.Wax||k==HostKind.Parallax))Chosen=k;}
        public void Steal(HostController host){HealthBeforeTheft=host.GetComponent<ActorMotor>().Health;embargo.Steal(host,Chosen);HealthAfterTheft=host.GetComponent<ActorMotor>().Health;Thefts++;}
    }
}
