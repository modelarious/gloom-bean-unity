using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class StageSession : MonoBehaviour
    {
        public static StageSession Current {get;private set;}
        public StageDefinition definition;
        public SaveStore save;
        public ActorMotor player;
        public FollowCamera Camera;
        public Vector2 checkpoint;
        public RunPhase Phase {get;private set;}
        public int Coins {get;private set;}
        public int Shards {get;private set;}
        public bool HasKey {get;private set;}
        public float Remaining {get;private set;}
        public string Message {get;private set;}
        public event Action Turned;
        public event Action Completed;
        public event Action Failed;
        public readonly HashSet<string> Mercies=new HashSet<string>();
        public readonly HashSet<string> Collected=new HashSet<string>();
        public string FailureReason {get;private set;}
        float noticeUntil;
        void Awake(){Current=this;}
        public void Configure(StageDefinition d,SaveStore store){definition=d;save=store;Remaining=d.escapeSeconds;Phase=RunPhase.Explore;}
        void Start(){if(player){player.Died+=PlayerDied;checkpoint=player.Body.position;}}
        void OnDestroy(){if(player)player.Died-=PlayerDied;if(Current==this)Current=null;}
        void Update()
        {
            if(Time.time>noticeUntil)Message=null;
            if(Phase==RunPhase.Returning&&definition.timed){Remaining-=Time.deltaTime;if(Remaining<=0){Remaining=0;Fail("The return route closed.");}}
        }
        public void TickClock(float seconds)
        {if(Phase==RunPhase.Returning&&definition.timed){Remaining=Mathf.Max(0,Remaining-seconds);if(Remaining==0)Fail("The return route closed.");}}
        public void Collect(Pickup item,ActorMotor actor)
        {
            if(!Collected.Add(item.stableId))return;
            switch(item.kind)
            {
                case PickupKind.Coin:Coins+=item.amount;break;
                case PickupKind.Shard:Shards+=item.amount;Notice("Seal fragment recovered",2);break;
                case PickupKind.Key:
                    HasKey=true;var f=PrimitiveArt.Shape("Keyling follower",transform,item.transform.position,new Vector2(.5f,.5f),new Color(1,.8f,.25f),PrimitiveArt.Icon.Key,12).AddComponent<KeyFollower>();f.target=actor;Notice("The Keyling follows you. Return with it.",3);break;
                case PickupKind.Mercy:Mercies.Add(item.stableId);Notice("A small mercy. It will be kept when you clear the level.",4);break;
                case PickupKind.Health:actor.Heal(item.amount);break;
                case PickupKind.Time:if(Phase==RunPhase.Returning)Remaining+=item.amount;break;
            }
            RuntimeEvents.Emit("collect",item.stableId);
        }
        public void Turn()
        {
            if(Phase!=RunPhase.Explore)return;Phase=RunPhase.Returning;Remaining=definition.escapeSeconds;
            Turned?.Invoke();RuntimeEvents.Emit("turn",definition.id);Notice(definition.timed?"THE TURN — return to the entrance!":"THE TURN — familiar ground, different rules.",4);
        }
        public bool TryClear()
        {
            if(Phase!=RunPhase.Returning)return false;
            if(definition.requiresKey&&!HasKey){Notice("Find the Keyling before leaving.");return false;}
            if(Shards<definition.requiredShards){Notice("Missing seal fragments: "+Shards+" / "+definition.requiredShards);return false;}
            Phase=RunPhase.Cleared;save?.CommitRun(definition,Coins,Mercies);RuntimeEvents.Emit("clear",definition.id);Completed?.Invoke();return true;
        }
        public void BossClear(){if(Phase==RunPhase.Cleared)return;Phase=RunPhase.Returning;HasKey=true;Shards=definition.requiredShards;TryClear();}
        void PlayerDied(){Fail("The Host was overwhelmed.");}
        public void Fail(string why){if(Phase==RunPhase.Cleared||Phase==RunPhase.Failed)return;FailureReason=why;Phase=RunPhase.Failed;RuntimeEvents.Emit("fail",why);Failed?.Invoke();}
        public void Notice(string text,float seconds=3){Message=text;noticeUntil=Time.time+seconds;}
    }
}
