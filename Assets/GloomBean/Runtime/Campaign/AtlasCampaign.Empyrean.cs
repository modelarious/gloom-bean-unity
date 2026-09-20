using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Halos(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,96,48),new Vector2(2,1));var b=a.b;
            a.Floor(-5,18);a.Floor(18,82,-6);a.Exit(2,1.1f);a.Source(HostKind.Lodestone,8);
            var docks=new[]{new Vector2(16,2),new Vector2(27,4),new Vector2(38,6),new Vector2(49,8),new Vector2(60,10),new Vector2(71,12)};
            var poles=new List<MagneticBody>();
            for(int i=0;i<docks.Length;i++)
            {
                var d=docks[i];a.Ledge(d.x,d.y,i==5?13:6);
                var coil=a.Metal(d+Vector2.down*.95f,new Vector2(2.2f,.7f),20,true,1);coil.name="Launch coil "+d.x;coil.strength=170;coil.enabled=false;
                var lever=b.Switch(d+Vector2.up*.9f,"COIL "+d.x);lever.name="Coil lever "+d.x;lever.Changed+=on=>coil.enabled=on;
                PrimitiveArt.Line("Copper circuit",b.root,d+Vector2.down*.6f,d+Vector2.up*.9f,.08f,new Color(.87f,.62f,.31f),2);
                // Clearly separated overhead orbital lanes. They are obstacles and force sources,
                // not invisible room-wide fields affecting every previous launch.
                if(i<5){var m=a.Metal(d+new Vector2(5.5f,14),Vector2.one*1.4f,4,true,1);m.name="Orbiting iron halo "+i;m.strength=170;m.fieldRadius=3.5f;
                    m.GetComponent<BoxCollider2D>().enabled=false;var rim=m.gameObject.AddComponent<CircleCollider2D>();rim.radius=.7f;rim.sharedMaterial=new PhysicsMaterial2D("Iron rim"){friction=0};
                    var orbit=m.gameObject.AddComponent<MotionPlatform>();orbit.pattern=MotionPlatform.Pattern.Orbit;orbit.origin=m.transform.position;orbit.radius=1.4f;orbit.speed=.35f;orbit.phase=i*.4f;poles.Add(m);}
            }
            var choirObject=new GameObject("Iron choir clock");choirObject.transform.SetParent(b.root);var choir=choirObject.AddComponent<HaloChoir>();choir.halos=poles.ToArray();choir.measure=4;
            a.Key(71,13.3f);a.Nail(76,12.4f);a.Ledge(53,24,6);a.Mercy(54,25.3f);
            b.session.Turned+=()=>choir.desynchronized=true;a.Health(38,7.3f);a.Cure(HostKind.Lodestone,4,1);
            b.Tip(new Vector2(8,2),"U changes your pole. E powers each visible north coil. South holds you; north pushes you away. The choir reverses the overhead iron. Start a launch from the forward edge, not directly over the coil.");
        }
        ShadowSun Sun(AtlasBuilder a,Vector2 p,float reach=22)
        {var g=PrimitiveArt.Shape("Noon lamp",a.b.root,p,Vector2.one*2,new Color(1,.94f,.68f),PrimitiveArt.Icon.Star,-1);var s=g.AddComponent<ShadowSun>();s.reach=reach;return s;}
        void Noon(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-10,103,42),new Vector2(2,1));var b=a.b;a.Floor(-5,90);a.Exit(2,1.1f);a.Source(HostKind.Shadow,8);
            var sun=Sun(a,new Vector2(43,25),22);sun.directional=true;sun.moving=true;sun.speed=.3f;sun.sweep=1.4f;sun.renderFilled=true;
            var latches=new List<ShadowReceiver>();var screens=new List<MagneticBody>();
            for(int i=0;i<2;i++)
            {
                float dx=i*32;
                var statue=b.Solid("Suspended saint "+i,new Vector2(13+dx,5),new Vector2(2,4),new Color(.8f,.77f,.66f));statue.AddComponent<ShadowCaster>();
                var gate=b.Door(new Vector2(24+dx,4),new Vector2(.65f,8));gate.name="Noon shutter "+i;
                var latch=b.Trigger("Shadow latch "+i,new Vector2(20.5f+dx,.35f),Vector2.one*.6f,new Color(.32f,.27f,.46f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();latch.gate=gate;latch.radius=.6f;latches.Add(latch);
                var screen=a.Metal(new Vector2(18+dx,6.5f),new Vector2(8,.5f),.6f,false,1);screen.name="Manufactured shadow screen "+i;screen.strength=80;
                var body=screen.GetComponent<Rigidbody2D>();body.gravityScale=0;body.constraints=RigidbodyConstraints2D.FreezePositionY|RigidbodyConstraints2D.FreezeRotation;body.linearDamping=.15f;screen.gameObject.layer=Layers.Moving;screens.Add(screen);
                b.Solid("Screen left stop "+i,new Vector2(13.75f+dx,6.5f),new Vector2(.5f,1));b.Solid("Screen right stop "+i,new Vector2(26.65f+dx,6.5f),new Vector2(.5f,1));
                PrimitiveArt.Line("Screen rail "+i,b.root,new Vector2(14+dx,7.2f),new Vector2(26.4f+dx,7.2f),.06f,new Color(.76f,.59f,.26f),0);
            }
            a.Source(HostKind.Lodestone,28,1,true);a.Source(HostKind.Shadow,34,1,true);
            a.Ledge(65,1.6f,5);a.Ledge(71,3.2f,5);a.Ledge(80,4.8f,8);a.Key(78,6.1f);a.Nail(82,5.2f);
            a.Ledge(72,9,5);a.Mercy(72,10.3f);a.Health(63,1.2f);
            b.session.Turned+=()=>{sun.LockNoon();for(int i=0;i<latches.Count;i++){latches[i].active=false;latches[i].gate.SetOpen(false);latches[i].requiredSun=sun;latches[i].requiredCaster=screens[i].GetComponent<Collider2D>();}};
            b.Tip(new Vector2(8,2),"I detaches your shadow. The moving sunlight makes a bridge under each hanging saint. Return the shadow to your feet before walking on.");
            b.Tip(new Vector2(60,2),"At noon a vertical ray cannot make a sideways bridge. Pull the iron screen along its visible rail; its real shadow must connect your feet to the latch.");
            a.Cure(HostKind.None,4,1,true);
        }
        void Scripture(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-13,119,53),new Vector2(2,1));var b=a.b;a.Floor(-5,17);a.Floor(17,102,-6);a.Exit(2,1.1f);a.Source(HostKind.Ink,8);a.Ledge(15,2,5);
            var words=new List<Transform>();for(int i=0;i<18;i++){Vector2 p=new Vector2(21+i%5*5,i/5*2.2f+2);var g=a.Ledge(p.x,p.y,3.2f);g.name="Word "+i;PrimitiveArt.Label(new[]{"WE","WERE","HERE","BEFORE","YOU"}[i%5],g.transform,p+Vector2.up*.55f,.08f);words.Add(g.transform);}
            var punctuation=b.Trigger("Comma that rewraps the sentence",new Vector2(16,2.9f),Vector2.one,new Color(.8f,.65f,.89f),PrimitiveArt.Icon.Key).AddComponent<ScriptureLayout>();punctuation.words=words.ToArray();punctuation.origin=new Vector2(21,1.8f);punctuation.spacing=5;punctuation.rowHeight=2.2f;
            a.Ledge(52,10,6);a.Source(HostKind.Shadow,53,11,true);var sun=Sun(a,new Vector2(58,21),21);a.Metal(new Vector2(62,10),new Vector2(2,5),4,true);
            a.Ledge(63,12,5);a.Ledge(73,14,5);a.Ledge(83,16,5);a.Ledge(95,18,12);a.Key(93,19.3f);a.Nail(100,18.4f);
            var secretGate=b.Door(new Vector2(63,14),new Vector2(.6f,4));var semicolon=b.Trigger("Semicolon's footnote",new Vector2(62,11),Vector2.one*.6f,new Color(.4f,.3f,.58f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();semicolon.gate=secretGate;a.Ledge(67,12,4);a.Mercy(67,13.3f);
            var returnComma=b.Switch(new Vector2(97,19),"WRAP THE RETURN");returnComma.Changed+=v=>punctuation.wrap=v?3:5;
            b.session.Turned+=()=>punctuation.erasing=true;a.Health(52,11.4f);b.Tip(new Vector2(8,2),"A footstep is wet ink. One second later it becomes a one-way platform; eight seconds later it is gone. U lifts the pen. Land on a permanent word before your sentence expires.");a.Cure(HostKind.None,4,1,true);
        }
        void WhiteGate(AtlasBuilder a)
        {
            a.Begin(new Rect(-12,-25,215,86),new Vector2(2,1));var b=a.b;a.Floor(-6,195);var whiteExit=a.Exit(2,1.1f);b.session.Camera.bounds=new Rect(-10,-10,212,70);
            // Five distinct reinterpretations are live from entry, before the final Nail.
            a.Source(HostKind.Echo,8);var leading=b.Trigger("The first footsteps are not yours",new Vector2(8,1),new Vector2(4,6),Color.clear).AddComponent<LeadingEchoZone>();
            a.Source(HostKind.Molt,12,1,true);var p1=b.Plate(new Vector2(18,.14f),.6f);var p2=b.Plate(new Vector2(28,.14f),.35f);var gate=b.Door(new Vector2(33,4),new Vector2(.7f,8),p1,p2);gate.latched=true;
            a.Source(HostKind.Mirror,9,7);a.Source(HostKind.Ink,13,7,true);a.Steps(7,2,3,2,2,3);a.Ledge(12,6,8);a.Ledge(29,6,7); // Mirror + Ink is the upper alternative.
            a.Cure(HostKind.None,37,1,true);a.Source(HostKind.Root,40);a.Source(HostKind.Gullet,43,1,true);a.Source(HostKind.Wax,46,1,true);
            var seasonObject=b.Trigger("Oblique season",new Vector2(49,1),Vector2.one,new Color(.7f,.74f,.42f),PrimitiveArt.Icon.Arch);var season=seasonObject.AddComponent<SeasonWheel>();season.width=10;season.drifting=true;season.speed=.6f;season.bandAngle=45;
            b.Solid("Diagonal seasonal buttress",new Vector2(54,3),new Vector2(8,12));a.SoilPath(new Vector2(47,1),new Vector2(47,-4),new Vector2(60,-4),new Vector2(60,1));a.Source(HostKind.Root,48,1,true);
            a.Source(HostKind.Stitch,41,1);var seasonalHinge=a.Hinge(new Vector2(45,7),19,-25,"season");a.Seam(new Vector2(62,15.5f),"season");a.Steps(41,2,4,2,2,3);a.Ledge(63,15,6);a.Source(HostKind.Marionette,61,16).rail=a.Rail(new Vector2(60,24),new Vector2(83,24));
            a.Cure(HostKind.None,69,1,true);a.Source(HostKind.Parallax,71);a.Projection(new Vector2(75,4),new Vector2(10,10));a.Depth(new Vector2(77,2),new Vector2(7,.5f),0);a.Depth(new Vector2(83,4),new Vector2(7,.5f),2);a.Projection(new Vector2(83,5),new Vector2(10,8));
            var folded=a.Hinge(new Vector2(90,3),10,0,"depth");a.Seam(new Vector2(97,10),"depth");a.Source(HostKind.Stitch,86,5,true);a.Ledge(97,10,5);a.Source(HostKind.InsideOut,101,1.8f);var fresco=PaintedPassage(a,new Vector2(99,0));a.Cure(HostKind.InsideOut,125,12);a.Ledge(125,10,6);
            a.Source(HostKind.Censer,130,1);a.Source(HostKind.Coffin,134,1,true);var risers=new List<MotionPlatform>();for(int i=0;i<4;i++)risers.Add(b.Slider(new Vector2(135+i*6,1),new Vector2(135+i*6,15+i*2),new Vector2(5,.5f),1.3f+i*.2f));a.Ledge(158,20,7);
            a.Source(HostKind.Lodestone,160,21);a.Source(HostKind.Shadow,164,21,true);a.Ledge(172,21,8);a.Ledge(184,23,13);
            var domainObject=new GameObject("The bright side of shadow");domainObject.transform.SetParent(b.root);var domain=domainObject.AddComponent<ShadowDomain>();domain.area=new Rect(158,15,40,18);var lightPaths=new List<Collider2D>();
            foreach(var pair in new[]{new Vector2(165,20),new Vector2(171,22),new Vector2(179,24)})lightPaths.Add(b.Trigger("Luminous path",pair,new Vector2(8,4),new Color(.98f,.95f,.77f,.18f)).GetComponent<Collider2D>());domain.lightPaths=lightPaths.ToArray();
            a.Metal(new Vector2(175,24),Vector2.one*2,3,false,-1);var finalGate=b.Door(new Vector2(181,25),new Vector2(.65f,5));var hand=b.Trigger("Hand inside the light",new Vector2(178,24),Vector2.one*.6f,new Color(.5f,.4f,.7f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();hand.gate=finalGate;
            a.Key(188,24.3f);a.Nail(194,23.45f);
            var rhythmGate=b.Door(new Vector2(188,28),new Vector2(.6f,3));a.Ledge(190,26.5f,5);a.Mercy(192,27.8f);var rhyme=b.Trigger("The old parade remembered",new Vector2(185,24),new Vector2(5,7),Color.clear).AddComponent<RhythmMemory>();rhyme.gate=rhythmGate;
            var retreat=new List<GameObject>();for(int i=0;i<23;i++){var g=a.Ledge(6+i*8,29,7);g.SetActive(false);retreat.Add(g);}a.Ledge(194,25,4);a.Ledge(193,27,4);
            var collapseObject=new GameObject("Final controlled collapse");collapseObject.transform.SetParent(b.root);var collapse=collapseObject.AddComponent<DescentController>();collapse.altitude=500;collapse.speed=.8f;collapse.exit=whiteExit.transform;collapse.Capture(b);
            b.session.Turned+=()=>{collapse.falling=true;foreach(var g in retreat)g.SetActive(true);foreach(var r in risers){var old=r.origin;r.origin=r.end;r.end=old;}finalGate.SetOpen(true);};
            a.Health(70,1.2f);a.Health(130,1.2f);b.Tip(new Vector2(8,2),"Here your echo receives each command first. Your body executes it two seconds later. Four more sanctums bend familiar rules. The final Nail still does not heal you.");
        }
    }
}