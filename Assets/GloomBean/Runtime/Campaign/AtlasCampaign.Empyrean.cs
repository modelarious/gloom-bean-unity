using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Halos(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-13,127,50),new Vector2(2,1));var b=a.b;a.Floor(-5,18);a.Floor(18,112,-6);a.Exit(2,1.1f);a.Source(HostKind.Lodestone,8);
            var poles=new List<MagneticBody>();for(int i=0;i<7;i++){var m=a.Metal(new Vector2(22+i*11,8+i*.6f),Vector2.one*1.5f,4,true,i%2==0?1:-1);m.name="Orbiting iron halo "+i;m.strength=170;
                // A rounded moving hub preserves glancing/tangential motion. The old
                // square stand-in stopped the Host dead against an invisible flat underside.
                var box=m.GetComponent<BoxCollider2D>();if(box)box.enabled=false;
                var rim=m.gameObject.AddComponent<CircleCollider2D>();rim.radius=.75f;rim.sharedMaterial=new PhysicsMaterial2D("Smooth iron"){friction=0,bounciness=0};
                var view=m.GetComponent<SpriteRenderer>();view.sprite=PrimitiveArt.Sprite(PrimitiveArt.Icon.Round);view.drawMode=SpriteDrawMode.Sliced;view.size=Vector2.one*1.5f;
                var caster=m.GetComponent<ShadowCaster>();if(caster)caster.shape=rim;
                var orbit=m.gameObject.AddComponent<MotionPlatform>();orbit.pattern=MotionPlatform.Pattern.Orbit;orbit.origin=m.transform.position;orbit.radius=2;orbit.speed=.35f;orbit.phase=i*.4f;poles.Add(m);}
            var choirObject=new GameObject("Iron choir clock");choirObject.transform.SetParent(b.root);var choir=choirObject.AddComponent<HaloChoir>();choir.halos=poles.ToArray();choir.measure=4;
            a.Ledge(16,2,5);a.Ledge(29,4,4);a.Ledge(46,6,4);a.Ledge(65,8,4);a.Ledge(83,10,4);a.Ledge(99,12,13);a.Key(96,13.3f);a.Nail(104,12.4f);
            a.Metal(new Vector2(38,5),new Vector2(3,.65f),3,false,-1);a.Metal(new Vector2(74,7),new Vector2(3,.65f),3,false,1);
            a.Ledge(60,18,6);a.Mercy(61,19.3f);var secret=a.Metal(new Vector2(57,17),Vector2.one*1.2f,4,true,-1);secret.strength=160;poles.Add(secret);choir.halos=poles.ToArray();
            b.session.Turned+=()=>choir.desynchronized=true;a.Health(46,7.3f);b.Tip(new Vector2(8,2),"Equal poles repel; opposite poles attract. The anchored halo pulls you. The loose iron is pulled back just as hard. U changes your pole.");a.Cure(HostKind.Lodestone,4,1);
        }
        ShadowSun Sun(AtlasBuilder a,Vector2 p,float reach=22)
        {var g=PrimitiveArt.Shape("Noon lamp",a.b.root,p,Vector2.one*2,new Color(1,.94f,.68f),PrimitiveArt.Icon.Star,-1);var s=g.AddComponent<ShadowSun>();s.reach=reach;return s;}
        void Noon(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-11,123,43),new Vector2(2,1));var b=a.b;a.Floor(-5,113);a.Exit(2,1.1f);a.Source(HostKind.Shadow,8);
            var sun=Sun(a,new Vector2(10,15),24);a.Metal(new Vector2(16,2),new Vector2(2,4),4,false,1);var screen=a.Metal(new Vector2(26,3),new Vector2(1,6),7,false,-1);
            var gate=b.Door(new Vector2(35,4),new Vector2(.7f,8));var receiver=b.Trigger("Shadow hand at the window",new Vector2(28,.6f),Vector2.one*.7f,new Color(.35f,.26f,.5f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();receiver.gate=gate;receiver.radius=.9f;
            a.Source(HostKind.Lodestone,22,1,true);a.Source(HostKind.Shadow,40,1,true);a.Metal(new Vector2(45,3),new Vector2(1.2f,6),5,false,1);var second=Sun(a,new Vector2(46,15),22);
            var highGate=b.Door(new Vector2(60,4),new Vector2(.7f,8));var high=b.Trigger("Far shadow latch",new Vector2(55,.5f),Vector2.one*.7f,new Color(.35f,.26f,.5f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();high.gate=highGate;
            a.Steps(65,2,6,6,1.6f,4);a.Ledge(103,10,10);a.Key(98,11.4f);a.Nail(108,10.4f);
            var mercyGate=b.Door(new Vector2(78,5),new Vector2(.6f,4));var own=b.Trigger("The missing final shadow",new Vector2(71,.6f),Vector2.one*.7f,new Color(.4f,.3f,.58f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();own.gate=mercyGate;
            a.Metal(new Vector2(68,3),new Vector2(1,6),5,false,-1);Sun(a,new Vector2(65,16),20);a.Ledge(82,3.4f,5);a.Mercy(84,4.7f);
            b.session.Turned+=()=>{sun.moving=second.moving=true;sun.orbit=new Vector2(12,0);second.orbit=new Vector2(10,0);sun.speed=second.speed=.45f;};
            b.Enemy(new Vector2(52,1),true);a.Health(62,1.2f);b.Tip(new Vector2(8,2),"I leaves your body to control its shadow. Only connected cast silhouettes support it. Shift the metal screen, then follow the new silhouette while your exposed body waits.");a.Cure(HostKind.None,4,1,true);
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