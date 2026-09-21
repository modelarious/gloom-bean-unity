using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Native render fixtures, NOT input-only gameplay or reachability acceptance.
    public sealed class VisualReviewV6 : MonoBehaviour
    {
        [Serializable] public class Shot {public string id,stage,form,reference;public float x,y,cx,cy,size;public bool turn;public Shot(string i,string s,float px,float py,float ax,float ay,string f,string r,float z=7,bool t=false){id=i;stage=s;x=px;y=py;cx=ax;cy=ay;form=f;reference=r;size=z;turn=t;}}
        [Serializable] class Receipt {public string status,scope,unity;public int images;public Shot[] shots;public string[] errors;}
        readonly List<string> errors=new List<string>();GameRoot game;string dir;
        public static readonly Shot[] Shots={
            new Shot("01-cute-parade","GB-L01",9,2.5f,12,4,"None","palm"),
            new Shot("02-puppet-machinery","GB-L03",37,4,36,7,"Marionette","factory"),
            new Shot("03-orchard","GB-L05",41,4,42,7,"Wax","wildflower"),
            new Shot("04-mirror-tenement","GB-L09",12,1,19,5,"Mirror","crescent"),
            new Shot("05-falling-witnesses","GB-L13",28,9,28,11,"Censer","fiery"),
            new Shot("06-cathedral","GB-L16",42,16,42,19,"Stitch","hotel"),
            new Shot("07-iron-choir","GB-L17",29,5,29,7,"Lodestone","arabian"),
            new Shot("08-scripture","GB-L19",29,10,30,10,"Ink","pinball"),
            new Shot("09-mass-boss","GB-B4",42,16,43,18,"Coffin","diva",9),
            new Shot("10-final-host","GB-B5",16,1,24,7,"None","diva",9)
        };
        public void Begin(GameRoot root){game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);Application.logMessageReceived+=Error;StartCoroutine(Run());}
        void Error(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error)errors.Add(message);}
        IEnumerator Run()
        {
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;UnityEngine.Random.InitState(601);
            int count=0;
            foreach(var shot in Shots)
            {
                game.SelectSource(1);var stage=game.AvailableWorlds.SelectMany(w=>w.levels.Concat(new[]{w.boss})).Single(s=>s.id==shot.stage);
                yield return game.Load(stage,true);var session=game.Session;var actor=session.player;var host=actor.GetComponent<HostController>();
                actor.GetComponent<HumanInput>().disabled=true;actor.input=new ScriptedInput();
                var cam=UnityEngine.Camera.main;var follow=cam.GetComponent<FollowCamera>();follow.enabled=false;
                cam.transform.position=new Vector3(shot.cx,shot.cy,-10);cam.orthographicSize=shot.size;cam.clearFlags=CameraClearFlags.SolidColor;
                actor.Reposition(new Vector2(shot.x,shot.y));
                if(shot.form!="None"){var kind=(HostKind)Enum.Parse(typeof(HostKind),shot.form);host.Acquire(kind);}
                if(shot.turn)session.Turn();
                // Freeze only the review actor so every rendition shows the same silhouette location.
                // This branch is reachable solely via the explicit visual-fixture CLI flag.
                actor.enabled=false;actor.Body.bodyType=RigidbodyType2D.Kinematic;actor.Body.linearVelocity=Vector2.zero;
                for(int f=0;f<45;f++)yield return new WaitForFixedUpdate();
                actor.Body.position=new Vector2(shot.x,shot.y);actor.transform.position=new Vector3(shot.x,shot.y,0);actor.Body.linearVelocity=Vector2.zero;
                Time.timeScale=0;session.Notice("",0);yield return null;yield return new WaitForEndOfFrame();
                var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,shot.id+".png"),image.EncodeToPNG());Destroy(image);count++;
                var shapes=session.GetComponentsInChildren<Collider2D>(true);File.WriteAllText(Path.Combine(dir,shot.id+"-geometry.txt"),string.Join("\n",shapes.Select(c=>c.GetType().Name+"|"+c.name+"|"+c.isTrigger+"|"+c.gameObject.layer).OrderBy(s=>s)));
                Time.timeScale=1;follow.enabled=true;
            }
            File.WriteAllText(Path.Combine(dir,"visual-result.json"),JsonUtility.ToJson(new Receipt{status=errors.Count==0&&count==Shots.Length?"PASS":"FAIL",scope="Native fixed-camera art fixtures. Bodies/forms are staged; not a route-playthrough claim. No campaign progress is awarded.",unity=Application.unityVersion,images=count,shots=Shots,errors=errors.ToArray()},true));
            Application.logMessageReceived-=Error;Application.Quit(errors.Count==0?0:1);
        }
    }
}
