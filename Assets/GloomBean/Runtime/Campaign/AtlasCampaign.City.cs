using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign
    {
        void Suns(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-9,116,47),new Vector2(2,1));var b=a.b;a.Floor(-5,43);a.Floor(49,104);a.Floor(43,49,-3);a.Exit(2,1.1f);
            GameObject Balcony(float x,float y,float w){var g=a.Ledge(x,y,w);g.AddComponent<OneWaySurface>();return g;}
            var source=a.Source(HostKind.Mirror,7);source.explicitAxis=true;source.mirrorAxis=21;
            Balcony(12,2,5);Balcony(30,2,5);Balcony(16,4,5);Balcony(26,4,5);Balcony(21,6,7);
            b.Solid("The cabinet without a reflection",new Vector2(30,3.1f),new Vector2(.8f,2.2f));
            var west=b.Plate(new Vector2(16,4.14f));var east=b.Plate(new Vector2(26,4.14f));
            var joined=b.Door(new Vector2(41,4),new Vector2(.8f,8),west,east);joined.latched=true;joined.name="Paired apartment interlock";
            var liftObj=b.Solid("Cross-building lift",new Vector2(44,-.2f),new Vector2(3.5f,.4f),b.accent,Layers.Moving);
            var lift=liftObj.AddComponent<ApartmentLift>();lift.lower=new Vector2(44,-.2f);lift.upper=new Vector2(44,3.8f);lift.access=joined;
            a.Cure(HostKind.Mirror,42.3f,1);
            var courtyardVelvet=a.Cure(HostKind.Mirror,18.8f,3);courtyardVelvet.GetComponent<BoxCollider2D>().size=new Vector2(1.4f,9);
            b.Tip(new Vector2(43,1),"The two balcony scales release the lift. E aboard it calls the upper floor.");

            // The Mercy is a physical desynchronization problem, not a form-name lock.
            Balcony(6,6,14);Balcony(32,6,9);
            var still=b.Plate(new Vector2(10,6.14f));var shifted=b.Plate(new Vector2(34.8f,6.14f));
            var secret=b.Door(new Vector2(6,8),new Vector2(.6f,4),still,shifted);secret.name="Off-register Mercy shutters";secret.latched=true;
            var shutterObj=b.Solid("Heavy east window shutter",new Vector2(30.6f,10),new Vector2(1,1.6f),new Color(.42f,.56f,.63f),Layers.Moving);
            var pusher=shutterObj.AddComponent<WindowShutter>();pusher.park=new Vector2(30.6f,10);pusher.lowered=new Vector2(30.6f,6.82f);pusher.closed=new Vector2(33.7f,6.82f);
            var handle=b.Switch(new Vector2(10,6.8f),"CLOSE EAST SHUTTER");handle.Changed+=pusher.SetClosed;
            a.Mercy(3,7.1f);b.Tip(new Vector2(11,7),"Hold your own scale still. A real shutter can move the reflection without moving you.");
            var sunlight1=PrimitiveArt.Shape("Western sun",b.root,new Vector2(6,19),Vector2.one*3,new Color(.97f,.72f,.46f),PrimitiveArt.Icon.Star,-2);
            var sunlight2=PrimitiveArt.Shape("Eastern sun",b.root,new Vector2(93,23),Vector2.one*3,new Color(.61f,.8f,.96f),PrimitiveArt.Icon.Star,-2);
            var shade=b.Switch(new Vector2(45.35f,4.8f),"DRAW THE SUN CURTAINS");
            var shutters=new List<SunShutter>();for(int i=0;i<4;i++){
                var g=Balcony(49+i*6,4.3f+i*.65f,5);g.name=(i%2==0?"West":"East")+" cast-shadow bridge";
                var sh=g.AddComponent<SunShutter>();sh.phase=i%2;sh.occluder=shade;shutters.Add(sh);
                PrimitiveArt.Line("Projected sunlight "+i,b.root,i%2==0?(Vector2)sunlight1.transform.position:(Vector2)sunlight2.transform.position,(Vector2)g.transform.position,.04f,new Color(.67f,.7f,.78f,.16f),-4);
            }
            Balcony(72,7,4);for(int i=0;i<4;i++)Balcony(76+i*5,8.2f+i*1.8f,4);Balcony(96,13.6f,10);a.Key(90.7f,14.6f);a.Nail(99,14.0f);
            // On the Turn, one sun and its bridges vanish. A new mirrored return room
            // preserves the second-body rule but supplies different physical furniture.
            var back=Balcony(67,7.5f,42);back.name="Western sun return gallery";back.SetActive(false);
            var twinLanding=Balcony(90,7.5f,6);twinLanding.SetActive(false);
            var returnSource=a.Source(HostKind.Mirror,78,9);returnSource.explicitAxis=true;returnSource.mirrorAxis=79;returnSource.GetComponent<BoxCollider2D>().size=new Vector2(1.3f,4);returnSource.gameObject.SetActive(false);
            var returnCabinet=b.Solid("Cabinet left behind by the extinguished sun",new Vector2(85,8.3f),new Vector2(.8f,1.6f));returnCabinet.SetActive(false);
            var rw=b.Plate(new Vector2(68,7.64f));var re=b.Plate(new Vector2(90,7.64f));
            var returnGate=b.Door(new Vector2(59,11),new Vector2(.7f,7),rw,re);returnGate.name="Return apartment interlock";returnGate.latched=true;returnGate.gameObject.SetActive(false);
            var velvet=a.Cure(HostKind.Mirror,61.5f,8.4f);velvet.gameObject.SetActive(false);
            b.session.Turned+=()=>{sunlight2.SetActive(false);foreach(var sh in shutters)sh.reverse=true;back.SetActive(true);twinLanding.SetActive(true);returnSource.gameObject.SetActive(true);returnCabinet.SetActive(true);returnGate.gameObject.SetActive(true);velvet.gameObject.SetActive(true);};
            a.Health(56,1.2f);b.Enemy(new Vector2(97,1),true);
            b.Tip(new Vector2(8,2),"Two bodies, one intention. Furniture interrupts each body separately; thin balcony rails admit jumps from below.");
            b.Tip(new Vector2(75,9),"The eastern sun is gone. Jump the cabinet with your reflection, then bring both bodies to the new balcony scales.");
        }
        TopologyRegion PaintedPassage(AtlasBuilder a,Vector2 origin,bool secret=false)
        {
            const int w=28;int h=secret?20:15;var grid=new char[h][];for(int y=0;y<h;y++){grid[y]=new char[w];for(int x=0;x<w;x++)grid[y][x]='.';}
            void Fill(int x0,int y0,int x1,int y1,char c){for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)grid[y][x]=c;}
            Fill(0,0,3,4,'A');Fill(4,1,11,4,'#');Fill(9,4,12,8,'#');Fill(12,6,21,9,'#');Fill(19,9,23,12,'#');Fill(24,10,27,13,'A');
            Fill(8,3,12,8,'#');Fill(17,7,23,12,'#');Fill(19,9,23,14,'#'); // Room for a full jump before each ascending interior corner.
            // The two domains share only their framed entry and exit. The staircase consists
            // of empty tiles in normal space, so it becomes solid ONLY inside the fresco.
            Fill(6,1,8,1,'.');Fill(8,1,10,2,'.');Fill(10,1,11,4,'.');Fill(11,4,12,5,'.');
            Fill(16,6,18,6,'.');Fill(18,6,20,7,'.');Fill(20,8,21,9,'.');Fill(22,9,23,10,'.');
            var rows=new string[h];for(int y=0;y<h;y++)rows[h-1-y]=new string(grid[y]);
            var go=new GameObject("Closed fresco complement");go.transform.SetParent(a.b.root);var r=go.AddComponent<TopologyRegion>();r.Build(rows,a.b,origin);
            // Both-sided thresholds make acquisition and curing safe in either collision domain.
            a.b.Solid("Shared threshold",origin+new Vector2(1.8f,-.2f),new Vector2(3.6f,.4f),a.b.accent,Layers.Interior);
            a.b.Solid("Shared exit sill",origin+new Vector2(26,10),new Vector2(4,.4f),a.b.accent,Layers.Interior);
            if(secret){
                var extras=new List<Vector2Int>();
                void Region(int x0,int y0,int x1,int y1){for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)extras.Add(new Vector2Int(x,y));}
                Region(3,14,14,17);Region(12,11,15,15);Region(14,11,22,14);
                extras.RemoveAll(v=>v.x>=12&&v.x<=13&&v.y>=11&&v.y<=12);r.DefineSupplement(extras);for(int x=14;x<=21;x++)r.DefineSupplementFloor(new Vector2Int(x,10));
                PrimitiveArt.Line("Unfinished moon contour",a.b.root,origin+new Vector2(3,18),origin+new Vector2(15,18),.07f,new Color(.89f,.82f,.56f),-1);
            }
            return r;
        }
        void Fresco(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-11,120,49),new Vector2(2,1));var b=a.b;a.Floor(-5,18);a.Floor(46,110);a.Exit(2,1.1f);
            GameObject Sill(float x,float y,float w=4){var g=a.Ledge(x,y,w);g.AddComponent<OneWaySurface>();return g;}
            var passage=PaintedPassage(a,new Vector2(15,0),true);
            a.Source(HostKind.InsideOut,17,.8f);a.Cure(HostKind.InsideOut,14.5f,.8f);
            a.Cure(HostKind.InsideOut,42.4f,12);a.Source(HostKind.InsideOut,39.6f,11.1f);Sill(42,10,6);
            var scaffoldObj=b.Solid("Painter's traveling scaffold",new Vector2(36,19),new Vector2(3,.4f),b.accent,Layers.Moving);
            var painter=scaffoldObj.AddComponent<OutlinePainter>();painter.region=passage;painter.start=new Vector2(36,19);painter.end=new Vector2(30,19);
            var handle=b.Switch(new Vector2(41,11.1f),"CLOSE THE MOON'S CONTOUR");handle.Changed+=painter.Draw;
            a.Mercy(21,15.1f);
            Sill(48,8,6);Sill(55,6,6);
            var mirror=a.Source(HostKind.Mirror,59,1);mirror.explicitAxis=true;mirror.mirrorAxis=67;
            var left=b.Plate(new Vector2(62,.14f));var right=b.Plate(new Vector2(75,.14f));
            var gate=b.Door(new Vector2(79,4),new Vector2(.7f,8),left,right);gate.latched=true;gate.name="Fresco paired lift brake";
            b.Solid("Unreflected paint pot",new Vector2(72,1),new Vector2(1,2));
            var velvet=a.Cure(HostKind.Mirror,65,1);velvet.gameObject.SetActive(false);
            var release=b.Switch(new Vector2(62,1),"DRAW MATTE CURTAIN");release.Changed+=v=>{if(gate.opened)velvet.gameObject.SetActive(v);};
            for(int i=0;i<4;i++)Sill(84+i*5,2+i*1.8f);Sill(104,7.4f,10);a.Key(95,7);a.Nail(107,7.8f);
            var peeling=Sill(65,11,44);peeling.name="Peeling fresco return";peeling.SetActive(false);
            var riser=Sill(103,9.2f,5);riser.SetActive(false);var high=Sill(98,11,5);high.SetActive(false);var across=Sill(91,11,10);across.SetActive(false);
            b.session.Turned+=()=>{peeling.SetActive(true);riser.SetActive(true);high.SetActive(true);across.SetActive(true);};
            a.Cure(HostKind.Mirror,81,1);a.Cure(HostKind.None,4,1,true);a.Health(51,1.2f);
            b.Tip(new Vector2(17,3),"Walk inside painted stone. Pale boundaries are solid only to the inside-out body. Empty frames join the two spaces.");
            b.Tip(new Vector2(41,12),"The moon has an unfinished edge. Move the painter's scaffold before trying to enter its outline.");
            b.Tip(new Vector2(61,2),"The paint pot pins only one body. Walk beyond your scale, then reverse to align the reflected tenant.");
        }
        void Tax(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-13,138,51),new Vector2(2,1));var b=a.b;a.Floor(-5,13);a.Floor(54,63);a.Floor(87,126,4);a.Exit(2,1.1f);a.Source(HostKind.Parallax,8);
            GameObject Sill(float x,float y,float w=4){var g=a.Ledge(x,y,w);g.AddComponent<OneWaySurface>();return g;}
            DepthGeometry Shelf(float x,float y,float w,int plane){var d=a.Depth(new Vector2(x,y),new Vector2(w,.6f),plane);d.gameObject.AddComponent<OneWaySurface>();return d;}
            var architecture=new List<DepthGeometry>();float[] heights={1,2.7f,4.3f,6,7.7f,9.3f};
            a.Projection(new Vector2(12,2.5f),new Vector2(8,6));
            for(int i=0;i<6;i++){float x=17+i*6;architecture.Add(Shelf(x,heights[i],8,i%3));a.Projection(new Vector2(x+2,heights[i]+2),new Vector2(9,7));}
            Sill(53,9.7f,6);var stampShelf=Shelf(57,9.5f,10,0);
            var stamp=b.Trigger("Perspective clerk's stamp",new Vector2(53,10.5f),Vector2.one,new Color(.8f,.66f,.36f),PrimitiveArt.Icon.Arch).AddComponent<PerspectiveStamp>();stamp.geometry=new[]{stampShelf};
            a.Cure(HostKind.Parallax,60,10.5f);
            var passage=PaintedPassage(a,new Vector2(58,0));passage.name="Interior of the enlarged filing cabinet";a.Source(HostKind.InsideOut,60,.8f);a.Cure(HostKind.InsideOut,57.2f,.8f);a.Cure(HostKind.InsideOut,85.4f,12);Sill(85,10,6);
            a.Source(HostKind.Parallax,89,10.8f);Sill(89,10,5);a.Projection(new Vector2(93,12),new Vector2(10,7));Shelf(97,11,9,0);a.Projection(new Vector2(102,13.5f),new Vector2(11,8));Shelf(106,12.5f,10,2);
            Sill(118,14,10);a.Projection(new Vector2(115,16),new Vector2(11,8));a.Key(109,14.2f);a.Nail(121,14.4f);

            // Middle-size registration is a physical two-contact fit: the far body
            // is too narrow for both contacts; the near body cannot fit under the roof.
            Sill(38,10,4);Sill(43,12,12);b.Solid("Registration frame upper jaw",new Vector2(43,14.2f),new Vector2(12,.6f));a.Projection(new Vector2(41,11.5f),new Vector2(10,7));
            var left=b.Plate(new Vector2(41.6f,12.8f),.1f);left.GetComponent<BoxCollider2D>().size=new Vector2(.2f,.6f);
            var right=b.Plate(new Vector2(42.4f,12.8f),.1f);right.GetComponent<BoxCollider2D>().size=new Vector2(.2f,.6f);
            var register=b.Door(new Vector2(46.7f,13),new Vector2(.4f,2),left,right);register.name="Tax form registration clamp";register.latched=true;a.Mercy(48,13.1f);
            PrimitiveArt.Label("ALIGN BOTH MARGINS",b.root,new Vector2(42,14.9f),.08f);

            var retreat=new List<GameObject>();
            var rf=Shelf(111,16,10,0);var rm=Shelf(102,16,10,1);var rn=Shelf(92,15,10,2);retreat.Add(rf.gameObject);retreat.Add(rm.gameObject);retreat.Add(rn.gameObject);
            a.Projection(new Vector2(107,18),new Vector2(11,8));a.Projection(new Vector2(97,18),new Vector2(11,8));
            retreat.Add(Sill(66,15,44));for(int i=0;i<6;i++)retreat.Add(Sill(38-i*6,13-i*2,5));foreach(var g in retreat)g.SetActive(false);
            var flat=a.Cure(HostKind.Parallax,84,15.8f);flat.gameObject.SetActive(false);
            b.session.Turned+=()=>{stamp.reversed=true;foreach(var d in architecture)d.SetPlane(0);foreach(var g in retreat)g.SetActive(true);flat.gameObject.SetActive(true);};
            b.Tip(new Vector2(10,2),"Far, middle, near change body and furniture scale together. Change plane during a jump where projected outlines overlap; standing growth may not fit.");
            b.Tip(new Vector2(53,11),"The clerk stamps the counter itself. Your depth and its depth must agree before it can hold you.");
            b.Tip(new Vector2(40,13),"Both registration contacts must touch at once. Too small misses a margin; too large does not fit the frame.");
            a.Health(55,1.2f);b.Enemy(new Vector2(62,1),true);a.Cure(HostKind.None,4,1,true);
        }
        RoomOrbit OrbitRoom(AtlasBuilder a,Vector2 p,Vector2 center,Vector2 radius,float phase,int index)
        {
            var b=a.b;int begin=b.root.childCount;a.Ledge(p.x,p.y,9);b.Wall(p.x-4.5f,p.y+2,4);b.Wall(p.x+4.5f,p.y+2,4);a.Ledge(p.x,p.y+4.7f,9);
            b.Solid("Rear doorway lintel",p+new Vector2(0,3.8f),new Vector2(2,.4f),b.accent);
            var parent=new GameObject("Orbiting room "+index);parent.transform.SetParent(b.root);parent.transform.position=p;
            var children=new List<Transform>();for(int i=begin;i<b.root.childCount-1;i++)children.Add(b.root.GetChild(i));foreach(var t in children)t.SetParent(parent.transform,true);
            var orbit=parent.AddComponent<RoomOrbit>();orbit.center=center;orbit.radius=radius;orbit.phase=phase;orbit.roomSize=new Vector2(10,8);return orbit;
        }
        void Hotel(AtlasBuilder a)
        {
            a.Begin(new Rect(-8,-11,103,59),new Vector2(2,1));var b=a.b;a.Floor(-5,23);a.Floor(68,91);a.Exit(2,1.1f);a.Source(HostKind.Parallax,9);a.Projection(new Vector2(16,4),new Vector2(12,10));
            a.Depth(new Vector2(17,2),new Vector2(6,.5f),0);a.Depth(new Vector2(23,4),new Vector2(6,.5f),1);a.Projection(new Vector2(24,5),new Vector2(8,7));
            Vector2 center=new Vector2(44,17),radius=new Vector2(17,10);var rooms=new List<RoomOrbit>();for(int i=0;i<4;i++){float phase=Mathf.PI*.5f*i;Vector2 p=center+new Vector2(Mathf.Cos(phase)*radius.x,Mathf.Sin(phase)*radius.y);rooms.Add(OrbitRoom(a,p,center,radius,phase,i));}
            a.Steps(28,6,5,3,2,4);a.Ledge(47,17,6);a.Steps(53,19,4,-3,2,4);a.Ledge(44,28,8);
            var mir=a.Source(HostKind.Mirror,40,28.9f);mir.explicitAxis=true;mir.mirrorAxis=44;var p1=b.Plate(new Vector2(40,28.14f));var p2=b.Plate(new Vector2(48,28.14f));var brake=b.root.gameObject.AddComponent<OrbitBrake>();brake.left=p1;brake.right=p2;brake.rooms=rooms.ToArray();
            a.Ledge(56,27,6);a.Ledge(65,25,6);a.Ledge(73,23,6);a.Cure(HostKind.Mirror,67,26);a.Key(71,25);a.Nail(78,23.4f);a.Ledge(78,23,9);
            var hidden=a.Ledge(44,34,7);hidden.SetActive(false);a.Mercy(44,35.2f);var rear=b.Slider(new Vector2(62,29),new Vector2(45,32),new Vector2(5,.5f),1.5f);rear.paused=true;
            b.session.Turned+=()=>{brake.released=true;foreach(var room in rooms)room.running=true;hidden.SetActive(true);rear.paused=false;};
            PaintedPassage(a,new Vector2(68,0));a.Source(HostKind.InsideOut,70,1.8f);a.Cure(HostKind.InsideOut,94,12);a.Ledge(92,15,4);a.Ledge(87,17,4);a.Ledge(82,19,4);
            a.Health(46,18);b.Tip(new Vector2(10,2),"Rooms keep their furniture when they orbit. Match the two roof scales, then use the backs of the rooms during the return.");a.Cure(HostKind.None,4,1,true);
        }
    }
}