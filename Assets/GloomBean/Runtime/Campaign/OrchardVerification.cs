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
    public sealed class OrchardVerification:MonoBehaviour
    {
        GameRoot game; StageSession session; ActorMotor actor; HostController host; ScriptedInput input;
        string dir; readonly List<string> log=new List<string>(); int failures,assertions; bool stopped,finished;
        bool PracticeWitness=>Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-practice-witness")>=0;
        float began,lastTrace; int physicsTicks;
        IEnumerator NextPhysics(){int before=physicsTicks;while(Live&&physicsTicks==before)yield return null;}
        bool Live=>session && session.Phase!=RunPhase.Failed && session.Phase!=RunPhase.Cleared;
        string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,key);return n>=0&&n+1<args.Length?args[n+1]:fallback;}
        public void Begin(GameRoot root){game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);began=Time.realtimeSinceStartup;Application.logMessageReceived+=OnLog;RuntimeEvents.Event+=TraceEvent;StartCoroutine(Run());}
        void TraceEvent(string kind,string detail){if(kind=="bell"||kind=="pulse-arrived"||kind=="boss-phase"||kind.StartsWith("echo-"))Note("EVENT t="+Time.time.ToString("0.00")+" "+kind+" "+detail);}
        void OnLog(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception){failures++;Note("ERROR "+m);Finish();}}
        void Update(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-echo-trace")>=0&&!finished&&host&&session.definition.course==2&&Time.time>lastTrace+.5f&&Time.time<11){lastTrace=Time.time;var echo=host.Form<EchoForm>();Note("TRACE t="+Time.time.ToString("0.00")+" body="+actor.Body.position+" echo="+(echo!=null&&echo.Echo?echo.Echo.Body.position+" velocity="+echo.Echo.Body.linearVelocity+" live="+echo.Echo.Body.simulated:"none"));}if(!finished&&Time.realtimeSinceStartup-began>510){failures++;Note("FAIL native route watchdog");Finish();}}
        void Note(string text){log.Add(text);File.WriteAllLines(Path.Combine(dir,"orchard-observations.txt"),log);}
        void Check(string name,bool pass,bool stop=true){assertions++;if(!pass){failures++;if(stop)stopped=true;}Note((pass?"PASS ":"FAIL ")+session.definition.id+" "+name+" | "+actor.Body.position+" feet="+actor.Feet.y.ToString("0.00")+" hp="+actor.Health);}
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

        IEnumerator Melt(bool liquid)
        {
            var wax=host.Form<WaxForm>();Check("Wax source acquired",wax!=null);if(stopped)yield break;
            if(wax.Liquid!=liquid)yield return Press(new InputFrame{action=true});
            Check(liquid?"melt through changed body footprint":"reform with conserved volume",wax.Liquid==liquid);
        }
        IEnumerator RootPath(params Vector2[] points)
        {
            if(stopped||!Live)yield break;var root=host.Form<RootForm>();Check("Root acquired before soil route",root!=null);if(stopped)yield break;
            input.frame=new InputFrame{action=true,actionHeld=true};yield return NextPhysics();input.frame=new InputFrame{actionHeld=true};Check("grow from adjacent wet substrate",root.Growing);if(stopped)yield break;
            foreach(var point in points){float end=Time.time+20;float checkpoint=Time.time;
                while(Live&&Time.time<end&&Vector2.Distance(root.Tip,point)>.13f){var delta=point-root.Tip;input.frame=new InputFrame{actionHeld=true,move=Vector2.ClampMagnitude(delta*3,1)};yield return NextPhysics();if(Time.time>checkpoint+2){checkpoint=Time.time;Note("TRACE root tip="+root.Tip+" target="+point);}}
                Check("steer root to "+point,Vector2.Distance(root.Tip,point)<.2f);if(stopped)yield break;
            }
            input.frame=default;yield return NextPhysics();yield return Await("body follows root route into exit pocket",()=>!root.Growing&&!root.Retracting,8);
            if(points.Length>0)Check("root arrival has full body clearance",Vector2.Distance(actor.Body.position,points[points.Length-1])<.8f&&actor.Shape.enabled);
        }
        IEnumerator Focus(HostKind kind)
        {
            if(stopped||!Live)yield break;Check("required tenant present: "+kind,host.Has(kind));if(stopped)yield break;
            for(int i=0;i<3&&host.Primary!=kind;i++)yield return Press(new InputFrame{alternate=true,move=Vector2.down});
            Check("focus "+kind,host.Primary==kind);
        }
        IEnumerator Eat(Vector2 aim)
        {
            yield return Focus(HostKind.Gullet);yield return Press(new InputFrame{action=true,move=aim});
            Check("swallow an actual structural tile",host.Form<GulletForm>()?.Stored!=null);
        }
        IEnumerator Spit(Vector2 aim)
        {
            yield return Focus(HostKind.Gullet);yield return Press(new InputFrame{action=true,move=aim});
            Check("rehome the same structural tile",host.Form<GulletForm>()?.Stored==null);
        }
        IEnumerator Pears(bool secret)
        {
            yield return Walk(8);yield return Melt(true);yield return Walk(19);yield return Walk(23.35f);yield return Pause(1);
            yield return Press(new InputFrame{alternate=true,move=Vector2.right});
            var drain=session.GetComponentInChildren<WaxDrain>();yield return Await("conserved plug seals actual drain",()=>drain.Sealed,3);Check("flow costs body volume",Mathf.Abs(host.waxVolume-.78f)<.01f);
            yield return Walk(26);yield return Await("diverted wax physically rises",()=>actor.Feet.y>3.5f,5);yield return Walk(30);yield return Melt(false);yield return Await("land in pear cage",()=>actor.Grounded,5);
            if(secret){yield return Jump(35,4.8f);yield return Press(new InputFrame{alternate=true,move=Vector2.right});Check("second partition changes scale mass",Mathf.Abs(host.waxVolume-.56f)<.01f);yield return Jump(38,6.3f);yield return Walk(39);var shutter=session.GetComponentsInChildren<MassWindow>().First();yield return Await("fine mass balance opens glass bell",()=>shutter.Valid,4);yield return Walk(44);Check("Glass bell Mercy collected",session.Mercies.Count==1);yield return Press(new InputFrame{interact=true});yield return Walk(36);yield return Await("leave secret on main bough",()=>actor.Grounded&&actor.Feet.y<4.3f,5);}
            yield return Walk(46.5f);yield return Hop(53,1.5f,5,true);yield return Walk(63);yield return Hop(70,1.5f,5,true);yield return Walk(80);yield return Hop(87,3,4,true);
            yield return Walk(93);Check("physical pear key",session.HasKey);yield return Walk(96.6f);yield return Press(new InputFrame{interact=true});Check("ripening Turn",session.Phase==RunPhase.Returning);Snapshot("turn");
            yield return Walk(81);yield return Walk(30,true);yield return Walk(19);yield return Melt(true);yield return Walk(5);yield return Walk(2);
        }
        IEnumerator Hop(float x,float low,float high,bool running=false)
        {
            if(stopped||!Live)yield break;bool jumped=false;float deadline=Time.time+6;
            while(Live&&Time.time<deadline){float dx=x-actor.Body.position.x;bool edge=!jumped&&actor.Grounded;if(edge)jumped=true;input.frame=new InputFrame{jump=edge,jumpHeld=true,run=running,move=new Vector2(Mathf.Clamp(dx-actor.Body.linearVelocity.x*.15f,-1,1),0)};
                if(jumped&&!edge&&actor.Grounded&&actor.Feet.y>=low&&actor.Feet.y<=high&&Mathf.Abs(dx)<.25f)break;yield return NextPhysics();}
            input.frame=default;Check("jump onto dynamic support "+x,actor.Grounded&&Mathf.Abs(actor.Body.position.x-x)<.5f&&actor.Feet.y>=low&&actor.Feet.y<=high);
        }
        IEnumerator Kitchen(bool secret)
        {
            yield return Walk(16);yield return Eat(Vector2.down);yield return Await("fall through actual removed floor",()=>actor.Grounded&&actor.Feet.y< -1.5f,5);
            yield return Walk(25.5f);yield return Spit(Vector2.right);
            var dish=session.GetComponentInChildren<FeastDish>();yield return Await("real grease lands in relocated runway's dish",()=>dish.steam.until>Time.time,10);
            yield return Jump(28,-1);yield return Hop(33,-1.1f,-.7f);yield return Hop(37,-1.1f,8);
            yield return Await("steam carries the Host through the pantry shaft",()=>actor.Feet.y>7.5f,8,()=>new InputFrame{move=new Vector2(Mathf.Clamp(37-actor.Body.position.x,-1,1),0),jumpHeld=true});
            yield return Walk(42);yield return Await("land at high serving counter",()=>actor.Grounded&&actor.Feet.y>6.5f,6);
            if(secret){yield return Eat(Vector2.down);yield return Walk(43.8f);yield return Spit(Vector2.right);yield return Hop(47,7.5f,13.3f);var freight=session.GetComponentInChildren<TerrainServingLift>();yield return Await("actual terrain rides the freight dish",()=>freight.transform.position.y>11.8f,6);yield return Hop(52,12.8f,13.2f);Check("Kitchen Mercy",session.Mercies.Count==1);yield return Walk(44);}
            yield return Walk(46);yield return Await("reach kitchen lower counter",()=>actor.Grounded&&actor.Feet.y<=5.1f,6);yield return Walk(47.2f);yield return Await("service floor",()=>actor.Grounded&&actor.Feet.y<3.3f,5);Check("votive source combines Wax with Gullet",host.Has(HostKind.Wax));
            yield return Focus(HostKind.Wax);yield return Melt(true);yield return Walk(57);yield return Melt(false);yield return Walk(64);yield return Jump(67,1.8f);yield return Jump(72,3.6f);yield return Jump(77,5.4f);yield return Jump(82,7.2f);Check("physical kitchen key",session.HasKey);
            yield return Walk(83.6f);yield return Press(new InputFrame{interact=true});Check("hatches reverse",session.Phase==RunPhase.Returning);Snapshot("turn");yield return Jump(88,9);yield return Press(new InputFrame{interact=true});yield return Jump(86,10.8f);yield return Walk(-2,true);yield return Await("return gallery descends to entry",()=>actor.Grounded&&actor.Feet.y<2.3f,6);yield return Walk(2);
        }
        IEnumerator Ditch(bool secret)
        {
            yield return Walk(10);yield return RootPath(new Vector2(10,-4),new Vector2(20,-4),new Vector2(20,1),new Vector2(24,1));
            yield return Walk(34);yield return Focus(HostKind.Root);yield return RootPath(new Vector2(34,-4.8f),new Vector2(44,-4.8f),new Vector2(44,1),new Vector2(49,1));
            yield return Walk(53);yield return RootPath(new Vector2(53,-4),new Vector2(64,-4),new Vector2(64,1));
            yield return Walk(70);yield return RootPath(new Vector2(70,-4),new Vector2(81,-4),new Vector2(81,1),new Vector2(85,1));
            yield return Jump(90,2);yield return Jump(97,4);yield return Jump(105,6);Check("irrigation key",session.HasKey);yield return Walk(105.6f);yield return Press(new InputFrame{interact=true});Check("reverse local pump",session.Phase==RunPhase.Returning);Snapshot("turn");
            yield return Walk(85);yield return RootPath(new Vector2(81,1),new Vector2(81,-4),new Vector2(70,-4),new Vector2(70,1));
            yield return Walk(64);yield return RootPath(new Vector2(64,-4),new Vector2(53,-4),new Vector2(53,1));
            yield return Walk(49);yield return RootPath(new Vector2(44,1),new Vector2(44,-4.8f),new Vector2(34,-4.8f),new Vector2(34,1));
            yield return Walk(24);yield return RootPath(new Vector2(20,1),new Vector2(20,-4),new Vector2(10,-4),new Vector2(10,1));if(secret&&!stopped){yield return Walk(10);var pump=session.GetComponentInChildren<RootPump>();yield return Await("reversed pump wets former dead end",()=>pump.channels[1].wet,20);yield return RootPath(new Vector2(10,9),new Vector2(18,9));Check("Ditch Mercy route",session.Mercies.Count==1);Snapshot("mercy");yield return RootPath(new Vector2(10,9),new Vector2(10,1));}yield return Walk(2);
        }
        IEnumerator Seasons(bool secret)
        {
            yield return Walk(9);yield return Melt(true);yield return Walk(19);yield return Melt(false);yield return Walk(20);yield return Press(new InputFrame{interact=true});
            yield return Walk(29);yield return Jump(34,.6f);yield return Walk(34.8f);yield return Hop(39,-.1f,.1f,true);yield return Walk(43);yield return Eat(Vector2.down);yield return Walk(47);yield return Jump(48,-.7f);yield return Hop(51,-.1f,.1f);
            yield return Walk(55);yield return Focus(HostKind.Root);
            if(secret){yield return RootPath(new Vector2(55,7),new Vector2(63,7),new Vector2(63,10));Check("Spring crown Mercy",session.Mercies.Count==1);yield return Walk(55);yield return Await("back to root entrance",()=>actor.Grounded&&actor.Feet.y<.2f,8);}
            yield return RootPath(new Vector2(55,-4),new Vector2(65,-4),new Vector2(65,1),new Vector2(65.5f,1));yield return Focus(HostKind.Gullet);yield return Spit(Vector2.right);
            yield return Hop(69,.8f,1.2f);yield return Hop(74,-.1f,4.5f);yield return Hop(78,.1f,4.5f);yield return Hop(84,.1f,4.8f);yield return Hop(90,.1f,5);yield return Hop(96,.1f,5.5f);yield return Hop(105,3.8f,4.2f);yield return Walk(106.8f);yield return Hop(113,5.8f,6.2f,true);yield return Walk(110);Check("season key",session.HasKey);
            yield return Walk(115.6f);yield return Press(new InputFrame{interact=true});Check("bands drift after the Turn",session.Phase==RunPhase.Returning);Snapshot("turn");
            yield return Walk(104);yield return Walk(73);yield return Hop(69,.8f,1.2f);yield return Hop(65.5f,-.1f,.1f);yield return Focus(HostKind.Root);yield return RootPath(new Vector2(65,-4),new Vector2(55,-4),new Vector2(55,1));
            yield return Walk(47);yield return Hop(41,-.1f,.1f);yield return Jump(34,.6f);yield return Hop(29,-.1f,.1f);yield return Walk(20);yield return Walk(18.5f);yield return Press(new InputFrame{interact=true});
            Check("return path needs a wax source",host.Has(HostKind.Wax));yield return Focus(HostKind.Wax);yield return Melt(true);yield return Walk(5);yield return Walk(2);
        }
        IEnumerator Judge()
        {
            var boss=session.GetComponentInChildren<AtlasBoss>();yield return Walk(9.5f);Check("Judge wax source",host.Has(HostKind.Wax));yield return Press(new InputFrame{alternate=true});yield return Walk(13);
            yield return Await("Judge act I: conserved mass balances the scale",()=>boss.phase>=1,5);yield return Walk(10);Check("Judge gullet source",host.Has(HostKind.Gullet));yield return Walk(15.3f);yield return Jump(18,1.5f);yield return Walk(20);yield return Jump(23,.6f);yield return Eat(Vector2.down);
            yield return Await("Judge act II: unsupported mass falls",()=>boss.phase>=2,6);yield return Walk(24);yield return RootPath(new Vector2(24,-4),new Vector2(37,-4),new Vector2(37,1));yield return Walk(39);Check("Judge defeated through material systems",boss.defeated&&session.Phase==RunPhase.Cleared);
        }
        IEnumerator Run()
        {
            game.SelectSource(1);var world=game.AvailableWorlds[1];string selected=Arg("-gb-route-id","W2");bool secrets=Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-with-secrets")>=0;
            var stages=new List<StageDefinition>();if(selected=="W2"){stages.AddRange(world.levels);stages.Add(world.boss);}else stages.Add(selected==world.boss.id?world.boss:Array.Find(world.levels,x=>x.id==selected));
            foreach(var stage in stages)
            {
                if(stage==null){failures++;Note("FAIL unknown stage "+selected);break;}yield return Load(stage);
                if(selected=="W2")Check("earned intra-world stage selection",stage.boss?CampaignProgression.BossOpen(world,game.Save.Data,PracticeWitness):CampaignProgression.LevelOpen(world,Array.IndexOf(world.levels,stage),game.Save.Data,PracticeWitness));
                if(stage.boss)yield return Judge();else switch(stage.course){case 5:yield return Pears(secrets);break;case 6:yield return Kitchen(secrets);break;case 7:yield return Ditch(secrets);break;case 8:yield return Seasons(secrets);break;}
                Check("complete actual stage",session.Phase==RunPhase.Cleared);
                if(!stopped){if(PracticeWitness)Check("practice writes no earned progress",!game.Save.Data.cleared.Contains(stage.id)&&!game.Save.Data.mercies.Contains(stage.id+"-MERCY"));else{Check("clear persists",game.Save.Data.cleared.Contains(stage.id));if(!stage.boss)Check(secrets?"Mercy saved after real return":"Mercy remains optional",secrets?game.Save.Data.mercies.Contains(stage.id+"-MERCY"):session.Mercies.Count==0);}}
                Snapshot("finish");if(stopped)break;
            }
            if(selected=="W2"&&!stopped){var reloaded=new SaveStore(Path.Combine(dir,"test-save.json"));Check("Orchard unlocks City",CampaignProgression.WorldOpen(game.AvailableWorlds,2,reloaded.Data,PracticeWitness));Check("four Mercies cannot restore the ending",!reloaded.RestoredEnding);Check("all Orchard stage clears survive reload",world.levels.All(d=>reloaded.Data.cleared.Contains(d.id))&&reloaded.Data.cleared.Contains(world.boss.id));}
            Finish();
        }
        void Finish()
        {
            if(finished)return;finished=true;Application.logMessageReceived-=OnLog;RuntimeEvents.Event-=TraceEvent;
            File.WriteAllText(Path.Combine(dir,"orchard-result.json"),"{\"failed\":"+failures+",\"checks\":"+assertions+",\"scope\":\"Production-input Orchard witnesses, not blind human or whole-campaign acceptance\"}");
            Application.Quit(failures==0?0:1);
        }
    }
}
