using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Executed by the native Windows player after the unchanged foundation scenarios.
    // Fixtures may position objects; they do not certify whole-course human solvability.
    public sealed class AtlasVerification:MonoBehaviour
    {
        GameRoot game;FoundationVerification check;GameObject fixture;StageBuilder b;AtlasBuilder a;ActorMotor actor;HostController host;ScriptedInput input;
        readonly WaitForFixedUpdate tick=new WaitForFixedUpdate();string dir;
        IEnumerator Steps(int count){for(int i=0;i<count;i++)yield return tick;}
        void C(string name,bool value,string observation="")
        {
            check.Check("atlas."+name,value,observation);
            File.WriteAllText(Path.Combine(dir,"progress.json"),JsonUtility.ToJson(check.report,true));
        }
        IEnumerator Arena()
        {
            Time.timeScale=1;
            if(fixture){fixture.SetActive(false);Destroy(fixture);yield return null;}
            fixture=new GameObject("Independent native mechanics fixture");fixture.transform.SetParent(game.Session.transform);
            b=new StageBuilder(fixture.transform,game.Session);b.Floor(600,0,120);
            actor=b.Player(new Vector2(600,.8f));actor.GetComponent<HumanInput>().disabled=true;input=new ScriptedInput();actor.input=input;host=actor.gameObject.AddComponent<HostController>();
            a=new AtlasBuilder(b,new StageDefinition{id="verification",worldId="W1"});a.host=host;
            Physics2D.SyncTransforms();yield return Steps(6);
        }
        HostSource Source(HostKind kind){var g=new GameObject("Fixture source");g.transform.SetParent(fixture.transform);var s=g.AddComponent<HostSource>();s.kind=kind;return s;}
        void Freeze(){input.frame=default;actor.disabled=true;actor.Body.simulated=false;}
        public IEnumerator Run(GameRoot root,FoundationVerification report)
        {
            game=root;check=report;dir=Path.Combine(game.reportDirectory,"Atlas");Directory.CreateDirectory(dir);
            C("extension.registered",game.SourceCount==2);game.SelectSource(0);yield return game.Load(game.AvailableWorlds[0].levels[0],true);
            game.Session.player.disabled=true;game.Session.player.Body.simulated=false;

            yield return Arena();host.Acquire(HostKind.Echo);var echo=host.Form<EchoForm>();float start=actor.Body.position.x;
            input.frame=new InputFrame{move=Vector2.right};yield return Steps(72);
            C("echo.waits-two-seconds",actor.Body.position.x>start+4&&Mathf.Abs(echo.Echo.Body.position.x-start)<.2f);
            yield return Steps(105);C("echo.replays-input-with-delay",echo.Echo.Body.position.x>start+2&&actor.Body.position.x-echo.Echo.Body.position.x>8,(actor.Body.position.x-echo.Echo.Body.position.x).ToString());
            // Regression for repeated simulated=true writes: velocities alone did not expose the old drift.
            yield return Arena();input.frame=new InputFrame{move=Vector2.right};yield return Steps(30);host.Acquire(HostKind.Echo);echo=host.Form<EchoForm>();
            float replayError=0;var witnessed=new List<Vector2>();int comparisons=0;
            Action<InputFrame,float> observe=(frame,dt)=>{
                witnessed.Add(actor.Body.position);
                int old=witnessed.Count-121;
                if(old>=0&&echo.Echo.Body.simulated){replayError=Mathf.Max(replayError,Vector2.Distance(echo.Echo.Body.position,witnessed[old]));comparisons++;}
            };
            actor.Stepped+=observe;yield return Steps(220);actor.Stepped-=observe;
            C("echo.position-replay-not-just-velocity",comparisons>80&&replayError<.14f,"max error="+replayError+" over "+comparisons+" fixed-step comparisons");

            yield return Arena();host.Acquire(HostKind.Molt);host.Form<MoltForm>().Shed();float inheritedMass=actor.Body.mass,inheritedHeight=actor.Height;
            host.Acquire(HostKind.Echo,null,true);echo=host.Form<EchoForm>();
            C("echo.inherits-core-dimensions-and-mass",Mathf.Abs(echo.Echo.Height-inheritedHeight)<.001f&&Mathf.Abs(echo.Echo.Body.mass-inheritedMass)<.001f&&echo.Echo.chargeDisabled,"height="+echo.Echo.Height+" mass="+echo.Echo.Body.mass);
            C("cure.absent-tenant-preserves-existing-body",host.Cure(HostKind.Marionette)&&host.Has(HostKind.Molt)&&Mathf.Abs(actor.Height-inheritedHeight)<.001f);

            yield return Arena();host.Acquire(HostKind.Molt);host.Form<MoltForm>().Shed();var oldSkin=host.Husks[0];actor.Body.position=new Vector2(601,.555f);Physics2D.SyncTransforms();
            var lowRoof=b.Solid("No room for full-size reclaim",new Vector2(601,1.35f),new Vector2(4,.4f));yield return Steps(2);
            C("molt.reclaim-refuses-unsafe-growth",!host.TryReclaim(oldSkin)&&host.Husks.Contains(oldSkin)&&oldSkin.GetComponent<Collider2D>().enabled);
            Destroy(lowRoof);yield return Steps(2);C("molt.reclaim-restores-the-body",host.TryReclaim(oldSkin)&&host.Husks.Count==0&&actor.Height>1.3f);
            host.Acquire(HostKind.Molt);host.Form<MoltForm>().Shed();oldSkin=host.Husks[0];actor.Body.position+=Vector2.right;host.Cure(HostKind.None,true);
            C("molt.abandoned-skin-remains-reclaimable-after-cure",host.TryReclaim(oldSkin)&&host.Husks.Count==0);

            yield return Arena();host.Acquire(HostKind.Molt);host.Form<MoltForm>().Shed();var tetherSource=Source(HostKind.Marionette);tetherSource.rail=a.Rail(new Vector2(596,9),new Vector2(606,9));host.Acquire(HostKind.Marionette,tetherSource,true);host.Cure(HostKind.Marionette);
            C("composition.cutting-thread-keeps-molt-cost",host.Has(HostKind.Molt)&&actor.chargeDisabled&&actor.Height<1.2f&&actor.Body.mass<.6f,"height="+actor.Height+" mass="+actor.Body.mass+" tackle disabled="+actor.chargeDisabled);

            yield return Arena();host.Acquire(HostKind.Echo);echo=host.Form<EchoForm>();echo.Leading=true;start=actor.Body.position.x;
            input.frame=new InputFrame{move=Vector2.right};yield return Steps(70);C("echo.leading-is-body-latency",echo.Echo.Body.position.x>start+3&&Mathf.Abs(actor.Body.position.x-start)<.5f,"body="+(actor.Body.position.x-start)+" echo="+(echo.Echo.Body.position.x-start));
            yield return Steps(75);C("echo.delayed-primary-eventually-moves",actor.Body.position.x>start+1);

            yield return Arena();var rail=a.Rail(new Vector2(600,9),new Vector2(604,9));var next=a.Rail(new Vector2(604,9),new Vector2(604,16));var source=Source(HostKind.Marionette);source.rail=rail;host.Acquire(source.kind,source);var puppet=host.Form<MarionetteForm>();input.frame=new InputFrame{move=Vector2.right};yield return Steps(55);
            C("marionette.anchor-follows-rail",puppet.Joint.connectedAnchor.x>603.8f);C("marionette.body-is-tethered",Mathf.Abs(Vector2.Distance(puppet.Joint.connectedAnchor,actor.Body.position+Vector2.up*.5f)-puppet.Joint.distance)<.5f);
            C("marionette.thread-does-not-push-like-a-strut",puppet.Joint.maxDistanceOnly&&puppet.Joint.enableCollision);
            C("marionette.transfers-at-junction",puppet.Transfer()&&puppet.Rail==next);float anchor=puppet.Joint.connectedAnchor.y;input.frame=new InputFrame{move=Vector2.up};yield return Steps(30);C("marionette.vertical-rail",puppet.Joint.connectedAnchor.y>anchor+1);

            yield return Arena();host.Acquire(HostKind.Molt);var molt=host.Form<MoltForm>();C("molt.first-real-skin",molt.Shed()&&host.Husks.Count==1&&actor.Height<1.2f);
            actor.Body.position+=Vector2.right*3;C("molt.second-skin-small-core",molt.Shed()&&host.Husks.Count==2&&actor.Height<.8f&&actor.chargeDisabled);C("molt.bounded-budget",!molt.Shed());C("molt.reclaims-nearby",molt.Reclaim()&&host.Husks.Count==1);

            yield return Arena();host.Acquire(HostKind.Wax);var wax=host.Form<WaxForm>();C("wax.conserves-volume",wax.Deposit()&&Mathf.Abs(host.waxVolume+host.Plugs.Sum(p=>p.volume)-1)<.001f);
            wax.Toggle();yield return Steps(4);C("wax.liquid-changes-collider",wax.Liquid&&actor.Height<.4f,"liquid="+wax.Liquid+" height="+actor.Height);yield return Steps(8);C("wax.physical-capsule-matches-puddle",actor.Shape.direction==CapsuleDirection2D.Horizontal&&actor.Shape.bounds.size.y<.4f,actor.Shape.bounds.size.ToString());
            var roof=b.Solid("Constrained reform space",new Vector2(600,.85f),new Vector2(4,.7f));yield return Steps(3);wax.Toggle();C("wax.cannot-reform-through-ceiling",wax.Liquid,"liquid="+wax.Liquid+" height="+actor.Height);Destroy(roof);yield return Steps(2);wax.Toggle();C("wax.reforms-in-clear-space",!wax.Liquid&&actor.Height>1);

            yield return Arena();host.Acquire(HostKind.Gullet);var gullet=host.Form<GulletForm>();var chunk=a.Chunk(new Vector2(602,1),new Vector2(2,2));EdibleChunk identity=chunk;yield return Steps(2);
            C("gullet.removes-actual-terrain",gullet.Bite(Vector2.right)&&!chunk.gameObject.activeSelf);C("gullet.no-duplicate-spit",gullet.PlaceAt(new Vector2(607,1))&&object.ReferenceEquals(chunk,identity)&&chunk.gameObject.activeSelf&&gullet.Stored==null);C("gullet.position-changed",Mathf.Abs(chunk.transform.position.x-607)<.01f);

            yield return Arena();a.SoilPath(new Vector2(600,.8f),new Vector2(600,6),new Vector2(608,6),new Vector2(608,.8f));b.Solid("Obstacle across the direct chord",new Vector2(604,2),new Vector2(3,4));host.Acquire(HostKind.Root);var rootForm=host.Form<RootForm>();bool began=rootForm.Begin();
            for(int i=0;i<13;i++)rootForm.Grow(Vector2.up*.4f);for(int i=0;i<20;i++)rootForm.Grow(Vector2.right*.4f);for(int i=0;i<13;i++)rootForm.Grow(Vector2.down*.4f);
            C("root.curve-recorded-in-wet-seam",began&&rootForm.Path.Count>40&&rootForm.Tip.x>607);
            rootForm.Retract();float high=actor.Body.position.y;for(int i=0;i<120;i++){yield return tick;high=Mathf.Max(high,actor.Body.position.y);}
            C("root.body-follows-curve-not-chord",high>5&&actor.Body.position.x>607&&actor.Shape.enabled,actor.Body.position+" peak="+high);

            yield return Arena();source=Source(HostKind.Mirror);source.explicitAxis=true;source.mirrorAxis=605;host.Acquire(source.kind,source);var mirror=host.Form<MirrorForm>();b.Wall(608,2,4);input.frame=new InputFrame{move=Vector2.right};yield return Steps(35);
            C("mirror.twins-remain-collidable",!Physics2D.GetIgnoreCollision(actor.Shape,mirror.Twin.Shape));
            C("mirror.opposite-controls",actor.Body.position.x>602&&mirror.Twin.Body.position.x<610);C("mirror.collision-desynchronizes",Mathf.Abs(actor.Body.position.x+mirror.Twin.Body.position.x-1210)>.5f);

            yield return Arena();b.Solid("Inverse floor",new Vector2(605,-.2f),new Vector2(30,.4f),Color.gray,Layers.Interior);b.Solid("Ordinary sealed mass",new Vector2(604,2),new Vector2(2,4));b.Solid("Inverse end stop",new Vector2(611,2),new Vector2(1,4),Color.gray,Layers.Interior);host.Acquire(HostKind.InsideOut);input.frame=new InputFrame{move=Vector2.right};yield return Steps(145);
            C("insideout.crosses-normal-solid",actor.Body.position.x>606,actor.Body.position.ToString());C("insideout.stops-at-complement",actor.Body.position.x<611&&actor.Grounded);

            yield return Arena();a.Projection(new Vector2(600,2),new Vector2(8,7));host.Acquire(HostKind.Parallax);var depth=host.Form<ParallaxForm>();Vector2 pos=actor.Body.position;bool changed=depth.StepPlane(-1);
            C("parallax.position-preserved",changed&&Vector2.Distance(pos,actor.Body.position)<.05f);C("parallax.scale-and-collision-domain",actor.Height<1.1f&&depth.Plane==0&&actor.collisionMask==(Layers.Solids|(1<<17)));
            actor.Body.position+=Vector2.right*15;C("parallax.requires-overlap",!depth.StepPlane(1));

            yield return Arena();host.Acquire(HostKind.Molt);host.Form<MoltForm>().Shed();var lowThread=Source(HostKind.Marionette);lowThread.rail=a.Rail(new Vector2(596,8),new Vector2(606,8));host.Acquire(HostKind.Marionette,lowThread,true);
            b.Solid("Small core-only clearance",new Vector2(600,1.4f),new Vector2(3,.4f));yield return Steps(2);
            C("cure.thread-cut-does-not-demand-full-size-space",host.Cure(HostKind.Marionette)&&host.Has(HostKind.Molt)&&actor.Height<1.2f&&actor.chargeDisabled);

            yield return Arena();host.Acquire(HostKind.Echo);var echoThread=Source(HostKind.Marionette);echoThread.rail=a.Rail(new Vector2(596,8),new Vector2(606,8));host.Acquire(HostKind.Marionette,echoThread,true);
            b.Solid("One-way shore shelf overlaps only the restoration probe",new Vector2(600,1.2f),new Vector2(3,.35f)).AddComponent<OneWaySurface>();Physics2D.SyncTransforms();float beforeCutHeight=actor.Height;
            C("cure.echo-thread-fixture-blocks-full-restoration",!host.CanStand(actor.Feet+Vector2.up*.75f));
            bool cutEchoThread=host.Cure(HostKind.Marionette);
            C("cure.echo-thread-cut-preserves-unchanged-footprint",cutEchoThread&&host.Has(HostKind.Echo)&&!host.Has(HostKind.Marionette)&&Mathf.Abs(actor.Height-beforeCutHeight)<.001f);

            yield return Arena();var overlap=a.Projection(new Vector2(600,4),new Vector2(8,12));host.Acquire(HostKind.Parallax);var planeForm=host.Form<ParallaxForm>();planeForm.StepPlane(-1);
            var wrong=a.Depth(new Vector2(600,2),new Vector2(4,.4f),1);wrong.gameObject.AddComponent<OneWaySurface>();actor.Body.position=new Vector2(600,4);actor.Body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return Steps(80);
            C("parallax.foreign-one-way-floor-is-not-solid",actor.Feet.y<.2f&&actor.Grounded,actor.Body.position+" effectorMask="+wrong.GetComponent<PlatformEffector2D>().useColliderMask);
            wrong.SetPlane(0);actor.Body.position=new Vector2(600,4);actor.Body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return Steps(80);
            C("parallax.selected-one-way-floor-supports-body",actor.Grounded&&actor.Feet.y>2&&actor.Feet.y<2.3f,actor.Body.position.ToString());

            yield return Arena();var near=b.Slider(new Vector2(602,2),new Vector2(618,2),new Vector2(2,.4f),2);var far=b.Slider(new Vector2(630,2),new Vector2(646,2),new Vector2(2,.4f),2);near.gameObject.AddComponent<TemporalBody>();far.gameObject.AddComponent<TemporalBody>();near.paused=far.paused=true;
            host.Acquire(HostKind.Censer);yield return Steps(200);float nx=near.transform.position.x,fx=far.transform.position.x;near.paused=far.paused=false;yield return Steps(60);
            C("censer.local-not-global-time",far.transform.position.x-fx>1.8f&&near.transform.position.x-nx<1.5f,"near="+(near.transform.position.x-nx)+" far="+(far.transform.position.x-fx));

            yield return Arena();var panel=a.Hinge(new Vector2(604,1),6,0);var end=panel.GetComponentInChildren<SeamNode>();var fixedSeam=a.Seam(new Vector2(604,7));host.Acquire(HostKind.Stitch);var stitch=host.Form<StitchForm>();bool joined=stitch.Select(end)&&stitch.Select(fixedSeam);stitch.Tug();yield return Steps(100);
            C("stitch.rotates-real-architecture",joined&&panel.angle>70,panel.angle.ToString());stitch.Cut();C("stitch.cut-does-not-teleport-panel",stitch.Active==null&&panel.angle>70);

            yield return Arena();host.Acquire(HostKind.Coffin);var coffin=host.Form<CoffinForm>();yield return Steps(8);start=actor.Body.position.x;bool flip=coffin.BeginFlip(1);yield return Steps(23);
            C("coffin.corner-pivot-not-jump",flip&&actor.Body.position.x>start+1&&coffin.Horizontal,actor.Body.position+" rot="+actor.Body.rotation);C("coffin.horizontal-brace",actor.GetComponent<LoadBearingBody>().bracing);
            b.Wall(actor.Body.position.x+1.6f,2,4);yield return Steps(2);C("coffin.rejects-blocked-swept-volume",!coffin.BeginFlip(1));
            host.Acquire(HostKind.Censer,null,true);host.Cure(HostKind.Coffin);yield return Steps(3);
            C("coffin.partial-cure-restores-capsule",host.Has(HostKind.Censer)&&!host.Has(HostKind.Coffin)&&actor.Shape.enabled&&actor.Height>1.3f&&actor.Height<1.6f&&actor.Body.bodyType==RigidbodyType2D.Dynamic);


            yield return Arena();var metal=a.Metal(new Vector2(605,2),new Vector2(1,2),4,false,-1);metal.strength=140;host.Acquire(HostKind.Lodestone);var magnet=host.Form<LodestoneForm>();float metalX=metal.transform.position.x;start=actor.Body.position.x;yield return Steps(24);
            C("lodestone.reciprocal-motion",actor.Body.position.x>start+.3f&&metal.transform.position.x<metalX-.1f,actor.Body.position.x+" / "+metal.transform.position.x);
            Vector2 pull=LodestoneForm.Force(Vector2.zero,Vector2.right*3,1,-1);Vector2 push=LodestoneForm.Force(Vector2.zero,Vector2.right*3,1,1);C("lodestone.polarity-reverses-force",pull.x>0&&push.x<0);

            yield return Arena();var sunObj=new GameObject("Fixture sun");sunObj.transform.SetParent(fixture.transform);sunObj.transform.position=new Vector2(590,10);var sun=sunObj.AddComponent<ShadowSun>();sun.reach=25;var occluder=b.Solid("Shadow screen",new Vector2(600,3),new Vector2(2,6));occluder.AddComponent<ShadowCaster>();host.Acquire(HostKind.Shadow);var shadow=host.Form<ShadowForm>();yield return Steps(4);sun.Rebuild();shadow.Toggle();Vector2 shadowStart=shadow.Position;bool walked=true;for(int i=0;i<8;i++)walked&=shadow.Advance(Vector2.right*.3f);
            C("shadow.follows-projected-silhouette",walked&&shadow.Position.x>shadowStart.x+2);C("shadow.cannot-cross-empty-light",!shadow.Advance(Vector2.up*15));C("shadow.primary-body-remains",actor.Shape.enabled&&actor.Body.simulated&&Vector2.Distance(actor.Body.position,shadow.Position)>1);

            yield return Arena();host.Acquire(HostKind.Shadow);shadow=host.Form<ShadowForm>();start=actor.Body.position.x;input.frame=new InputFrame{move=Vector2.left};yield return Steps(240);
            C("shadow.tether-constrains-physical-body",Vector2.Distance(actor.Body.position,shadow.Position)<=14.2f&&actor.Body.position.x<start-12,actor.Body.position+" shadow="+shadow.Position);

            yield return Arena();host.Acquire(HostKind.Ink);var ink=host.Form<InkForm>();var stroke=ink.Add(new Vector2(600,3),new Vector2(603,3));yield return Steps(35);C("ink.not-solid-immediately",stroke&&!stroke.Solid);yield return Steps(40);C("ink.hardens-after-delay",stroke&&stroke.Solid);yield return Steps(450);C("ink.expires",!stroke);
            for(int i=0;i<9;i++)ink.Add(new Vector2(600+i*3,5),new Vector2(603+i*3,5));yield return Steps(3);C("ink.finite-length-budget",ink.Length<=18.01f,ink.Length.ToString());

            yield return Arena();host.Acquire(HostKind.Echo);host.Acquire(HostKind.Ink,null,true);C("composition.compatible-pair-retained",host.Has(HostKind.Echo)&&host.Has(HostKind.Ink));host.Cure(HostKind.None,true);C("cure.restores-base-controller",host.Forms.Count==0&&actor.Shape.enabled&&actor.Shape.excludeLayers==0&&actor.Height>1.3f);
            yield return Arena();host.Acquire(HostKind.Gullet);var carried=a.Chunk(new Vector2(601.1f,.7f),Vector2.one);var pairGullet=host.Form<GulletForm>();bool swallowed=pairGullet.Bite(Vector2.right);
            host.Acquire(HostKind.Wax,null,true);host.Acquire(HostKind.Root,null,true);
            C("composition.locomotion-swap-preserves-stored-terrain",swallowed&&host.Has(HostKind.Root)&&host.Has(HostKind.Gullet)&&!host.Has(HostKind.Wax)&&pairGullet.Stored==carried&&!carried.gameObject.activeSelf);
            yield return Arena();var drainCover=a.Chunk(new Vector2(601,.5f),Vector2.one);var drainSoil=a.Soil(new Vector2(603,1),new Vector2(609,1));
            var materialCircuit=fixture.AddComponent<MaterialSluice>();materialCircuit.cover=drainCover;materialCircuit.channel=new[]{drainSoil};yield return Steps(3);
            C("sluice.cover-retains-local-moisture",drainSoil.wet);drainCover.gameObject.SetActive(false);yield return Steps(3);C("sluice.removed-terrain-dries-actual-soil",!drainSoil.wet);materialCircuit.reversed=true;yield return Steps(3);C("sluice.reverse-restores-actual-soil",drainSoil.wet);
            yield return Arena();yield return Steps(75);actor.Hit(new HitInfo(null,Vector2.left,1));int hurtHealth=actor.Health;actor.Reposition(new Vector2(601,1));C("arena.reposition-does-not-heal",actor.Health==hurtHealth&&hurtHealth<actor.tuning.maximumHealth);
            Destroy(fixture);fixture=null;yield return null;
            game.SelectSource(1);var worlds=game.AvailableWorlds;C("catalog.five-worlds-four-levels-five-bosses",worlds.Length==5&&worlds.All(w=>w.levels.Length==4&&w.boss!=null));
            var all=new HashSet<string>();var seen=new HashSet<HostKind>();
            foreach(var world in worlds)
            {
                foreach(var level in world.levels)
                {
                    yield return game.Load(level,true);yield return Steps(5);var session=game.Session;session.player.disabled=true;session.player.Body.simulated=false;
                    var pickups=session.GetComponentsInChildren<Pickup>();var mercy=pickups.Where(p=>p.kind==PickupKind.Mercy).ToArray();
                    C("course."+level.id+".construction-and-one-mercy",session.player&&mercy.Length==1&&all.Add(mercy[0].stableId)&&pickups.Count(p=>p.kind==PickupKind.Key)==1&&session.GetComponentsInChildren<TurnSwitch>().Length==1&&session.GetComponentsInChildren<ExitPortal>().Length==1);
                    var sourceKinds=session.GetComponentsInChildren<HostSource>(true).Select(s=>s.kind).ToHashSet();foreach(var k in sourceKinds)seen.Add(k);
                    C("course."+level.id+".declared-tenants-present",level.possessions.All(k=>sourceKinds.Contains((HostKind)Enum.Parse(typeof(HostKind),k))),string.Join(",",sourceKinds));
                    Overview(session,Path.Combine(dir,level.id+"-outward.png"));session.Turn();yield return Steps(40);
                    C("course."+level.id+".turn-state",session.Phase==RunPhase.Returning&&TurnState(level.course,session));if(level.course==1)C("nail.withdrawn-after-pulling",!session.GetComponentInChildren<TurnSwitch>().GetComponent<Collider2D>().enabled);Overview(session,Path.Combine(dir,level.id+"-return.png"));
                }
                yield return game.Load(world.boss,true);yield return Steps(8);game.Session.player.disabled=true;game.Session.player.Body.simulated=false;var boss=game.Session.GetComponentInChildren<AtlasBoss>();C("boss."+world.id+".distinct-mechanical-encounter",boss&&boss.phases==3&&boss.Solve!=null&&boss.EnterPhase!=null);
                Overview(game.Session,Path.Combine(dir,world.boss.id+".png"));
                if(world.id=="W1")
                {
                    boss.combat=false;var pendulum=game.Session.GetComponentInChildren<ChandelierImpact>();var rb=pendulum.GetComponent<Rigidbody2D>();
                    rb.AddForce(Vector2.right*18,ForceMode2D.Impulse);yield return Steps(170);
                    C("boss.usher.pendulum-can-physically-hit-catch",pendulum.struck,"real joint, force and collision; not a boss victory certificate");
                }

            }
            C("coverage.all-fifteen-sources",seen.Count==15,string.Join(",",seen));C("coverage.twenty-unique-mercy-secrets",all.Count==20);
            C("permanent-corruption.retained",game.IsCorrupted);
            var ending=new SaveStore(Path.Combine(dir,"ending-boundary.json"));ending.Data.mercies.Clear();
            for(int i=1;i<=19;i++)ending.Data.mercies.Add("GB-L"+i.ToString("00")+"-MERCY");
            ending.Data.mercies.Add("GB-L01-MERCY");ending.Data.mercies.Add("GB-L99-MERCY");
            C("ending.rejects-duplicates-and-unrelated-prefixes",!ending.RestoredEnding);
            ending.Data.mercies.Add("GB-L20-MERCY");C("ending.requires-every-canonical-mercy",ending.RestoredEnding);

            File.WriteAllText(Path.Combine(dir,"SCOPE.txt"),"Native production-component fixtures; 20 outward/return scene construction checks; five boss initializers. This is NOT 20 complete playthroughs, 20 secret solutions, five boss victories or human gamepad acceptance. Camera captures show actual Unity rendering, not concept art.");
        }
        bool TurnState(int id,StageSession s)
        {
            switch(id){case 1:return s.GetComponentsInChildren<Transform>().Any(t=>t.name=="Backstage return brace");case 2:return s.GetComponentInChildren<DualPulseLift>().latched;case 3:return s.GetComponentsInChildren<RailPath>().Any(x=>x.loose);case 4:return s.GetComponentInChildren<SkinReturn>().crawling;case 5:return s.GetComponentsInChildren<RipeningFruit>().All(x=>x.fallen);case 6:return s.GetComponentInChildren<FlowEmitter>().reversed;case 7:return s.GetComponentInChildren<RootPump>().reversed;case 8:return s.GetComponentInChildren<SeasonWheel>().drifting;case 9:return s.GetComponentsInChildren<SunShutter>().All(x=>x.reverse);case 10:return s.GetComponentsInChildren<Transform>().Any(t=>t.name=="Peeling fresco return");case 11:return s.GetComponentInChildren<PerspectiveStamp>().reversed;case 12:return s.GetComponentsInChildren<RoomOrbit>().All(x=>x.running);case 13:return s.GetComponentsInChildren<KneelingFigure>().Any(x=>x.delay<1);case 14:return s.GetComponentsInChildren<FoldPanel>().Any(x=>x.targetAngle== -15);case 15:return s.GetComponentsInChildren<ProcessionCarrier>().All(x=>x.deaf);case 16:case 20:return s.GetComponentInChildren<DescentController>().fallen>0;case 17:return s.GetComponentInChildren<HaloChoir>().desynchronized;case 18:return s.GetComponentsInChildren<ShadowSun>().Any(x=>x.moving);case 19:return s.GetComponentInChildren<ScriptureLayout>().erasing;default:return false;}
        }
        void Overview(StageSession s,string path)
        {
            var camera=s.Camera.GetComponent<UnityEngine.Camera>();s.Camera.enabled=false;var old=camera.transform.position;float size=camera.orthographicSize;Rect bounds=s.Camera.bounds;
            camera.transform.position=new Vector3(bounds.center.x,bounds.center.y,-10);camera.orthographicSize=Mathf.Max(bounds.height*.5f,bounds.width/camera.aspect*.5f)+2;
            FoundationVerification.Capture(camera,path);camera.transform.position=old;camera.orthographicSize=size;s.Camera.enabled=true;
        }
    }
}