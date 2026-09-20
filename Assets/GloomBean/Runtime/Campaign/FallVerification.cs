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
    public sealed class FallVerification:MonoBehaviour
    {
        GameRoot game; StageSession session; ActorMotor actor; HostController host; ScriptedInput input;
        string dir; readonly List<string> log=new List<string>(); int failures,assertions; bool stopped,finished;
        bool PracticeWitness=>Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-practice-witness")>=0;
        float began,lastTrace; int physicsTicks;
        IEnumerator NextPhysics(){int before=physicsTicks;while(Live&&physicsTicks==before)yield return null;}
        bool Live=>session && session.Phase!=RunPhase.Failed && session.Phase!=RunPhase.Cleared;
        string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,key);return n>=0&&n+1<args.Length?args[n+1]:fallback;}
        public void Begin(GameRoot root){game=root;dir=root.reportDirectory;Directory.CreateDirectory(dir);began=Time.realtimeSinceStartup;Application.logMessageReceived+=OnLog;RuntimeEvents.Event+=TraceEvent;StartCoroutine(Run());}
        void TraceEvent(string kind,string detail){if(kind=="stitch-tug"||kind=="boss-phase"||kind.StartsWith("nave-")||kind=="bearing-released")Note("EVENT t="+Time.time.ToString("0.00")+" "+kind+" "+detail);}
        void OnLog(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception){failures++;Note("ERROR "+m);Finish();}}
        void Update(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-echo-trace")>=0&&!finished&&host&&session.definition.course==2&&Time.time>lastTrace+.5f&&Time.time<11){lastTrace=Time.time;var echo=host.Form<EchoForm>();Note("TRACE t="+Time.time.ToString("0.00")+" body="+actor.Body.position+" echo="+(echo!=null&&echo.Echo?echo.Echo.Body.position+" velocity="+echo.Echo.Body.linearVelocity+" live="+echo.Echo.Body.simulated:"none"));}if(!finished&&Time.realtimeSinceStartup-began>510){failures++;Note("FAIL native route watchdog");Finish();}}
        void Note(string text){log.Add(text);File.WriteAllLines(Path.Combine(dir,"fall-observations.txt"),log);}
        void Check(string name,bool pass,bool stop=true){assertions++;if(!pass){failures++;if(stop&&!stopped){Snapshot("first-failure");var coffinDebug=host.Form<CoffinForm>();if(coffinDebug!=null)Note("COFFIN blocker="+coffinDebug.LastBlocker+" hull="+coffinDebug.Hull.bounds+" angle="+actor.Body.rotation+" ground="+actor.GroundCollider);Note("PHYSICAL shape="+actor.Shape.bounds+" axis="+actor.Shape.direction+" forms="+string.Join(",",host.Forms.Select(f=>f.Kind.ToString())));
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


        IEnumerator Calm()
        {
            if(stopped||!Live)yield break;Check("censer acquired from swinging incense",host.Has(HostKind.Censer));if(stopped)yield break;
            yield return Await("stillness expands the local time field",()=>host.Form<CenserForm>().Radius>6,4);
        }
        IEnumerator Back(KneelingFigure f)
        {
            if(stopped||!Live)yield break;yield return Calm();if(stopped)yield break;
            yield return Await("kneeling back becomes a physical landing",()=>f.pose==KneelingFigure.Pose.Kneeling&&f.clock<f.hold-2,25);
            if(stopped)yield break;float end=Time.time+6,nextTrace=0;bool sent=false;
            while(Live&&Time.time<end){float dx=f.transform.position.x-actor.Body.position.x;bool edge=!sent&&actor.Grounded;if(edge)sent=true;
                input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx*1.4f-actor.Body.linearVelocity.x*.14f,-1,1),0),jump=edge,jumpHeld=true};yield return NextPhysics();if(Time.time>=nextTrace){nextTrace=Time.time+.12f;Note("BACK t="+Time.time.ToString("0.00")+" body="+actor.Body.position+" feet="+actor.Feet.y+" velocity="+actor.Body.linearVelocity+" target="+f.body.position+" pose="+f.pose+" phaseTime="+f.clock+" scale="+f.CurrentTimeScale);}
                if(sent&&!edge&&actor.Grounded&&actor.GroundCollider==f.GetComponent<Collider2D>()&&f.pose==KneelingFigure.Pose.Kneeling&&Mathf.Abs(dx)<.3f)break;}
            input.frame=default;Check("land on real back of "+f.name,actor.Grounded&&actor.GroundCollider==f.GetComponent<Collider2D>());
            if(stopped)yield break;yield return Calm();Check("incense changes this body's simulation rate",f.CurrentTimeScale<.55f);Snapshot("penitent-"+f.name.Replace(" ","-"));
        }
        IEnumerator Rain(bool secret)
        {
            yield return Walk(8);yield return Calm();yield return Walk(11);
            var figures=session.GetComponentsInChildren<KneelingFigure>(true);var outward=figures.Where(f=>f.name.StartsWith("Outward penitent")).OrderBy(f=>f.transform.position.x).ToArray();
            for(int i=0;i<outward.Length;i++){
                yield return Back(outward[i]);if(stopped)yield break;
                if(secret&&i==5){yield return Jump(38,11);yield return Jump(34,12.8f);yield return Jump(29,14.6f);yield return Walk(27.1f);
                    var bearer=figures.First(f=>f.name=="Penitent bearing the Mercy");yield return Back(bearer);yield return Jump(20,16.7f);yield return Calm();
                    yield return Await("standing left makes a physical kneeling ramp",()=>bearer.pose==KneelingFigure.Pose.Kneeling&&bearer.IsLeaning,40);
                    yield return Back(bearer);yield return Press(new InputFrame{move=Vector2.right,jump=true,jumpHeld=true});yield return Hold(new InputFrame{move=Vector2.right,jumpHeld=true},.3f);yield return Await("Mercy touched on a moving body",()=>session.Mercies.Count==1,4);Snapshot("carried-mercy");
                    yield return Walk(43);yield return Await("return from secret to safe side masonry",()=>actor.Grounded&&actor.Feet.y<11,8);
                }
                if(i<outward.Length-1)yield return Walk(outward[i].transform.position.x+1.05f);
            }
            yield return Walk(61);yield return Jump(66,15);yield return Walk(67.6f);Check("Keyling on upper rain shelter",session.HasKey);yield return Press(new InputFrame{interact=true});Check("Nail opens the denser upper vault",session.Phase==RunPhase.Returning);if(stopped)yield break;
            var returns=figures.Where(f=>f.name.StartsWith("Return penitent")).OrderByDescending(f=>f.transform.position.x).ToArray();
            yield return Walk(64.7f);
            foreach(var f in returns){yield return Back(f);if(stopped)yield break;yield return Walk(f.transform.position.x-1.05f);}
            yield return Jump(14,30.2f);yield return Walk(8);yield return Walk(4);yield return Walk(-1);yield return Await("descend to the remembered entrance",()=>actor.Grounded&&actor.Feet.y<1,12);yield return Walk(2);
        }
        IEnumerator Leap(float x,float low,float high,bool running=true)
        {
            if(stopped||!Live)yield break;bool jumped=false;float deadline=Time.time+6;
            while(Live&&Time.time<deadline){float dx=x-actor.Body.position.x;bool edge=!jumped&&actor.Grounded;if(edge)jumped=true;
                input.frame=new InputFrame{jump=edge,jumpHeld=true,run=running,move=new Vector2(Mathf.Clamp(dx-actor.Body.linearVelocity.x*.15f,-1,1),0)};
                if(jumped&&!edge&&actor.Grounded&&actor.Feet.y>=low&&actor.Feet.y<=high&&Mathf.Abs(dx)<.25f)break;yield return NextPhysics();}
            input.frame=default;Check("physical running landing "+x,actor.Grounded&&Mathf.Abs(actor.Body.position.x-x)<.5f&&actor.Feet.y>=low&&actor.Feet.y<=high);
        }
        IEnumerator Focus(HostKind kind)
        {
            if(stopped||!Live)yield break;Check("required tenant present: "+kind,host.Has(kind));
            for(int i=0;i<3&&host.Primary!=kind;i++)yield return Press(new InputFrame{move=Vector2.down,alternate=true});
            Check("focus "+kind,host.Primary==kind);
        }
        IEnumerator Fold(string group,Vector2 aim,float angle)
        {
            if(stopped||!Live)yield break;yield return Focus(HostKind.Stitch);yield return Press(new InputFrame{alternate=true});
            yield return Press(new InputFrame{action=true,move=aim});var stitch=host.Form<StitchForm>();
            Check("catch visible "+group+" seam",stitch.First&&stitch.First.group==group);if(stopped)yield break;
            yield return Press(new InputFrame{action=true});Check("thread connects two real edges",stitch.Active&&stitch.Second);if(stopped)yield break;
            yield return Press(new InputFrame{action=true});yield return Await("physical fold reaches "+angle,()=>Mathf.Abs(Mathf.DeltaAngle(stitch.Active.angle,angle))<2,12);
            Snapshot("fold-"+group);yield return Press(new InputFrame{alternate=true});
        }
        IEnumerator SeamBridge(bool secret)
        {
            yield return Walk(11.8f);yield return Fold("span-a",Vector2.up,30);yield return Walk(26);
            yield return Walk(27);yield return Fold("span-b",Vector2.up,30);yield return Walk(38.8f);yield return Calm();
            if(secret){yield return Fold("island",Vector2.up,35.6f);yield return Jump(39,10.6f);yield return Leap(41.6f,11.8f,13.6f,false);yield return Walk(49.5f);Check("Mercy island moves with folded support",session.GetComponentInChildren<FoldTipIsland>().transform.position.y<17);Check("suspended island Mercy",session.Mercies.Count==1);yield return Await("real run-up on the moved island",()=>actor.Body.position.x>=51.2f,2,()=>new InputFrame{move=Vector2.right,run=true});
                yield return Leap(58.2f,13.8f,14.2f);}
            else{yield return Walk(46);yield return Fold("span-c",Vector2.down,30);yield return Walk(59);}
            yield return Walk(65.6f);Check("bridge Keyling",session.HasKey);yield return Press(new InputFrame{interact=true});Check("Nail separates bridge halves",session.Phase==RunPhase.Returning);if(stopped)yield break;
            yield return Walk(57.5f);var farTower=session.GetComponentsInChildren<FoldPanel>().First(x=>x.name=="Far folding tower");yield return Await("far tower completes its physical Turn",()=>Mathf.Abs(Mathf.DeltaAngle(farTower.angle,80))<2,9);yield return Fold("span-c",new Vector2(-1,.45f),14.04f);yield return Walk(43);yield return Calm();
            Check("incense slows the moving bridge half",session.GetComponentsInChildren<StructuralDrift>().Any(x=>x.released&&x.TimeScale<.55f));
            yield return Walk(38);yield return Fold("span-b",Vector2.left,7.13f);yield return Walk(24);
            yield return Fold("span-a",Vector2.left,9.46f);yield return Walk(2);
        }
        IEnumerator Flip(int sign)
        {
            if(stopped||!Live)yield break;var coffin=host.Form<CoffinForm>();Check("rigid coffin acquired",coffin!=null);if(stopped)yield break;
            float x=actor.Body.position.x;bool orientation=coffin.Horizontal;
            yield return Press(new InputFrame{move=new Vector2(sign,0)});yield return Await("quarter turn changes footprint",()=>!coffin.IsFlipping&&coffin.Horizontal!=orientation&&Mathf.Abs(actor.Body.position.x-x)>1,1.2f);
            yield return Pause(.2f);
        }
        IEnumerator CoffinTo(float x,float seconds=16)
        {
            if(stopped||!Live)yield break;float end=Time.time+seconds;int sign=x>actor.Body.position.x?1:-1;
            while(Live&&Time.time<end&&sign*(x-actor.Body.position.x)>.45f){input.frame=new InputFrame{move=new Vector2(sign,0)};yield return NextPhysics();}
            input.frame=default;yield return Pause(.5f);Check("coffin crosses real ground to "+x,sign*(actor.Body.position.x-x)>-.8f&&RelativeFeet()> -1.2f);
        }
        IEnumerator Ferry(ProcessionCarrier ferry,float board,float exit)
        {
            if(stopped||!Live)yield break;
            yield return Await("pallbearer docks within a real flip",()=>ferry.Progress<.025f,20);
            yield return CoffinTo(board,4);yield return Await("coffin rides moving pallbearers",()=>actor.GroundCollider&&actor.GroundCollider.attachedRigidbody==ferry.body&&ferry.Progress>.94f,20);
            yield return CoffinTo(exit,5);
        }
        IEnumerator ClosedLids(bool secret)
        {
            yield return Walk(6.5f);yield return Await("undertaker closes the coffin",()=>host.Has(HostKind.Coffin),4,()=>new InputFrame{move=Vector2.right});if(stopped)yield break;yield return CoffinTo(12.5f);
            var ferries=session.GetComponentsInChildren<ProcessionCarrier>().OrderBy(x=>x.a.x).ToArray();yield return Ferry(ferries[0],16.4f,28);
            yield return CoffinTo(30);Check("seamstress can share the coffin",host.Has(HostKind.Stitch));if(stopped)yield break;
            if(secret){yield return CoffinTo(34);var c=host.Form<CoffinForm>();if(!c.Horizontal)yield return Flip(1);
                var lift=session.GetComponentInChildren<InspectionLift>();Check("horizontal orientation selected before inspection",c.Horizontal);yield return Press(new InputFrame{interact=true});
                yield return Await("grille physically lowers the body",()=>actor.Feet.y< -2.7f,7);yield return Await("low inspection passage grants Mercy",()=>session.Mercies.Count==1,10);
                yield return Await("inspection trip brings the body back",()=>lift.trips>0&&!lift.moving&&actor.Feet.y>-.1f,15);Check("horizontal lid was not blocked by the ceiling",!lift.blocked);Snapshot("coffin-inspection");}
            if(stopped)yield break;yield return CoffinTo(41);if(stopped)yield break;var coffin=host.Form<CoffinForm>();if(!coffin.Horizontal)yield return Flip(-1);
            yield return Flip(1);yield return Flip(1);var press=session.GetComponentInChildren<BearingPress>();Check("horizontal footprint beneath the bearing head",coffin.Horizontal&&Mathf.Abs(actor.Body.position.x-44)<2.3f);
            yield return Press(new InputFrame{interact=true});yield return Await("actual bracing releases the counterweight",()=>press.released,6);yield return Await("counterweight physically retracts",()=>press.counterweight.position.y>5.5f,4);Snapshot("bearing-contact");
            yield return CoffinTo(47.2f);yield return Ferry(ferries[1],50.4f,63);yield return CoffinTo(78.8f);Check("procession Keyling",session.HasKey);yield return Press(new InputFrame{interact=true});Check("return silences bells",session.Phase==RunPhase.Returning);if(stopped)yield break;
            yield return CoffinTo(64);yield return Fold("procession-b",Vector2.up,0);yield return CoffinTo(40);
            yield return CoffinTo(28);yield return Fold("procession-a",Vector2.up,0);yield return CoffinTo(6);yield return Walk(2);
        }
        float RelativeFeet(){var frame=session.GetComponentInChildren<DescentController>();return actor.Feet.y+(frame?frame.fallen:0);}
        IEnumerator FrameJump(float x,float floor,bool running=false)
        {
            if(stopped||!Live)yield break;float end=Time.time+6;bool sent=false;
            while(Live&&Time.time<end){float dx=x-actor.Body.position.x;bool edge=!sent&&actor.Grounded;if(edge)sent=true;
                input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx-actor.Body.linearVelocity.x*.1f,-1,1),0),jump=edge,jumpHeld=true,run=running};
                if(sent&&!edge&&actor.Grounded&&Mathf.Abs(RelativeFeet()-floor)<.35f&&Mathf.Abs(dx)<.25f)break;yield return NextPhysics();}
            input.frame=default;Check("landing within falling frame "+x+" / "+floor,actor.Grounded&&Mathf.Abs(RelativeFeet()-floor)<.4f&&Mathf.Abs(actor.Body.position.x-x)<.5f);
        }
        IEnumerator Board(MotionPlatform platform)
        {
            if(stopped||!Live)yield break;float end=Time.time+9,lastJump=-10;var shape=platform.GetComponent<Collider2D>();
            while(Live&&Time.time<end&&actor.GroundCollider!=shape){float dx=platform.transform.position.x-actor.Body.position.x;bool jump=actor.Grounded&&Time.time-lastJump>.7f;if(jump)lastJump=Time.time;
                input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx-actor.Body.linearVelocity.x*.12f,-1,1),0),jump=jump,jumpHeld=true};yield return NextPhysics();}
            input.frame=default;Check("board real "+platform.name,actor.Grounded&&actor.GroundCollider==shape);
        }
        IEnumerator Freefall(bool secret)
        {
            yield return Walk(10);Check("opening incense tenant",host.Has(HostKind.Censer));yield return Walk(28);Check("transept stitch source",host.Has(HostKind.Stitch));
            yield return Walk(80.5f);Check("cathedral Keyling",session.HasKey);yield return Press(new InputFrame{interact=true});Check("Nail releases actual cathedral",session.Phase==RunPhase.Returning);if(stopped)yield break;
            var frame=session.GetComponentInChildren<DescentController>();var mover=session.GetComponentsInChildren<MotionPlatform>().First(x=>x.name=="Collapsing transept");
            yield return Walk(74.5f);yield return Await("transept aligns with its lower dock",()=>mover.transform.position.x>71.5f,15);yield return Board(mover);yield return Calm();
            Check("Censer slows physical transept",mover.timeScale<.55f);yield return Await("ride transept toward attached tower",()=>mover.transform.position.x<62.7f,40);yield return FrameJump(61,6);
            yield return Fold("transept",Vector2.left,150);yield return Walk(47.2f);yield return Await("coffin can bear the new joint",()=>host.Has(HostKind.Coffin),4,()=>new InputFrame{move=Vector2.left});if(stopped)yield break;
            yield return CoffinTo(43);var coffin=host.Form<CoffinForm>();if(!coffin.Horizontal)yield return Flip(-1);yield return Press(new InputFrame{interact=true});
            var load=session.GetComponentInChildren<FallingLoad>();var hoist=session.GetComponentInChildren<ImpactHoist>();yield return Await("falling nave strikes actual braced body",()=>load.impacted,8);Check("nave collision has real impact speed",load.impactSpeed>2.5f);
            yield return Await("bearing collision raises the return hoist",()=>hoist.Arrived,8);Snapshot("impact-hoist");yield return CoffinTo(45.5f);yield return Await("open the lid at the lifted grave mouth",()=>!host.Has(HostKind.Coffin),3);
            if(stopped)yield break;
            if(secret){var chapel=session.GetComponentsInChildren<MotionPlatform>().First(x=>x.name=="Passing Mercy chapel");yield return Await("chapel aligns for the limited detour",()=>chapel.transform.position.x<50.3f,18);yield return Board(chapel);
                Check("passing chapel Mercy",session.Mercies.Count==1);Snapshot("passing-chapel");yield return Await("chapel returns within jumping distance",()=>chapel.transform.position.x<50.5f,20);yield return FrameJump(43.5f,25,true);}
            yield return Walk(38.8f);yield return FrameJump(35,26.6f);yield return Walk(33);yield return FrameJump(29,28.2f);yield return Walk(2);
            Check("escape precedes altitude exhaustion",frame.RemainingAltitude>0&&frame.fallen>5);
        }
        IEnumerator Run()
        {
            game.SelectSource(1);var world=game.AvailableWorlds[3];
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-require-earned-world")>=0&&!CampaignProgression.WorldOpen(game.AvailableWorlds,3,game.Save.Data,false)){failures++;Note("FAIL Fall was not earned by the supplied real save");Finish();yield break;}
            string selected=Arg("-gb-route-id","GB-L13");bool secrets=Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-with-secrets")>=0;
            var stages=new List<StageDefinition>();if(selected=="W4"){stages.AddRange(world.levels);stages.Add(world.boss);}else stages.Add(selected==world.boss.id?world.boss:Array.Find(world.levels,x=>x.id==selected));
            foreach(var stage in stages){if(stage==null){failures++;Note("FAIL unknown Fall stage "+selected);break;}yield return Load(stage);
                if(selected=="W4")Check("earned intra-world stage selection",stage.boss?CampaignProgression.BossOpen(world,game.Save.Data,PracticeWitness):CampaignProgression.LevelOpen(world,Array.IndexOf(world.levels,stage),game.Save.Data,PracticeWitness));
                if(stage.course==13)yield return Rain(secrets);else if(stage.course==14)yield return SeamBridge(secrets);else if(stage.course==15)yield return ClosedLids(secrets);else if(stage.course==16)yield return Freefall(secrets);else Check("route not implemented yet",false);
                Check("stage completes through physical exit or boss solution",session.Phase==RunPhase.Cleared);
                if(!stopped){if(PracticeWitness)Check("practice writes no earned progress",!game.Save.Data.cleared.Contains(stage.id)&&!game.Save.Data.mercies.Contains(stage.id+"-MERCY"));else{Check("completion survives save",game.Save.Data.cleared.Contains(stage.id));if(!stage.boss)Check(secrets?"Mercy persists after real return":"Mercy remains optional",secrets?game.Save.Data.mercies.Contains(stage.id+"-MERCY"):session.Mercies.Count==0);}}Snapshot("finish");if(stopped)break;}
            if(selected=="W4"&&!stopped){var save=new SaveStore(Path.Combine(dir,"test-save.json"));Check("Fall earns the False Empyrean",CampaignProgression.WorldOpen(game.AvailableWorlds,4,save.Data,PracticeWitness));Check("sixteen Mercies cannot restore the ending",!save.RestoredEnding);}
            Finish();
        }
        void Finish(){if(finished)return;finished=true;Application.logMessageReceived-=OnLog;RuntimeEvents.Event-=TraceEvent;
            File.WriteAllText(Path.Combine(dir,"fall-result.json"),"{\"failed\":"+failures+",\"checks\":"+assertions+",\"scope\":\"Production-input Fall witnesses, not blind human or full-game acceptance\"}");
            File.WriteAllText(Path.Combine(dir,"saved-progress.json"),JsonUtility.ToJson(game.Save.Data,true));Application.Quit(failures==0?0:1);}
    }
}
