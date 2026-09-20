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
    public sealed class CityVerification:MonoBehaviour
    {
        GameRoot game; StageSession session; ActorMotor actor; HostController host; ScriptedInput input;
        string dir; readonly List<string> log=new List<string>(); int failures,assertions; bool stopped,finished;
        bool PracticeWitness=>Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-practice-witness")>=0;
        float began,lastTrace; int physicsTicks;
        IEnumerator NextPhysics(){int before=physicsTicks;while(Live&&physicsTicks==before)yield return null;}
        bool Live=>session && session.Phase!=RunPhase.Failed && session.Phase!=RunPhase.Cleared;
        string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,key);return n>=0&&n+1<args.Length?args[n+1]:fallback;}
        public void Begin(GameRoot root){game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);began=Time.realtimeSinceStartup;Application.logMessageReceived+=OnLog;RuntimeEvents.Event+=TraceEvent;StartCoroutine(Run());}
        void TraceEvent(string kind,string detail){if(kind=="depth"||kind=="paint-loop"||kind=="boss-phase")Note("EVENT t="+Time.time.ToString("0.00")+" "+kind+" "+detail);}
        void OnLog(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception){failures++;Note("ERROR "+m);Finish();}}
        void Update(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-echo-trace")>=0&&!finished&&host&&session.definition.course==2&&Time.time>lastTrace+.5f&&Time.time<11){lastTrace=Time.time;var echo=host.Form<EchoForm>();Note("TRACE t="+Time.time.ToString("0.00")+" body="+actor.Body.position+" echo="+(echo!=null&&echo.Echo?echo.Echo.Body.position+" velocity="+echo.Echo.Body.linearVelocity+" live="+echo.Echo.Body.simulated:"none"));}if(!finished&&Time.realtimeSinceStartup-began>510){failures++;Note("FAIL native route watchdog");Finish();}}
        void Note(string text){log.Add(text);File.WriteAllLines(Path.Combine(dir,"city-observations.txt"),log);}
        void Check(string name,bool pass,bool stop=true){assertions++;if(!pass){failures++;if(stop&&!stopped){Snapshot("first-failure");Note("PHYSICAL shape="+actor.Shape.bounds+" axis="+actor.Shape.direction+" forms="+string.Join(",",host.Forms.Select(f=>f.Kind.ToString())));
                foreach(var near in Physics2D.OverlapBoxAll(actor.Body.position,(Vector2)actor.Shape.bounds.size+Vector2.one*.15f,0))if(!near.isTrigger&&near!=actor.Shape)Note("CONTACT "+near.name+" "+near.bounds);}
                if(stop)stopped=true;}Note((pass?"PASS ":"FAIL ")+session.definition.id+" "+name+" | "+actor.Body.position+" feet="+actor.Feet.y.ToString("0.00")+" hp="+actor.Health+" twin="+(host.Form<MirrorForm>()?.Twin ? host.Form<MirrorForm>().Twin.Body.position.ToString():"none"));}
        void Snapshot(string label){if(session&&session.Camera)FoundationVerification.Capture(session.Camera.GetComponent<UnityEngine.Camera>(),Path.Combine(dir,session.definition.id+"-"+label+".png"));}
        IEnumerator Pause(float seconds){input.frame=default;float end=Time.time+seconds;while(Live&&Time.time<end)yield return NextPhysics();}
        IEnumerator Hold(InputFrame value,float seconds){if(stopped||!Live)yield break;float end=Time.time+seconds;while(Live&&Time.time<end){input.frame=value;yield return NextPhysics();}input.frame=default;}
        IEnumerator Press(InputFrame value){if(stopped||!Live)yield break;input.frame=value;yield return NextPhysics();input.frame=default;yield return NextPhysics();}
        IEnumerator Walk(float x,bool run=false,bool crouch=false,float seconds=14){
            if(stopped||!Live)yield break;float end=Time.time+seconds;
            while(Live&&Time.time<end){float dx=x-actor.Body.position.x;bool liquid=host.Form<WaxForm>()?.Liquid??false;if(Mathf.Abs(dx)<.22f&&(!liquid||Mathf.Abs(actor.Body.linearVelocity.x)<.6f))break;input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx*(liquid?1.5f:2)-actor.Body.linearVelocity.x*(liquid?.65f:.06f),-1,1),crouch?-1:0),run=run};yield return NextPhysics();}
            input.frame=default;yield return null;Check("walk "+x,(session.Phase==RunPhase.Cleared)||Mathf.Abs(actor.Body.position.x-x)<.5f);
        }
        IEnumerator Jump(float x,float floor,bool crouch=false){
            if(stopped||!Live)yield break;float end=Time.time+5;bool launched=false;float peak=actor.Feet.y;int trace=0;
            // Leaving crouch is a real input transition, and may fail under a low ceiling.
            if(actor.Crouched)yield return Pause(.15f);
            while(Live&&Time.time<end){float dx=x-actor.Body.position.x;peak=Mathf.Max(peak,actor.Feet.y);if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-echo-trace")>=0&&session.definition.course==4&&Mathf.Abs(floor-7)<.01f&&trace++<60)Note("TRACE seam x="+actor.Body.position.x.ToString("0.000")+" feet="+actor.Feet.y.ToString("0.000")+" shape="+actor.Shape.size+" vy="+actor.Body.linearVelocity.y.ToString("0.00"));bool edge=!launched&&actor.Grounded;if(edge)launched=true;
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
            stopped=false;game.SelectSource(1);yield return game.Load(def,PracticeWitness);session=game.Session;actor=session.player;host=actor.GetComponent<HostController>();actor.GetComponent<HumanInput>().disabled=true;input=new ScriptedInput();actor.input=input;physicsTicks=0;actor.Stepped+=(f,dt)=>physicsTicks++;
            Note("BEGIN "+def.id+" "+def.title);yield return Pause(.35f);
        }


        IEnumerator Suns(bool secret)
        {
            yield return Walk(7);Check("vanity nun creates independently colliding twin",host.Has(HostKind.Mirror));
            yield return Jump(10.5f,2);yield return Jump(13,2);yield return Jump(16,4);
            var joint=session.GetComponentsInChildren<Gate>().First(g=>g.name=="Paired apartment interlock");
            yield return Await("two bodies hold the balcony scales",()=>joint.opened,4);Snapshot("paired-balconies");
            if(secret&&!stopped){yield return Walk(14.1f);yield return Jump(10,6);yield return Press(new InputFrame{interact=true});
                var shutter=session.GetComponentInChildren<WindowShutter>();var secretGate=session.GetComponentsInChildren<Gate>().First(g=>g.name=="Off-register Mercy shutters");
                yield return Await("physical shutter displaces the twin onto off-register scale",()=>secretGate.opened,6);Check("shutter completes an actual movement",shutter.IsClosed);Snapshot("shutter-desynchronization");yield return Walk(3);Check("Mercy inside the original apartment",session.Mercies.Count==1);yield return Walk(14);}
            yield return Walk(44);yield return Await("matte velvet removes reflection",()=>!host.Has(HostKind.Mirror),4);yield return Press(new InputFrame{interact=true});
            yield return Await("released lift physically carries the Host",()=>actor.Grounded&&actor.Feet.y>3.9f,7,()=>new InputFrame{move=new Vector2(Mathf.Clamp(44-actor.Body.position.x,-1,1),0)});
            yield return Walk(45.2f);yield return Press(new InputFrame{interact=true});yield return Pause(.15f);
            Check("curtains create real cast-shadow bridge collision",session.GetComponentsInChildren<SunShutter>().All(s=>s.GetComponent<Collider2D>().enabled));
            yield return Jump(49,4.3f);yield return Walk(50.7f);yield return Jump(55,4.95f);yield return Walk(56.7f);yield return Jump(61,5.6f);yield return Walk(62.7f);yield return Jump(67,6.25f);yield return Walk(68.7f);yield return Jump(72,7);yield return Walk(73.4f);
            yield return Jump(76,8.2f);yield return Walk(77.5f);yield return Jump(81,10);yield return Walk(82.5f);yield return Jump(86,11.8f);yield return Walk(87.5f);yield return Jump(91,13.6f);
            Check("roof key collected by contact",session.HasKey);yield return Jump(96,13.6f);yield return Walk(97.6f);yield return Press(new InputFrame{interact=true});
            Check("one sun extinguishes at the Turn",session.Phase==RunPhase.Returning);yield return Pause(.1f);Check("only eastern shadow bridges disappear",session.GetComponentsInChildren<SunShutter>().All(s=>s.GetComponent<Collider2D>().enabled==(s.phase==0)));Snapshot("turn");
            yield return Walk(77.8f);yield return Await("return vanity creates a new paired route",()=>host.Has(HostKind.Mirror),4);yield return Walk(75);yield return Jump(70,7.5f);yield return Walk(68);
            var returnGate=session.GetComponentsInChildren<Gate>().First(g=>g.name=="Return apartment interlock");yield return Await("changed furniture solved by both return bodies",()=>returnGate.opened,4);Snapshot("return-mirror");
            yield return Walk(61.5f);yield return Await("return velvet releases both-body constraint",()=>!host.Has(HostKind.Mirror),4);yield return Walk(43);yield return Await("return reaches old street",()=>actor.Grounded&&actor.Feet.y<1,6);yield return Walk(2);
        }
        IEnumerator Run()
        {
            game.SelectSource(1);var world=game.AvailableWorlds[2];
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-require-earned-world")>=0&&!CampaignProgression.WorldOpen(game.AvailableWorlds,2,game.Save.Data,false))
            {failures++;Note("FAIL City was not earned by the supplied real save");Finish();yield break;}
            string selected=Arg("-gb-route-id","GB-L09");bool secrets=Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-with-secrets")>=0;
            var stages=new List<StageDefinition>();if(selected=="W3"){stages.AddRange(world.levels);stages.Add(world.boss);}else stages.Add(selected==world.boss.id?world.boss:Array.Find(world.levels,x=>x.id==selected));
            foreach(var stage in stages)
            {
                if(stage==null){failures++;Note("FAIL unknown City stage "+selected);break;}yield return Load(stage);
                if(selected=="W3")Check("earned intra-world selection",stage.boss?CampaignProgression.BossOpen(world,game.Save.Data,PracticeWitness):CampaignProgression.LevelOpen(world,Array.IndexOf(world.levels,stage),game.Save.Data,PracticeWitness));
                switch(stage.course){case 9:yield return Suns(secrets);break;default:Check("route not implemented yet",false);break;}
                Check("stage cleared by actual return or boss solution",session.Phase==RunPhase.Cleared);
                if(!stopped){if(PracticeWitness)Check("practice writes no progress",!game.Save.Data.cleared.Contains(stage.id)&&!game.Save.Data.mercies.Contains(stage.id+"-MERCY"));else{Check("stage clear persisted",game.Save.Data.cleared.Contains(stage.id));if(!stage.boss)Check(secrets?"Mercy saved after physical collection and return":"ordinary route requires no Mercy",secrets?game.Save.Data.mercies.Contains(stage.id+"-MERCY"):session.Mercies.Count==0);}}
                Snapshot("finish");if(stopped)break;
            }
            if(selected=="W3"&&!stopped){var reload=new SaveStore(Path.Combine(dir,"test-save.json"));Check("City unlocks the Fall",CampaignProgression.WorldOpen(game.AvailableWorlds,3,reload.Data,PracticeWitness));Check("City does not restore normal ending",!reload.RestoredEnding);Check("all City clears survive reload",world.levels.All(d=>reload.Data.cleared.Contains(d.id))&&reload.Data.cleared.Contains(world.boss.id));}
            Finish();
        }
        void Finish()
        {
            if(finished)return;finished=true;Application.logMessageReceived-=OnLog;RuntimeEvents.Event-=TraceEvent;
            File.WriteAllText(Path.Combine(dir,"city-result.json"),"{\"failed\":"+failures+",\"checks\":"+assertions+",\"scope\":\"Production-input City witness; not blind human or whole-game acceptance\"}");
            File.WriteAllText(Path.Combine(dir,"saved-progress.json"),JsonUtility.ToJson(game.Save.Data,true));Application.Quit(failures==0?0:1);
        }
    }
}
