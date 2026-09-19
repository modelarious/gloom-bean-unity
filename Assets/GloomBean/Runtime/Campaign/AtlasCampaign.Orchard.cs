using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Pears(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,120,38),new Vector2(2,1));var b=a.b;
            a.Floor(-5,20);a.Floor(20,30,-2);a.Floor(83,102,3.2f);a.Exit(2,1.1f);a.Source(HostKind.Wax,7);
            b.Solid("Tall capillary canopy",new Vector2(13.5f,3.5f),new Vector2(7,6.2f));
            var drain=b.Trigger("Pear cage drain",new Vector2(22,-1.75f),new Vector2(1.2f,.5f),Color.black).AddComponent<WaxDrain>();
            var channel=b.Trigger("Pear cage rising runnel",new Vector2(26,.5f),new Vector2(5,7),new Color(.82f,.71f,.38f,.2f)).AddComponent<WaxChannel>();channel.drain=drain;channel.normal=new Vector2(0,-3);channel.diverted=new Vector2(0,5);
            PrimitiveArt.Label("OPEN DRAIN / LOW BASIN",b.root,new Vector2(23,-3),.08f);
            a.Ledge(31,3.3f,6);
            var branches=new FoldPanel[3];var masses=new MassBranch[3];
            for(int i=0;i<3;i++){
                branches[i]=a.Hinge(new Vector2(34+i*17,3.1f),14,3,"bough");branches[i].name="Living pear bough "+i;
                masses[i]=branches[i].gameObject.AddComponent<MassBranch>();masses[i].body=branches[i].body;masses[i].rest=3;masses[i].stiffness=.7f;masses[i].maxDeflection=8;
                var fruit=b.Solid("Ripening pear counterweight",new Vector2(42+i*17,7.4f),new Vector2(1.2f,1.7f),new Color(.53f,.65f,.27f),Layers.Prop);
                var rb=fruit.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;var pear=fruit.AddComponent<RipeningFruit>();pear.ripeAfter=18+i*3;pear.rotAfter=45+i*4;fruit.AddComponent<TemporalBody>();
            }
            a.Key(92,4.4f);a.Nail(98,3.65f);a.Health(88,4.4f);
            var returned=a.Floor(18,88,0);returned.name="Collapsed orchard maintenance path";returned.SetActive(false);
            b.session.Turned+=()=>{returned.SetActive(true);foreach(var m in masses)m.rest=-3;var collapse=b.root.gameObject.AddComponent<BoughCollapse>();collapse.branches=new[]{branches[2],branches[1]};};
            // Optional fine balance: one plug is needed for flow; a second makes a short, 0.56-mass body.
            a.Ledge(35,4.8f,4).AddComponent<OneWaySurface>();a.Ledge(39,6.3f,5).AddComponent<OneWaySurface>();a.Ledge(44,6.3f,6).AddComponent<OneWaySurface>();
            b.Solid("Glass fruit bell roof",new Vector2(43,8.2f),new Vector2(8,1.4f));
            var gate=b.Door(new Vector2(41,7),new Vector2(.4f,1.4f));gate.name="Glass fruit balance shutter";
            var scale=b.Plate(new Vector2(39,6.44f),.1f);var window=gate.gameObject.AddComponent<MassWindow>();window.plate=scale;window.gate=gate;window.minimum=.50f;window.maximum=.62f;window.holdSeconds=4;
            var release=b.Switch(new Vector2(44,7.05f),"UNHOOK GLASS BELL");release.Changed+=v=>{if(v){gate.latched=true;gate.SetOpen(true);}};a.Mercy(44,7.15f);
            b.Tip(new Vector2(8,2),"The wax enters the capillary below the canopy. Leave some of your body over the basin drain; the stopped outflow rises into the pear cage.");
            b.Tip(new Vector2(23,-1),"I leaves wax behind your heels. Stand one body-width right of the drain; U reforms only where there is headroom.");
            a.Cure(HostKind.Wax,4,1);
        }
        void Kitchen(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,108,43),new Vector2(2,3));var b=a.b;
            a.Floor(-5,14,2);a.Floor(18,24,2);a.Floor(10,30,-2);a.Floor(42,60,3);a.Floor(60,90,0);a.Exit(2,3.1f);a.Source(HostKind.Gullet,8,3);
            a.Chunk(new Vector2(16,1.5f),new Vector2(4,1));a.Socket(new Vector2(28,-1.5f),new Vector2(4.1f,1.1f));
            var jet=b.Trigger("Dish steam plume",new Vector2(37,2),new Vector2(4,11),new Color(.83f,.84f,.8f,.17f)).AddComponent<AirJet>();jet.force=new Vector2(0,70);
            var dish=b.Solid("Ingredient serving dish",new Vector2(33,-1.15f),new Vector2(3,.3f),new Color(.76f,.72f,.53f)).AddComponent<FeastDish>();dish.steam=jet;
            var hatch=new GameObject("Grease hatch");hatch.transform.SetParent(b.root);hatch.transform.position=new Vector2(26,3);var emitter=hatch.AddComponent<FlowEmitter>();emitter.normalVelocity=new Vector2(3,0);emitter.divertedVelocity=new Vector2(-3,0);emitter.interval=.65f;
            a.Ledge(37,-1,3);a.Ledge(42,7,7);a.Ledge(46,5,4);a.Source(HostKind.Wax,46,4,true);
            b.Solid("Kitchen wax service roof",new Vector2(52,6.3f),new Vector2(7,5.8f));
            a.Ledge(67,1.8f,4);a.Ledge(72,3.6f,4);a.Ledge(77,5.4f,4);a.Ledge(83,7.2f,8);a.Key(80,8.35f);a.Nail(85,7.65f);a.Health(64,1.2f);
            // Plate a real chunk on the high serving tray; it remains the floor of its destination.
            var garnish=a.Chunk(new Vector2(42,7.5f),new Vector2(3,1));garnish.name="Edible garnish of architecture";
            var tray=b.Solid("Freight serving platter",new Vector2(47,6.6f),new Vector2(4,.4f),new Color(.8f,.72f,.52f),Layers.Moving);var rb=tray.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;
            var freight=tray.AddComponent<TerrainServingLift>();freight.lower=new Vector2(47,6.6f);freight.upper=new Vector2(47,12);a.Socket(new Vector2(47,7.3f),new Vector2(3.2f,1.1f));a.Ledge(52,13,5);a.Mercy(52,14.1f);
            var back=a.Floor(-1,90,10.8f);back.name="Reversed kitchen service gallery";back.SetActive(false);
            // The overhead gallery opens only after the first circuit has been physically routed.
            a.Ledge(88,9,4);var drop=b.Switch(new Vector2(88,10),"UNLATCH SERVICE WALK");drop.Changed+=v=>{if(v&&b.session.Phase==RunPhase.Returning)back.SetActive(true);};
            b.session.Turned+=()=>{emitter.reversed=true;jet.always=true;jet.force=new Vector2(-4,65);freight.returning=true;};
            b.Tip(new Vector2(15,3),"The floor can be a serving runway somewhere else. Swallow its four-unit tile, descend into the pantry and place that SAME piece below the falling grease.");
            b.Tip(new Vector2(28,-1),"A raised tile carries grease to the high dish. Grease heats its steam pipe; missing support sends the ingredient to waste.");a.Cure(HostKind.None,4,3,true);
        }
        void Ditch(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-13,125,41),new Vector2(2,1));var b=a.b;a.Floor(-5,108);a.Exit(2,1.1f);a.Source(HostKind.Root,9);
            b.Solid("Packed earth buttress A",new Vector2(17,1),new Vector2(10,12));a.SoilPath(new Vector2(10,1),new Vector2(10,-4),new Vector2(20,-4),new Vector2(20,1),new Vector2(24,1));
            a.Source(HostKind.Gullet,28,1,true);a.Source(HostKind.Root,33,1,true);
            b.Solid("Packed earth buttress B",new Vector2(41,1),new Vector2(10,12));a.SoilPath(new Vector2(34,1),new Vector2(34,-4.8f),new Vector2(44,-4.8f),new Vector2(44,1),new Vector2(50,1));
            a.Source(HostKind.Root,51);a.Chunk(new Vector2(55,.1f),new Vector2(3,1.2f));a.SoilPath(new Vector2(53,1),new Vector2(53,-4),new Vector2(64,-4),new Vector2(64,1));
            b.Solid("Packed earth buttress C",new Vector2(60,2),new Vector2(5,9));a.Source(HostKind.Root,69);b.Solid("Packed earth buttress D",new Vector2(77,1),new Vector2(9,12));a.SoilPath(new Vector2(70,1),new Vector2(70,-4),new Vector2(81,-4),new Vector2(81,1),new Vector2(85,1));
            var upper=a.Soil(new Vector2(10,1),new Vector2(10,9));a.Soil(new Vector2(10,9),new Vector2(18,9));a.Ledge(18,8,6);a.Mercy(18,9.3f);
            var lower=a.Soil(new Vector2(10,-4),new Vector2(20,-4));var pump=b.Trigger("Local irrigation wheel",new Vector2(7,1),Vector2.one,new Color(.53f,.7f,.7f),PrimitiveArt.Icon.Arch).AddComponent<RootPump>();pump.channels=new[]{lower,upper};pump.interval=16;upper.wet=false;
            a.Ledge(90,2,5);a.Ledge(97,4,6);a.Ledge(105,6,8);a.Key(99,5.3f);a.Nail(107,6.45f);a.Health(29,1.2f);a.Health(67,1.2f);
            b.session.Turned+=()=>{pump.reversed=true;pump.automatic=true;};b.Tip(new Vector2(10,2),"Hold U to grow through the moist seam. Release only when the tip reaches a body-sized pocket. Your body follows every bend, not a straight teleport.");a.Cure(HostKind.Root,4,1);
        }
        void Seasons(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-13,135,43),new Vector2(2,1));var b=a.b;a.Floor(-5,31);a.Floor(38,42);a.Floor(47,63);a.Floor(40,49,-2);a.Floor(73,99);a.Floor(100,122,2);a.Exit(2,1.1f);
            var wheel=b.Trigger("Season wheel",new Vector2(20,1.4f),Vector2.one*1.5f,new Color(.87f,.71f,.43f),PrimitiveArt.Icon.Arch).AddComponent<SeasonWheel>();wheel.width=24;wheel.speed=6;a.Source(HostKind.Wax,19,1,true);
            var bands=new Transform[4];Color[] colors={new Color(.36f,.71f,.45f,.1f),new Color(.9f,.47f,.27f,.1f),new Color(.55f,.32f,.65f,.1f),new Color(.51f,.76f,.92f,.1f)};for(int i=0;i<4;i++)bands[i]=PrimitiveArt.Shape("Moving climate band",b.root,new Vector2(i*24+20,10),new Vector2(24,45),colors[i],PrimitiveArt.Icon.Block,-6).transform;wheel.bands=bands;
            a.Source(HostKind.Wax,9);b.Solid("Wax capillary roof",new Vector2(15,.9f),new Vector2(5,1.1f));
            a.Source(HostKind.Gullet,40,1,true);a.Chunk(new Vector2(44.5f,-.5f),new Vector2(5,1));a.Ledge(48,-.7f,3);a.Socket(new Vector2(69,.5f),new Vector2(5.2f,1.1f));a.Ledge(34,.6f,2.5f);a.Ledge(65.5f,0,2);
            a.Source(HostKind.Root,54,1,true);b.Solid("Climate earth",new Vector2(61,1),new Vector2(6,11));a.SoilPath(new Vector2(55,1),new Vector2(55,-4),new Vector2(65,-4),new Vector2(65,1),new Vector2(65.5f,1));
            for(int i=0;i<4;i++){var g=b.Solid("Seasonal growing step",new Vector2(78+i*6,1),new Vector2(4,2),new Color(.42f,.58f,.36f),Layers.Moving);var growth=g.AddComponent<SeasonGrowth>();growth.wheel=wheel;growth.size=new Vector2(4,3+i*.4f);growth.floor=0;}
            a.Ledge(105,4,5);a.Ledge(113,6,8);a.Key(108,5.4f);a.Nail(117,6.45f);
            a.SoilPath(new Vector2(55,1),new Vector2(55,7),new Vector2(63,7),new Vector2(63,10));a.Ledge(63,9,5);a.Mercy(63,10.2f);a.Health(42,1.2f);
            b.session.Turned+=()=>wheel.drifting=true;b.Tip(new Vector2(20,2),"E turns the season wheel by one band. Look at wet roots and growing wood before committing. The wheel will not remain still on the return.");a.Cure(HostKind.None,4,1,true);
        }
    }
}