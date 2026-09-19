using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Pears(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-13,125,43),new Vector2(2,1));var b=a.b;a.Floor(-5,18);a.Floor(34,46);a.Floor(59,72);a.Floor(88,111,3);a.Exit(2,1.1f);a.Source(HostKind.Wax,8);a.Ledge(13,1.7f,4);
            var branch1=a.Hinge(new Vector2(18,3.2f),16,-5,"bough");var mass1=branch1.gameObject.AddComponent<MassBranch>();mass1.body=branch1.body;mass1.rest=-5;mass1.stiffness=1.5f;
            var branch2=a.Hinge(new Vector2(43,3.5f),17,8,"bough");var mass2=branch2.gameObject.AddComponent<MassBranch>();mass2.body=branch2.body;mass2.rest=8;mass2.stiffness=1;
            var branch3=a.Hinge(new Vector2(72,5),18,-2,"bough");var mass3=branch3.gameObject.AddComponent<MassBranch>();mass3.body=branch3.body;mass3.rest=-2;mass3.stiffness=1;
            for(int i=0;i<3;i++){float x=new[]{27f,51f,81f}[i],y=new[]{7f,9f,9f}[i];var g=b.Solid("Pear counterweight",new Vector2(x,y),new Vector2(1.3f,1.8f),new Color(.53f,.65f,.27f),Layers.Prop);var rb=g.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;var fruit=g.AddComponent<RipeningFruit>();fruit.ripeAfter=10+i*3;fruit.rotAfter=22+i*3;g.AddComponent<TemporalBody>();}
            a.Ledge(65,1.8f,5);a.Ledge(70,3.6f,4);a.Ledge(97,5,8);a.Key(95,6.2f);a.Nail(103,5.4f);
            var water=b.Water(new Vector2(26,1.5f),new Vector2(10,1),new Vector2(-3,0));var drain=b.Trigger("Pear cage drain",new Vector2(25,2),new Vector2(1.2f,.5f),Color.black).AddComponent<WaxDrain>();drain.channel=water;
            var flow=b.Trigger("Sloping wax runnel",new Vector2(29,2.2f),new Vector2(8,.8f),new Color(.82f,.71f,.38f,.18f)).AddComponent<WaxChannel>();flow.drain=drain;flow.normal=new Vector2(-5,0);flow.diverted=new Vector2(3,6);
            a.Ledge(31,6,4);a.Ledge(36,7.8f,4);var glass=b.Door(new Vector2(38.5f,8.6f),new Vector2(.45f,2));var scale=b.Plate(new Vector2(35.8f,7.94f),.1f);var window=glass.gameObject.AddComponent<MassWindow>();window.plate=scale;window.gate=glass;window.minimum=.65f;window.maximum=.85f;a.Ledge(41,7.8f,4);a.Mercy(41,8.8f);
            b.Solid("Low wax gutter",new Vector2(14,.95f),new Vector2(5,1.05f));a.Health(65,1.1f);
            b.session.Turned+=()=>{mass1.rest=-14;mass2.rest=-10;mass3.rest=-12;var chain=b.root.gameObject.AddComponent<BoughCollapse>();chain.branches=new[]{branch3,branch2};};
            b.Tip(new Vector2(8,2),"The angel seals you in wax. Melt to follow a runnel; leave part of yourself to block its drain. Pears bend the wood when they gain mass.");a.Cure(HostKind.Wax,4,1);
        }
        void Kitchen(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,133,46),new Vector2(2,1));var b=a.b;a.Floor(-5,54);a.Floor(64,91);a.Floor(98,119,3);a.Exit(2,1.1f);a.Source(HostKind.Gullet,8);
            a.Chunk(new Vector2(24,2.5f),new Vector2(2,5));a.Socket(new Vector2(59,-.5f),new Vector2(6,2));
            var loose=a.Chunk(new Vector2(36,3),new Vector2(5,1));a.Ledge(32,1.8f,4);a.Ledge(43,4.8f,4);
            var jet=b.Trigger("Dish steam plume",new Vector2(49,4),new Vector2(4,10),new Color(.83f,.84f,.8f,.12f)).AddComponent<AirJet>();jet.force=new Vector2(0,47);
            var dish=b.Solid("The steaming dish",new Vector2(39,.45f),new Vector2(5,.9f),new Color(.76f,.72f,.53f)).AddComponent<FeastDish>();dish.steam=jet;
            var emitterObject=new GameObject("Grease hatch");emitterObject.transform.SetParent(b.root);emitterObject.transform.position=new Vector2(31,8);var emitter=emitterObject.AddComponent<FlowEmitter>();emitter.normalVelocity=new Vector2(4.3f,0);emitter.divertedVelocity=new Vector2(8,2);
            // The stream and dish occupy actual physics space: moving the tile changes where drops land.
            a.Ledge(53,9,5);a.Ledge(63,9,5);a.Ledge(71,7.5f,6);a.Source(HostKind.Wax,74,1,true);
            var drain=b.Trigger("Service sink",new Vector2(78,.35f),new Vector2(1.4f,.6f),Color.black).AddComponent<WaxDrain>();drain.emitter=emitter;
            a.Chunk(new Vector2(86,2.5f),new Vector2(2,5));a.Socket(new Vector2(94,-.5f),new Vector2(6,2));a.Ledge(84,5.2f,6);a.Ledge(92,7,5);a.Ledge(101,9,8);a.Ledge(110,11,9);a.Key(102,10.2f);a.Nail(113,11.4f);
            a.Ledge(32,7,4);a.Ledge(39,9,4);a.Mercy(42,10.2f);a.Health(69,1.2f);
            var returnJet=b.Trigger("Return service draft",new Vector2(61,4),new Vector2(6,10),new Color(.7f,.76f,.78f,.08f)).AddComponent<AirJet>();returnJet.force=new Vector2(-10,44);
            b.session.Turned+=()=>{emitter.reversed=true;returnJet.always=true;jet.always=true;jet.force=new Vector2(-5,43);};
            b.Tip(new Vector2(8,2),"The communion snail lends you its gullet. Swallow one real piece of the room. Its old support is gone until you spit it somewhere else.");a.Cure(HostKind.None,4,1,true);
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
            a.Begin(new Rect(-8,-13,135,43),new Vector2(2,1));var b=a.b;a.Floor(-5,31);a.Floor(38,63);a.Floor(73,94);a.Floor(100,122,2);a.Exit(2,1.1f);
            var wheel=b.Trigger("Season wheel",new Vector2(20,1.4f),Vector2.one*1.5f,new Color(.87f,.71f,.43f),PrimitiveArt.Icon.Arch).AddComponent<SeasonWheel>();wheel.width=24;wheel.speed=1.2f;
            var bands=new Transform[4];Color[] colors={new Color(.36f,.71f,.45f,.1f),new Color(.9f,.47f,.27f,.1f),new Color(.55f,.32f,.65f,.1f),new Color(.51f,.76f,.92f,.1f)};for(int i=0;i<4;i++)bands[i]=PrimitiveArt.Shape("Moving climate band",b.root,new Vector2(i*24+20,10),new Vector2(24,45),colors[i],PrimitiveArt.Icon.Block,-6).transform;wheel.bands=bands;
            a.Source(HostKind.Wax,9);b.Solid("Wax capillary roof",new Vector2(15,.9f),new Vector2(5,1.1f));
            a.Source(HostKind.Gullet,41,1,true);a.Chunk(new Vector2(48,2.8f),new Vector2(2,5.6f));a.Socket(new Vector2(68,-.5f),new Vector2(7,2));
            a.Source(HostKind.Root,54,1,true);b.Solid("Climate earth",new Vector2(61,1),new Vector2(6,11));a.SoilPath(new Vector2(55,1),new Vector2(55,-4),new Vector2(65,-4),new Vector2(65,1),new Vector2(70,1));
            for(int i=0;i<4;i++){var g=b.Solid("Seasonal growing step",new Vector2(78+i*6,1),new Vector2(4,2),new Color(.42f,.58f,.36f),Layers.Moving);var growth=g.AddComponent<SeasonGrowth>();growth.wheel=wheel;growth.size=new Vector2(4,3+i*.4f);growth.floor=0;}
            a.Ledge(105,4,5);a.Ledge(113,6,8);a.Key(108,5.4f);a.Nail(117,6.45f);
            a.SoilPath(new Vector2(55,1),new Vector2(55,7),new Vector2(63,7),new Vector2(63,10));a.Ledge(63,9,5);a.Mercy(63,10.2f);a.Health(42,1.2f);
            b.session.Turned+=()=>wheel.drifting=true;b.Tip(new Vector2(20,2),"E turns the season wheel by one band. Look at wet roots and growing wood before committing. The wheel will not remain still on the return.");a.Cure(HostKind.None,4,1,true);
        }
    }
}