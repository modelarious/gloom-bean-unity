using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    [Serializable]
    public struct InputFrame
    {
        public Vector2 move;
        public bool jump, jumpHeld, attack, run, grab, interact, action, alternate, pound;
        public InputFrame WithoutEdges() { var v = this; v.jump=v.attack=v.grab=v.interact=v.action=v.alternate=v.pound=false; return v; }
    }

    public interface IActorInput { InputFrame Consume(); }
    public interface IActorModifier
    {
        // Return true when the possession takes responsibility for locomotion this tick.
        bool BeforeMovement(ActorMotor actor, InputFrame input, float dt);
        void AfterMovement(ActorMotor actor, InputFrame input, float dt);
    }
    public interface IHittable { void Hit(HitInfo hit); }
    public interface IInteractable { void Interact(ActorMotor actor); }
    public struct HitInfo
    {
        public ActorMotor owner;
        public Vector2 direction;
        public int power;
        public bool downward, projectile;
        public HitInfo(ActorMotor who, Vector2 dir, int strength, bool down=false, bool thrown=false)
        { owner=who; direction=dir; power=strength; downward=down; projectile=thrown; }
    }
    public enum MotionState { Idle, Walk, Run, Air, Crouch, Crawl, Roll, Tackle, RunTackle, AirTackle, PoundWindup, Pound, SuperPound, Swim, SwimDash, Hurt, Carried, Custom, Dead }
    public enum RunPhase { Explore, Returning, Cleared, Failed }
    public enum PickupKind { Coin, Shard, Key, Mercy, Health, Time }

    public static class Layers
    {
        public const int Terrain=8, Moving=9, Prop=10, Actor=11, Enemy=12, Sensor=13, Interior=14, Shadow=15;
        public const int Solids=(1<<Terrain)|(1<<Moving)|(1<<Prop);
    }



    // This component is deliberately independent of any campaign or possession.

    public sealed class ScriptedInput : IActorInput
    {
        public InputFrame frame;
        public bool keepEdges;
        public InputFrame Consume() { var f=frame; if(!keepEdges) frame=frame.WithoutEdges(); return f; }
    }

    [Serializable]
    public sealed class StageDefinition
    {
        public string id, title, subtitle, worldId;
        public int course;
        public int requiredShards;
        public bool requiresKey=true, timed=true, startsReturning, boss, atlas;
        public float escapeSeconds=180;
        public string[] possessions=Array.Empty<string>();
        public string gimmick, turn, mercy, evidence="EXPERIMENTAL";
        public int atlasPage;
    }
    [Serializable]
    public sealed class WorldDefinition { public string id,title; public Color color; public StageDefinition[] levels; public StageDefinition boss; }
    public interface ICampaignSource { WorldDefinition[] Worlds(); void Build(StageDefinition definition, StageBuilder builder); }

    public static class RuntimeEvents
    {
        public static event Action<string,string> Event;
        public static void Emit(string kind, string detail="") { Event?.Invoke(kind,detail); }
        public static IHittable Hittable(Collider2D c)
        {
            foreach(var b in c.GetComponentsInParent<MonoBehaviour>()) if(b is IHittable h)return h;
            return null;
        }
    }
}
