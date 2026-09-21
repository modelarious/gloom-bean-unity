using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public enum QualityPose { Idle,Tackle,Swim,Pound,Hurt,Walk,Run,Rise,Fall,Windup,Carry,Crouch,Brake,SwimDash,Land,Dead }
    // Read-only presentation selection. No motor, input, transform or collision writes.
    public static class QualityBarMotion
    {
        public const int Frames=8,Poses=16,Cell=64;
        static readonly Dictionary<bool,Color32[]> sheets=new Dictionary<bool,Color32[]>();
        static readonly Dictionary<int,Color32[]> cells=new Dictionary<int,Color32[]>();
        public static QualityPose Select(MotionState state,bool grounded,bool crouched,bool carried,float vx,float vy,float inputX)
        {
            if(state==MotionState.Dead)return QualityPose.Dead;
            if(state==MotionState.Hurt)return QualityPose.Hurt;
            if(state==MotionState.PoundWindup)return QualityPose.Windup;
            if(state==MotionState.Pound||state==MotionState.SuperPound)return QualityPose.Pound;
            if(state==MotionState.SwimDash)return QualityPose.SwimDash;
            if(state==MotionState.Swim)return QualityPose.Swim;
            if(state==MotionState.Tackle||state==MotionState.RunTackle||state==MotionState.AirTackle)return QualityPose.Tackle;
            if(carried)return QualityPose.Carry;
            if(crouched)return QualityPose.Crouch;
            if(!grounded)return vy>1?QualityPose.Rise:QualityPose.Fall;
            if(Mathf.Abs(vx)>2&&inputX*vx<-.2f)return QualityPose.Brake;
            if(state==MotionState.Run)return QualityPose.Run;
            if(Mathf.Abs(vx)>.25f)return QualityPose.Walk;
            return QualityPose.Idle;
        }
        public static Color32[] Pixels(bool open,int pose,int frame)
        {
            pose=Mathf.Clamp(pose,0,Poses-1);frame&=7;int key=(open?1000:0)+pose*8+frame;
            if(cells.TryGetValue(key,out var result))return result;
            if(!sheets.TryGetValue(open,out var source)){
                var texture=Resources.Load<Texture2D>("QualityBar/host_motion_"+(open?"open":"original"));
                if(!texture||texture.width!=Frames*Cell||texture.height!=Poses*Cell||!texture.isReadable)return null;
                source=texture.GetPixels32();sheets[open]=source;
            }
            result=new Color32[Cell*Cell];int top=(Poses-pose-1)*Cell;
            for(int y=0;y<Cell;y++)for(int x=0;x<Cell;x++)result[x+y*Cell]=source[x+frame*Cell+(y+top)*Cell*Frames];
            cells[key]=result;return result;
        }
    }
}
