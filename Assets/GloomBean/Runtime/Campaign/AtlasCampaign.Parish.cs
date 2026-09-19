using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Sunday(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,120,40),new Vector2(2,1.1f));var b=a.b;a.Floor(-5,32);a.Floor(33,101,-4);a.Exit(2,1.15f);
            var welcomeSteps=new[]{a.Ledge(10,1.6f,4),a.Ledge(17,2.6f,5),a.Ledge(25,1.5f,4)};b.CoinLine(new Vector2(7,2.5f),new Vector2(28,3.6f),9);
            var pretty=new List<GameObject>();for(int i=0;i<5;i++){var g=PrimitiveArt.Shape("Smiling parade facade",b.root,new Vector2(i*6+5,5),new Vector2(5,7),new Color(.64f+i*.04f,.77f,.61f),PrimitiveArt.Icon.Arch,-3);pretty.Add(g);}
            var horror=b.Solid("Behind the mascot",new Vector2(18,7.5f),new Vector2(5,.4f),new Color(.39f,.17f,.3f));horror.SetActive(false);
            var infection=b.Trigger("The thing under the smiling face",new Vector2(35,-1),new Vector2(3,12),new Color(.9f,.16f,.44f,.35f),PrimitiveArt.Icon.Eye).AddComponent<CorruptionTrigger>();infection.pretty=pretty.ToArray();infection.revealed=new[]{horror};
            if(GameRoot.Instance&&GameRoot.Instance.IsCorrupted)infection.Apply();
            a.Source(HostKind.Echo,40,-3);var plate=b.Plate(new Vector2(46,-3.88f));var door=b.Door(new Vector2(55,-1.5f),new Vector2(.8f,5),plate);door.holdSeconds=.7f;
            b.Tip(new Vector2(39,-2),"A bell-wisp caught in your torn body. Keep walking; the second pair of footsteps arrives later.");
            a.Steps(61,-2.2f,6,5,1.5f,4);a.Ledge(92,5.3f,13);a.Key(81,5);a.Nail(96,5.75f);a.Health(60,-2.8f);
            var backs=new List<GameObject>();for(int i=0;i<9;i++){var g=a.Ledge(28+i*7,5.3f,7.4f);g.name="Backstage return brace";g.SetActive(false);backs.Add(g);}
            b.session.Turned+=()=>{foreach(var g in welcomeSteps)g.SetActive(false);foreach(var g in backs)g.SetActive(true);foreach(var g in pretty)g.SetActive(false);horror.SetActive(true);};
            var jawPlate=b.Plate(new Vector2(27,5.45f));var jaw=b.Door(new Vector2(20,9),new Vector2(.4f,3),jawPlate);jaw.holdSeconds=.6f;
            // The secret lives above the exit route. Its closed jaw must never gate ordinary completion.
            a.Ledge(22,7.4f,3);a.Mercy(18,8.9f);
            // Return-only shelves must not create a 1.2 m headroom trap over the opening 1.5 m Host.
            var returnStepA=a.Ledge(10,3.2f,4);returnStepA.SetActive(false);backs.Add(returnStepA);
            var returnStepB=a.Ledge(17,4.7f,4);returnStepB.SetActive(false);backs.Add(returnStepB);a.Cure(HostKind.Echo,5,1);
            var rhythm=b.Trigger("Parade jump-rope rhythm",new Vector2(11,3.3f),new Vector2(6,7),Color.clear).AddComponent<RhythmMemory>();rhythm.teaching=true;
        }
        void Belfry(AtlasBuilder a)
        {
            a.Begin(new Rect(-7,-7,60,53),new Vector2(2,1));var b=a.b;a.Floor(-5,46);a.Exit(2,1.15f);a.Source(HostKind.Echo,5);
            a.Steps(9,2,4,4,2,3.5f);a.Ledge(26,8,6);
            var r1=a.Receiver(new Vector2(23,10));var r2=a.Receiver(new Vector2(29,10));r1.hold=r2.hold=2;
            a.Bell(new Vector2(8,1),new[]{new Vector2(8,1),new Vector2(8,14),new Vector2(23,14),new Vector2(23,10)},r1);
            a.Bell(new Vector2(19,7),new[]{new Vector2(19,7),new Vector2(19,15),new Vector2(29,15),new Vector2(29,10)},r2);
            var lift=b.Slider(new Vector2(28,8),new Vector2(28,20),new Vector2(4,.5f),3.4f);lift.paused=true;var dual=lift.gameObject.AddComponent<DualPulseLift>();dual.a=r1;dual.b=r2;dual.lift=lift;
            a.Ledge(34,20,5);a.Steps(37,22,5,-4,2,4);a.Ledge(18,32,10);a.Key(20,33.4f);a.Nail(15,32.4f);
            var upper=b.Slider(new Vector2(38,23),new Vector2(38,31),new Vector2(3,.4f),2);upper.SetClock(2);
            var returnGate=b.Door(new Vector2(11,24),new Vector2(.7f,8));var strong=a.Receiver(new Vector2(11,28));strong.gate=returnGate;strong.minimumStrength=2;strong.hold=12;
            a.Bell(new Vector2(13,31.7f),new[]{new Vector2(13,31.7f),new Vector2(10,32),new Vector2(10,28),new Vector2(11,28)},strong);
            a.Ledge(6,27,8);a.Ledge(5,20,6);a.Ledge(7,13,5);a.Ledge(5,6,6);
            var secretPlate=b.Plate(new Vector2(18,8.15f));var secretDoor=b.Door(new Vector2(16,17),new Vector2(.5f,4),secretPlate);secretDoor.holdSeconds=9;
            b.Solid("Echo-catching cabinet",new Vector2(16.7f,9),new Vector2(1,2));a.Ledge(16,15,7);a.Mercy(14,16.3f);a.Health(26,9.2f);
            b.session.Turned+=()=>{dual.latched=true;upper.speed=3.4f;};
            b.Tip(new Vector2(7,2),"E rings a bell. Follow the traveling light along its rope. Two brakes must hear together to release the lift.");a.Cure(HostKind.Echo,3,1);
        }
        void Laundry(AtlasBuilder a)
        {
            a.Begin(new Rect(-7,-12,120,44),new Vector2(2,1));var b=a.b;a.Floor(-5,13);a.Floor(33,40,1);a.Floor(58,66);a.Floor(83,106,2);a.Exit(2,1.1f);
            var rail1=a.Rail(new Vector2(7,12),new Vector2(53,12),1.3f);var rail2=a.Rail(new Vector2(53,12),new Vector2(102,14),.7f);var side=a.Rail(new Vector2(43,12),new Vector2(43,23));
            a.Source(HostKind.Marionette,8).rail=rail1;a.Source(HostKind.Marionette,96,3).rail=rail2;
            b.Solid("Heavy hanging sheet",new Vector2(24,9.5f),new Vector2(2,8));b.Solid("Second hanging sheet",new Vector2(49,8),new Vector2(2,8));b.Solid("Upper laundry shelf",new Vector2(70,6.5f),new Vector2(8,1));
            a.Ledge(43,19,7);a.Mercy(43,20.2f);a.Ledge(96,6,13);a.Key(89,7.3f);a.Nail(100,6.45f);
            var fan=b.Trigger("Laundry crosswind",new Vector2(30,3),new Vector2(7,8),new Color(.65f,.79f,.88f,.08f)).AddComponent<AirJet>();fan.always=true;fan.force=new Vector2(12,0);
            var low=b.Slider(new Vector2(37,2),new Vector2(37,7),new Vector2(5,.5f),1.4f);var high=b.Slider(new Vector2(60,7),new Vector2(60,2),new Vector2(5,.5f),1.4f);
            var weights=b.root.gameObject.AddComponent<CounterweightPair>();weights.left=low;weights.right=high;
            b.session.Turned+=()=>{rail1.loose=rail2.loose=true;rail1.sag=2.4f;rail2.sag=1.8f;fan.force=-fan.force;};
            b.Spikes(23,-8,18);b.Spikes(50,-8,15);b.Spikes(75,-8,14);a.Health(37,2.2f);
            b.Tip(new Vector2(8,2),"You steer the ceiling hook, not the Host. Up/down changes thread length. U at a rail junction changes the hooked rail.");a.Cure(HostKind.Marionette,4,1);
        }
        void Skins(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-10,118,39),new Vector2(2,1));var b=a.b;a.Floor(-5,26);a.Floor(26,46,2);a.Floor(62,105,2);a.Exit(2,1.1f);a.Source(HostKind.Molt,7);
            var plate=b.Plate(new Vector2(13,.14f),.7f);var throat=b.Door(new Vector2(25,3.5f),new Vector2(.6f,7),plate);throat.holdSeconds=.5f;
            b.Solid("Borrowed skin low ceiling",new Vector2(34,3.85f),new Vector2(12,1));a.Ledge(23,1.6f,5);
            var bridge=a.Hinge(new Vector2(46,2),16,82);var seam=b.Switch(new Vector2(42,3),"PEEL SEAM");seam.Changed+=v=>{bridge.targetAngle=v?0:82;bridge.folding=true;};
            var rail=a.Rail(new Vector2(42,13),new Vector2(78,13));a.Source(HostKind.Marionette,43,3,true).rail=rail;a.Source(HostKind.Echo,68,3,true);
            var hide=b.Solid("Returning room-skin",new Vector2(79,4),new Vector2(1,4),new Color(.67f,.35f,.4f),Layers.Moving).AddComponent<SkinReturn>();hide.home=new Vector2(64,4);
            a.Ledge(84,4,5);a.Ledge(91,6,6);a.Ledge(100,8,9);a.Key(93,7.5f);a.Nail(103,8.4f);
            a.Ledge(30,5,4);a.Ledge(34,7,4);a.Ledge(39,7,5);b.Solid("Fine decorative seam roof",new Vector2(38,8.25f),new Vector2(10,.75f));a.Mercy(41,7.65f);
            b.session.Turned+=()=>{hide.crawling=true;rail.loose=true;};a.Health(67,3.2f);a.Cure(HostKind.None,4,1,true);
            b.Tip(new Vector2(9,2),"Shed a real skin on the scale. The smaller body can move through the wardrobe's throat; the body you leave behind still matters.");
        }
    }
}