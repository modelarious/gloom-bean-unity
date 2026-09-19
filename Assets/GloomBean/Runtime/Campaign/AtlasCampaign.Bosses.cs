using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Boss(AtlasBuilder a,int world)
        {
            switch(world){case 1:Usher(a);break;case 2:Judge(a);break;case 3:Surveyor(a);break;case 4:Everyone(a);break;case 5:Hosts(a);break;default:throw new System.ArgumentOutOfRangeException(nameof(world));}
        }
        AtlasBoss Director(AtlasBuilder a,Vector2 p,string title,Color color)
        {
            var go=new GameObject(title+" encounter");go.transform.SetParent(a.b.root);var boss=go.AddComponent<AtlasBoss>();boss.title=title;boss.arenaCenter=new Vector2(24,7);
            var art=PrimitiveArt.Shape(title,a.b.root,p,new Vector2(6,9),color,PrimitiveArt.Icon.Arch,-1);PrimitiveArt.Shape("Witnessing eye",art.transform,p+Vector2.up*2,new Vector2(2,2.5f),new Color(.97f,.86f,.58f),PrimitiveArt.Icon.Eye,0);boss.body=art.transform;return boss;
        }
        void Usher(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-8,64,39),new Vector2(3,1));var b=a.b;a.Floor(-5,48);var boss=Director(a,new Vector2(31,6),"The Kindly Usher",new Color(.57f,.28f,.44f));
            var echo=a.Source(HostKind.Echo,7);var stringSource=a.Source(HostKind.Marionette,7,1);stringSource.enabledSource=false;stringSource.gameObject.SetActive(false);var molt=a.Source(HostKind.Molt,7);molt.enabledSource=false;molt.gameObject.SetActive(false);
            var left=b.Plate(new Vector2(12,.14f));var right=b.Plate(new Vector2(23,.14f));var front=b.Door(new Vector2(29,3),new Vector2(1,6));
            stringSource.rail=a.Rail(new Vector2(5,15),new Vector2(40,15));var target=b.Solid("The Usher's chandelier catch",new Vector2(28.5f,11.8f),new Vector2(2.2f,3),new Color(.73f,.42f,.5f));
            var chandelier=b.Solid("A weight, not a damage button",new Vector2(24,10),new Vector2(2,1),new Color(.79f,.64f,.35f),Layers.Prop);var rb=chandelier.AddComponent<Rigidbody2D>();rb.mass=2;rb.gravityScale=2;var joint=chandelier.AddComponent<DistanceJoint2D>();joint.autoConfigureConnectedAnchor=false;joint.connectedAnchor=new Vector2(24,16);joint.autoConfigureDistance=false;joint.distance=6;var impact=chandelier.AddComponent<ChandelierImpact>();impact.receiver=target.GetComponent<Collider2D>();
            var shellPlate=b.Plate(new Vector2(19,.14f),.6f);var skinGate=b.Door(new Vector2(35,1.6f),new Vector2(.5f,3.2f),shellPlate);b.Solid("Thin backstage crawl",new Vector2(39,1.8f),new Vector2(7,2));
            a.Ledge(14,2,4);a.Ledge(18,4,4);a.Health(4,1.2f);a.Health(45,1.2f);float together=0;
            boss.Configure(boss.title,p=>{if(p==0){together=left.Pressed&&right.Pressed?together+Time.deltaTime:0;return together>.65f;}if(p==1)return impact.struck;return a.Player.Body.position.x>40&&a.Player.Height<.9f&&shellPlate.Pressed;},p=>{
                if(p==0)boss.objective="Hold both pew scales with two bodies.";
                if(p==1){echo.gameObject.SetActive(false);a.host.Cure(HostKind.None,true);stringSource.gameObject.SetActive(true);stringSource.enabledSource=true;front.SetOpen(true);boss.objective="Swing the chandelier's physical weight into the Usher's high catch.";}
                if(p==2){a.host.Cure(HostKind.None,true);stringSource.gameObject.SetActive(false);molt.gameObject.SetActive(true);molt.enabledSource=true;boss.objective="Leave a skin on the backstage scale; take the smaller body under the curtain.";}
            });
            b.Tip(new Vector2(5,2),"The Usher never takes ordinary tackle damage. Each act changes what its own furniture demands.");
        }
        void Judge(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-12,67,40),new Vector2(3,1));var b=a.b;a.Floor(-5,18);a.Floor(34,50);var boss=Director(a,new Vector2(27,7),"The Orchard Judge",new Color(.54f,.52f,.27f));
            a.Source(HostKind.Wax,7);var scale=b.Plate(new Vector2(13,.14f),.1f);var body=b.Solid("The Judge's true weight",new Vector2(26,2.1f),new Vector2(3,3),new Color(.62f,.61f,.34f),Layers.Prop);var weight=body.AddComponent<Rigidbody2D>();weight.bodyType=RigidbodyType2D.Kinematic;weight.mass=12;
            var floor=a.Chunk(new Vector2(26,.1f),new Vector2(8,1));var gullet=a.Source(HostKind.Gullet,10);gullet.gameObject.SetActive(false);
            a.Ledge(18,1.5f,5);a.Ledge(33,1.5f,5);var root=a.Source(HostKind.Root,23);root.gameObject.SetActive(false);var rootFloor=a.Floor(18,50);rootFloor.SetActive(false);
            var wall=b.Solid("Back of the hanging scale",new Vector2(32,3.8f),new Vector2(5,7.6f));wall.SetActive(false);a.SoilPath(new Vector2(24,1),new Vector2(24,-4),new Vector2(37,-4),new Vector2(37,1));
            a.Health(4,1.2f);a.Health(45,1.2f);float balanced=0;
            boss.Configure(boss.title,p=>{if(p==0){balanced=scale.Mass>.65f&&scale.Mass<.86f?balanced+Time.deltaTime:0;return balanced>1;}if(p==1)return weight.position.y< -2;return a.Player.Body.position.x>38;},p=>{
                if(p==0)boss.objective="Balance a living body between 0.65 and 0.86 mass. Leave excess wax elsewhere.";
                if(p==1){a.host.Cure(HostKind.None,true);gullet.gameObject.SetActive(true);weight.bodyType=RigidbodyType2D.Dynamic;weight.gravityScale=2;boss.objective="Remove the support beneath the Judge. The floor is actual edible terrain.";}
                if(p==2){a.host.Cure(HostKind.None,true);gullet.gameObject.SetActive(false);rootFloor.SetActive(true);wall.SetActive(true);root.gameObject.SetActive(true);a.Player.Revive(new Vector2(22,1));boss.objective="Grow around the scale's sealed back; surface behind its support.";}
            });
        }
        void Surveyor(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-9,74,42),new Vector2(3,1));var b=a.b;a.Floor(-5,57);var boss=Director(a,new Vector2(47,8),"The Surveyor",new Color(.31f,.43f,.62f));
            var mirror=a.Source(HostKind.Mirror,7);mirror.explicitAxis=true;mirror.mirrorAxis=19;var p1=b.Plate(new Vector2(12,.14f));var p2=b.Plate(new Vector2(26,.14f));b.Solid("Off-register drafting cabinet",new Vector2(30,1),new Vector2(1.5f,2));
            var inner=a.Source(HostKind.InsideOut,7);inner.gameObject.SetActive(false);var terrain=b.Solid("Solid attack outline",new Vector2(20,3),new Vector2(12,6));terrain.SetActive(false);var innerFloor=b.Solid("Interior of the attack line",new Vector2(20,-.2f),new Vector2(26,.4f),new Color(.62f,.81f,.83f),Layers.Interior);innerFloor.SetActive(false);
            var depth=a.Source(HostKind.Parallax,31);depth.gameObject.SetActive(false);a.Projection(new Vector2(33,2),new Vector2(9,6));var caliper=b.Solid("Caliper upper jaw",new Vector2(39,2),new Vector2(10,2));a.Ledge(39,0,10); // 1 m gap admits the far silhouette, not the full body.
            a.Ledge(51,2,5);a.Ledge(56,4,4);a.Health(4,1.2f);
            boss.Configure(boss.title,p=>p==0?p1.Pressed&&p2.Pressed:p==1?a.Player.Body.position.x>25&&a.Player.Body.position.x<29:a.Player.Body.position.x>47&&a.Player.Height<1.1f,p=>{
                if(p==0)boss.objective="Correct two independently colliding reflections against unmatched furniture.";
                if(p==1){a.host.Cure(HostKind.None,true);mirror.gameObject.SetActive(false);inner.gameObject.SetActive(true);terrain.SetActive(true);innerFloor.SetActive(true);a.Player.Revive(new Vector2(7,1));boss.objective="Walk inside the Surveyor's supposedly solid attack shape.";}
                if(p==2){terrain.SetActive(false);innerFloor.SetActive(false);a.host.Cure(HostKind.None,true);inner.gameObject.SetActive(false);depth.gameObject.SetActive(true);a.Player.Revive(new Vector2(30,1));boss.objective="Change projected scale at the registration mark; fit the calipers without changing your screen position.";}
            });
        }
        void Everyone(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-15,78,69),new Vector2(3,1));var b=a.b;a.Floor(-5,65);var boss=Director(a,new Vector2(37,25),"The Weight of Everyone",new Color(.48f,.39f,.43f));boss.combat=false;
            a.Source(HostKind.Censer,7);a.Ledge(10,1.5f,4);for(int i=0;i<5;i++){var f=a.Figure(14+i*5,32+i,2+i*2.1f,1+i*.35f);f.hold=7;}a.Ledge(37,12,8);a.Source(HostKind.Stitch,38,13,true);
            var hinge=a.Hinge(new Vector2(41,13),12,-40,"mass");a.Seam(new Vector2(49.5f,21.5f),"mass");a.Ledge(43,16,4);a.Ledge(47,18,4);a.Ledge(51,21,6);
            a.Source(HostKind.Coffin,50.2f,22,true);a.Ledge(57,21,12);var moving=b.Slider(new Vector2(55,23),new Vector2(55,21.7f),new Vector2(4,.8f),.7f);var finish=b.Door(new Vector2(59,24),new Vector2(.7f,6));var receiver=b.Trigger("Keystone bearing",new Vector2(53,22.2f),new Vector2(4,1.3f),new Color(.74f,.69f,.54f,.2f)).AddComponent<BraceReceiver>();receiver.gate=finish;receiver.holdRequired=1.8f;a.Ledge(63,21,7);
            var release=moving.gameObject.AddComponent<BracedLiftRelease>();release.receiver=receiver;release.motion=moving;release.destination=new Vector2(55,27);a.Cure(HostKind.Coffin,59.5f,22);
            float deadline=200;boss.Configure(boss.title,p=>p==0?a.Player.Feet.y>11.5f:p==1?hinge.angle>30&&a.Player.Feet.y>20:receiver.latched&&a.Player.Body.position.x>59,p=>boss.objective=p==0?"No health bar. Cross the falling congregation before the foundation gives way.":p==1?"Fold the hanging mass into a traversable incline.":"Brace the settling keystone with a horizontal lid, then reach the released arch.");
            var budget=b.root.gameObject.AddComponent<EncounterBudget>();budget.boss=boss;budget.remaining=deadline;a.Health(37,13.2f);
        }
        void Hosts(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-10,100,47),new Vector2(3,1));var b=a.b;a.Floor(-5,87);var boss=Director(a,new Vector2(49,10),"The Host of Hosts",new Color(.64f,.37f,.56f));boss.attackInterval=4.5f;
            var embargo=boss.gameObject.AddComponent<HostEmbargo>();var stolen=boss.gameObject.AddComponent<StolenAttack>();stolen.embargo=embargo;stolen.boss=boss;
            var sources=new List<HostSource>();sources.Add(a.Source(HostKind.Echo,8));sources.Add(a.Source(HostKind.Wax,8,5));sources.Add(a.Source(HostKind.Parallax,8,9));a.Ledge(8,4,7);a.Ledge(8,8,7);a.Ledge(3,2,4);a.Ledge(3,6,4);
            var p1=b.Plate(new Vector2(17,.14f));var p2=b.Plate(new Vector2(27,.14f));var weigh=b.Plate(new Vector2(39,.14f),.1f);a.Projection(new Vector2(34,2),new Vector2(9,6));b.Solid("A physical far-scale slot",new Vector2(46,2),new Vector2(8,2));
            HostKind chosen=HostKind.None;a.host.Acquired+=k=>{if(boss.phase==0)chosen=k;};
            var stage2=new List<HostSource>{a.Source(HostKind.Gullet,56,1,true),a.Source(HostKind.Censer,61,1,true),a.Source(HostKind.Lodestone,66,1,true),a.Source(HostKind.Shadow,72,1,true),a.Source(HostKind.Ink,77,1,true)};foreach(var s in stage2)s.gameObject.SetActive(false);
            var chunk=a.Chunk(new Vector2(59,2),new Vector2(3,4));var movable=a.Metal(new Vector2(68,2),new Vector2(1.4f,4),4,false,-1);Sun(a,new Vector2(62,13),23);var shadowGate=b.Door(new Vector2(82,3),new Vector2(.7f,6));var hand=b.Trigger("The other Host's shadow",new Vector2(77,.6f),Vector2.one*.7f,new Color(.4f,.3f,.56f),PrimitiveArt.Icon.Eye).AddComponent<ShadowReceiver>();hand.gate=shadowGate;
            var pairSources=new[]{a.Source(HostKind.Stitch,18,1,true),a.Source(HostKind.Coffin,23,1,true),a.Source(HostKind.Echo,30,1,true),a.Source(HostKind.Ink,34,1,true),a.Source(HostKind.Wax,40,1,true),a.Source(HostKind.Gullet,44,1,true)};foreach(var s in pairSources)s.gameObject.SetActive(false);
            var panel=a.Hinge(new Vector2(19,7),11,-15,"host");a.Seam(new Vector2(28,13.3f),"host");a.Steps(14,2,3,2,2,3);a.Ledge(32,14,7);
            var brace=b.Trigger("Final load",new Vector2(32,14.7f),new Vector2(4,1.4f),Color.clear).AddComponent<BraceReceiver>();brace.holdRequired=1;
            a.Ledge(72,9,7);a.Health(4,1.2f);a.Health(84,1.2f);float dwell=0;Vector2 metalOrigin=movable.transform.position;
            boss.Configure(boss.title,p=>{
                bool two=p1.Pressed&&p2.Pressed;bool light=weigh.Mass>.6f&&weigh.Mass<.85f;bool far=a.Player.Body.position.x>50&&a.Player.Height<1.1f;
                if(p==0){dwell=chosen!=HostKind.None&&(two||light||far)?dwell+Time.deltaTime:0;return dwell>.8f;}
                if(p==1)return !chunk.gameObject.activeSelf||hand.active||Vector2.Distance(movable.transform.position,metalOrigin)>4;
                return panel.angle>20&&brace.latched || hand.active&&Vector2.Distance(movable.transform.position,metalOrigin)>3 || two&&a.Player.Feet.y>7 || light&&!chunk.gameObject.activeSelf;
            },p=>{
                if(p==0)boss.objective="Choose a tenant, then solve the scales or the physical caliper route.";
                if(p==1){embargo.Steal(a.host,chosen);foreach(var s in sources)s.enabledSource=s.kind!=chosen;foreach(var s in stage2)s.gameObject.SetActive(true);a.Player.Revive(new Vector2(54,1));boss.objective="Your chosen tenant is now its weapon. Relocate terrain, rearrange the iron screen, or reach the shadow latch.";}
                if(p==2){foreach(var s in pairSources){s.gameObject.SetActive(true);s.enabledSource=s.kind!=chosen;}a.Player.Revive(new Vector2(14,1));boss.objective="Use two systems together: fold and brace, iron and shadow, echo and ink, or wax and displaced terrain.";}
            });
        }
    }
}