using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Halos(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,128,54),new Vector2(2,1));var b=a.b;
            a.Floor(-5,18);a.Floor(18,112,-6);a.Exit(2,1.1f);a.Source(HostKind.Lodestone,8);
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
            BellScreen(a);
            a.Ledge(44.5f,8,6);var secretCoil=a.Metal(new Vector2(44.5f,7.05f),new Vector2(2,.7f),20,true,1);secretCoil.name="Orbital entry coil";secretCoil.strength=170;secretCoil.enabled=false;
            var secretLever=b.Switch(new Vector2(44.5f,8.9f),"ORBITAL ENTRY");secretLever.Changed+=on=>secretCoil.enabled=on;
            var ring=new GameObject("Orbiting Mercy halo");ring.transform.SetParent(b.root);ring.transform.position=new Vector2(45,17.2f);ring.layer=Layers.Moving;
            var edge=ring.AddComponent<EdgeCollider2D>();var arc=new Vector2[42];for(int i=0;i<arc.Length;i++){float theta=Mathf.Lerp(-30,210,i/(float)(arc.Length-1))*Mathf.Deg2Rad;arc[i]=new Vector2(Mathf.Cos(theta),Mathf.Sin(theta))*1.65f;}edge.points=arc;edge.edgeRadius=.07f;
            var rimLine=PrimitiveArt.Line("Open iron rim",ring.transform,Vector2.zero,Vector2.zero,.13f,new Color(.9f,.75f,.35f),8);rimLine.transform.localPosition=Vector3.zero;rimLine.useWorldSpace=false;rimLine.positionCount=arc.Length;for(int i=0;i<arc.Length;i++)rimLine.SetPosition(i,arc[i]);
            var ringMotion=ring.AddComponent<MotionPlatform>();ringMotion.pattern=MotionPlatform.Pattern.Orbit;ringMotion.origin=new Vector2(45,17.2f);ringMotion.radius=.9f;ringMotion.speed=.65f;
            var mercy=b.Collect(PickupKind.Mercy,new Vector2(45,17.2f),"GB-L17-MERCY");mercy.transform.SetParent(ring.transform,true);mercy.transform.localPosition=Vector3.zero;
            foreach(var anchor in new[]{new Vector2(42.5f,19.5f),new Vector2(47.5f,19.5f)}){var m=a.Metal(anchor,Vector2.one,12,true,anchor.x<45?1:-1);m.name="Orbital velocity matching mass";m.strength=100;m.fieldRadius=5;}
            b.Tip(new Vector2(49,9),"The Mercy travels inside an open iron halo. Leave the ordinary route to match it between the two opposed masses; enter through the moving gap.");
            b.session.Turned+=()=>choir.desynchronized=true;a.Health(38,7.3f);a.Cure(HostKind.Lodestone,4,1);
            b.Tip(new Vector2(8,2),"U changes your pole. E powers each visible north coil. South holds you; north pushes you away. The choir reverses the overhead iron. Start a launch from the forward edge, not directly over the coil.");
        }
        void BellScreen(AtlasBuilder a)
        {
            var b=a.b;
            var screen=a.Metal(new Vector2(83,11.7f),new Vector2(4.4f,.6f),1,false,1);screen.name="Hanging iron screen";screen.strength=60;screen.gameObject.layer=Layers.Prop; // Dynamic load uses solver contact, not kinematic platform carry.
            var deck=screen.GetComponent<Rigidbody2D>();deck.gravityScale=2;deck.constraints=RigidbodyConstraints2D.FreezeRotation;
            var rail=screen.gameObject.AddComponent<SliderJoint2D>();rail.autoConfigureConnectedAnchor=false;rail.connectedAnchor=deck.position;rail.autoConfigureAngle=false;rail.angle=90;rail.useLimits=true;rail.limits=new JointTranslationLimits2D{min=-8,max=0};rail.enableCollision=true;
            var bell=a.Metal(new Vector2(78.2f,15),new Vector2(1.2f,1.4f),4,false,1);bell.name="Loose bell counterweight";bell.strength=170;bell.fieldRadius=4.5f;
            var weight=bell.GetComponent<Rigidbody2D>();weight.gravityScale=2;weight.linearDamping=.08f;
            var saddle=b.Solid("Greased bell saddle",new Vector2(78.2f,13.9f),new Vector2(1,.4f));var slick=new PhysicsMaterial2D("Greased iron"){friction=0};saddle.GetComponent<Collider2D>().sharedMaterial=slick;bell.GetComponent<Collider2D>().sharedMaterial=slick;
            b.Solid("Bell saddle backstop",new Vector2(77.2f,15.4f),new Vector2(.4f,3));
            b.Solid("Separate counterweight chute",new Vector2(80.35f,11.6f),new Vector2(.2f,8.2f),new Color(.57f,.60f,.64f));
            a.Ledge(81,7,10);a.Ledge(92,19.4f,10);a.Key(91,20.7f);a.Nail(96,19.8f);
            var pulleyObject=new GameObject("Loose-bell hanging-screen cable");pulleyObject.transform.SetParent(b.root);var cable=pulleyObject.AddComponent<CablePulley>();cable.Configure(deck,weight,new Vector2(83,28),new Vector2(78.2f,28));
            b.Tip(new Vector2(71,13),"Repel from the final coil and catch the suspended screen. Brace against the left guide: SOUTH pulls the loose bell off its saddle; its falling mass tensions the cable and lifts your real platform.");
            PrimitiveArt.Line("Screen guide",b.root,new Vector2(83,11),new Vector2(83,21),.04f,new Color(.66f,.68f,.72f),-1);
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
                var gate=b.Door(new Vector2(24+dx,3),new Vector2(.65f,6));gate.name="Noon shutter "+i;
                var latch=b.Trigger("Shadow latch "+i,new Vector2(20.5f+dx,.35f),Vector2.one*.6f,new Color(.32f,.27f,.46f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();latch.gate=gate;latch.radius=.6f;latches.Add(latch);
                var screen=a.Metal(new Vector2(18+dx,6.5f),new Vector2(8,.5f),.6f,false,1);screen.name="Manufactured shadow screen "+i;screen.strength=80;
                var body=screen.GetComponent<Rigidbody2D>();body.gravityScale=0;body.constraints=RigidbodyConstraints2D.FreezePositionY|RigidbodyConstraints2D.FreezeRotation;body.linearDamping=.15f;screen.gameObject.layer=Layers.Moving;screens.Add(screen);
                b.Solid("Screen left stop "+i,new Vector2(13.75f+dx,6.5f),new Vector2(.5f,1));b.Solid("Screen right stop "+i,new Vector2(26.65f+dx,6.5f),new Vector2(.5f,1));
                PrimitiveArt.Line("Screen rail "+i,b.root,new Vector2(14+dx,7.2f),new Vector2(26.4f+dx,7.2f),.06f,new Color(.76f,.59f,.26f),0);
            }
            a.Source(HostKind.Lodestone,28,1,true);a.Source(HostKind.Shadow,34,1,true);
            a.Ledge(65,1.6f,5);a.Ledge(71,3.2f,5);a.Ledge(80,4.8f,9);a.Key(78,6.1f);a.Nail(82,5.2f);
            var chamberFloor=a.Ledge(72,6.5f,9);chamberFloor.name="Suspension chamber floor";chamberFloor.AddComponent<OneWaySurface>();
            var mercyShutter=b.Door(new Vector2(70,10.5f),new Vector2(.55f,8));mercyShutter.name="Own-body shadow shutter";a.Mercy(68.4f,7.8f);
            var chamberSun=Sun(a,new Vector2(72,18),20);chamberSun.name="Single lamp of the perfectly lit chamber";chamberSun.renderFilled=true;
            var chamberObject=new GameObject("Opaque chamber lighting boundary");chamberObject.transform.SetParent(b.root);var chamber=chamberObject.AddComponent<ShadowDomain>();chamber.area=new Rect(66,4,12,12);chamber.onlySun=chamberSun;
            var suspension=new List<MagneticBody>();foreach(float x in new[]{68f,76f}){var m=a.Metal(new Vector2(x,12),Vector2.one,20,true,1);m.name="Suspension screen "+x;m.strength=260;m.fieldRadius=7;m.enabled=false;suspension.Add(m);}
            // Transparent physical guides constrain drift without casting an opaque bridge.
            foreach(float x in new[]{71.2f,72.8f})b.Solid("Clear glass suspension guide",new Vector2(x,10.5f),new Vector2(.18f,4),new Color(.62f,.8f,.82f,.28f));
            var power=b.Switch(new Vector2(72,8.4f),"SUSPENSION COILS");power.name="Suspension coils switch";power.Changed+=on=>{foreach(var m in suspension)m.enabled=on;};
            var own=b.Trigger("Only your body can cast this bridge",new Vector2(72,5.6f),Vector2.one*.55f,new Color(.32f,.27f,.46f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();own.gate=mercyShutter;own.radius=.55f;own.requiredSun=chamberSun;own.requiredCaster=a.Player.Shape;
            b.Tip(new Vector2(74,7.5f),"The two screens hold a SOUTH Host between them. Detach your shadow while your real body remains suspended: nothing else casts the central bridge.");a.Health(63,1.2f);
            b.session.Turned+=()=>{sun.LockNoon();for(int i=0;i<latches.Count;i++){latches[i].active=false;latches[i].gate.SetOpen(false);latches[i].requiredSun=sun;latches[i].requiredCaster=screens[i].GetComponent<Collider2D>();}};
            b.Tip(new Vector2(8,2),"I detaches your shadow. The moving sunlight makes a bridge under each hanging saint. Return the shadow to your feet before walking on.");
            b.Tip(new Vector2(60,2),"At noon a vertical ray cannot make a sideways bridge. Pull the iron screen along its visible rail; its real shadow must connect your feet to the latch.");
            a.Cure(HostKind.None,4,1,true);
        }
        void Scripture(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,130,48),new Vector2(2,9.1f));var b=a.b;
            a.Floor(-5,12,8);a.Floor(12,110,-8);a.Exit(2,9.1f);a.Source(HostKind.Ink,8,9);
            a.Ledge(17,1,4).name="Margin landing alcove";a.Ledge(20.5f,5,8).name="Punctuation desk";
            var comma=b.Prop(new Vector2(19.5f,5.6f),new Vector2(.7f,1.1f),.65f);comma.name="Movable line-break comma";
            var cart=comma.AddComponent<PunctuationCart>();cart.firstSlot=19.5f;cart.slotWidth=3;cart.lineLengths=new[]{5,3};
            comma.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeRotation;comma.GetComponent<Rigidbody2D>().linearDamping=2;
            b.Solid("Left comma stop",new Vector2(16.6f,5.4f),new Vector2(.25f,.8f));b.Solid("Right comma stop",new Vector2(23.3f,5.4f),new Vector2(.25f,.8f));
            for(int k=0;k<2;k++)PrimitiveArt.Label(k==0?"FIVE WORDS":"THREE WORDS",b.root,new Vector2(19.5f+k*3,4.3f),.06f);
            var words=new List<Transform>();for(int k=0;k<6;k++){
                var word=a.Ledge(27+k%5*4,9.4f-k/5*2.2f,3);word.name="Scrolling word "+k;word.AddComponent<OneWaySurface>();word.AddComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;
                PrimitiveArt.Label(new[]{"WE","WERE","HERE","BEFORE","YOU","RETURN"}[k],word.transform,(Vector2)word.transform.position+Vector2.up*.6f,.07f);words.Add(word.transform);
            }
            var layout=b.root.gameObject.AddComponent<ScriptureLayout>();layout.words=words.ToArray();layout.punctuation=cart;layout.origin=new Vector2(27,9.2f);layout.spacing=4;layout.rowHeight=-2.2f;layout.wrap=5;layout.scrollAmplitude=.25f;
            var corrector=b.root.gameObject.AddComponent<ScriptureCorrector>();
            a.Ledge(40,7.2f,5).name="Last permanent margin before the semicolon";a.Source(HostKind.Shadow,40,8.2f,true);
            // The upper dot is a collectible in shadow space, beyond the tether from the safe margin.
            var sun=Sun(a,new Vector2(50,25),24);sun.directional=true;sun.direction=new Vector2(-1,-.8f);sun.renderFilled=true;
            var stroke=b.Solid("Giant semicolon curved stem",new Vector2(58,15),new Vector2(3,7),new Color(.28f,.19f,.39f));stroke.AddComponent<ShadowCaster>();
            var dot=b.Collect(PickupKind.Mercy,new Vector2(58.5f,17.5f),"GB-L19-MERCY");dot.gameObject.AddComponent<ShadowMercy>();dot.name="The semicolon dot";
            a.Ledge(47,5.2f,3).name="Optional suspended paper margin";a.Ledge(50,3.3f,4).name="Lower line-break refuge";
            var lowerWords=new List<GameObject>();for(int k=0;k<9;k++){
                var word=a.Ledge(55+k*5,2+k*.7f,3.5f);word.name="Imperative word "+k;word.AddComponent<OneWaySurface>();lowerWords.Add(word);
                PrimitiveArt.Label(new[]{"DO","NOT","REPEAT","THE","PATH","YOU","TOOK","TO","ME"}[k],word.transform,(Vector2)word.transform.position+Vector2.up*.5f,.075f);
            }
            a.Ledge(103,9.2f,10);a.Key(101,10.5f);a.Nail(106,9.6f);
            // The correcting manuscript leaves two blank ascents. A short jump writes a
            // low arc; a full jump can later stand on it, then reach the higher new margin.
            var returnLetters=new List<GameObject>();
            foreach(var point in new[]{new Vector2(98,10.8f),new Vector2(92,14),new Vector2(86,9.6f),new Vector2(80,9.6f),new Vector2(74,9.6f),new Vector2(68,9.6f),new Vector2(58,9.6f),new Vector2(48,9.6f),new Vector2(42,12.8f),new Vector2(36,9.6f),new Vector2(30,9.6f),new Vector2(24,9.6f),new Vector2(18,9.6f)}){
                float width=point.x==98?10:point.x==48?10:point.x==58?15:3.5f;
                var g=a.Ledge(point.x,point.y,width);g.name="Fresh return margin "+point.x;g.AddComponent<OneWaySurface>();g.SetActive(false);returnLetters.Add(g);
            }
            foreach(var word in lowerWords)word.AddComponent<ReadOnceWord>();
            b.session.Turned+=()=>{layout.erasing=true;corrector.imperative=true;foreach(var g in returnLetters)g.SetActive(true);};
            b.Tip(new Vector2(97,12),"Old footpaths are being erased. Sketch a short hop here, wait for it to dry solid, then use a full jump to revisit that arc and reach the blank line above.");
            b.Tip(new Vector2(9,10),"Your falling path is wet for one second. Land in the margin, then climb your drying ink back to the comma. E grips punctuation; move it one slot and release.");
            b.Tip(new Vector2(40,9),"The dot is too far from safe paper. Let an ink arc hold your body closer while your shadow travels across the letter's cast silhouette.");
            a.Cure(HostKind.None,4,9,true);a.Health(50,4.5f);
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