using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class EmpyreanVerification
    {
        IEnumerator ChooseFinalTenant(HostKind kind)
        {
            var boss=session.GetComponentInChildren<AtlasBoss>();
            if(kind==HostKind.Echo){yield return Walk(8);yield return Walk(12);yield return Pause(2.2f);yield return Walk(23);yield return Wait("two delayed bodies release the first arch",()=>session.GetComponentsInChildren<Gate>().Any(g=>g.plates.Length==2&&g.opened),3);}
            else {
                yield return Jump(5,2);yield return Jump(8,4);
                if(kind==HostKind.Wax){Check("votive angel supplies conserved wax",host.Has(HostKind.Wax));if(stopped)yield break;yield return Press(new InputFrame{alternate=true});yield return Walk(21);yield return Wait("actual reduced living mass releases the upper arch",()=>session.GetComponentsInChildren<FinalCircuit>().Any(c=>c.circuitId=="First act conserved balance"&&c.Latched),3);}
                else {yield return Jump(5,6);yield return Jump(8,8);yield return SanctuaryPlane(0);yield return Walk(31);Check("far bridge is actual plane-specific support",host.Form<ParallaxForm>()?.Plane==0);}
            }
            if(stopped)yield break;Check("first act tenant follows the player's actual route",session.GetComponentInChildren<FinalChoiceLedger>().Chosen==kind);
            yield return Walk(38);yield return Wait("the opponent steals the chosen tenant without relocating the player",()=>boss.phase==1,3);if(stopped)yield break;
            var ledger=session.GetComponentInChildren<FinalChoiceLedger>();var embargo=session.GetComponentInChildren<HostEmbargo>();
            Check("theft removes and embargoes exactly the chosen tenant",ledger.Thefts==1&&embargo.stolen==kind&&embargo.Blocks(kind)&&!host.Has(kind));
            Check("theft does not refill player health",ledger.HealthAfterTheft==ledger.HealthBeforeTheft);
            if(kind==HostKind.Echo){yield return Walk(8);yield return Pause(.15f);Check("real contact cannot reacquire the stolen tenant",!host.Has(HostKind.Echo));yield return Walk(38);}
            Snapshot("stolen-"+kind);
        }
        IEnumerator AdaptToFinalTheft()
        {
            if(stopped||!Live)yield break;yield return Walk(60);Check("a different tenant is acquired from the vault",host.Has(HostKind.Gullet));yield return Walk(65.8f);
            yield return Press(new InputFrame{action=true,move=Vector2.right});var gullet=host.Form<GulletForm>();Check("swallow the actual vault wall",gullet!=null&&gullet.Stored&&gullet.Stored.name=="Second act removable support");if(stopped)yield break;var tile=gullet.Stored;
            yield return Walk(70.5f);yield return Wait("fall into the opened vault",()=>actor.Grounded&&actor.Feet.y< -5.5f,7);yield return Jump(73,-4);yield return Walk(74.15f);
            yield return Press(new InputFrame{action=true,move=Vector2.right});Check("the same removed wall becomes a bridge in the pit",gullet.Stored==null&&tile.gameObject.activeInHierarchy&&Vector2.Distance(tile.transform.position,new Vector2(77,-4))<.15f);if(stopped)yield break;
            yield return Jump(77,-2);yield return Walk(77.7f);yield return Jump(80.5f,0);yield return Walk(88);yield return Wait("new physical solution earns the final act",()=>session.GetComponentInChildren<AtlasBoss>().phase==2,3);
            Check("stolen rule was actually used against the Host",session.GetComponentInChildren<StolenAttack>().Shots>0);yield return Walk(104);yield return Pause(.1f);Check("the gallery threshold clears the temporary adaptation",host.Forms.Count==0);Snapshot("adapted-to-theft");
        }
        IEnumerator FinalMagnetShadowRoute()
        {
            yield return Walk(109);yield return Jump(114,2);yield return Walk(117);Check("iron and shadow coexist in the chosen circuit",host.Has(HostKind.Lodestone)&&host.Has(HostKind.Shadow));if(stopped)yield break;yield return Walk(126.3f);
            var screen=session.GetComponentsInChildren<MagneticBody>().Single(m=>m.name=="Final suspended iron screen");yield return Wait("reciprocal force moves the suspended screen into alignment",()=>screen.Position.x>125,14,()=>new InputFrame{move=new Vector2(Mathf.Clamp((126.3f-actor.Body.position.x)*3-actor.Body.linearVelocity.x,-1,1),0)});if(stopped)yield break;
            yield return Focus(HostKind.Shadow);yield return Press(new InputFrame{alternate=true});yield return ShadowTravel(new Vector2(129.5f,2.35f));yield return Wait("the manufactured cast silhouette severs its tendon",()=>session.GetComponentInChildren<FinalHeartAnchor>().Released,4);Snapshot("final-magnet-shadow");
        }
        IEnumerator FinalEchoInkRoute()
        {
            yield return Walk(147);yield return Jump(150,2);yield return Jump(152,4);yield return Walk(155);Check("two footsteps share the actual writing tenant",host.Has(HostKind.Echo)&&host.Has(HostKind.Ink));if(stopped)yield break;
            yield return Walk(156.5f,true);yield return ShortArc(152.5f,4);yield return InkLanding(154,5,6.9f);if(stopped)yield break;yield return Jump(158,7.6f);yield return Pause(2.2f);
            yield return Walk(162);yield return Walk(172);yield return Wait("ink access and delayed twin jointly hold the separated scales",()=>session.GetComponentInChildren<FinalHeartAnchor>().Released,4);Snapshot("final-echo-ink");
        }
        IEnumerator FinalWaxGulletRoute()
        {
            yield return Walk(185);yield return Jump(188.5f,2);yield return Walk(191);Check("the structural morsel is approached with a real Gullet",host.Has(HostKind.Gullet));if(stopped)yield break;
            yield return Press(new InputFrame{action=true,move=Vector2.down});var gullet=host.Form<GulletForm>();Check("the cargo was removed from the actual entry floor",gullet!=null&&gullet.Stored);if(stopped)yield break;var tile=gullet.Stored;
            yield return Wait("removing the floor changes support",()=>actor.Grounded&&actor.Feet.y<.3f,4);yield return Walk(196);Check("wax joins the terrain-carrying Gullet",host.Has(HostKind.Wax)&&host.Has(HostKind.Gullet));yield return Focus(HostKind.Wax);yield return Press(new InputFrame{alternate=true});Check("excess wax remains outside the cargo tray",Mathf.Abs(actor.Body.mass-.78f)<.02f&&host.Plugs.Count>0);
            yield return Walk(199.5f);yield return Focus(HostKind.Gullet);yield return Press(new InputFrame{action=true,move=Vector2.right});Check("the same terrain becomes the lift's actual floor",gullet.Stored==null&&tile.gameObject.activeInHierarchy&&Mathf.Abs(tile.transform.position.x-202)<.1f);if(stopped)yield break;
            yield return Jump(202,1.5f);var lift=session.GetComponentInChildren<FinalCargoHoist>();yield return Wait("conserved living load and structural cargo raise the physical lift",()=>lift.Delivered&&actor.Feet.y>9.1f,10);if(stopped)yield break;
            Check("lift carries the same tile rather than an inventory key",lift.Cargo==tile);yield return Jump(207,11.5f);yield return Walk(210);yield return Wait("the balanced cargo route reaches the real tendon",()=>session.GetComponentInChildren<FinalHeartAnchor>().Released,3);Snapshot("final-wax-gullet");
        }
        IEnumerator FinalCoffinTravel(float target)
        {
            if(stopped||!Live)yield break;var coffin=host.Form<CoffinForm>();Check("a physical rigid coffin exists",coffin!=null);if(stopped)yield break;float end=Time.time+14;
            while(Live&&Time.time<end&&Mathf.Abs(actor.Body.position.x-target)>.8f){input.frame=new InputFrame{move=new Vector2(Mathf.Sign(target-actor.Body.position.x),0)};yield return Tick();}input.frame=default;yield return Pause(.5f);Check("quarter-turn footprint reaches "+target,Mathf.Abs(actor.Body.position.x-target)<1.8f&&actor.Feet.y>1.5f);
        }
        IEnumerator FinalStitchCoffinRoute()
        {
            yield return Walk(223);yield return Jump(228,2);yield return Walk(231);Check("the seamstress shares a real rigid body",host.Has(HostKind.Stitch)&&host.Has(HostKind.Coffin));if(stopped)yield break;
            yield return FinalCoffinTravel(236.5f);var coffin=host.Form<CoffinForm>();if(!coffin.Horizontal){yield return Press(new InputFrame{move=Vector2.right});yield return Wait("finish the load-bearing horizontal orientation",()=>coffin.Horizontal&&!coffin.IsFlipping,2);yield return Pause(.2f);}
            yield return Focus(HostKind.Stitch);yield return Press(new InputFrame{action=true,move=new Vector2(1,1)});var stitch=host.Form<StitchForm>();Check("catch the actual load beam seam",stitch.First&&stitch.First.group=="heart-brace");if(stopped)yield break;
            yield return Press(new InputFrame{action=true,move=new Vector2(1,-.2f)});Check("join the beam to its physical lower abutment",stitch.Active!=null);if(stopped)yield break;yield return Press(new InputFrame{action=true});
            yield return Wait("the horizontal coffin physically arrests the stitched load",()=>stitch.Active.HeldLoad&&stitch.Active.BracedSeconds>.8f,5);yield return Wait("the sustained bearing severs the heart tendon",()=>session.GetComponentInChildren<FinalHeartAnchor>().Released,3);Snapshot("final-stitch-coffin");
        }
        IEnumerator FinalMirrorParallaxRoute()
        {
            yield return Walk(261);yield return Jump(264,2);yield return Walk(267);Check("mirrored bodies and depth coexist",host.Has(HostKind.Mirror)&&host.Has(HostKind.Parallax));if(stopped)yield break;
            yield return SanctuaryPlane(0);yield return SanctuaryPlaneJump(266,3.5625f,0);if(stopped)yield break;var twin=host.Form<MirrorForm>().Twin;
            yield return Walk(271);yield return Walk(268);yield return Wait("asymmetric calipers align bodies in different physical planes",()=>session.GetComponentInChildren<FinalHeartAnchor>().Released,5);Check("the other collision body participates in the near plane",twin&&twin.Shape.includeLayers==(1<<19));Snapshot("final-mirror-parallax");
        }
        IEnumerator HostBossRoute()
        {
            string pair=Arg("-gb-final-pair","magnet-shadow");string desired=Arg("-gb-final-choice",pair=="echo-ink"||pair=="mirror-parallax"?"Wax":pair=="wax-gullet"?"Parallax":"Echo");HostKind kind;
            Check("final witness selects a real authored route",Enum.TryParse(desired,out kind)&&(kind==HostKind.Echo||kind==HostKind.Wax||kind==HostKind.Parallax));if(stopped)yield break;
            yield return ChooseFinalTenant(kind);if(stopped)yield break;yield return AdaptToFinalTheft();if(stopped)yield break;
            switch(pair){case "magnet-shadow":yield return FinalMagnetShadowRoute();break;case "echo-ink":yield return FinalEchoInkRoute();break;case "wax-gullet":yield return FinalWaxGulletRoute();break;case "stitch-coffin":yield return FinalStitchCoffinRoute();break;case "mirror-parallax":yield return FinalMirrorParallaxRoute();break;default:Check("known physical circuit",false);break;}
            if(stopped)yield break;var anchor=session.GetComponentInChildren<FinalHeartAnchor>();yield return Wait("released heart falls through actual geometry to the bottom",()=>session.Phase==RunPhase.Cleared&&anchor.LowestHeight< -10,8);
            Check("the chosen pair caused the physical victory",anchor.ReleasedBy==pair&&session.GetComponentInChildren<AtlasBoss>().defeated);Check("the first corruption remains recorded after victory",game.Save.Data.corrupted||Practice);Snapshot("physical-final-victory");
        }
    }
}
