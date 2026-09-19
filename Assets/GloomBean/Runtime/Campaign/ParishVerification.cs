using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Route witness, not a solver: the only writes to gameplay are production InputFrames.
    // Reads of physical state are allowed. No warps, direct pickups, invulnerability or gate injection.
    public sealed class ParishVerification:MonoBehaviour
    {
        GameRoot game; StageSession session; ActorMotor actor; HostController host; ScriptedInput input;
        string dir; readonly List<string> log=new List<string>(); int failures,assertions; bool stopped,finished;
        float began,lastTrace; int physicsTicks;
        IEnumerator NextPhysics(){int before=physicsTicks;while(Live&&physicsTicks==before)yield return null;}
        bool Live=>session && session.Phase!=RunPhase.Failed && session.Phase!=RunPhase.Cleared;
        string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,key);return n>=0&&n+1<args.Length?args[n+1]:fallback;}
        public void Begin(GameRoot root){game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);began=Time.realtimeSinceStartup;Application.logMessageReceived+=OnLog;RuntimeEvents.Event+=TraceEvent;StartCoroutine(Run());}
        void TraceEvent(string kind,string detail){if(kind=="bell"||kind=="pulse-arrived"||kind=="boss-phase"||kind.StartsWith("echo-"))Note("EVENT t="+Time.time.ToString("0.00")+" "+kind+" "+detail);}
        void OnLog(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception){failures++;Note("ERROR "+m);Finish();}}
        void Update(){if(!finished&&host&&session.definition.course==2&&Time.time>lastTrace+.5f&&Time.time<11){lastTrace=Time.time;var echo=host.Form<EchoForm>();Note("TRACE t="+Time.time.ToString("0.00")+" body="+actor.Body.position+" echo="+(echo!=null&&echo.Echo?echo.Echo.Body.position+" velocity="+echo.Echo.Body.linearVelocity+" live="+echo.Echo.Body.simulated:"none"));}if(!finished&&Time.realtimeSinceStartup-began>420){failures++;Note("FAIL native route watchdog");Finish();}}
        void Note(string text){log.Add(text);File.WriteAllLines(Path.Combine(dir,"parish-observations.txt"),log);}
        void Check(string name,bool pass,bool stop=true){assertions++;if(!pass){failures++;if(stop)stopped=true;}Note((pass?"PASS ":"FAIL ")+session.definition.id+" "+name+" | "+actor.Body.position+" feet="+actor.Feet.y.ToString("0.00")+" hp="+actor.Health);}
        void Snapshot(string label){if(session&&session.Camera)FoundationVerification.Capture(session.Camera.GetComponent<UnityEngine.Camera>(),Path.Combine(dir,session.definition.id+"-"+label+".png"));}
        IEnumerator Pause(float seconds){input.frame=default;float end=Time.time+seconds;while(Live&&Time.time<end)yield return NextPhysics();}
        IEnumerator Press(InputFrame value){if(stopped||!Live)yield break;input.frame=value;yield return NextPhysics();input.frame=default;yield return NextPhysics();}
        IEnumerator Walk(float x,bool run=false,bool crouch=false,float seconds=14){
            if(stopped||!Live)yield break;float end=Time.time+seconds;
            while(Live&&Time.time<end){float dx=x-actor.Body.position.x;if(Mathf.Abs(dx)<.22f)break;input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx*2-actor.Body.linearVelocity.x*.06f,-1,1),crouch?-1:0),run=run};yield return NextPhysics();}
            input.frame=default;yield return null;Check("walk "+x,(session.Phase==RunPhase.Cleared)||Mathf.Abs(actor.Body.position.x-x)<.5f);
        }
        IEnumerator Jump(float x,float floor,bool crouch=false){
            if(stopped||!Live)yield break;float end=Time.time+5;bool launched=false;float peak=actor.Feet.y;
            // Leaving crouch is a real input transition, and may fail under a low ceiling.
            if(actor.Crouched)yield return Pause(.15f);
            while(Live&&Time.time<end){float dx=x-actor.Body.position.x;peak=Mathf.Max(peak,actor.Feet.y);bool edge=!launched&&actor.Grounded;if(edge)launched=true;
                input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx*.8f-actor.Body.linearVelocity.x*.08f,-1,1),0),jump=edge,jumpHeld=true};
                if(launched&&!edge&&actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.25f&&Mathf.Abs(dx)<.22f)break;yield return NextPhysics();}
            input.frame=default;yield return null;Check("jump "+x+" / "+floor+" peak "+peak.ToString("0.00"),actor.Grounded&&Mathf.Abs(actor.Feet.y-floor)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.5f);if(stopped)Snapshot("failed-jump");
        }
        IEnumerator Await(string label,Func<bool> predicate,float seconds,Func<InputFrame> command=null){
            if(stopped||!Live)yield break;float end=Time.time+seconds;while(Live&&Time.time<end&&!predicate()){input.frame=command==null?default:command();yield return NextPhysics();}input.frame=default;Check(label,predicate());
        }
        IEnumerator Thread(float anchor,float length,bool vertical=false,float seconds=16,bool acceptCure=false){
            if(stopped||!Live)yield break;var form=host.Form<MarionetteForm>();if(form==null){Check("Marionette acquired before rail route",false);yield break;}
            float end=Time.time+seconds;
            while(Live&&Time.time<end){var j=form.Joint;if(!j){if(acceptCure&&!host.Has(HostKind.Marionette))yield break;if(session.definition.boss&&session.GetComponentInChildren<AtlasBoss>().phase>=2){Note("PASS Usher chandelier collision ended suspension during the swing");yield break;}Check("rope unexpectedly absent",false);yield break;}float along=vertical?j.connectedAnchor.y:j.connectedAnchor.x;float error=anchor-along;
                float reel=Mathf.Clamp((j.distance-length)*2,-1,1);input.frame=new InputFrame{move=vertical?new Vector2(0,Mathf.Clamp(error*2,-1,1)):new Vector2(Mathf.Clamp(error*2,-1,1),reel)};
                if(Mathf.Abs(error)<.12f&&(vertical||Mathf.Abs(j.distance-length)<.12f))break;yield return NextPhysics();}
            input.frame=default;yield return Pause(.7f);if(!form.Joint&&acceptCure&&!host.Has(HostKind.Marionette))yield break;if(!form.Joint&&session.definition.boss&&session.GetComponentInChildren<AtlasBoss>().phase>=2)yield break;Check("thread "+anchor+" length "+length,form.Joint&&Mathf.Abs((vertical?form.Joint.connectedAnchor.y:form.Joint.connectedAnchor.x)-anchor)<.3f);
        }
        IEnumerator Load(StageDefinition def){
            stopped=false;game.SelectSource(1);yield return game.Load(def,false);session=game.Session;actor=session.player;host=actor.GetComponent<HostController>();actor.GetComponent<HumanInput>().disabled=true;input=new ScriptedInput();actor.input=input;physicsTicks=0;actor.Stepped+=(f,dt)=>physicsTicks++;
            Note("BEGIN "+def.id+" "+def.title);yield return Pause(.35f);
        }
        IEnumerator Sunday(bool secret){
            yield return Walk(6.7f);yield return Jump(9.5f,1.6f);yield return Walk(11.6f);yield return Jump(14.9f,2.6f);yield return Walk(20.4f);yield return Walk(22);yield return Jump(24.7f,1.5f);
            yield return Walk(43);Check("corruption and Echo contact",game.IsCorrupted&&host.Has(HostKind.Echo));yield return Walk(58.2f,false,false,18);yield return Jump(61,-2.2f);
            float[] xs={66,71,76,81,86};float[] ys={-.7f,.8f,2.3f,3.8f,5.3f};for(int i=0;i<xs.Length&&!stopped;i++){yield return Walk(xs[i]-3.5f);yield return Jump(xs[i]-.4f,ys[i]);}
            if(stopped)yield break;Check("physical key",session.HasKey);yield return Walk(94.9f);yield return Press(new InputFrame{interact=true});Check("World Nail",session.Phase==RunPhase.Returning);Snapshot("turn");yield return Walk(27,false,false,25);
            if(secret&&!stopped){yield return Pause(.8f);yield return Jump(22.8f,7.4f);yield return Walk(18,false,false,5);Check("Mercy collected",session.Mercies.Count==1);}
            yield return Walk(2,false,false,16);
        }
        IEnumerator Belfry(bool secret){
            yield return Walk(6.8f);Check("bell-wisp contact",host.Has(HostKind.Echo));yield return Press(new InputFrame{interact=true});yield return Jump(9,2);yield return Jump(13,4);yield return Jump(17,6);yield return Walk(17.8f);yield return Press(new InputFrame{interact=true});yield return Jump(21,8);yield return Walk(26);
            var dual=session.GetComponentInChildren<DualPulseLift>();yield return Await("two traveling signals release lift",()=>dual.latched,12);yield return Walk(28);
            yield return Await("ride physical lower lift",()=>actor.Feet.y>19.4f&&actor.Grounded,12);yield return Walk(29.1f);yield return Jump(33,20);yield return Jump(37,22);yield return Jump(33,24);yield return Jump(29,26);yield return Jump(25,28);yield return Jump(21,30);yield return Jump(18,32);
            yield return Walk(20);Check("upper key",session.HasKey);yield return Walk(16.2f);yield return Press(new InputFrame{interact=true});Check("crack the main bell",session.Phase==RunPhase.Returning);Snapshot("turn");yield return Walk(14.4f);yield return Press(new InputFrame{interact=true});
            yield return Pause(2.3f);yield return Jump(9.5f,27);yield return Walk(6);yield return Await("left balcony landing",()=>actor.Grounded&&actor.Feet.y<28,6);
            if(secret&&!stopped){yield return Jump(14,15);Check("Mercy collected",session.Mercies.Count==1);}
            yield return Walk(0);yield return Await("return descent reaches ground",()=>actor.Grounded&&actor.Feet.y<.3f,8);yield return Walk(2);
        }
        IEnumerator Laundry(bool secret){
            yield return Walk(8);Check("spider strings Host",host.Has(HostKind.Marionette));yield return Thread(29,9.5f);yield return Thread(43,9.5f);
            if(secret&&!stopped){yield return Thread(43,2);yield return Press(new InputFrame{action=true});yield return Thread(23,2,true);yield return Press(new InputFrame{action=true});yield return Thread(47,2);Check("cabinet Mercy",session.Mercies.Count==1);yield return Thread(43,2);yield return Press(new InputFrame{action=true});yield return Thread(12,2,true);yield return Press(new InputFrame{action=true});}
            yield return Thread(53,9.5f);yield return Press(new InputFrame{action=true});yield return Thread(76,10.5f);yield return Thread(89,5.5f);yield return Pause(1);Check("physical laundry key",session.HasKey);yield return Thread(99,6.4f);yield return Await("lines cut on Turn",()=>session.Phase==RunPhase.Returning,12,()=>new InputFrame{interact=true});Snapshot("turn");yield return Thread(77,5.5f);yield return Thread(62,10.5f);yield return Thread(53,9.5f);yield return Press(new InputFrame{action=true});yield return Thread(40,9.5f);yield return Thread(7,10.5f,acceptCure:true);yield return Await("shears release the body",()=>!host.Has(HostKind.Marionette),8,()=>new InputFrame{move=new Vector2(-1,0)});yield return Walk(2);
        }
        IEnumerator Skins(bool secret){
            yield return Walk(13);Check("wardrobe moth contact",host.Has(HostKind.Molt));yield return Press(new InputFrame{action=true});Check("first persistent skin",host.Husks.Count==1);yield return Walk(20.3f);yield return Jump(23,1.6f);yield return Jump(27,2);yield return Walk(30);
            yield return Walk(41,false,true);yield return Press(new InputFrame{interact=true});
            if(secret&&!stopped){yield return Jump(43,3.3f);yield return Jump(39,4.35f);yield return Walk(33);yield return Jump(30,5);yield return Press(new InputFrame{action=true});Check("two shed skins",host.Husks.Count==2);yield return Jump(32.4f,7);yield return Walk(41,false,true);Check("seam Mercy",session.Mercies.Count==1);yield return Walk(44,false,true);}
            yield return Walk(52);var f=host.Form<MarionetteForm>();Check("spider at peeled bridge",f!=null);
            yield return Thread(66,9.7f,acceptCure:true);yield return Await("bridge shears preserve the molt",()=>!host.Has(HostKind.Marionette)&&host.Has(HostKind.Molt),5,()=>new InputFrame{move=new Vector2(-.2f,0)});
            yield return Walk(81);yield return Jump(84,4);yield return Walk(85.3f);yield return Jump(89,6);yield return Walk(92.5f);yield return Jump(97,8);Check("wardrobe key",session.HasKey);yield return Walk(101.6f);yield return Press(new InputFrame{interact=true});Check("discarded skins crawl home",session.Phase==RunPhase.Returning);Snapshot("turn");
            yield return Walk(83);yield return Await("return spider catches the host",()=>host.Has(HostKind.Marionette),8);
            yield return Thread(78,3);yield return Thread(60,3);yield return Thread(43,9.7f,acceptCure:true);yield return Await("shore shears",()=>!host.Has(HostKind.Marionette),8,()=>new InputFrame{move=new Vector2(-.3f,0)});
            yield return Walk(30,false,true);yield return Walk(13,false,true);yield return Walk(2);
        }
        IEnumerator Usher(){
            var boss=session.GetComponentInChildren<AtlasBoss>();yield return Walk(12);yield return Pause(.85f);yield return Walk(23,true);yield return Await("Usher act I: overlap two scales",()=>boss.phase>=1,4);
            yield return Walk(7,true);Check("Usher strings Host",host.Has(HostKind.Marionette));yield return Thread(17,4.3f);yield return Thread(27,4.3f);yield return Await("Usher act II: real chandelier impact",()=>boss.phase>=2,4);
            yield return Walk(7,true);Check("coat-check moth",host.Has(HostKind.Molt));yield return Walk(19);yield return Press(new InputFrame{action=true});yield return Walk(42,false,true,20);Check("Usher defeated by backstage route",boss.defeated&&session.Phase==RunPhase.Cleared);
        }
        IEnumerator Run(){
            game.SelectSource(1);var world=game.AvailableWorlds[0];string selected=Arg("-gb-route-id","GB-L02");bool secrets=Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-with-secrets")>=0;
            var stages=new List<StageDefinition>();if(selected=="W1"){stages.AddRange(world.levels);stages.Add(world.boss);}else{stages.Add(selected==world.boss.id?world.boss:Array.Find(world.levels,x=>x.id==selected));}
            foreach(var stage in stages){if(stage==null){failures++;Note("FAIL unknown stage "+selected);break;}yield return Load(stage);if(stage.boss){yield return Usher();}else switch(stage.course){case 1:yield return Sunday(secrets);break;case 2:yield return Belfry(secrets);break;case 3:yield return Laundry(secrets);break;case 4:yield return Skins(secrets);break;default:if(stage.boss)yield return Usher();else Check("route not authored",false);break;}
                if(!stage.boss)Check("critical path returns to entrance",session.Phase==RunPhase.Cleared);Snapshot("finish");if(stopped)break;}
            Finish();
        }
        void Finish(){if(finished)return;finished=true;Application.logMessageReceived-=OnLog;RuntimeEvents.Event-=TraceEvent;File.WriteAllText(Path.Combine(dir,"parish-result.json"),"{\"failed\":"+failures+",\"checks\":"+assertions+",\"scope\":\"Production-input Parish route witnesses; not human or whole-campaign acceptance\"}");Application.Quit(failures==0?0:1);}
    }
}
