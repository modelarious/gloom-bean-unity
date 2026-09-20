using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GloomBean.Foundation
{
    [Serializable]public sealed class VerificationCase{public string name;public bool pass;public string observation;}
    [Serializable]public sealed class VerificationReport{public string unity,platform,scope;public int passed,failed,exceptions;public List<VerificationCase> tests=new List<VerificationCase>();}
    public sealed class FoundationVerification : MonoBehaviour
    {
        public readonly VerificationReport report=new VerificationReport();
        GameRoot game;StageBuilder b;GameObject fixture;ActorMotor actor;ScriptedInput input;
        bool fatalScheduled;
        readonly WaitForFixedUpdate tick=new WaitForFixedUpdate();
        public void Begin(GameRoot root){game=root;Application.logMessageReceived+=Log;StartCoroutine(Run());}
        void Log(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error){report.exceptions++;Debug.LogWarning("Verification captured: "+message);if(!fatalScheduled){fatalScheduled=true;StartCoroutine(FatalExit());}}}
        IEnumerator FatalExit()
        {
            yield return null;
            report.failed+=report.exceptions;
            File.WriteAllText(Path.Combine(game.reportDirectory,"verification.json"),JsonUtility.ToJson(report,true));
            Application.Quit(1);
        }
        public void Check(string name,bool pass,string observation="")
        {
            report.tests.Add(new VerificationCase{name=name,pass=pass,observation=observation});if(pass)report.passed++;else report.failed++;
            Debug.Log((pass?"PASS ":"FAIL ")+name+" - "+observation);
        }
        IEnumerator Steps(int count){for(int i=0;i<count;i++)yield return tick;}
        IEnumerator Place(Vector2 p)
        {
            input.frame=default;actor.modifier=null;actor.gravityFactor=actor.speedFactor=1;actor.disabled=false;actor.Revive(p);actor.Body.position=p;actor.Body.linearVelocity=Vector2.zero;
            Physics2D.SyncTransforms();yield return Steps(5);
        }
        IEnumerator Run()
        {
            Directory.CreateDirectory(game.reportDirectory);
            report.unity=Application.unityVersion;report.platform=Application.platform.ToString();report.scope="Actual Unity physics and production components; not a human-feel or full-campaign solvability certificate.";
            game.SelectSource(0);var catalog=game.AvailableWorlds;
            Check("foundation.catalog.4-levels-and-boss",catalog.Length==1&&catalog[0].levels.Length==4&&catalog[0].boss.boss);
            yield return game.Load(catalog[0].levels[0],true);yield return Steps(5);
            game.Session.player.disabled=true;game.Session.player.Body.simulated=false;
            fixture=new GameObject("Physics verification fixtures");b=new StageBuilder(fixture.transform,game.Session);
            b.Floor(600,0,100);
            var host=new GameObject("Test actor");host.transform.SetParent(fixture.transform);host.transform.position=new Vector2(600,1);
            host.AddComponent<Rigidbody2D>();host.AddComponent<CapsuleCollider2D>();actor=host.AddComponent<ActorMotor>();input=new ScriptedInput();actor.input=input;
            yield return Place(new Vector2(600,.8f));
            Check("ground.contact",actor.Grounded,actor.Feet.ToString());
            Vector2 resizeCenter=actor.Body.position;int resizeHealth=actor.Health;
            bool resizeSmall=actor.TrySetStandingSizeCentered(new Vector2(.572f,.975f),Layers.Solids);
            Check("shape.centered-shrink-preserves-position",resizeSmall&&Vector2.Distance(resizeCenter,actor.Body.position)<.001f&&actor.Height<1);
            Vector2 smallShape=actor.Shape.size;
            bool rejected=actor.TrySetStandingSizeCentered(new Vector2(1.4f,3),Layers.Solids);
            Check("shape.centered-growth-rejects-ground-overlap",!rejected&&actor.Shape.size==smallShape&&actor.Body.position==resizeCenter);
            bool normalAgain=actor.TrySetStandingSizeCentered(new Vector2(.88f,1.5f),Layers.Solids);
            Check("shape.centered-resize-preserves-health",normalAgain&&actor.Health==resizeHealth&&actor.Body.position==resizeCenter);
            yield return Place(new Vector2(600,.8f));

            float start=actor.Body.position.x;input.frame=new InputFrame{move=Vector2.right};yield return Steps(30);
            Check("walk.acceleration",actor.Body.position.x-start>1.6f&&actor.Body.position.x-start<3.5f,actor.Body.position.x-start+" units / 0.5s");
            input.frame=default;yield return Steps(15);Check("walk.braking",Mathf.Abs(actor.Body.linearVelocity.x)<.2f,actor.Body.linearVelocity.ToString());

            yield return Place(new Vector2(600,.8f));float full=actor.Body.position.y;input.frame=new InputFrame{jump=true,jumpHeld=true};
            for(int i=0;i<50;i++){yield return tick;full=Mathf.Max(full,actor.Body.position.y);}yield return Steps(25);
            yield return Place(new Vector2(600,.8f));float shortHeight=actor.Body.position.y;input.frame=new InputFrame{jump=true,jumpHeld=true};
            for(int i=0;i<5;i++){yield return tick;shortHeight=Mathf.Max(shortHeight,actor.Body.position.y);}input.frame=default;
            for(int i=0;i<50;i++){yield return tick;shortHeight=Mathf.Max(shortHeight,actor.Body.position.y);}
            Check("jump.variable-height",full>shortHeight+.65f,"full="+full+" short="+shortHeight);
            yield return Place(new Vector2(600,.8f));actor.Revive(new Vector2(600,1.25f));actor.Body.linearVelocity=new Vector2(0,-4);input.frame=new InputFrame{jump=true,jumpHeld=true};
            yield return Steps(14);Check("jump.buffered-on-landing",actor.Body.linearVelocity.y>1||actor.Body.position.y>1.7f,actor.Body.position+" "+actor.Body.linearVelocity);
            yield return Place(new Vector2(649.6f,.8f));input.frame=new InputFrame{move=Vector2.right};
            int safe=0;while(actor.Grounded&&safe++<45)yield return tick;
            input.frame=new InputFrame{move=Vector2.right,jump=true,jumpHeld=true};yield return Steps(2);
            Check("jump.coyote-after-edge",actor.Body.linearVelocity.y>7,actor.Body.linearVelocity.ToString());

            yield return Place(new Vector2(605,.8f));input.frame=new InputFrame{move=Vector2.down};yield return Steps(4);
            Check("crouch.reduces-collider",actor.Crouched&&actor.Height<1,actor.Height.ToString());
            var roof=b.Solid("Headroom roof",new Vector2(605,1.4f),new Vector2(4,.5f));yield return Steps(2);input.frame=default;yield return Steps(4);
            Check("crouch.cannot-stand-through-roof",actor.Crouched&&actor.Height<1);
            Destroy(roof);yield return Steps(3);Check("crouch.stands-when-clear",!actor.Crouched&&actor.Height>1.3f);
            input.frame=new InputFrame{move=new Vector2(1,-1)};yield return Steps(20);Check("crawl.speed-limited",actor.Crouched&&Mathf.Abs(actor.Body.linearVelocity.x)<=actor.tuning.crawlSpeed+.1f);
            yield return Place(new Vector2(605,.8f));var oneWayRoof=b.Platform(new Vector2(605,1.25f),new Vector2(4,.3f));oneWayRoof.AddComponent<OneWaySurface>();
            input.frame=new InputFrame{move=Vector2.down};yield return Steps(5);input.frame=default;yield return Steps(5);
            Check("crouch.stands-beneath-one-way-floor",!actor.Crouched&&actor.Height>1.4f);
            Destroy(oneWayRoof);yield return Steps(2);input.frame=new InputFrame{move=Vector2.down};yield return Steps(4);
            var solidAgain=b.Solid("Actual solid roof negative control",new Vector2(605,1.25f),new Vector2(4,.3f));input.frame=default;yield return Steps(5);
            Check("crouch.one-way-exception-does-not-ignore-solid-roofs",actor.Crouched&&actor.Height<1);Destroy(solidAgain);yield return Steps(2);


            yield return Place(new Vector2(600,.8f));input.frame=new InputFrame{attack=true};yield return Steps(2);Check("tackle.normal-tier",actor.AttackPower==1,actor.State.ToString());
            yield return Place(new Vector2(600,.8f));input.frame=new InputFrame{move=Vector2.right,run=true};yield return Steps(50);input.frame=new InputFrame{move=Vector2.right,run=true,attack=true};yield return Steps(2);
            Check("tackle.run-up-tier",actor.AttackPower==2&&actor.State==MotionState.RunTackle,actor.State.ToString());
            input.frame=new InputFrame{move=Vector2.right,run=true,jump=true,jumpHeld=true};yield return Steps(3);
            Check("tackle.running-jump-preserves-attack",actor.State==MotionState.AirTackle&&actor.Body.linearVelocity.y>4,actor.State+" "+actor.Body.linearVelocity);

            int lastPound=0;actor.LandedPound+=p=>lastPound=p;
            yield return Place(new Vector2(600,.8f));actor.Revive(new Vector2(600,3));input.frame=new InputFrame{pound=true};yield return Steps(55);
            Check("pound.short-drop-normal",lastPound==2,lastPound.ToString());
            lastPound=0;actor.Revive(new Vector2(600,11));input.frame=new InputFrame{pound=true};yield return Steps(70);
            Check("pound.long-drop-uber",lastPound==3,lastPound.ToString());
            var tough=b.Break(new Vector2(614,1),Vector2.one,3,true);tough.Hit(new HitInfo(actor,Vector2.down,2,true));yield return null;
            Check("break.rejects-weak-hit",tough&&tough.GetComponent<Collider2D>().enabled);
            tough.Hit(new HitInfo(actor,Vector2.down,3,true));yield return null;Check("break.accepts-uber",!tough);

            yield return Place(new Vector2(600,.8f));var enemy=b.Enemy(new Vector2(601,.6f));enemy.Stun();yield return Steps(3);actor.GrabOrThrow(Vector2.zero);yield return Steps(2);
            Check("enemy.pick-up-stunned",actor.Carried==enemy&&enemy.state==EnemyState.Carried);
            var victim=b.Enemy(new Vector2(605,1));victim.patrolSpeed=0;actor.GrabOrThrow(Vector2.right);yield return Steps(40);
            Check("enemy.throw-collides-with-enemy",!victim||victim.state==EnemyState.Dead,"victim "+(victim?victim.state.ToString():"destroyed"));
            if(enemy)Destroy(enemy.gameObject);if(victim)Destroy(victim.gameObject);
            var turner=b.Enemy(new Vector2(620,1),false,false,1);b.Wall(623,1.5f,3);yield return Steps(120);
            Check("enemy.patrol-turns-at-wall",turner.direction==-1,"direction="+turner.direction);Destroy(turner.gameObject);
            var recovery=b.Enemy(new Vector2(618,1));recovery.recoverSeconds=.2f;recovery.Stun();yield return Steps(25);Check("enemy.recovers-from-stun",recovery.state==EnemyState.Patrol);Destroy(recovery.gameObject);

            yield return Place(new Vector2(635,.8f));var water=b.Water(new Vector2(638,2.5f),new Vector2(16,5));input.frame=new InputFrame{move=Vector2.up};yield return Steps(12);
            Check("water.swim-vertical",actor.State==MotionState.Swim&&actor.Body.linearVelocity.y>1,actor.State.ToString());
            input.frame=new InputFrame{move=Vector2.right,attack=true};yield return Steps(3);Check("water.dash",actor.State==MotionState.SwimDash&&actor.Body.linearVelocity.x>8,actor.Body.linearVelocity.ToString());
            Destroy(water.gameObject);yield return Steps(3);

            var ramp=b.Ramp(new Vector2(580,5),new Vector2(590,0));yield return Place(new Vector2(582,5));yield return Steps(12);
            input.frame=new InputFrame{move=Vector2.down};start=actor.Body.position.x;yield return Steps(45);
            Check("roll.slope-accelerates-without-forward-input",actor.Body.position.x>start+1&&actor.State==MotionState.Roll,actor.State+" dx="+(actor.Body.position.x-start));Destroy(ramp);

            input.frame=default;var slider=b.Slider(new Vector2(610,2),new Vector2(620,2),new Vector2(4,.4f),2);
            yield return Place(new Vector2(610,3.1f));yield return Steps(15);float relative=actor.Body.position.x-slider.transform.position.x;yield return Steps(50);
            Check("platform.carries-standing-actor",Mathf.Abs(actor.Body.position.x-slider.transform.position.x-relative)<.6f,"relative delta="+(actor.Body.position.x-slider.transform.position.x-relative));
            slider.gameObject.layer=Layers.Interior;actor.Shape.includeLayers=1<<Layers.Interior;actor.Shape.layerOverridePriority=20;actor.collisionMask=Layers.Solids|(1<<Layers.Interior);
            yield return Steps(10);relative=actor.Body.position.x-slider.transform.position.x;yield return Steps(45);
            Check("platform.carries-kinematic-support-in-another-domain",Mathf.Abs(actor.Body.position.x-slider.transform.position.x-relative)<.6f&&actor.Grounded);
            actor.Shape.includeLayers=0;actor.Shape.layerOverridePriority=0;actor.collisionMask=Layers.Solids;Destroy(slider.gameObject);
            var carousel=b.Wheel(new Vector2(600,10),3,.6f);yield return Steps(20);bool four=carousel.arms.Length==4;
            foreach(var arm in carousel.arms)four&=arm&&Mathf.Abs(Vector2.Distance(arm.transform.position,carousel.transform.position)-3)<.1f&&Mathf.Abs(arm.transform.eulerAngles.z)<.1f;
            Check("carousel.four-upright-arms",four);
            foreach(var arm in carousel.arms)if(arm)Destroy(arm.gameObject);Destroy(carousel.gameObject);

            var belt=b.Solid("Conveyor regression",new Vector2(615,2),new Vector2(10,.5f));belt.AddComponent<Conveyor>().speed=2;
            yield return Place(new Vector2(612,3.1f));input.frame=default;yield return Steps(15);float bx=actor.Body.position.x,peakBelt=0;
            for(int i=0;i<120;i++){yield return tick;peakBelt=Mathf.Max(peakBelt,Mathf.Abs(actor.Body.linearVelocity.x));}
            Check("conveyor.bounded-support-relative-speed",peakBelt<=2.1f&&actor.Body.position.x>bx+2,"peak="+peakBelt+" dx="+(actor.Body.position.x-bx));Destroy(belt);

            var plate=b.Plate(new Vector2(630,.14f));var gate=b.Door(new Vector2(632,1),new Vector2(1,2),plate);
            yield return Place(new Vector2(630,.8f));yield return Steps(10);Check("plate.actor-mass-opens-gate",plate.Pressed&&gate.opened);
            yield return Place(new Vector2(626,.8f));yield return Steps(10);Check("plate.release-closes-gate",!plate.Pressed&&!gate.opened);
            var structural=b.Solid("Structural shutter must not pretend to be a body",new Vector2(630,.2f),new Vector2(1,.4f),Color.gray,Layers.Moving);var structuralBody=structural.AddComponent<Rigidbody2D>();structuralBody.bodyType=RigidbodyType2D.Kinematic;structuralBody.mass=10;
            yield return Steps(5);Check("plate.rejects-self-propelled-structural-shutter",!plate.Pressed&&!gate.opened);
            structuralBody.bodyType=RigidbodyType2D.Dynamic;structuralBody.gravityScale=2;yield return Steps(10);Check("plate.accepts-released-physical-counterweight",plate.Pressed&&gate.opened);Destroy(structural);
            yield return Steps(70);actor.Hit(new HitInfo(null,Vector2.up,99));Check("damage.lethal-hit-clamps-health-to-zero",actor.Health==0&&actor.State==MotionState.Dead);yield return Place(new Vector2(626,.8f));


            yield return Place(new Vector2(600,.8f));var excludedTarget=b.Break(new Vector2(601,.8f),new Vector2(.6f,1.3f),1);excludedTarget.gameObject.layer=17;
            actor.Shape.excludeLayers=1<<17;actor.Shape.layerOverridePriority=20;input.frame=new InputFrame{move=Vector2.right,attack=true};yield return Steps(8);
            Check("attack.cannot-hit-geometry-in-excluded-domain",excludedTarget&&excludedTarget.gameObject.activeSelf);
            yield return Place(new Vector2(600,.8f));actor.Shape.excludeLayers=0;actor.Shape.includeLayers=1<<17;input.frame=new InputFrame{move=Vector2.right,attack=true};yield return Steps(8);
            Check("attack.can-hit-same-target-in-contact-domain",!excludedTarget||!excludedTarget.gameObject.activeSelf);actor.Shape.includeLayers=0;actor.Shape.layerOverridePriority=0;yield return Place(new Vector2(626,.8f));

            var session=game.Session;float remaining=session.Remaining;session.TickClock(20);Check("escape.no-clock-before-turn",Mathf.Abs(remaining-session.Remaining)<.01f);
            Check("escape.cannot-clear-before-switch",!session.TryClear());int turns=0;session.Turned+=()=>turns++;session.Turn();session.Turn();Check("escape.switch-one-shot",turns==1);
            Check("escape.required-items-enforced",!session.TryClear());
            for(int i=0;i<4;i++){var item=b.Collect(PickupKind.Shard,Vector2.zero,"test-shard-"+i);session.Collect(item,actor);session.Collect(item,actor);}
            var key=b.Collect(PickupKind.Key,Vector2.zero,"test-key");session.Collect(key,actor);
            Check("collect.stable-id-deduplication",session.Shards==4);Check("escape.optional-mercy-not-required",session.Mercies.Count==0&&session.TryClear());
            string path=Path.Combine(game.reportDirectory,"save-roundtrip.json");var store=new SaveStore(path);store.MarkCorruption();store.CommitRun(catalog[0].levels[0],12,new[]{"GB-L01-MERCY"});var loaded=new SaveStore(path);
            Check("save.round-trip",loaded.Data.corrupted&&loaded.Data.cleared.Contains("BASE-1")&&loaded.Data.mercies.Contains("GB-L01-MERCY")&&loaded.Data.totalCoins>=12);
            File.WriteAllText(path,"not json");loaded.Load();Check("save.corrupt-primary-recovers-backup",loaded.Data.corrupted);
            var timerObject=new GameObject("Timer verification");var timed=timerObject.AddComponent<StageSession>();timed.Configure(new StageDefinition{id="timer",timed=true,escapeSeconds=.5f},null);timed.Turn();timed.TickClock(1);Check("escape.timeout-fails",timed.Phase==RunPhase.Failed);Destroy(timerObject);

            Destroy(fixture);yield return null;
            // Build each real course; this checks scene construction, not full playthrough solvability.
            foreach(var d in catalog[0].levels)
            {
                yield return game.Load(d,true);yield return Steps(5);
                Check("course.construct."+d.id,game.Session.player&&FindObjectsByType<Collider2D>(FindObjectsSortMode.None).Length>20);
                Capture(game.Session.Camera.GetComponent<UnityEngine.Camera>(),Path.Combine(game.reportDirectory,d.id+".png"));
            }
            yield return game.Load(catalog[0].boss,true);yield return Steps(5);Check("boss.foundation-constructed",FindFirstObjectByType<ForemanBoss>()!=null);
            yield return game.gameObject.AddComponent<FoundationIterationVerification>().Run(game,this);
            var extension=Type.GetType("GloomBean.Campaign.AtlasVerification, Assembly-CSharp");
            if(extension!=null)
            {
                var instance=game.gameObject.AddComponent(extension);
                var method=extension.GetMethod("Run");
                if(method!=null)yield return (IEnumerator)method.Invoke(instance,new object[]{game,this});
            }
            report.failed+=report.exceptions;
            File.WriteAllText(Path.Combine(game.reportDirectory,"verification.json"),JsonUtility.ToJson(report,true));
            Debug.Log("GLOOM_VERIFY_COMPLETE passed="+report.passed+" failed="+report.failed+" exceptions="+report.exceptions);
            Application.logMessageReceived-=Log;
            Application.Quit(report.failed==0?0:1);
        }
        public static void Capture(UnityEngine.Camera camera,string path)
        {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
            var rt=new RenderTexture(1280,800,24);var old=camera.targetTexture;var active=RenderTexture.active;
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(1280,800,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,800),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            camera.targetTexture=old;RenderTexture.active=active;Destroy(texture);rt.Release();Destroy(rt);
        }
    }
}
