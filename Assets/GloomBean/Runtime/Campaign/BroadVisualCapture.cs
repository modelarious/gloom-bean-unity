using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // All-stage stratified art capture. Stage/floor quantiles, not authored hero coordinates.
    // Staging is isolated to this opt-in test. These images are never claimed as route proofs.
    public sealed class BroadVisualCapture:MonoBehaviour
    {
        [Serializable] public class Shot{public string id,stage,title,anchor,form;public int stratum;public bool returned,holdout;public float x,y,cx,cy,size;public int colliders,renderers,profile,eligible,applied,visualColliders,qualityEligible,qualityApplied,qualityColliders;}
        [Serializable] public class Receipt{public string status,scope;public Shot[] shots;public string[] errors;}
        GameRoot game;string dir;List<Shot> shots=new List<Shot>();List<string> errors=new List<string>();
        public void Begin(GameRoot root){game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);Application.logMessageReceived+=Error;StartCoroutine(Run());}
        void Error(string text,string trace,LogType kind){if(kind==LogType.Error||kind==LogType.Exception)errors.Add(text);}
        IEnumerator Run(){QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
            game.SelectSource(1);var stages=game.AvailableWorlds.SelectMany(w=>w.levels.Concat(new[]{w.boss})).ToArray();
            foreach(var stage in stages)for(int sample=0;sample<5;sample++){
                UnityEngine.Random.InitState(28001+stage.course*73+sample);yield return game.Load(stage,true);
                var session=game.Session;var actor=session.player;var host=actor.GetComponent<HostController>();var camera=Camera.main;var follow=camera.GetComponent<FollowCamera>();follow.enabled=false;
                var candidates=session.GetComponentsInChildren<BoxCollider2D>(true).Where(c=>!c.isTrigger&&c.enabled&&c.bounds.size.x>=2&&c.bounds.size.y<=3&&!c.GetComponentInParent<ActorMotor>()&&!c.GetComponentInParent<CarryableEnemy>()).OrderBy(c=>c.bounds.center.x).ThenBy(c=>c.bounds.center.y).ToArray();
                if(candidates.Length==0){errors.Add("No support anchor for "+stage.id);continue;}
                int index=Mathf.Clamp(Mathf.FloorToInt(candidates.Length*new[]{.22f,.63f,.88f,.43f,.37f}[sample]),0,candidates.Length-1);var anchor=candidates[index];
                // Old ten fixtures are explicitly excluded; nearest alternate support is used deterministically.
                Vector2 pos=anchor.bounds.center;float px=pos.x,py=anchor.bounds.max.y+.85f;float cx=px+1.5f,cy=py+1.8f;
                var old=VisualReviewV6.Shots.FirstOrDefault(s=>s.stage==stage.id);
                if(old!=null&&Vector2.Distance(new Vector2(cx,cy),new Vector2(old.cx,old.cy))<6){index=(index+Mathf.Max(1,candidates.Length/3))%candidates.Length;anchor=candidates[index];px=anchor.bounds.center.x;py=anchor.bounds.max.y+.85f;cx=px+1.5f;cy=py+1.8f;}
                if(old!=null&&Vector2.Distance(new Vector2(cx,cy),new Vector2(old.cx,old.cy))<5){cx-=6;px-=Mathf.Min(2,anchor.bounds.extents.x*.5f);}
                if(sample==3&&stage.boss){var boss=session.GetComponentInChildren<AtlasBoss>();if(boss&&boss.body){var sr=boss.body.GetComponent<SpriteRenderer>();if(sr){cx=sr.bounds.center.x;cy=sr.bounds.center.y;px=cx-3;py=Mathf.Max(1,cy-3);}}}
                // Quantiles can land on the same long floor. Keep old comparison cameras,
                // but require the LAST validation camera to be genuinely unseen in this stage.
                if(sample==4&&shots.Any(s=>s.stage==stage.id&&Vector2.Distance(new Vector2(s.cx,s.cy),new Vector2(cx,cy))<3f)){
                    Vector2 original=new Vector2(cx,cy);bool chosen=false;
                    foreach(var delta in new[]{new Vector2(6,0),new Vector2(-6,0),new Vector2(0,4),new Vector2(3,3)}){var point=original+delta;
                        if(shots.All(s=>s.stage!=stage.id||Vector2.Distance(new Vector2(s.cx,s.cy),point)>=3f)&&(old==null||Vector2.Distance(point,new Vector2(old.cx,old.cy))>=5f)){cx=point.x;cy=point.y;chosen=true;break;}}
                    if(!chosen)errors.Add("Fresh validation camera could not be selected: "+stage.id);
                }
                string form="None";if(stage.possessions!=null&&stage.possessions.Length>0){form=stage.possessions[sample%stage.possessions.Length];host.Acquire((HostKind)Enum.Parse(typeof(HostKind),form));}
                bool returned=sample==1&&!stage.boss;if(returned)session.Turn();
                actor.GetComponent<HumanInput>().disabled=true;actor.input=new ScriptedInput();actor.enabled=false;actor.Body.bodyType=RigidbodyType2D.Kinematic;actor.Body.linearVelocity=Vector2.zero;
                actor.Reposition(new Vector2(px,py));camera.transform.position=new Vector3(cx,cy,-10);camera.orthographicSize=stage.boss?7:5.5f;
                for(int f=0;f<30;f++)yield return new WaitForFixedUpdate();
                actor.Body.position=new Vector2(px,py);actor.transform.position=new Vector3(px,py,0);Time.timeScale=0;session.Notice("",0);yield return null;yield return new WaitForEndOfFrame();
                string id=stage.id+"-"+(sample==0?"A-explore":sample==1?"B-altered":sample==2?"C-holdout":sample==3?"D-validation":"E-final-check");var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,id+".png"),image.EncodeToPNG());Destroy(image);
                var quality=session.GetComponent<QualityBarWorld>();if(quality)quality.Scan();var dressing=session.GetComponent<BroadWorldDressing>();if(dressing)dressing.Scan();var shape=session.GetComponentsInChildren<Collider2D>(true);var sprites=session.GetComponentsInChildren<SpriteRenderer>(true);
                shots.Add(new Shot{id=id,stage=stage.id,title=stage.title,anchor=anchor.name,form=form,stratum=sample,returned=returned,holdout=sample>=2,x=px,y=py,cx=cx,cy=cy,size=camera.orthographicSize,colliders=shape.Length,renderers=sprites.Length,profile=BroadArt.Profile(session),eligible=dressing?dressing.EligibleCount:0,applied=dressing?dressing.AppliedCount:0,visualColliders=dressing?dressing.VisualColliders:0,qualityEligible=quality?quality.EligibleCount:0,qualityApplied=quality?quality.AppliedCount:0,qualityColliders=quality?quality.AddedColliders:0});
                File.WriteAllLines(Path.Combine(dir,id+"-geometry.txt"),shape.Select(c=>c.GetType().Name+"|"+c.name+"|"+c.isTrigger+"|"+c.gameObject.layer+"|"+c.bounds.center+"|"+c.bounds.size).OrderBy(s=>s));
                File.WriteAllLines(Path.Combine(dir,id+"-objects.txt"),sprites.Where(s=>s.enabled&&!s.forceRenderingOff&&s.sprite).Select(s=>s.name+"|"+s.sprite.name+"|"+s.bounds.center+"|"+s.bounds.size+"|order="+s.sortingOrder));
                Time.timeScale=1;follow.enabled=true;
            }
            File.WriteAllText(Path.Combine(dir,"broad-result.json"),JsonUtility.ToJson(new Receipt{status=errors.Count==0&&shots.Count==125?"PASS":"FAIL",scope="125 NEW native images across all25stages. ABC remain; D full-body boss validation found a heart classification issue. E is a fresh final generalization stratum, no earlier before-image claimed. Staged camera/body/form; not reachable-route proof. No old frame reuse.",shots=shots.ToArray(),errors=errors.ToArray()},true));
            Application.logMessageReceived-=Error;Application.Quit(errors.Count==0?0:1);
        }
    }
}
