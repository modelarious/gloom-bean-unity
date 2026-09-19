using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // This suite starts at the actual entrance and feeds only the production input contract.
    // No teleportation, direct pickups, invulnerability, unlocked gates or direct Turn/Clear calls.
    public sealed class RouteVerification:MonoBehaviour
    {
        GameRoot game;ActorMotor actor;ScriptedInput input;StageSession session;string dir;
        readonly List<string> observations=new List<string>();int failures;bool stopped;
        public void Begin(GameRoot root){game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);Application.logMessageReceived+=Log;StartCoroutine(Run());}
        void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception){failures++;observations.Add("ERROR "+m);}}
        void Check(string name,bool pass){observations.Add((pass?"PASS ":"FAIL ")+name+" | "+(actor?actor.Body.position.ToString():""));if(!pass)failures++;Debug.Log(observations[observations.Count-1]);}
        IEnumerator Walk(float x,float seconds=12)
        {
            float deadline=Time.realtimeSinceStartup+seconds;
            while(Time.realtimeSinceStartup<deadline&&session.Phase!=RunPhase.Cleared&&session.Phase!=RunPhase.Failed)
            {
                float dx=x-actor.Body.position.x;if(Mathf.Abs(dx)<.28f)break;
                input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx*1.8f,-1,1),0)};yield return null;
            }
            input.frame=default;yield return null;
            bool arrived=Mathf.Abs(actor.Body.position.x-x)<.5f||session.Phase==RunPhase.Cleared;
            Check("walk to "+x,arrived);stopped|=!arrived;
        }
        IEnumerator Jump(float x,float floor)
        {
            float deadline=Time.realtimeSinceStartup+5;bool launched=false;float peak=actor.Feet.y;float sample=0;
            while(Time.realtimeSinceStartup<deadline&&session.Phase!=RunPhase.Failed)
            {
                float dx=x-actor.Body.position.x;peak=Mathf.Max(peak,actor.Feet.y);
                if(Time.realtimeSinceStartup>sample){sample=Time.realtimeSinceStartup+.1f;observations.Add("TRACE jump x="+actor.Body.position.x+" feet="+actor.Feet.y+" vy="+actor.Body.linearVelocity.y+" ground="+actor.Grounded+" lastJump="+actor.LastInput.jump+" lastHeld="+actor.LastInput.jumpHeld);}
                bool jump=!launched&&actor.Grounded;if(jump)launched=true;
                // Brake before the centre of the landing; no position correction is used.
                float command=Mathf.Clamp(dx*.8f-actor.Body.linearVelocity.x*.08f,-1,1);
                input.frame=new InputFrame{move=new Vector2(command,0),jump=jump,jumpHeld=true};
                if(launched&&!jump&&actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.22f&&Mathf.Abs(dx)<1.15f)break;
                // A render frame can precede the next physics tick. Do not overwrite a queued jump edge.
                if(jump)yield return new WaitForFixedUpdate();else yield return null;
            }
            input.frame=default;yield return null;
            bool arrived=Mathf.Abs(actor.Feet.y-floor)<.28f&&actor.Grounded&&Mathf.Abs(actor.Body.position.x-x)<1.3f;
            Check("jump to "+x+" @ "+floor+" peak="+peak,arrived);stopped|=!arrived;
        }
        IEnumerator Run()
        {
            game.SelectSource(1);yield return game.Load(game.AvailableWorlds[0].levels[0],true);
            session=game.Session;actor=session.player;actor.GetComponent<HumanInput>().disabled=true;input=new ScriptedInput();actor.input=input;
            yield return new WaitForSeconds(.3f);
            yield return Walk(6.7f);yield return Jump(9.5f,1.6f);
            if(!stopped){yield return Walk(11.6f);yield return Jump(14.9f,2.6f);}
            if(!stopped){yield return Walk(20.4f);yield return Walk(22);yield return Jump(24.7f,1.5f);}
            if(!stopped){yield return Walk(43);Check("world contact permanently corrupts run",game.IsCorrupted);Check("source contact acquires Echo",actor.GetComponent<HostController>().Has(HostKind.Echo));}
            if(!stopped){yield return Walk(58.2f,18);Check("Echo reopens real forward door",actor.Body.position.x>56);}
            if(!stopped){yield return Jump(61,-2.2f);}
            float[] xs={66,71,76,81,86};float[] ys={-.7f,.8f,2.3f,3.8f,5.3f};
            for(int i=0;i<xs.Length&&!stopped;i++){yield return Walk(xs[i]-3.5f);yield return Jump(xs[i]-.4f,ys[i]);}
            if(!stopped)
            {
                Check("Keyling collected by actual overlap",session.HasKey);yield return Walk(94.9f);
                input.frame=new InputFrame{interact=true};yield return new WaitForSeconds(.25f);input.frame=default;
                Check("interact pulls World Nail",session.Phase==RunPhase.Returning);
                FoundationVerification.Capture(session.Camera.GetComponent<UnityEngine.Camera>(),Path.Combine(dir,"Sunday-at-turn.png"));
                yield return Walk(27,25);yield return Walk(2,12);
                float timeout=Time.realtimeSinceStartup+4;while(session.Phase!=RunPhase.Cleared&&Time.realtimeSinceStartup<timeout)yield return null;
                Check("Sunday Best entrance-to-exit with production inputs",session.Phase==RunPhase.Cleared);
                Check("Mercy never required for ordinary clear",session.Mercies.Count==0);
            }
            else Check("Sunday Best input-only playthrough",false);
            if(actor&&session.Camera)FoundationVerification.Capture(session.Camera.GetComponent<UnityEngine.Camera>(),Path.Combine(dir,"last-frame.png"));
            File.WriteAllLines(Path.Combine(dir,"route-observations.txt"),observations);
            File.WriteAllText(Path.Combine(dir,"route-result.json"),"{\"failed\":"+failures+",\"checks\":"+observations.Count+",\"scope\":\"Input-only Sunday Best critical path; no whole-campaign claim\"}");
            Application.logMessageReceived-=Log;Application.Quit(failures==0?0:1);
        }
    }
}
