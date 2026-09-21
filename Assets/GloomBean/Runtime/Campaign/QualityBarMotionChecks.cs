using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public static class QualityBarMotionChecks
    {
        static string Hash(Color32[] pixels){var bytes=new byte[pixels.Length*4];for(int i=0;i<pixels.Length;i++){bytes[i*4]=pixels[i].r;bytes[i*4+1]=pixels[i].g;bytes[i*4+2]=pixels[i].b;bytes[i*4+3]=pixels[i].a;}using(var h=SHA256.Create())return Convert.ToBase64String(h.ComputeHash(bytes));}
        public static IEnumerator Run(GameRoot game,Action<string,bool,string> check)
        {
            foreach(bool open in new[]{false,true}){
                var texture=Resources.Load<Texture2D>("QualityBar/host_motion_"+(open?"open":"original"));
                check("motion.import."+open,texture&&texture.width==512&&texture.height==1024&&texture.filterMode==FilterMode.Point&&texture.isReadable,"Exact native readable sprite atlas");
                var first=new HashSet<string>();
                foreach(QualityPose pose in Enum.GetValues(typeof(QualityPose))){var unique=new HashSet<string>();bool valid=true;
                    for(int frame=0;frame<8;frame++){var pixels=QualityBarMotion.Pixels(open,(int)pose,frame);valid&=pixels!=null&&pixels.Length==4096&&pixels.Count(c=>c.a>0)>500&&pixels.Count(c=>c.a>0)<4000;if(pixels!=null){unique.Add(Hash(pixels));if(frame==0)first.Add(Hash(pixels));}}
                    check("motion.cells."+open+"."+pose,valid,"All8 authored cells readable and nonempty, not an opaque rectangle");
                    if(pose==QualityPose.Walk||pose==QualityPose.Run||pose==QualityPose.Carry)check("motion.stride."+open+"."+pose,unique.Count==8,"eight independently drawn stride frames="+unique.Count);
                }
                check("motion.distinct-actions."+open,first.Count==16,"Pose silhouettes distinct="+first.Count);yield return null;
            }
            check("motion.identity-distinct",Hash(QualityBarMotion.Pixels(false,0,0))!=Hash(QualityBarMotion.Pixels(true,0,0)),"Cute and permanently corrupted identities never share one body cell");
            var cases=new[]{MotionState.Idle,MotionState.Run,MotionState.Swim,MotionState.SwimDash,MotionState.Tackle,MotionState.PoundWindup,MotionState.Pound,MotionState.SuperPound,MotionState.Hurt,MotionState.Dead};
            var expected=new[]{QualityPose.Idle,QualityPose.Run,QualityPose.Swim,QualityPose.SwimDash,QualityPose.Tackle,QualityPose.Windup,QualityPose.Pound,QualityPose.Pound,QualityPose.Hurt,QualityPose.Dead};
            for(int i=0;i<cases.Length;i++)check("motion.select."+cases[i],QualityBarMotion.Select(cases[i],true,false,false,0,0,0)==expected[i],"Read-only priority selection");
            check("motion.select.rise",QualityBarMotion.Select(MotionState.Air,false,false,false,0,3,0)==QualityPose.Rise,"Vertical ascent");
            check("motion.select.fall",QualityBarMotion.Select(MotionState.Air,false,false,false,0,-3,0)==QualityPose.Fall,"Falling silhouette differs");
            check("motion.select.brake",QualityBarMotion.Select(MotionState.Run,true,false,false,5,0,-1)==QualityPose.Brake,"Reversing stance, no friction change");
            check("motion.select.carry",QualityBarMotion.Select(MotionState.Walk,true,false,true,3,0,1)==QualityPose.Carry,"Hands carry the real actor object");
            check("motion.select.crouch",QualityBarMotion.Select(MotionState.Crawl,true,true,false,1,0,1)==QualityPose.Crouch,"Art does not change clearance");
        }
    }
}
