using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // These are temporary component fixtures, not gameplay reachability or art approval.
    public static class QualityBarLivingChecks
    {
        public static IEnumerator Run(GameRoot game,Action<string,bool,string> check)
        {
            check("living.phase-forward",QualityLivingArt.Frame(1.2f)==1&&QualityLivingArt.Frame(9.2f)==1,"Finite eight-frame wrap");
            check("living.phase-reverse",QualityLivingArt.Frame(-1.2f)==6,"Signed displacement is not clamped away");
            int resources=0;
            foreach(string prefix in new[]{"living_gear_","living_tread_","living_wake_"})for(int i=0;i<8;i++){var t=Resources.Load<Texture2D>("QualityBar/"+prefix+i);if(t&&t.filterMode==FilterMode.Point&&t.isReadable)resources++;}
            for(int family=0;family<3;family++)foreach(string prefix in new[]{"living_water_","living_surface_"})for(int i=0;i<8;i++){var t=Resources.Load<Texture2D>("QualityBar/"+prefix+family+"_"+i);if(t&&t.filterMode==FilterMode.Point&&t.isReadable)resources++;}
            check("living.frame-resource-contract",resources==72&&Resources.Load<Texture2D>("QualityBar/living_casing"),"Actual imported frame cells="+resources);
            game.SelectSource(1);var stage=game.AvailableWorlds[0].levels[0];yield return game.Load(stage,true);yield return null;var session=game.Session;var quality=session.GetComponent<QualityBarWorld>();
            var probe=new GameObject("Q10 component fixtures outside any review view");probe.transform.SetParent(session.transform,false);
            var waterObject=PrimitiveArt.Shape("Q10 late water",probe.transform,new Vector2(-700,-700),Vector2.one,Color.white);var original=waterObject.GetComponent<SpriteRenderer>();original.drawMode=SpriteDrawMode.Sliced;original.size=new Vector2(7,4);var box=waterObject.AddComponent<BoxCollider2D>();box.size=original.size;box.isTrigger=true;
            var water=waterObject.AddComponent<WaterVolume>();water.current=new Vector2(-3,1);int before=probe.GetComponentsInChildren<Collider2D>(true).Length;quality.Scan();yield return null;var skin=water.GetComponent<QualityBarWaterDetail>();
            check("living.offcamera-water-installs",skin&&skin.Applied&&probe.GetComponentsInChildren<Collider2D>(true).Length==before,"Current late object, not a camera fixture");
            check("living.water-current-observation",skin&&skin.ObservedCurrent==water.current&&water.current==new Vector2(-3,1),"No current mutation");
            check("living.water-surface-aligned",skin&&Mathf.Abs(waterObject.GetComponentsInChildren<SpriteRenderer>().Single(r=>r.name.Contains("animated actual waterline")).bounds.center.y-(box.bounds.max.y-.14f))<.01f,"Rendered surface is the real collider surface");
            original.enabled=false;yield return null;check("living.disabled-water-hides",waterObject.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.Contains("living ")).All(r=>!r.enabled),"No false water/ledge remains");original.enabled=true;skin.enabled=false;yield return null;check("living.disabled-visual-restores-original",!original.forceRenderingOff,"Disabling only the new layer restores the underlying water renderer");skin.enabled=true;yield return null;water.enabled=false;yield return null;check("living.disabled-water-component-hides",waterObject.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.Contains("living ")).All(r=>!r.enabled),"Owner behavior disabled");water.enabled=true;yield return null;
            var beltObject=PrimitiveArt.Shape("Q10 late conveyor",probe.transform,new Vector2(-700,-710),Vector2.one,Color.white);var beltPicture=beltObject.GetComponent<SpriteRenderer>();beltPicture.drawMode=SpriteDrawMode.Tiled;beltPicture.size=new Vector2(6,.65f);beltObject.AddComponent<BoxCollider2D>().size=beltPicture.size;var belt=beltObject.AddComponent<Conveyor>();belt.speed=-2;
            quality.Scan();yield return null;var tread=belt.GetComponent<QualityBarConveyorDetail>();check("living.late-belt-installs",tread&&tread.Applied&&belt.speed==-2,"Real speed unchanged");belt.speed=0;yield return null;int frozen=tread.DisplayFrame;yield return null;check("living.stationary-belt-stops",tread.DisplayFrame==frozen,"Art stops with actual belt");beltPicture.enabled=false;yield return null;check("living.disabled-belt-hides",beltObject.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.Contains("living ")).All(r=>!r.enabled),"No active fake rollers");
            var wheelObject=PrimitiveArt.Shape("Q10 late carousel",probe.transform,new Vector2(-720,-700),Vector2.one,Color.white);var wheel=wheelObject.AddComponent<Carousel>();wheel.radius=2.2f;wheel.angularSpeed=0;for(int i=0;i<4;i++){float angle=i*Mathf.PI*.5f;var armObject=PrimitiveArt.Shape("Q10 fixture arm "+i,probe.transform,new Vector2(-720,-700)+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*2.2f,new Vector2(2,.35f),Color.white);var arm=armObject.AddComponent<MotionPlatform>();arm.origin=armObject.transform.position;arm.speed=0;wheel.arms[i]=arm;}quality.Scan();yield return null;var hub=wheel.GetComponent<QualityBarCarouselDetail>();check("living.late-carousel-installs",hub&&hub.Applied,"Actual wheel gets casing/arm/pins");
            var positions=wheel.arms.Select(a=>a.transform.position).ToArray();hub.Refresh();check("living.carousel-does-not-move-arms",wheel.arms.Select((arm,index)=>arm.transform.position==positions[index]).All(v=>v),"Rendering only reads live endpoints");int oldFrame=hub.DisplayFrame;wheel.arms[0].transform.position=wheel.transform.position+new Vector3(0,2,0);hub.Refresh();check("living.carousel-uses-actual-angle",hub.DisplayFrame!=oldFrame,"Fixture moves the actual arm once; renderer follows without setting orbit phase");wheel.enabled=false;yield return null;check("living.disabled-carousel-hides",wheelObject.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.Contains("living ")).All(r=>!r.enabled),"Disabled parent mechanism has no independent animation");
            check("living.no-added-colliders",probe.GetComponentsInChildren<Collider2D>(true).All(c=>!c.name.StartsWith("QualityBar visual /")),"No collision added by any new visual child");
            UnityEngine.Object.Destroy(probe);yield return null;
        }
        public static void Stage(StageSession session,Action<string,bool,string> check)
        {
            int group=QualityBarArt.Group(session);string horizontal=QualityBarArt.FaceMaterial(session,new Vector2(6,2),false,false),vertical=QualityBarArt.FaceMaterial(session,new Vector2(2,6),false,false);
            bool material=QualityBarArt.FaceMaterial(session,Vector2.one,true,false)=="solid_food"&&QualityBarArt.FaceMaterial(session,Vector2.one,false,true)=="solid_cloth";
            if(group==2)material&=session.definition.course==6?horizontal=="kiln_masonry"&&vertical=="kiln_masonry":horizontal=="rooted_earth"&&vertical=="rooted_vertical";
            check("living.material-semantics-"+session.definition.id,material,"Explicit edible/cloth overrides survive soil orientation and per-level furnace identity");
            bool water=session.GetComponentsInChildren<WaterVolume>(true).All(w=>w.GetComponent<QualityBarWaterDetail>()&&w.GetComponent<QualityBarWaterDetail>().Applied);
            bool belt=session.GetComponentsInChildren<Conveyor>(true).All(w=>w.GetComponent<QualityBarConveyorDetail>()&&w.GetComponent<QualityBarConveyorDetail>().Applied);
            bool wheel=session.GetComponentsInChildren<Carousel>(true).All(w=>w.GetComponent<QualityBarCarouselDetail>()&&w.GetComponent<QualityBarCarouselDetail>().Applied);
            bool rail=session.GetComponentsInChildren<RailPath>(true).All(w=>w.GetComponent<QualityBarRailDrive>()&&w.GetComponent<QualityBarRailDrive>().Applied);
            check("living.all-existing-mechanisms-"+session.definition.id,water&&belt&&wheel&&rail,"Every actual water/belt/wheel/rail receives its render-only layer, including stages outside review cameras");
        }
    }
}
