using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        FinalCircuit FinalSignal(AtlasBuilder a,AtlasBoss boss,string id,Vector2 position,Func<bool> condition,Action completed,int act=2,float hold=.2f)
        {
            var g=new GameObject(id);g.transform.SetParent(a.b.root);g.transform.position=position;
            var c=g.AddComponent<FinalCircuit>();c.boss=boss;c.act=act;c.circuitId=id;c.Evaluate=condition;c.Completed=completed;c.commitment=hold;return c;
        }
        void HostFinal(AtlasBuilder a)
        {
            a.Begin(new Rect(-10,-23,338,64),new Vector2(3,1));var b=a.b;
            a.Floor(-6,67);a.Floor(67,81,-6);a.Floor(79,303);a.Floor(305,320,-17);
            var boss=Director(a,new Vector2(311,22),"The Host of Hosts",new Color(.61f,.24f,.44f));boss.attackInterval=6.2f;boss.arenaCenter=new Vector2(0,7);
            var embargo=boss.gameObject.AddComponent<HostEmbargo>();var attacks=boss.gameObject.AddComponent<StolenAttack>();attacks.embargo=embargo;attacks.boss=boss;
            var choice=boss.gameObject.AddComponent<FinalChoiceLedger>();choice.boss=boss;choice.embargo=embargo;a.host.Acquired+=choice.Observe;
            var heart=b.Solid("The exposed source of the first corruption",new Vector2(311,22),new Vector2(4,4),new Color(.83f,.2f,.43f),Layers.Prop);
            heart.GetComponent<SpriteRenderer>().sprite=PrimitiveArt.Sprite(PrimitiveArt.Icon.Eye);
            var heartBody=heart.AddComponent<Rigidbody2D>();heartBody.bodyType=RigidbodyType2D.Kinematic;heartBody.mass=12;heartBody.freezeRotation=true;heartBody.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var anchor=boss.gameObject.AddComponent<FinalHeartAnchor>();anchor.heart=heartBody;
            b.Solid("Left side of heart shaft",new Vector2(307,-1),new Vector2(1,32));b.Solid("Right side of heart shaft",new Vector2(316,-1),new Vector2(1,32));
            // Three physical routes answer the same obstruction with time, conserved
            // mass or projection. Choice is observed from contact, not a menu grant.
            a.Source(HostKind.Echo,8);a.Source(HostKind.Wax,8,5);a.Source(HostKind.Parallax,8,9);
            a.Ledge(5,2,4);a.Ledge(8,4,5);a.Ledge(5,6,4);a.Ledge(8,8,5);
            a.Ledge(21,4,24);a.Ledge(31,8,8);
            var echoLeft=b.Plate(new Vector2(12,.14f),.7f);var echoRight=b.Plate(new Vector2(23,.14f),.7f);
            var echoGate=b.Door(new Vector2(29,2),new Vector2(.7f,4),echoLeft,echoRight);echoGate.latched=true;echoGate.holdSeconds=.1f;
            var mass=b.Plate(new Vector2(21,4.14f),.1f);var waxGate=b.Door(new Vector2(27,6),new Vector2(.7f,4));
            FinalSignal(a,boss,"First act conserved balance",new Vector2(21,5),()=>mass.Mass>.65f&&mass.Mass<.86f,()=>waxGate.SetOpen(true),0,.35f);
            a.Depth(new Vector2(20,7.7f),new Vector2(32,.5f),0).gameObject.AddComponent<OneWaySurface>();a.Projection(new Vector2(18,9),new Vector2(30,6));
            b.Solid("The unprojected upper absence",new Vector2(20,11),new Vector2(18,1));
            a.Cure(HostKind.None,44,1,true);a.Health(40,1.3f);
            b.Tip(new Vector2(4,2),"Three tenants; three routes. Choose one. Whatever gets you through is what it will take from you.");
            b.Tip(new Vector2(21,5.7f),"A wax partition changes the living weight on the scale. The plug still has mass wherever you leave it.");
            b.Tip(new Vector2(8,10),"The far bridge exists only in projection. Align before stepping into it.");
            var second=new List<GameObject>();var gullet=a.Source(HostKind.Gullet,60);second.Add(gullet.gameObject);
            var root=a.Source(HostKind.Root,63,3);second.Add(root.gameObject);a.Ledge(63,2,4);
            var tile=a.Chunk(new Vector2(68,2),new Vector2(2,4));tile.name="Second act removable support";
            b.Solid("Sealed digestion vault",new Vector2(69.5f,5.3f),new Vector2(15,1));
            a.Ledge(73,-4,4);a.Socket(new Vector2(77,-4),new Vector2(2,4));a.SoilPath(new Vector2(65,1),new Vector2(65,-4),new Vector2(80,-4),new Vector2(80,1));
            a.Cure(HostKind.None,104,1,true);a.Health(98,1.3f);
            b.Tip(new Vector2(61,2),"It owns the old answer. Move the wall into the pit, or thread a root around the same structure.");
            foreach(var o in second)o.SetActive(false);
            // Alternative circuits are one interconnected encounter, never five
            // mandatory matching locks. Their cables all hold the same physical heart.
            var before=new HashSet<Transform>();foreach(Transform t in b.root)before.Add(t);
            FinalMagnetShadow(a,boss,anchor,110);FinalEchoInk(a,boss,anchor,148);FinalWaxGullet(a,boss,anchor,186);FinalStitchCoffin(a,boss,anchor,224);FinalMirrorParallax(a,boss,anchor,262);
            var choices=new List<GameObject>();foreach(Transform t in b.root)if(!before.Contains(t))choices.Add(t.gameObject);
            foreach(var o in choices)o.SetActive(false);
            boss.Configure(boss.title,p=>p==0?choice.Chosen!=HostKind.None&&a.Player.Body.position.x>36:p==1?a.Player.Body.position.x>87:anchor.Released&&heartBody.position.y< -10,p=>{
                if(p==0)boss.objective="Choose a tenant: delayed footsteps, conserved living wax, or the far bridge.";
                else if(p==1){choice.Steal(a.host);foreach(var o in second)o.SetActive(true);boss.objective="It stole "+HostController.Display(choice.Chosen)+". Traverse the vault with a different physical rule.";}
                else {foreach(var o in choices)o.SetActive(true);boss.objective="One heart, five possible cuts. Combine two systems to sever a load-bearing cable.";}
            });
            // No phase transition moves the Host, restores health or grants a tenant.
        }
        void Cable(AtlasBuilder a,FinalHeartAnchor heart,Vector2 at,string label)
        {heart.cables.Add(PrimitiveArt.Line(label,a.b.root,at,new Vector2(311,22),.07f,new Color(.59f,.23f,.38f,.45f),-4));}
        void FinalMagnetShadow(AtlasBuilder a,AtlasBoss boss,FinalHeartAnchor heart,float x)
        {
            var b=a.b;a.Ledge(x+2,1,3);a.Ledge(x+14,2,28);a.Source(HostKind.Lodestone,x+4,3);a.Source(HostKind.Shadow,x+7,3,true);
            var screen=a.Metal(new Vector2(x+10,8),new Vector2(12,.65f),2,false,-1);screen.name="Final suspended iron screen";screen.strength=100;
            var rb=screen.GetComponent<Rigidbody2D>();rb.gravityScale=0;rb.constraints=RigidbodyConstraints2D.FreezePositionY|RigidbodyConstraints2D.FreezeRotation;rb.linearDamping=1.2f;
            b.Solid("Opaque heart partition",new Vector2(x+18.5f,6),new Vector2(1,8));
            var sg=new GameObject("Final noon witness");sg.transform.SetParent(b.root);sg.transform.position=new Vector2(x+15,18);var sun=sg.AddComponent<ShadowSun>();sun.directional=true;sun.noon=true;sun.reach=16;sun.renderFilled=true;
            var dg=new GameObject("Final screen shadow domain");dg.transform.SetParent(b.root);var domain=dg.AddComponent<ShadowDomain>();domain.area=new Rect(x,1.8f,31,18);domain.onlySun=sun;
            var hand=b.Trigger("Final shadow tendon",new Vector2(x+19.5f,2.35f),Vector2.one*.6f,new Color(.78f,.65f,.38f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();hand.requiredSun=sun;hand.requiredCaster=screen.GetComponent<Collider2D>();
            FinalSignal(a,boss,"magnet-shadow",hand.transform.position,()=>hand.active&&Mathf.Abs(rb.position.x-(x+10))>2,()=>heart.Release("magnet-shadow"));
            Cable(a,heart,hand.transform.position,"The shadow tendon");b.Tip(new Vector2(x+5,4),"Move real iron to manufacture its shadow. The other side of the wall sees only the cast silhouette.");
        }
        void FinalEchoInk(AtlasBuilder a,AtlasBoss boss,FinalHeartAnchor heart,float x)
        {
            var b=a.b;a.Ledge(x+2,2,4);a.Ledge(x+6,4,10);a.Source(HostKind.Echo,x+4,5);a.Source(HostKind.Ink,x+7,5,true);
            a.Ledge(x+18,7.6f,23).AddComponent<OneWaySurface>();var l=b.Plate(new Vector2(x+14,7.74f),.7f);var r=b.Plate(new Vector2(x+24,7.74f),.7f);
            l.GetComponent<BoxCollider2D>().size=new Vector2(2.2f,.32f);r.GetComponent<BoxCollider2D>().size=new Vector2(2.2f,.32f);
            FinalSignal(a,boss,"echo-ink",new Vector2(x+19,8),()=>l.Pressed&&r.Pressed,()=>heart.Release("echo-ink"),2,.15f);
            Cable(a,heart,new Vector2(x+19,8),"The second-footstep tendon");b.Tip(new Vector2(x+6,6),"Ink writes a route for the body that arrives later. Neither a lone footprint nor an unwritten jump reaches both scales.");
        }
        void FinalWaxGullet(AtlasBuilder a,AtlasBoss boss,FinalHeartAnchor heart,float x)
        {
            var b=a.b;a.Ledge(x+1,1,3);a.Ledge(x+2.5f,2,2);a.Ledge(x+7.5f,2,2);
            var tile=a.Chunk(new Vector2(x+5,1.5f),new Vector2(3,1));tile.name="Final reusable structural morsel";a.Source(HostKind.Gullet,x+4.5f,3);a.Source(HostKind.Wax,x+10,1,true);
            var tray=b.Solid("Conserved body cargo lift",new Vector2(x+16,.3f),new Vector2(4,.4f),new Color(.73f,.58f,.4f),Layers.Moving);tray.AddComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;
            var lift=tray.AddComponent<FinalCargoHoist>();lift.lower=new Vector2(x+16,.3f);lift.upper=new Vector2(x+16,8.3f);a.Socket(new Vector2(x+16,1),new Vector2(3,1));
            var counter=b.Solid("Cargo scale counterweight",new Vector2(x+20,12),Vector2.one,new Color(.58f,.44f,.31f),Layers.Moving);lift.counterweight=counter.AddComponent<Rigidbody2D>();lift.counterweight.bodyType=RigidbodyType2D.Kinematic;
            a.Ledge(x+23,11.5f,10).AddComponent<OneWaySurface>();var scale=b.Plate(new Vector2(x+24,11.64f),.2f);
            FinalSignal(a,boss,"wax-gullet",scale.transform.position,()=>lift.Delivered&&scale.Pressed,()=>heart.Release("wax-gullet"));Cable(a,heart,scale.transform.position,"The conserved hunger tendon");
            b.Tip(new Vector2(x+11,2),"The tray needs a real floor and a balanced living load. Leave excess wax beside it, not on it.");
        }
        void FinalStitchCoffin(AtlasBuilder a,AtlasBoss boss,FinalHeartAnchor heart,float x)
        {
            var b=a.b;a.Ledge(x+1,1,3);a.Ledge(x+14,2,28);a.Source(HostKind.Stitch,x+4,3);a.Source(HostKind.Coffin,x+7,3,true);
            var hinge=a.Hinge(new Vector2(x+4,4),16,30,"heart-brace");hinge.name="Final stitched load";a.Seam(new Vector2(x+20,.8f),"heart-brace");
            FinalSignal(a,boss,"stitch-coffin",new Vector2(x+13,3),()=>hinge.HeldLoad&&hinge.BracedSeconds>.8f,()=>heart.Release("stitch-coffin"));Cable(a,heart,new Vector2(x+13,5),"The bearing tendon");
            b.Tip(new Vector2(x+6,4),"Fold the beam toward its lower seam. A horizontal body can bear the load; an upright body cannot hold the same sweep.");
        }
        void FinalMirrorParallax(AtlasBuilder a,AtlasBoss boss,FinalHeartAnchor heart,float x)
        {
            var b=a.b;a.Ledge(x+1,1,3);a.Ledge(x+5,2,10);a.Ledge(x+20,2,9);
            var mirror=a.Source(HostKind.Mirror,x+2,3);mirror.explicitAxis=true;mirror.mirrorAxis=x+11;a.Source(HostKind.Parallax,x+5,3,true);
            var left=a.Depth(new Vector2(x+6,3.4f),new Vector2(10,.5f),0);left.gameObject.AddComponent<OneWaySurface>();var right=a.Depth(new Vector2(x+20,3.4f),new Vector2(10,.5f),2);right.gameObject.AddComponent<OneWaySurface>();
            a.Projection(new Vector2(x+8,5),new Vector2(20,10));var stamp=b.Trigger("The reflected near-plane stamp",new Vector2(x+19,6),new Vector2(12,9),Color.clear).AddComponent<ReplicaDepthStamp>();stamp.plane=2;
            b.Solid("Reflection left caliper",new Vector2(x+17,6),new Vector2(1,4.5f));b.Solid("Reflection right caliper",new Vector2(x+22,6),new Vector2(1,4.5f));
            var l=b.Plate(new Vector2(x+6,3.70f),.2f);var r=b.Plate(new Vector2(x+21,3.89f),.5f);l.GetComponent<BoxCollider2D>().size=new Vector2(1.8f,.32f);
            FinalSignal(a,boss,"mirror-parallax",new Vector2(x+14,6),()=>l.Pressed&&r.Pressed&&stamp.Stamped&&stamp.Stamped.Grounded&&stamp.Stamped.GroundCollider&&stamp.Stamped.GroundCollider.gameObject.layer==19&&a.Player.Grounded&&a.Player.GroundCollider&&a.Player.GroundCollider.gameObject.layer==17,()=>heart.Release("mirror-parallax"));
            Cable(a,heart,new Vector2(x+14,6),"The misregistered twin tendon");b.Tip(new Vector2(x+5,4),"Your reflected body's near plane is not yours. Let the calipers stop one body while the other corrects its register.");
        }
    }
}
