using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A state-reading, input-only witness. It never grants a tenant, edits terrain,
    // changes physical state, or awards progress. Not a human-readability certificate.
    public sealed class EmpyreanVerification:MonoBehaviour
    {
        GameRoot game;StageSession session;ActorMotor actor;HostController host;ScriptedInput input;
        string dir;readonly List<string> log=new List<string>();int failures,checks,ticks;bool stopped,finished;float began;
        bool Practice=>Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-practice-witness")>=0;
        bool Live=>session&&session.Phase!=RunPhase.Failed&&session.Phase!=RunPhase.Cleared;
        string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        public void Begin(GameRoot root){game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);began=Time.realtimeSinceStartup;Application.logMessageReceived+=Error;StartCoroutine(Run());}
        void Error(string m,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception){failures++;Note("ERROR "+m);Finish();}}
        void Update(){if(!finished&&Time.realtimeSinceStartup-began>510){failures++;Note("FAIL witness watchdog");Finish();}}
        void Note(string s){log.Add(s);File.WriteAllLines(Path.Combine(dir,"empyrean-observations.txt"),log);}
        void Snapshot(string id){if(session&&session.Camera)FoundationVerification.Capture(session.Camera.GetComponent<UnityEngine.Camera>(),Path.Combine(dir,session.definition.id+"-"+id+".png"));}
        void Check(string name,bool ok){checks++;Note((ok?"PASS ":"FAIL ")+session.definition.id+" "+name+" body="+actor.Body.position+" velocity="+actor.Body.linearVelocity+" feet="+actor.Feet.y+" hp="+actor.Health);if(!ok){failures++;stopped=true;Snapshot("first-failure");}}
        IEnumerator Tick(){int before=ticks;while(Live&&ticks==before)yield return null;}
        IEnumerator Pause(float seconds){input.frame=default;float end=Time.time+seconds;while(Live&&Time.time<end)yield return Tick();}
        IEnumerator Press(InputFrame f){if(stopped||!Live)yield break;if(f.action)f.actionHeld=true;input.frame=f;yield return Tick();input.frame=default;yield return Tick();}
        IEnumerator Wait(string name,Func<bool> done,float seconds,Func<InputFrame> command=null){if(stopped||!Live)yield break;float end=Time.time+seconds;while(Live&&Time.time<end&&!done()){input.frame=command==null?default:command();yield return Tick();}input.frame=default;Check(name,done());}
        IEnumerator Walk(float x,bool run=false){if(stopped||!Live)yield break;float end=Time.time+16;while(Live&&Time.time<end&&Mathf.Abs(x-actor.Body.position.x)>.22f){input.frame=new InputFrame{move=new Vector2(Mathf.Clamp((x-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.1f,-1,1),0),run=run};yield return Tick();}input.frame=default;Check("walk "+x,session.Phase==RunPhase.Cleared||Mathf.Abs(x-actor.Body.position.x)<.5f);}
        IEnumerator Jump(float x,float floor){if(stopped||!Live)yield break;bool sent=false;float end=Time.time+6;
            while(Live&&Time.time<end){float dx=x-actor.Body.position.x;bool edge=!sent&&actor.Grounded;if(edge)sent=true;input.frame=new InputFrame{jump=edge,jumpHeld=true,move=new Vector2(Mathf.Clamp(dx-actor.Body.linearVelocity.x*.15f,-1,1),0)};
                if(sent&&!edge&&actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(dx)<.3f)break;yield return Tick();}
            input.frame=default;Check("jump to "+x+" / "+floor,actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(x-actor.Body.position.x)<.5f);
        }
        IEnumerator MagnetTo(Vector2 target,float timeout=18,bool landing=true)
        {
            if(stopped||!Live)yield break;var magnet=host.Form<LodestoneForm>();Check("Lodestone acquired by actual source",magnet!=null);if(stopped)yield break;
            float end=Time.time+timeout,nextToggle=0,lastJump=-1,trace=0;int reversals=0;
            while(Live&&Time.time<end){Vector2 error=target-actor.Body.position;var velocity=actor.Body.linearVelocity;
                if(error.magnitude<(landing?.8f:1.1f)&&(!landing||actor.Grounded&&Mathf.Abs(velocity.x)<1.5f))break;
                Vector2 northForce=Vector2.zero;
                foreach(var metal in MagneticBody.All)if(metal&&metal.enabled&&Vector2.Distance(actor.Body.position,metal.transform.position)<magnet.range)
                    northForce+=LodestoneForm.Force(actor.Body.position,metal.transform.position,1,metal.polarity,metal.strength);
                Vector2 desired=new Vector2(error.x*6-velocity.x*3,error.y*9-velocity.y*4+actor.tuning.gravity);
                int pole=Vector2.Dot(northForce,desired)>=0?1:-1;bool reverse=pole!=magnet.Polarity&&Time.time>=nextToggle;
                if(reverse){nextToggle=Time.time+.18f;reversals++;}
                bool jump=actor.Grounded&&error.y>.35f&&Time.time-lastJump>.5f;if(jump)lastJump=Time.time;
                input.frame=new InputFrame{action=reverse,actionHeld=reverse,jump=jump,jumpHeld=true,move=new Vector2(Mathf.Clamp(error.x*2-velocity.x*.6f,-1,1),0)};
                yield return Tick();if(Time.time>trace){trace=Time.time+.5f;Note("MAGNET t="+Time.time+" body="+actor.Body.position+" velocity="+actor.Body.linearVelocity+" pole="+magnet.Polarity+" target="+target+" reversals="+reversals);}
            }
            input.frame=default;Check((landing?"magnetic landing at ":"magnetic transit through ")+target,(!landing||actor.Grounded)&&Vector2.Distance(actor.Body.position,target)<(landing?.9f:1.2f));Snapshot("magnet-"+target.x);
        }
        IEnumerator Halos(bool secret)
        {
            yield return Walk(8);Check("iron halo source",host.Has(HostKind.Lodestone));yield return Walk(11.8f);yield return MagnetTo(new Vector2(16,2.75f));yield return Walk(17.8f);
            yield return MagnetTo(new Vector2(21,7),12,false);
            yield return MagnetTo(new Vector2(29,4.75f));yield return Walk(30.2f);
            yield return MagnetTo(new Vector2(35,10),12,false);yield return MagnetTo(new Vector2(41,10),12,false);yield return MagnetTo(new Vector2(46,6.75f));
            yield return Walk(47.4f);yield return MagnetTo(new Vector2(53,12),12,false);yield return MagnetTo(new Vector2(60,12),12,false);yield return MagnetTo(new Vector2(65,8.75f));
            yield return Walk(66.4f);yield return MagnetTo(new Vector2(72,14),12,false);yield return MagnetTo(new Vector2(79,14),12,false);yield return MagnetTo(new Vector2(83,10.75f));
            yield return Walk(84.4f);yield return MagnetTo(new Vector2(90,16),12,false);yield return MagnetTo(new Vector2(96,16),12,false);yield return MagnetTo(new Vector2(99,12.75f));
            if(stopped)yield break;
            Check("Keyling reached through magnetic traversal",session.HasKey);
            if(secret){yield return MagnetTo(new Vector2(61,18.75f));Check("orbiting loft Mercy",session.Mercies.Count==1);yield return MagnetTo(new Vector2(99,12.75f));}
            yield return Walk(103.5f);yield return Press(new InputFrame{interact=true});Check("Nail desynchronizes actual choir",session.Phase==RunPhase.Returning&&session.GetComponentInChildren<HaloChoir>().desynchronized);
            foreach(var point in new[]{new Vector2(83,10.75f),new Vector2(65,8.75f),new Vector2(46,6.75f),new Vector2(29,4.75f),new Vector2(16,2.75f)}){yield return MagnetTo(point);if(stopped)yield break;}
            yield return Walk(2,true);
        }
        IEnumerator Run()
        {
            game.SelectSource(1);var world=game.AvailableWorlds[4];
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-require-earned-world")>=0&&!CampaignProgression.WorldOpen(game.AvailableWorlds,4,game.Save.Data,false)){failures++;Note("FAIL Empyrean was not earned by the supplied real save");Finish();yield break;}
            string route=Arg("-gb-route-id","GB-L17");bool secrets=Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-with-secrets")>=0;
            var definition=world.levels.FirstOrDefault(d=>d.id==route);
            if(definition==null||definition.course!=17){failures++;Note("FAIL No complete input witness authored for "+route+". This is not a campaign success.");Finish();yield break;}
            yield return game.Load(definition,Practice);session=game.Session;actor=session.player;host=actor.GetComponent<HostController>();actor.GetComponent<HumanInput>().disabled=true;input=new ScriptedInput();actor.input=input;actor.Stepped+=(f,dt)=>ticks++;
            Note("BEGIN "+definition.id+" "+definition.title);yield return Pause(.35f);yield return Halos(secrets);
            if(!stopped){Check("physical return completed",session.Phase==RunPhase.Cleared);if(!stopped){Check(Practice?"practice does not award progress":"clear is persistent",Practice?!game.Save.Data.cleared.Contains(route):game.Save.Data.cleared.Contains(route));if(!Practice)Check(secrets?"Mercy is saved":"Mercy was optional",secrets?game.Save.Data.mercies.Contains(route+"-MERCY"):session.Mercies.Count==0);}}
            Finish();
        }
        void Finish(){if(finished)return;finished=true;Application.logMessageReceived-=Error;
            File.WriteAllText(Path.Combine(dir,"empyrean-result.json"),"{\"failed\":"+failures+",\"checks\":"+checks+",\"scope\":\"Input-only Empyrean witness under development; no full-game or human acceptance claim\"}");
            File.WriteAllText(Path.Combine(dir,"saved-progress.json"),JsonUtility.ToJson(game.Save.Data,true));Application.Quit(failures==0?0:1);
        }
    }
}
