using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Opt-in read-only native backbuffer observer for an input-driven verification run.
    // Unlike art fixtures it never repositions/freezes an actor or changes time/camera/input.
    public sealed class GbaActionCapture:MonoBehaviour
    {
        [Serializable] public class Shot {public string file,stage,form,phase,screen,input,state,pose;public int poseFrame;public float time,x,y,cameraSize;public bool motorEnabled,dynamicBody,cameraFollow;}
        [Serializable] class Receipt {public string scope="Native backbuffers during real scripted-input verification; observer changes no simulation/camera/input. Not a human playtest.";public Shot[] shots;}
        GameRoot game;string directory;bool motion;readonly List<Shot> shots=new List<Shot>();
        public static void Install(GameRoot root){if(root.testMode&&(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-action-captures")>=0||Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-motion-captures")>=0)){var capture=root.gameObject.AddComponent<GbaActionCapture>();capture.game=root;capture.motion=Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-motion-captures")>=0;}}
        IEnumerator Start()
        {
            directory=Path.Combine(game.reportDirectory,motion?"MotionAction":"NativeAction");Directory.CreateDirectory(directory);
            while(shots.Count<(motion?240:30)){yield return new WaitForSecondsRealtime(motion?.125f:4f);
                if(!game||!game.Session||game.CurrentScreen!="Play")continue;
                yield return new WaitForEndOfFrame();var session=game.Session;var actor=session.player;if(!actor)continue;
                var camera=Camera.main;var host=actor.GetComponent<HostController>();string file=(shots.Count+1).ToString("00")+"-"+session.definition.id+".png";
                var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(directory,file),image.EncodeToPNG());Destroy(image);
                var view=actor.GetComponent<HostPixelView>();
                shots.Add(new Shot{state=actor.State.ToString(),pose=view?view.DisplayPose.ToString():"Unknown",poseFrame=view?view.DisplayFrame:-1,file=file,stage=session.definition.id,form=host?host.Primary.ToString():"None",phase=session.Phase.ToString(),screen=game.CurrentScreen,input=actor.input==null?"None":actor.input.GetType().Name,time=Time.time,x=actor.transform.position.x,y=actor.transform.position.y,cameraSize=camera?camera.orthographicSize:0,motorEnabled=actor.enabled,dynamicBody=actor.Body.bodyType==RigidbodyType2D.Dynamic,cameraFollow=camera&&camera.GetComponent<FollowCamera>()&&camera.GetComponent<FollowCamera>().enabled});
                File.WriteAllText(Path.Combine(directory,"native-action.json"),JsonUtility.ToJson(new Receipt{shots=shots.ToArray()},true));
            }
        }
    }
}
