using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public enum QualityPatrolPose { Idle,Walk,Stunned,Carried,Thrown }
    public enum QualityBossPose { Watch,Commit,Strain }
    // Immutable authored cells. State comes from the actor, not a review camera.
    public static class QualityBarActorArt
    {
        static readonly Dictionary<string,Sprite> cells=new Dictionary<string,Sprite>();
        public static QualityPatrolPose PatrolPose(EnemyState state,float speed)
        {
            if(state==EnemyState.Carried)return QualityPatrolPose.Carried;
            if(state==EnemyState.Thrown)return QualityPatrolPose.Thrown;
            if(state==EnemyState.Stunned)return QualityPatrolPose.Stunned;
            return Mathf.Abs(speed)>.08f?QualityPatrolPose.Walk:QualityPatrolPose.Idle;
        }
        public static int Frame(float travel)=>((int)Mathf.Floor(travel)%8+8)%8;
        static Sprite Cell(string resource,int size,int rows,int row,int frame)
        {
            frame&=7;row=Mathf.Clamp(row,0,rows-1);string key=resource+":"+row+":"+frame;
            if(cells.TryGetValue(key,out var cached))return cached;
            var texture=Resources.Load<Texture2D>("QualityBar/"+resource);
            if(!texture||texture.width!=size*8||texture.height!=size*rows)return null;
            var sprite=Sprite.Create(texture,new Rect(frame*size,(rows-row-1)*size,size,size),Vector2.one*.5f,size,0,SpriteMeshType.FullRect);
            sprite.name="Q11 authored "+key;cells[key]=sprite;return sprite;
        }
        public static Sprite Patrol(int world,QualityPatrolPose pose,int frame,bool armor)=>Cell("actor_patrol_"+Mathf.Clamp(world,1,5)+"_"+(armor?1:0),32,5,(int)pose,frame);
        public static Sprite Boss(int world,int phase,QualityBossPose pose,int frame)=>Cell("actor_boss_"+Mathf.Clamp(world,1,5),128,9,Mathf.Clamp(phase,0,2)*3+(int)pose,frame);
    }
}
