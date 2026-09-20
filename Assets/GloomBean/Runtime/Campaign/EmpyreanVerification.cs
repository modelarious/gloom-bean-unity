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
        GameRoot game;StageSession session;ActorMotor actor;HostController host;WitnessInput input;
        sealed class WitnessInput:IActorInput
        {
            public InputFrame frame;public Func<InputFrame> rule;
            public InputFrame Consume(){if(rule!=null)return rule();var value=frame;frame=frame.WithoutEdges();return value;}
        }
        string dir;readonly List<string> log=new List<string>();int failures,checks,ticks;bool stopped,finished;float began;
        bool Practice=>Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-practice-witness")>=0;
        bool Live=>session&&session.Phase!=RunPhase.Failed&&session.Phase!=RunPhase.Cleared;
        string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        public void Begin(GameRoot root){int fps;if(int.TryParse(Arg("-gb-render-fps","120"),out fps)){QualitySettings.vSyncCount=0;Application.targetFrameRate=Mathf.Clamp(fps,15,240);}game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);began=Time.realtimeSinceStartup;Application.logMessageReceived+=Error;StartCoroutine(Run());}
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
        IEnumerator MagnetTo(Vector2 target,float timeout=18,bool landing=true,int fixedPole=0)
        {
            if(stopped||!Live)yield break;var magnet=host.Form<LodestoneForm>();Check("Lodestone acquired by actual source",magnet!=null);if(stopped)yield break;
            float end=Time.time+timeout,nextToggle=0,lastJump=-1,trace=0;int reversals=0;
            input.rule=()=>{
                if(!Live)return default;
                Vector2 error=target-actor.Body.position;var velocity=actor.Body.linearVelocity;
                Vector2 northForce=Vector2.zero;
                foreach(var metal in MagneticBody.All)if(metal&&metal.isActiveAndEnabled&&Vector2.Distance(actor.Body.position,metal.Position)<magnet.range)
                    northForce+=metal.ForceOn(actor.Body.position,1,magnet.range);
                Vector2 desired=new Vector2(error.x*6-velocity.x*3,error.y*9-velocity.y*4+actor.tuning.gravity);
                int pole=fixedPole!=0&&(!landing||error.magnitude<3.5f)?fixedPole:(Vector2.Dot(northForce,desired)>=0?1:-1);
                bool reverse=pole!=magnet.Polarity&&Time.fixedTime>=nextToggle;
                if(reverse){nextToggle=Time.fixedTime+.18f;reversals++;}
                bool jump=actor.Grounded&&error.y>.35f&&Time.fixedTime-lastJump>.5f;if(jump)lastJump=Time.fixedTime;
                return new InputFrame{action=reverse,actionHeld=reverse,jump=jump,jumpHeld=true,move=new Vector2(Mathf.Clamp(error.x*2-velocity.x*.6f,-1,1),0)};
            };
            while(Live&&Time.time<end){
                Vector2 error=target-actor.Body.position;
                if(error.magnitude<(landing?.8f:1.1f)&&(!landing||actor.Grounded&&Mathf.Abs(actor.Body.linearVelocity.x)<1.5f))break;
                yield return Tick();if(Time.time>trace){trace=Time.time+.5f;Note("MAGNET t="+Time.time+" body="+actor.Body.position+" velocity="+actor.Body.linearVelocity+" pole="+magnet.Polarity+" target="+target+" reversals="+reversals);}
            }
            input.rule=null;
            input.frame=default;Note("SUPPORT "+(actor.GroundCollider?actor.GroundCollider.name:"none"));Check((landing?"magnetic landing at ":"magnetic transit through ")+target,(!landing||actor.Grounded)&&Vector2.Distance(actor.Body.position,target)<(landing?.9f:1.2f));Snapshot("magnet-"+target.x);
        }
        IEnumerator PowerAltar(float x,int direction=1)
        {
            if(stopped||!Live)yield break;
            var coil=session.GetComponentsInChildren<MagneticBody>(true).Single(m=>m.name==("Launch coil "+x));
            if(coil.enabled)yield return Press(new InputFrame{interact=true});
            Vector2 stance=new Vector2(x+direction*.9f,actor.Body.position.y);float until=Time.time+4;
            input.rule=()=>new InputFrame{move=new Vector2(Mathf.Clamp((stance.x-actor.Body.position.x)*3-actor.Body.linearVelocity.x*.7f,-1,1),0)};
            while(Live&&Time.time<until&&(Mathf.Abs(actor.Body.position.x-stance.x)>.04f||Mathf.Abs(actor.Body.linearVelocity.x)>.12f))yield return Tick();
            input.rule=null;input.frame=default;
            Check("visible coil launch stance "+x,!coil.enabled&&actor.Grounded&&Mathf.Abs(actor.Body.position.x-stance.x)<.12f);
        }
        IEnumerator Flight(Vector2 destination)
        {
            if(stopped||!Live)yield break;
            var magnet=host.Form<LodestoneForm>();Vector2 start=actor.Body.position;int direction=destination.x>start.x?1:-1;
            float cruise=Mathf.Max(start.y,destination.y)+6.5f,deadline=Time.time+12,nextToggle=0,trace=0;bool jumped=false,landed=false,landingInteract=false,activated=false;
            // Only genuine controls: launch, adjust pole in the observed field, steer, land.
            input.rule=()=>{
                Vector2 pos=actor.Body.position,v=actor.Body.linearVelocity;float dx=destination.x-pos.x;
                bool launch=direction*(pos.x-start.x)<5;
                if(jumped&&actor.Grounded&&Vector2.Distance(pos,destination)<1.65f){landed=true;bool interact=!landingInteract;landingInteract=true;return new InputFrame{action=magnet.Polarity!=-1,interact=interact,move=new Vector2(Mathf.Clamp(dx*3-v.x*1.1f,-1,1),0)};}
                float goalY=Mathf.Abs(dx)>4?cruise:destination.y;
                Vector2 wanted=new Vector2(dx*4-v.x*3,(goalY-pos.y)*5-v.y*4+actor.tuning.gravity);
                Vector2 north=Vector2.zero;foreach(var m in MagneticBody.All)if(m&&m.isActiveAndEnabled&&Vector2.Distance(pos,m.Position)<magnet.range)north+=m.ForceOn(pos,1,magnet.range);
                bool orbitalContact=false;foreach(var m in MagneticBody.All)if(m&&m.fieldRadius>0&&m.Influence(pos,magnet.range)>.05f){orbitalContact=true;break;}
                int pole=orbitalContact?(Vector2.Dot(north,wanted)>=0?1:-1):1;bool toggle=pole!=magnet.Polarity&&Time.fixedTime>=nextToggle;if(toggle)nextToggle=Time.fixedTime+.12f;
                bool jump=!jumped&&actor.Grounded;if(jump)jumped=true;
                float steer=launch?direction:Mathf.Clamp(dx*2-v.x*.5f,-1,1);
                bool activate=!activated;activated=true;
                return new InputFrame{action=toggle,actionHeld=toggle,interact=activate,move=new Vector2(steer,0),jump=jump,jumpHeld=true};
            };
            while(Live&&Time.time<deadline){
                if(session.Phase==RunPhase.Returning&&destination.x==16&&actor.Grounded&&actor.Body.position.x<18&&actor.Feet.y<.25f)break;
                if(landed&&Vector2.Distance(actor.Body.position,destination)<1.1f&&actor.Grounded&&Mathf.Abs(actor.Body.linearVelocity.x)<2)break;
                if(actor.Feet.y<Mathf.Min(start.y,destination.y)-4)break;
                yield return Tick();if(Time.time>trace){trace=Time.time+.5f;Note("FLIGHT body="+actor.Body.position+" v="+actor.Body.linearVelocity+" pole="+magnet.Polarity+" target="+destination);}
            }
            input.rule=null;input.frame=default;Check("coil flight lands on physical support "+destination,actor.Grounded&&(Vector2.Distance(actor.Body.position,destination)<1.1f||(session.Phase==RunPhase.Returning&&destination.x==16&&actor.Body.position.x<18&&actor.Feet.y<.25f)));Snapshot("flight-"+destination.x);
        }
        IEnumerator OrbitMercy()
        {
            if(stopped||!Live)yield break;var ring=session.GetComponentsInChildren<MotionPlatform>().Single(x=>x.name=="Orbiting Mercy halo");var magnet=host.Form<LodestoneForm>();var coil=session.GetComponentsInChildren<MagneticBody>(true).Single(x=>x.name=="Launch coil 49");
            if(coil.enabled)yield return Press(new InputFrame{interact=true});yield return Walk(48.7f);bool powered=false;float next=0,end=Time.time+18;bool interacted=false;int exchanges=0;
            input.rule=()=>{
                Vector2 target=ring.GetComponent<Rigidbody2D>().position, pos=actor.Body.position,v=actor.Body.linearVelocity;bool low=pos.y<12;
                Vector2 north=Vector2.zero;foreach(var m in MagneticBody.All)if(m)north+=m.ForceOn(pos,1,magnet.range);
                Vector2 desired=new Vector2((target.x-pos.x)*5-v.x*3,(target.y-pos.y)*7-v.y*3+actor.tuning.gravity);
                int pole=low?1:Vector2.Dot(north,desired)>=0?1:-1;bool toggle=pole!=magnet.Polarity&&Time.fixedTime>=next;if(toggle){next=Time.fixedTime+.14f;exchanges++;}
                bool engage=!interacted;interacted=true;bool jump=actor.Grounded&&!powered;if(jump)powered=true;
                return new InputFrame{interact=engage,action=toggle,actionHeld=toggle,jump=jump,jumpHeld=true,move=new Vector2(Mathf.Clamp((target.x-pos.x)*2-v.x*.6f,-1,1),0)};
            };
            while(Live&&Time.time<end&&session.Mercies.Count==0)yield return Tick();input.rule=null;input.frame=default;
            Check("collect Mercy inside the actual orbiting rim",session.Mercies.Count==1);Snapshot("orbiting-mercy");if(stopped)yield break;
            end=Time.time+10;input.rule=()=>new InputFrame{action=magnet.Polarity!=-1,move=new Vector2(Mathf.Clamp((49-actor.Body.position.x)*2-actor.Body.linearVelocity.x*.8f,-1,1),0)};
            while(Live&&Time.time<end&&!(actor.Grounded&&Mathf.Abs(actor.Feet.y-8)<.3f))yield return Tick();input.rule=null;input.frame=default;
            Check("leave the orbital secret through real geometry",actor.Grounded&&Mathf.Abs(actor.Feet.y-8)<.3f);
        }
        IEnumerator Halos(bool secret)
        {
            yield return Walk(8);Check("iron halo source",host.Has(HostKind.Lodestone));yield return Walk(11.8f);yield return MagnetTo(new Vector2(16,2.75f));
            var docks=new[]{new Vector2(16,2.75f),new Vector2(27,4.75f),new Vector2(38,6.75f),new Vector2(49,8.75f),new Vector2(60,10.75f),new Vector2(71,12.75f)};
            for(int i=0;i<docks.Length-1;i++){yield return PowerAltar(docks[i].x);yield return Flight(docks[i+1]);if(stopped)yield break;if(secret&&i==2)yield return OrbitMercy();}
            Check("Keyling reached through magnetic traversal",session.HasKey);
            if(secret)Check("orbiting Mercy retained before return",session.Mercies.Count==1);
            yield return Press(new InputFrame{interact=true});yield return Walk(74.5f);yield return Press(new InputFrame{interact=true});Check("Nail desynchronizes actual choir",session.Phase==RunPhase.Returning&&session.GetComponentInChildren<HaloChoir>().desynchronized);
            yield return Walk(71);for(int i=docks.Length-1;i>0;i--){yield return PowerAltar(docks[i].x,-1);yield return Flight(docks[i-1]);if(stopped)yield break;}
            yield return Walk(2,true);
        }
        IEnumerator Focus(HostKind kind)
        {
            if(stopped||!Live)yield break;Check("tenant acquired "+kind,host.Has(kind));
            for(int i=0;i<3&&host.Primary!=kind;i++)yield return Press(new InputFrame{alternate=true,move=Vector2.down});
            Check("focus "+kind,host.Primary==kind);
        }
        IEnumerator ShadowTravel(Vector2 point)
        {
            if(stopped||!Live)yield break;var shadow=host.Form<ShadowForm>();float end=Time.time+8;
            input.rule=()=>new InputFrame{move=Vector2.ClampMagnitude((point-shadow.Position)*4,1)};
            while(Live&&Time.time<end&&Vector2.Distance(point,shadow.Position)>.15f)yield return Tick();
            input.rule=null;input.frame=default;Check("traverse connected silhouette to "+point,shadow.Controlling&&shadow.Allowed(shadow.Position)&&Vector2.Distance(point,shadow.Position)<.25f);
        }
        IEnumerator ShadowWindow(int index,bool returning)
        {
            if(stopped||!Live)yield break;float dx=index*32;var sun=session.GetComponentInChildren<ShadowSun>();var latch=session.GetComponentsInChildren<ShadowReceiver>().Single(x=>x.name=="Shadow latch "+index);var screen=session.GetComponentsInChildren<MagneticBody>().Single(x=>x.name=="Manufactured shadow screen "+index);
            yield return Walk((returning?24.8f:16.3f)+dx);
            if(returning){
                yield return Focus(HostKind.Lodestone);var magnet=host.Form<LodestoneForm>();if(magnet.Polarity!=-1)yield return Press(new InputFrame{action=true});
                float end=Time.time+9;input.rule=()=>new InputFrame{move=Vector2.left};
                while(Live&&Time.time<end&&screen.Position.x<22+dx)yield return Tick();input.rule=null;input.frame=default;
                Check("physical iron screen manufactures the noon bridge "+index,sun.noon&&screen.Position.x>22+dx);Snapshot("manufactured-shadow-"+index);
            }else{
                float end=Time.time+25;while(Live&&Time.time<end&&sun.direction.x<.76f)yield return Tick();Check("moving sunlight opens a real shadow connection "+index,sun.direction.x>=.76f);
            }
            yield return Focus(HostKind.Shadow);yield return Press(new InputFrame{alternate=true});yield return ShadowTravel(new Vector2(20.5f+dx,.35f));
            yield return Pause(.08f);Check("controlled valid shadow operates latch "+index,latch.active);
            yield return ShadowTravel(actor.Feet);yield return Press(new InputFrame{alternate=true});Check("shadow rejoins without teleporting through light",host.Form<ShadowForm>().Attached);yield return Walk((returning?18:28)+dx);
        }
        IEnumerator NoShadows(bool secret)
        {
            yield return Walk(8);Check("noon lamp tears off an available shadow",host.Has(HostKind.Shadow));yield return ShadowWindow(0,false);yield return ShadowWindow(1,false);
            yield return Walk(61.5f);yield return Jump(65,1.6f);yield return Walk(67);yield return Jump(71,3.2f);yield return Walk(73.35f);yield return Jump(78,4.8f);Check("Noon Keyling reached",session.HasKey);
            if(secret){Check("suspended own-body Mercy remains an unverified gate",false);yield break;}
            yield return Walk(80.5f);yield return Press(new InputFrame{interact=true});Check("Nail fixes the sun overhead rather than accelerating it",session.Phase==RunPhase.Returning&&session.GetComponentInChildren<ShadowSun>().noon);
            yield return ShadowWindow(1,true);yield return ShadowWindow(0,true);yield return Walk(2,true);
        }
        IEnumerator Run()
        {
            game.SelectSource(1);var world=game.AvailableWorlds[4];
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-require-earned-world")>=0&&!CampaignProgression.WorldOpen(game.AvailableWorlds,4,game.Save.Data,false)){failures++;Note("FAIL Empyrean was not earned by the supplied real save");Finish();yield break;}
            string route=Arg("-gb-route-id","GB-L17");bool secrets=Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-with-secrets")>=0;
            var definition=world.levels.FirstOrDefault(d=>d.id==route);
            if(definition==null||(definition.course!=17&&definition.course!=18)){failures++;Note("FAIL No complete input witness authored for "+route+". This is not a campaign success.");Finish();yield break;}
            yield return game.Load(definition,Practice);session=game.Session;actor=session.player;host=actor.GetComponent<HostController>();actor.GetComponent<HumanInput>().disabled=true;input=new WitnessInput();actor.input=input;actor.Stepped+=(f,dt)=>ticks++;
            Note("BEGIN "+definition.id+" "+definition.title);float startDelay;float.TryParse(Arg("-gb-start-delay","0"),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out startDelay);yield return Pause(.35f+Mathf.Clamp(startDelay,0,20));if(definition.course==17)yield return Halos(secrets);else yield return NoShadows(secrets);
            if(!stopped){Check("physical return completed",session.Phase==RunPhase.Cleared);if(!stopped){Check(Practice?"practice does not award progress":"clear is persistent",Practice?!game.Save.Data.cleared.Contains(route):game.Save.Data.cleared.Contains(route));if(!Practice)Check(secrets?"Mercy is saved":"Mercy was optional",secrets?game.Save.Data.mercies.Contains(route+"-MERCY"):session.Mercies.Count==0);}}
            Finish();
        }
        void Finish(){if(finished)return;finished=true;Application.logMessageReceived-=Error;
            File.WriteAllText(Path.Combine(dir,"empyrean-result.json"),"{\"failed\":"+failures+",\"checks\":"+checks+",\"scope\":\"Input-only Empyrean witness under development; no full-game or human acceptance claim\"}");
            File.WriteAllText(Path.Combine(dir,"saved-progress.json"),JsonUtility.ToJson(game.Save.Data,true));Application.Quit(failures==0?0:1);
        }
    }
}
