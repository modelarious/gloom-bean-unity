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
            yield return Jump(10.5f,2);yield return Jump(13,2);
            yield return Await("reflection lands after its cabinet detour",()=>host.Form<MirrorForm>()!=null&&host.Form<MirrorForm>().Twin.Grounded,4);
            yield return Jump(16,4);if(stopped)yield break;
            var joint=session.GetComponentsInChildren<Gate>().First(g=>g.name=="Paired apartment interlock");
            yield return Await("two bodies hold the balcony scales",()=>joint.opened,4);Snapshot("paired-balconies");if(stopped)yield break;
            if(secret&&!stopped){yield return Walk(14.1f);yield return Jump(10,6);yield return Press(new InputFrame{interact=true});
                var shutter=session.GetComponentInChildren<WindowShutter>();var secretGate=session.GetComponentsInChildren<Gate>().First(g=>g.name=="Off-register Mercy shutters");
                yield return Await("physical shutter displaces the twin onto off-register scale",()=>secretGate.opened,6);yield return Await("shutter completes an actual movement",()=>shutter.IsClosed,2);Snapshot("shutter-desynchronization");yield return Walk(3);Check("Mercy inside the original apartment",session.Mercies.Count==1);yield return Walk(14);}
            if(stopped)yield break;yield return Walk(44);yield return Await("matte velvet removes reflection",()=>!host.Has(HostKind.Mirror),4);yield return Press(new InputFrame{interact=true});
            yield return Await("released lift physically carries the Host",()=>actor.Grounded&&actor.Feet.y>3.9f,7,()=>new InputFrame{move=new Vector2(Mathf.Clamp(44-actor.Body.position.x,-1,1),0)});
            yield return Walk(45.2f);yield return Press(new InputFrame{interact=true});yield return Pause(.15f);
            Check("curtains create real cast-shadow bridge collision",session.GetComponentsInChildren<SunShutter>().All(s=>s.GetComponent<Collider2D>().enabled));
            yield return Jump(49,4.3f);yield return Walk(50.7f);yield return Jump(55,4.95f);yield return Walk(56.7f);yield return Jump(61,5.6f);yield return Walk(62.7f);yield return Jump(67,6.25f);yield return Walk(68.7f);yield return Jump(72,7);yield return Walk(73.4f);
            yield return Jump(76,8.2f);yield return Walk(77.5f);yield return Jump(81,10);yield return Walk(82.5f);yield return Jump(86,11.8f);yield return Walk(87.5f);yield return Jump(91,13.6f);
            if(stopped)yield break;Check("roof key collected by contact",session.HasKey);yield return Jump(96,13.6f);yield return Walk(97.6f);yield return Press(new InputFrame{interact=true});
            Check("one sun extinguishes at the Turn",session.Phase==RunPhase.Returning);yield return Pause(.1f);Check("only eastern shadow bridges disappear",session.GetComponentsInChildren<SunShutter>().All(s=>s.GetComponent<Collider2D>().enabled==(s.phase==0)));Snapshot("turn");
            yield return Walk(77.8f);yield return Await("return vanity creates a new paired route",()=>host.Has(HostKind.Mirror),4);yield return Walk(75);yield return Jump(70,7.5f);yield return Walk(67.5f);
            if(stopped)yield break;var returnGate=session.GetComponentsInChildren<Gate>().First(g=>g.name=="Return apartment interlock");yield return Await("changed furniture solved by both return bodies",()=>returnGate.opened,4);Snapshot("return-mirror");
            yield return Walk(61.5f);yield return Await("return velvet releases both-body constraint",()=>!host.Has(HostKind.Mirror),4);yield return Walk(43);yield return Press(new InputFrame{interact=true});yield return Await("return reaches old street",()=>actor.Grounded&&actor.Feet.y<1,6);yield return Walk(2);
        }
        IEnumerator Fresco(bool secret)
        {
            yield return Walk(17);Check("cherub opens the stone interior",host.Has(HostKind.InsideOut));
            yield return Jump(20,1);yield return Jump(22,2);yield return Jump(24.2f,3);yield return Jump(25.6f,5);yield return Jump(27.5f,6);yield return Walk(30.25f);
            yield return Jump(32.2f,7);yield return Jump(34.2f,8);yield return Jump(35.6f,10);yield return Jump(37.5f,11);if(stopped)yield break;
            Snapshot("inside-painted-arch");yield return Walk(42.4f);yield return Await("empty frame restores ordinary collision",()=>!host.Has(HostKind.InsideOut),3);
            if(secret&&!stopped){yield return Walk(41.1f);yield return Press(new InputFrame{interact=true});var region=session.GetComponentInChildren<TopologyRegion>();
                yield return Await("scaffold finishes actual moon topology",()=>region.SupplementClosed,5);yield return Walk(39.6f);Check("re-enter changed fresco",host.Has(HostKind.InsideOut));
                yield return Jump(37.5f,11);yield return Walk(29.5f);yield return Jump(27.5f,13);yield return Jump(25.5f,14);yield return Walk(21);Check("Mercy inside closed moon",session.Mercies.Count==1);Snapshot("moon-interior");
                yield return Walk(30);yield return Walk(37);yield return Walk(42.4f);yield return Await("leave moon through the same frame",()=>!host.Has(HostKind.InsideOut),3);yield return Walk(41.1f);yield return Press(new InputFrame{interact=true});yield return Await("erase the temporary moon route safely from outside",()=>!region.SupplementClosed,5);}
            if(stopped)yield break;yield return Walk(59);yield return Await("Mirror acquired on landing in second court",()=>host.Has(HostKind.Mirror)&&actor.Grounded,4);yield return Walk(64);yield return Walk(62);
            var gate=session.GetComponentsInChildren<Gate>().First(g=>g.name=="Fresco paired lift brake");yield return Await("paint-pot desynchronization aligns both scales",()=>gate.opened,4);yield return Press(new InputFrame{interact=true});yield return Walk(66);yield return Await("drawn curtain removes the twin",()=>!host.Has(HostKind.Mirror),3);
            yield return Walk(70);yield return Jump(74,0);yield return Walk(82);yield return Jump(84,2);yield return Walk(85.3f);yield return Jump(89,3.8f);yield return Walk(90.3f);yield return Jump(94,5.6f);yield return Walk(95.3f);yield return Jump(99,7.4f);yield return Walk(105.6f);if(stopped)yield break;
            Check("fresco roof key",session.HasKey);yield return Press(new InputFrame{interact=true});Check("fresco peels into return gallery",session.Phase==RunPhase.Returning);yield return Jump(103,9.2f);yield return Jump(98,11);yield return Walk(42.4f);yield return Walk(39.6f);Check("return uses physical interior again",host.Has(HostKind.InsideOut));
            yield return Jump(37.5f,11);yield return Walk(34.2f);yield return Walk(32.2f);yield return Walk(29);yield return Walk(27.5f);yield return Walk(25.6f);yield return Walk(24.2f);yield return Walk(22);yield return Walk(20);yield return Walk(17);yield return Walk(14.3f);yield return Await("street-side empty frame cures",()=>!host.Has(HostKind.InsideOut),3);yield return Walk(2);
        }
        IEnumerator Align(float x)
        {
            if(stopped||!Live)yield break;float end=Time.time+5;
            while(Live&&Time.time<end){float dx=x-actor.Body.position.x;if(Mathf.Abs(dx)<.05f&&Mathf.Abs(actor.Body.linearVelocity.x)<.12f)break;input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx*6-actor.Body.linearVelocity.x*.7f,-1,1),0)};yield return NextPhysics();}
            input.frame=default;Check("align physical body at "+x,Mathf.Abs(actor.Body.position.x-x)<.08f);
        }
        IEnumerator Plane(int target)
        {
            if(stopped||!Live)yield break;var form=host.Form<ParallaxForm>();Check("perspective tenant exists",form!=null);if(stopped)yield break;float end=Time.time+3;
            while(Live&&Time.time<end&&form.Plane!=target){input.frame=new InputFrame{move=new Vector2(0,Mathf.Sign(target-form.Plane))};yield return NextPhysics();}
            input.frame=default;Check("project into plane "+target,form.Plane==target);yield return Pause(.1f);
        }
        IEnumerator PlaneJump(float x,float top,int target)
        {
            if(stopped||!Live)yield break;var form=host.Form<ParallaxForm>();Check("plane jump has perspective tenant",form!=null);if(stopped)yield break;
            float end=Time.time+7,beginFoot=actor.Feet.y,nextTrace=0;bool sent=false;
            while(Live&&Time.time<end){float dx=x-actor.Body.position.x;bool jump=!sent&&actor.Grounded;if(jump)sent=true;
                float depth=sent&&actor.Feet.y>beginFoot+.45f&&form.Plane!=target?Mathf.Sign(target-form.Plane):0;
                input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx*2-actor.Body.linearVelocity.x*.15f,-1,1),depth),jump=jump,jumpHeld=true};yield return NextPhysics();
                if(form.Plane!=target&&Time.time>=nextTrace){nextTrace=Time.time+.35f;Note("DEPTH target="+target+" actual="+form.Plane+" pos="+actor.Body.position+" size="+actor.Shape.size+" mask="+actor.collisionMask+" excluded="+actor.Shape.excludeLayers.value+" ground="+(actor.GroundCollider?actor.GroundCollider.name+":"+actor.GroundCollider.gameObject.layer:"none")+" notice="+session.Message);}
                if(sent&&!jump&&actor.Grounded&&Mathf.Abs(actor.Feet.y-top)<.25f&&Mathf.Abs(dx)<.22f&&form.Plane==target)break;}
            input.frame=default;Check("land projected body "+target+" at "+x+" / "+top,actor.Grounded&&Mathf.Abs(actor.Feet.y-top)<.3f&&Mathf.Abs(actor.Body.position.x-x)<.5f&&form.Plane==target);
        }
        IEnumerator Tax(bool secret)
        {
            yield return Walk(11);yield return Plane(0);yield return Walk(12.2f);yield return PlaneJump(17,1.195f,0);yield return Walk(18.5f);yield return PlaneJump(23,3,1);yield return Walk(25);yield return PlaneJump(29,4.72f,2);yield return Walk(32);yield return PlaneJump(35,6.195f,0);yield return Walk(36.8f);yield return PlaneJump(41,8,1);
            if(secret&&!stopped){yield return Press(new InputFrame{interact=true});yield return Pause(.15f);yield return Jump(38,10);yield return Jump(40.5f,12);yield return Align(42);var gate=session.GetComponentsInChildren<Gate>().First(g=>g.name=="Tax form registration clamp");yield return Await("body contacts both printed margins",()=>gate.opened,3);yield return Walk(48);Check("registered tax-form Mercy",session.Mercies.Count==1);Snapshot("middle-size-registration");yield return Walk(50);yield return Await("leave secret onto near counter",()=>actor.Grounded&&actor.Feet.y<11,6);}
            else{yield return Walk(43.4f);yield return PlaneJump(47,9.72f,2);}
            if(stopped)yield break;if(host.Form<ParallaxForm>().Plane!=2)yield return PlaneJump(51,9.72f,2);yield return Walk(53);yield return Press(new InputFrame{interact=true});
            var stamp=session.GetComponentInChildren<PerspectiveStamp>();Check("clerk relocates furniture into matching plane",stamp.geometry[0].plane==2);yield return Walk(60);yield return Await("flat sign removes depth before cabinet",()=>!host.Has(HostKind.Parallax),4);yield return Walk(60.5f);yield return Await("drop to filing-cabinet interior entrance",()=>actor.Grounded&&actor.Feet.y<1,7);Check("flensing clerk exposes cabinet interior",host.Has(HostKind.InsideOut));
            yield return Jump(63,1);yield return Jump(65,2);yield return Jump(67.2f,3);yield return Jump(68.6f,5);yield return Jump(70.5f,6);yield return Walk(73.25f);yield return Jump(75.2f,7);yield return Jump(77.2f,8);yield return Jump(78.6f,10);yield return Jump(80.5f,11);yield return Walk(85.4f);yield return Await("cabinet frame returns ordinary body",()=>!host.Has(HostKind.InsideOut),3);yield return Walk(89);
            yield return Walk(91.4f);yield return PlaneJump(97,11.195f,0);yield return Walk(99);yield return PlaneJump(106,12.92f,2);yield return Walk(110.5f);yield return PlaneJump(118,14,2);yield return Walk(119.6f);if(stopped)yield break;
            Check("office key physically collected",session.HasKey);yield return Press(new InputFrame{interact=true});Check("office counters retract during closure",session.Phase==RunPhase.Returning&&stamp.reversed);Snapshot("office-closure");
            yield return Walk(116);yield return PlaneJump(111,16.195f,0);yield return Walk(108.5f);yield return PlaneJump(102,16.3f,1);yield return Walk(98.5f);yield return PlaneJump(92,15.42f,2);yield return Walk(84);yield return Await("monochrome sign restores body for fire escape",()=>!host.Has(HostKind.Parallax),3);yield return Walk(40);yield return Walk(2);
        }
        IEnumerator RideRoom(RoomOrbit room,Vector2 destination)
        {
            if(stopped||!Live)yield break;
            yield return Await("room carries its actual occupant to "+destination,()=>room.Settled&&Vector2.Distance(room.transform.position,destination)<.2f&&actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.transform.IsChildOf(room.transform),8);
            Check("support belongs to preserved moving room",actor.GroundCollider&&actor.GroundCollider.transform.IsChildOf(room.transform));
        }
        IEnumerator Hotel(bool secret)
        {
            yield return Walk(9);Check("perspective fly acquired in hotel lobby",host.Has(HostKind.Parallax));yield return Walk(10);yield return Jump(12,2);yield return Walk(13.4f);yield return Jump(17,4);yield return Walk(18.4f);yield return Jump(22,6);yield return Walk(38);if(stopped)yield break;
            var room=session.GetComponentsInChildren<RoomOrbit>().First(r=>r.name=="Orbiting room 0");yield return Plane(0);yield return RideRoom(room,new Vector2(22,14));if(stopped)yield break;
            yield return Walk(18.5f);Check("vanity reflection composes with perspective",host.Has(HostKind.Mirror)&&host.Has(HostKind.Parallax));
            yield return Jump(17,15.395f);var stamp=room.GetComponentInChildren<ReplicaDepthStamp>();var brake=session.GetComponentInChildren<OrbitBrake>();
            yield return Await("reflection receives a distinct depth footprint",()=>stamp.Stamped,4);
            yield return Align(17);yield return Await("two depths hold the same room brake",()=>brake.released,5);Snapshot("split-depth-room-brake");yield return Press(new InputFrame{interact=true});
            yield return RideRoom(room,new Vector2(38,22));if(stopped)yield break;yield return Walk(33);yield return Await("roof velvet releases the reflected tenant",()=>!host.Has(HostKind.Mirror),4);
            yield return Jump(34,23.4f);yield return Walk(35.5f);Check("formerly exterior wall becomes inside-out corridor",host.Has(HostKind.InsideOut));
            yield return Jump(37.5f,24.4f);yield return Jump(39,25.4f);yield return Jump(41,26.4f);yield return Jump(43,28.4f);yield return Walk(47);yield return Await("wall frame returns normal collision",()=>!host.Has(HostKind.InsideOut),3);
            yield return Walk(50);Check("depth transfer remains available past wall",host.Has(HostKind.Parallax));yield return Walk(52);yield return PlaneJump(57,29.995f,0);yield return Walk(59);yield return PlaneJump(65,31.72f,2);yield return Walk(68);yield return PlaneJump(73,33.2f,2);yield return Walk(73.6f);if(stopped)yield break;
            Check("hotel Keyling collected physically",session.HasKey);yield return Press(new InputFrame{interact=true});Check("facade removal releases orbiting rooms",session.Phase==RunPhase.Returning&&room.running);yield return Walk(71.5f);yield return Await("flat hotel sign restores normal body",()=>!host.Has(HostKind.Parallax),4);Snapshot("hotel-return-orbit");
            if(secret&&!stopped){yield return Walk(47);yield return Press(new InputFrame{interact=true});var lift=session.GetComponentsInChildren<ApartmentLift>().First();
                yield return Await("chandelier winch physically docks at its back",()=>Vector2.Distance(lift.transform.position,lift.upper)<.1f&&actor.Grounded,8,()=>new InputFrame{move=new Vector2(Mathf.Clamp(lift.transform.position.x-actor.Body.position.x,-1,1),0)});
                yield return Walk(33.1f);Check("reveal inside of lobby chandelier",host.Has(HostKind.InsideOut));yield return Walk(38);Check("lobby chandelier Mercy collected",session.Mercies.Count==1);Snapshot("chandelier-interior");yield return Walk(30.8f);yield return Await("chandelier frame restores exterior body",()=>!host.Has(HostKind.InsideOut),3);yield return Walk(31.5f);yield return Press(new InputFrame{interact=true});
                yield return Await("winch returns through same physical space",()=>Vector2.Distance(lift.transform.position,lift.lower)<.1f&&actor.Grounded,8,()=>new InputFrame{move=new Vector2(Mathf.Clamp(lift.transform.position.x-actor.Body.position.x,-1,1),0)});}
            if(stopped)yield break;yield return Walk(41);var orbiting=session.GetComponentsInChildren<RoomOrbit>();
            yield return Await("land on an actual moving hotel roof",()=>actor.Grounded&&actor.GroundCollider&&actor.GroundCollider.GetComponentInParent<RoomOrbit>(),10,()=>{var best=orbiting.OrderByDescending(r=>r.transform.position.y).First();return new InputFrame{move=new Vector2(Mathf.Clamp(best.transform.position.x-actor.Body.position.x,-1,1),0)};});
            yield return Walk(28);yield return Walk(23);yield return Walk(18);yield return Walk(13);yield return Walk(8);yield return Walk(3);yield return Walk(-2);yield return Await("lobby exit reached from rear of known rooms",()=>actor.Grounded&&actor.Feet.y<1,8);yield return Walk(2);
        }
        IEnumerator Strike(SurveyorCore core)
        {
            if(stopped||!Live)yield break;Check("physical weak point exists",core!=null);if(stopped)yield break;float end=Time.time+5,nextAttack=0;
            while(Live&&Time.time<end&&!core.struck){float dx=core.transform.position.x-actor.Body.position.x;bool hit=Mathf.Abs(dx)<2&&Time.time>=nextAttack;if(hit)nextAttack=Time.time+.55f;
                input.frame=new InputFrame{move=new Vector2(Mathf.Clamp(dx,-1,1),0),attack=hit};yield return NextPhysics();}
            input.frame=default;Check("actual tackle strikes "+core.name,core.struck);yield return Pause(.45f);
        }
        IEnumerator Surveyor()
        {
            var boss=session.GetComponentInChildren<AtlasBoss>();yield return Walk(7);Check("Surveyor creates real player reflection",host.Has(HostKind.Mirror));yield return Walk(12.1f);
            var first=session.GetComponentsInChildren<SurveyorCore>().First(c=>c.name=="Surveyor exposed reflection");var twin=host.Form<MirrorForm>()?.Twin;
            yield return Await("cabinet pins present body while twin reaches exposed reflection",()=>twin&&twin.Body.position.x<33.2f,5,()=>new InputFrame{move=Vector2.right});
            int before=actor.Health;Vector2 position=actor.Body.position;yield return Press(new InputFrame{move=Vector2.right,attack=true});yield return Await("reflected tackle defeats Surveyor act I",()=>boss.phase>=1,3);
            Check("first act transition preserves health and location",actor.Health<=before&&Vector2.Distance(position,actor.Body.position)<3);Snapshot("reflected-weak-point");if(stopped)yield break;
            yield return Walk(14);Check("enter enclosed attack as inside-out body",host.Has(HostKind.InsideOut));
            yield return Jump(17,1);yield return Jump(19,2);yield return Jump(21.2f,3);yield return Jump(22.6f,5);yield return Jump(24.5f,6);yield return Walk(27.25f);yield return Jump(29.2f,7);yield return Jump(31.2f,8);yield return Jump(32.6f,10);yield return Jump(34.5f,11);if(stopped)yield break;
            var second=session.GetComponentsInChildren<SurveyorCore>().First(c=>c.name=="Surveyor internal outline");before=actor.Health;yield return Strike(second);yield return Await("internal hit opens moving calipers",()=>boss.phase>=2,3);Check("second act does not heal the player",actor.Health<=before);Snapshot("interior-weak-point");
            yield return Walk(37);Check("perspective source acquired without a forced grant",host.Has(HostKind.Parallax));yield return Walk(38);yield return PlaneJump(42.4f,12.395f,0);if(stopped)yield break;
            var cores=session.GetComponentsInChildren<SurveyorCore>();yield return Strike(cores.First(c=>c.name=="Surveyor caliper 0"));yield return Walk(44.5f);yield return PlaneJump(51.4f,14.2f,1);if(stopped)yield break;
            yield return Strike(cores.First(c=>c.name=="Surveyor caliper 1"));yield return Walk(54.5f);yield return PlaneJump(60.4f,15.92f,2);if(stopped)yield break;
            yield return Strike(cores.First(c=>c.name=="Surveyor caliper 2"));Check("three physical depth weak points defeat Surveyor",boss.defeated&&session.Phase==RunPhase.Cleared);Snapshot("surveyor-victory");
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
                if(stage.boss)yield return Surveyor();else switch(stage.course){case 9:yield return Suns(secrets);break;case 10:yield return Fresco(secrets);break;case 11:yield return Tax(secrets);break;case 12:yield return Hotel(secrets);break;default:Check("route not implemented yet",false);break;}
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
