using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Rain(AtlasBuilder a)
        {
            a.Begin(new Rect(-10,-12,88,65),new Vector2(2,1));var b=a.b;a.Floor(-6,12);a.Floor(12,70,-7);a.Exit(2,1.1f);a.Source(HostKind.Censer,7);
            GameObject Sill(float x,float y,float width=4){var g=a.Ledge(x,y,width);g.AddComponent<OneWaySurface>();return g;}
            var onward=new List<KneelingFigure>();
            for(int i=0;i<10;i++){float floor=1+i*1.5f;var f=a.Figure(15+i*5,floor+9,floor,.6f+i*.17f);f.name="Outward penitent "+i;f.showLandingTell=f.safeUpperSurface=true;f.hold=5;f.speed=8;onward.Add(f);}
            Sill(27.5f,5.1f,3);Sill(42.5f,9.6f,3);Sill(57.5f,14.1f,3);Sill(66,15,10);a.Key(64,16.2f);a.Nail(69,15.4f);
            var returns=new List<KneelingFigure>();for(int i=0;i<10;i++){float floor=16.2f+i*1.5f;var f=a.Figure(63-i*5,floor+8,floor,.35f+i*.11f);f.name="Return penitent "+i;f.showLandingTell=f.safeUpperSurface=true;f.speed=10;f.hold=4.5f;f.gameObject.SetActive(false);returns.Add(f);}
            var landings=new List<GameObject>{Sill(60.5f,16.5f,3),Sill(45.5f,21,3),Sill(30.5f,25.5f,3),Sill(14,30.2f,7)};
            for(int i=0;i<9;i++)landings.Add(Sill(9-i%2*5,27.5f-i*3,6));foreach(var g in landings)g.SetActive(false);
            // A side penitent carries its Mercy. Standing on the left changes its
            // actual kneeling slope; the reward moves with that body, not a switch flag.
            Sill(38,11,4);Sill(34,12.8f,4);Sill(29,14.6f,5);
            var witness=a.Figure(24,24,16.1f,1.7f);witness.name="Penitent bearing the Mercy";witness.respondsToWitness=witness.showLandingTell=witness.safeUpperSurface=true;witness.hold=7;witness.speed=8;
            Sill(20,16.7f,4);var mercy=a.Mercy(24,20);mercy.transform.SetParent(witness.transform,true);mercy.transform.localPosition=new Vector2(.7f,3.8f);
            b.session.Turned+=()=>{foreach(var f in onward){f.delay=.35f;f.hold=3.5f;}foreach(var f in returns)f.gameObject.SetActive(true);foreach(var g in landings)g.SetActive(true);};
            a.Health(42.5f,10.8f);b.Enemy(new Vector2(37,-5.8f),true);a.Cure(HostKind.Censer,3,1);
            b.Tip(new Vector2(8,2),"Be still. Incense slows the falling congregation, but not you. Read each landing tell; the kneeling back is only temporary.");
            b.Tip(new Vector2(28,16),"The witness looks toward the body beneath it. Approach from its left to make its back lean toward the carried Mercy.");
            b.Tip(new Vector2(66,16),"The vault opens after the Nail. Your return goes UP through the denser rain, then down the old entrance shaft.");
        }
        void Seam(AtlasBuilder a)
        {
            a.Begin(new Rect(-9,-13,119,52),new Vector2(2,1));var b=a.b;a.Floor(-6,18);a.Floor(18,103,-5);a.Exit(2,1.1f);a.Source(HostKind.Stitch,8);
            var panels=new List<FoldPanel>();
            // The low scaffold lets both edges be inspected first. Pulling the thread changes
            // the actual route above it, rather than teleporting through a sewn 'door'.
            var p1=a.Hinge(new Vector2(20,1),10,-25);panels.Add(p1);a.Seam(new Vector2(28.7f,6));a.Steps(17,-3,5,2.5f,1.9f,3.5f);
            var p2=a.Hinge(new Vector2(40,7),12,-35);panels.Add(p2);a.Seam(new Vector2(50.4f,13));a.Steps(35,-3,8,2.1f,2,3.4f);
            a.Source(HostKind.Censer,56,11,true);a.Ledge(57,11,8);
            var p3=a.Hinge(new Vector2(64,11),13,-20);panels.Add(p3);a.Seam(new Vector2(75.3f,17.5f));a.Ledge(72,13.2f,4);a.Ledge(77,15.4f,4);
            a.Ledge(86,18,6);a.Ledge(95,20,10);a.Key(91,21.3f);a.Nail(99,20.4f);
            var moving=b.Slider(new Vector2(60,7),new Vector2(70,7),new Vector2(5,.5f),2.2f);a.Figure(80,31,14,2.2f);
            var hidden=a.Hinge(new Vector2(52,18),8,95,"tower");a.Seam(new Vector2(59,21.9f),"tower");a.Ledge(55,17,4);a.Ledge(62,22,4);a.Mercy(63,23.2f);
            b.session.Turned+=()=>{moving.origin+=Vector2.right*7;moving.end+=Vector2.left*7;foreach(var panel in panels){panel.speed=22;panel.targetAngle=-15;panel.folding=true;}hidden.targetAngle=20;hidden.folding=true;};
            a.Health(56,12);b.Tip(new Vector2(8,2),"U catches a seam. Visit its partner, select again, then tug. The supporting wall rotates through physical space; do not stand in the crease.");a.Cure(HostKind.None,4,1,true);
        }
        ProcessionCarrier Carrier(AtlasBuilder a,Vector2 from,Vector2 to,PulseReceiver bell,float speed=2)
        {
            var g=a.b.Solid("Pallbearer's lid",from,new Vector2(4,.65f),new Color(.56f,.45f,.45f),Layers.Moving);var rb=g.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;
            var c=g.AddComponent<ProcessionCarrier>();c.a=from;c.b=to;c.bell=bell;c.speed=speed;return c;
        }
        void Procession(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,122,43),new Vector2(2,1));var b=a.b;a.Floor(-5,19);a.Floor(19,100,-4);a.Floor(100,112,2);a.Exit(2,1.1f);a.Source(HostKind.Coffin,8);
            var receiver=a.Receiver(new Vector2(18,7));a.Bell(new Vector2(14,1),new[]{new Vector2(14,1),new Vector2(14,7),new Vector2(18,7)},receiver);
            var carriers=new[]{Carrier(a,new Vector2(23,.4f),new Vector2(34,2.4f),receiver,2),Carrier(a,new Vector2(41,2),new Vector2(49,6),receiver,1.8f),Carrier(a,new Vector2(56,6),new Vector2(67,6),receiver,2.2f),Carrier(a,new Vector2(75,6),new Vector2(88,9),receiver,1.7f)};
            a.Ledge(35,2.8f,5);a.Ledge(51,6.3f,5);a.Ledge(70,6.3f,5);a.Ledge(92,9.2f,8);a.Ledge(104,8.7f,10);a.Key(103,9.65f);a.Nail(108,9.1f);
            var braceGate=b.Door(new Vector2(72,8.4f),new Vector2(.8f,4.2f));var brace=b.Trigger("Carrier pressure point",new Vector2(66,6.9f),new Vector2(4,1.5f),new Color(.74f,.59f,.5f,.2f)).AddComponent<BraceReceiver>();brace.gate=braceGate;brace.holdRequired=.65f;
            a.Source(HostKind.Stitch,52,7.3f,true);var fold=a.Hinge(new Vector2(71,3),10,-5);a.Seam(new Vector2(79,9));
            a.Ledge(37,1.2f,9);b.Solid("Horizontal coffin grille",new Vector2(38,3),new Vector2(9,1.2f));a.Mercy(41,1.9f);
            b.session.Turned+=()=>{foreach(var c in carriers)c.deaf=true;receiver.minimumStrength=99;braceGate.SetOpen(true);};a.Health(94,10.4f);
            b.Tip(new Vector2(8,2),"The coffin turns around its leading corner. No jump. Lie horizontally at the carrier's pressure point to brace it, or use the needle's folded route.");a.Cure(HostKind.None,4,1,true);
        }
        void Cathedral(AtlasBuilder a)
        {
            a.Begin(new Rect(-9,-235,125,280),new Vector2(2,1));var b=a.b;
            // Camera initially frames the structure, not the entire altitude budget.
            b.session.Camera.bounds=new Rect(-9,-9,125,65);
            a.Floor(-6,106);var exit=a.Exit(2,1.1f);a.Source(HostKind.Censer,8);a.Source(HostKind.Stitch,20,1,true);
            a.Steps(14,2,7,5,2,4);a.Ledge(48,14,7);a.Figure(54,32,15,2);
            var hinge=a.Hinge(new Vector2(51,15),12,-30);a.Seam(new Vector2(61,21.5f));a.Ledge(61,19,4);a.Ledge(67,21,5);
            a.Source(HostKind.Coffin,68,22,true);a.Ledge(73,22,5);var platform=b.Slider(new Vector2(78,22),new Vector2(90,25),new Vector2(6,.5f),1.8f);a.Ledge(98,25,13);a.Key(95,26.3f);a.Nail(103,25.4f);
            var upperReturn=new List<GameObject>();for(int i=0;i<11;i++){var g=a.Ledge(3+i*8.5f,24,7);g.SetActive(false);upperReturn.Add(g);}
            var chapel=b.Slider(new Vector2(7,28),new Vector2(23,31),new Vector2(6,.5f),1.5f);chapel.paused=true;a.Mercy(18,32.2f);a.Health(46,15.3f);
            var obj=new GameObject("Cathedral descent clock");obj.transform.SetParent(b.root);var descent=obj.AddComponent<DescentController>();descent.altitude=210;descent.speed=1.6f;descent.exit=exit.transform;descent.Capture(b);
            b.session.Turned+=()=>{foreach(var g in upperReturn)g.SetActive(true);descent.falling=true;chapel.paused=false;hinge.targetAngle=0;hinge.folding=true;};
            b.Tip(new Vector2(9,2),"Pulling the high Nail cuts the cathedral loose. The return arch rises through the falling building. The altitude gauge is your deadline.");a.Cure(HostKind.None,4,25,true);
        }
    }
}