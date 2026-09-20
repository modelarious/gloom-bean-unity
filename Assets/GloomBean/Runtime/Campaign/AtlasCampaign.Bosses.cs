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
            var art=PrimitiveArt.Shape(title,a.b.root,p,new Vector2(6,9),color,PrimitiveArt.Icon.Arch,-1);PrimitiveArt.Shape("Witnessing eye",a.b.root,p+Vector2.up*2,new Vector2(2,2.5f),new Color(.97f,.86f,.58f),PrimitiveArt.Icon.Eye,0);boss.body=art.transform;return boss;
        }
        void Usher(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-8,64,39),new Vector2(3,1));var b=a.b;a.Floor(-5,48);var boss=Director(a,new Vector2(31,6),"The Kindly Usher",new Color(.57f,.28f,.44f));
            var echo=a.Source(HostKind.Echo,7);var stringSource=a.Source(HostKind.Marionette,7,1);stringSource.enabledSource=false;stringSource.gameObject.SetActive(false);var molt=a.Source(HostKind.Molt,7);molt.enabledSource=false;molt.gameObject.SetActive(false);
            var left=b.Plate(new Vector2(12,.14f));var right=b.Plate(new Vector2(23,.14f));var front=b.Door(new Vector2(29,3),new Vector2(1,6));
            stringSource.rail=a.Rail(new Vector2(5,15),new Vector2(40,15));var target=b.Solid("The Usher's chandelier catch",new Vector2(28.5f,11.8f),new Vector2(2.2f,3),new Color(.73f,.42f,.5f));
            var chandelier=b.Solid("A weight, not a damage button",new Vector2(24,10),new Vector2(2,1),new Color(.79f,.64f,.35f),Layers.Prop);var rb=chandelier.AddComponent<Rigidbody2D>();rb.mass=2;rb.gravityScale=2;var joint=chandelier.AddComponent<DistanceJoint2D>();joint.autoConfigureConnectedAnchor=false;joint.connectedAnchor=new Vector2(24,16);joint.autoConfigureDistance=false;joint.distance=6;joint.enableCollision=true;var impact=chandelier.AddComponent<ChandelierImpact>();impact.receiver=target.GetComponent<Collider2D>();
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
                if(p==2){a.host.Cure(HostKind.None,true);gullet.gameObject.SetActive(false);rootFloor.SetActive(true);wall.SetActive(true);root.gameObject.SetActive(true);a.Player.Reposition(new Vector2(22,1));boss.objective="Grow around the scale's sealed back; surface behind its support.";}
            });
        }
        void Surveyor(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-9,93,46),new Vector2(3,1));var b=a.b;a.Floor(-5,80);var boss=Director(a,new Vector2(45,9),"The Surveyor",new Color(.31f,.43f,.62f));boss.attackInterval=5;
            GameObject GroupSince(int first,string name){var children=new List<Transform>();for(int i=first;i<b.root.childCount;i++)children.Add(b.root.GetChild(i));var group=new GameObject(name);group.transform.SetParent(b.root);foreach(var child in children)child.SetParent(group.transform,true);return group;}
            SurveyorCore Core(string name,Vector2 at,int layer){var g=b.Solid(name,at,new Vector2(1,1.6f),new Color(.86f,.72f,.35f),layer);return g.AddComponent<SurveyorCore>();}
            GameObject Sill(float x,float y,float width){var g=a.Ledge(x,y,width);g.AddComponent<OneWaySurface>();return g;}
            int first=b.root.childCount;var mirror=a.Source(HostKind.Mirror,7);mirror.explicitAxis=true;mirror.mirrorAxis=22;
            var anchor=b.Plate(new Vector2(12,.14f));b.Solid("The unreflected measuring cabinet",new Vector2(13,1),new Vector2(.5f,2));var shutter=b.Door(new Vector2(34,2),new Vector2(.6f,4),anchor);
            PrimitiveArt.Shape("Surveyor's elevated reflection",b.root,new Vector2(12,4.5f),new Vector2(3,4),new Color(.31f,.43f,.62f),PrimitiveArt.Icon.Arch,-1);
            var reflected=Core("Surveyor exposed reflection",new Vector2(32,1),Layers.Terrain);reflected.Exposed=()=>anchor.Pressed;
            var phaseOne=GroupSince(first,"Surveyor act I mirrored apparatus");var counter=boss.gameObject.AddComponent<SurveyorCounterstroke>();counter.boss=boss;counter.actor=a.Player;

            first=b.root.childCount;var interior=PaintedPassage(a,new Vector2(12,0));a.Source(HostKind.InsideOut,14,.8f);
            var inside=Core("Surveyor internal outline",new Vector2(35.8f,11.8f),Layers.Interior);var phaseTwo=GroupSince(first,"Surveyor act II enclosed attack shape");phaseTwo.SetActive(false);

            first=b.root.childCount;Sill(34.5f,11,9);a.Source(HostKind.Parallax,37,11.8f);a.Projection(new Vector2(39,13),new Vector2(12,9));a.Projection(new Vector2(47,15),new Vector2(14,10));a.Projection(new Vector2(57,17),new Vector2(14,10));
            var cores=new SurveyorCore[3];float[] x={42,51,60},y={12.2f,13.9f,15.5f};
            for(int i=0;i<3;i++){
                var group=new GameObject("Surveyor moving caliper "+i);group.transform.SetParent(b.root);group.transform.position=new Vector2(x[i],y[i]);
                var surface=a.Depth(new Vector2(x[i],y[i]),new Vector2(10,.6f),i);surface.gameObject.AddComponent<OneWaySurface>();surface.transform.SetParent(group.transform,true);
                cores[i]=Core("Surveyor caliper "+i,new Vector2(x[i]+1.7f,y[i]+.3f*DepthGeometry.Factor(i)+.8f),17+i);cores[i].transform.SetParent(group.transform,true);
                var jaw=b.Solid("Caliper end stop "+i,new Vector2(x[i]+5*DepthGeometry.Factor(i)-.25f,y[i]+.3f*DepthGeometry.Factor(i)+.7f),new Vector2(.4f,1.4f),b.accent,17+i);jaw.transform.SetParent(group.transform,true);
                var motion=group.AddComponent<MotionPlatform>();motion.origin=new Vector2(x[i],y[i]);motion.end=motion.origin+Vector2.right*.8f;motion.speed=.65f;
            }
            var phaseThree=GroupSince(first,"Surveyor act III moving projected calipers");phaseThree.SetActive(false);
            a.Health(4,1.2f);b.Tip(new Vector2(7,2),"Pin your present body against the measuring cabinet. The reflected attack can reach the other Surveyor while your own mass holds its shutter.");
            boss.Configure(boss.title,phase=>phase==0?reflected.struck:phase==1?inside.struck:System.Array.TrueForAll(cores,c=>c.struck),phase=>{
                if(phase==0)boss.objective="Make only the reflected Surveyor vulnerable, then strike it with the reflected body. Your attack is copied back.";
                if(phase==1){phaseOne.SetActive(false);phaseTwo.SetActive(true);a.host.Cure();boss.objective="Enter the Surveyor's enclosed attack outline. Its core can only be reached from the physical interior.";}
                if(phase==2){phaseTwo.SetActive(false);phaseThree.SetActive(true);a.host.Cure();boss.objective="Strike the three moving calipers. Their weak points occupy different projection planes; no one body configuration reaches them all.";}
            });
        }
        void Everyone(AtlasBuilder a)
        {
            a.Begin(new Rect(-9,-14,84,62),new Vector2(2,1));var b=a.b;a.Floor(-6,65,-8);a.Floor(-5,11);a.Source(HostKind.Censer,7);
            var boss=Director(a,new Vector2(48,23),"The Weight of Everyone",new Color(.53f,.42f,.45f));boss.combat=false;
            if(boss.body)Object.Destroy(boss.body.gameObject);
            var limbs=new List<KneelingFigure>();for(int i=0;i<7;i++){float y=1+1.7f*i;var limb=a.Figure(12+i*4,y+9,y,.8f+i*.15f);limb.name="Falling congregation limb "+i;limb.hold=6;limb.speed=8;limb.safeUpperSurface=true;limb.respondsToWitness=false;limbs.Add(limb);}
            a.Ledge(9,1,3);a.Ledge(22,4.9f,3).AddComponent<OneWaySurface>();a.Ledge(30,8.3f,3).AddComponent<OneWaySurface>();
            a.Ledge(39,13.2f,4);a.Ledge(43,15,5);var stitchSource=a.Source(HostKind.Stitch,43,16,true);stitchSource.gameObject.SetActive(false);
            var upper=a.Hinge(new Vector2(43,19.74f),10,0,"avalanche");upper.name="Upper load-bearing slab";a.Seam(new Vector2(51.66f,23.74f),"avalanche");
            var catchFloor=a.Hinge(new Vector2(32,12.8f),12,0,"catch");catchFloor.name="Lower load-bearing slab";a.Seam(new Vector2(39.7f,3.6f),"catch");
            var leftStop=b.Solid("Left catch abutment",new Vector2(30.5f,16),new Vector2(1,7));
            var rightStop=b.Solid("Retractable catch abutment",new Vector2(45.5f,15),new Vector2(1,7));
            var bodyArt=PrimitiveArt.Shape("The physical falling congregation",b.root,new Vector2(48,23),Vector2.one*4,new Color(.58f,.44f,.49f),PrimitiveArt.Icon.Round,7);bodyArt.layer=Layers.Prop;
            var circle=bodyArt.AddComponent<CircleCollider2D>();circle.radius=2;bodyArt.transform.localScale=Vector3.one;bodyArt.GetComponent<SpriteRenderer>().drawMode=SpriteDrawMode.Sliced;bodyArt.GetComponent<SpriteRenderer>().size=Vector2.one*4;
            circle.sharedMaterial=new PhysicsMaterial2D("Congregation slides under its own mass"){friction=.015f,bounciness=0};var rb=bodyArt.AddComponent<Rigidbody2D>();rb.mass=24;rb.gravityScale=3.4f;rb.freezeRotation=true;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var mass=bodyArt.AddComponent<ColossusMass>();boss.body=bodyArt.transform;
            for(int i=0;i<6;i++){float theta=i*Mathf.PI/3;var face=PrimitiveArt.Shape("Congregation face "+i,bodyArt.transform,new Vector2(48,23)+new Vector2(Mathf.Cos(theta),Mathf.Sin(theta))*1.15f,new Vector2(.5f,.7f),new Color(.82f,.73f,.65f),PrimitiveArt.Icon.Eye,8);}
            var braceFloor=a.Floor(28,40,10);braceFloor.SetActive(false);var accessFloor=a.Floor(40,53,8);accessFloor.SetActive(false);
            var coffinSource=a.Source(HostKind.Coffin,32,11,true);coffinSource.gameObject.SetActive(false);
            var lowerLatch=b.Switch(new Vector2(34,11.1f),"RELEASE LOWER ABUTMENT");lowerLatch.gameObject.SetActive(false);lowerLatch.Changed+=v=>{rightStop.SetActive(!v);accessFloor.SetActive(!v);};
            a.Health(43,16.3f);a.Cure(HostKind.Coffin,29,11);
            boss.Configure(boss.title,phase=>phase==0?a.Player.Feet.y>14.7f:phase==1?rb.position.x<41&&rb.position.y<18:mass.AtBottom,phase=>{
                if(phase==0)boss.objective="Slow the falling limbs and climb the congregation. The mass above is real, not a health bar.";
                if(phase==1){stitchSource.gameObject.SetActive(true);boss.objective="Tilt its supporting slab. Let its own weight carry it into the lower catch.";}
                if(phase==2){foreach(var limb in limbs)limb.gameObject.SetActive(false);braceFloor.SetActive(true);accessFloor.SetActive(true);coffinSource.gameObject.SetActive(true);lowerLatch.gameObject.SetActive(true);boss.objective="Work beneath the load: release the right abutment, then brace the folding catch so the congregation rolls into the bottom chute.";}
            });
            b.Tip(new Vector2(42,16.8f),"Aim U at a loose architectural edge and its partner. Tilting a support moves everything resting on it.");
            b.Tip(new Vector2(33,11.8f),"Choose a horizontal footing before tugging the lower catch. The slab stops against your physical brace; the load keeps moving.");
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