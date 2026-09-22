using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public static class BroadVisualChecks
    {
        public static IEnumerator Run(GameRoot game,Action<string,bool,string> check)
        {
            var assets=Resources.LoadAll<Texture2D>("BroadVisual");
            check("broad.pixel-import-contract",assets.Length>=160&&assets.All(t=>t.filterMode==FilterMode.Point),"Actual imported textures="+assets.Length);
            for(int i=0;i<26;i++)check("broad.profile-"+i,new[]{"face_","trim_","bay_","scene_","cap_"}.All(prefix=>Resources.Load<Texture2D>("BroadVisual/"+prefix+i)),"Full material and room profile");
            game.SelectSource(1);var stages=game.AvailableWorlds.SelectMany(w=>w.levels.Concat(new[]{w.boss})).ToArray();
            foreach(var stage in stages){
                yield return game.Load(stage,true);yield return null;var session=game.Session;var dressing=session.GetComponent<BroadWorldDressing>();dressing.Scan();yield return null;
                var quality=session.GetComponent<QualityBarWorld>();int colliderBefore=session.GetComponentsInChildren<Collider2D>(true).Length;
                if(quality)quality.Scan();yield return null;
                check("quality.stage-coverage-"+stage.id,quality&&quality.EligibleCount>0&&quality.AppliedCount==quality.EligibleCount&&quality.AddedColliders==0&&session.GetComponentsInChildren<Collider2D>(true).Length==colliderBefore,"Whole-stage eligible="+(quality?quality.EligibleCount:0)+" rendered="+(quality?quality.AppliedCount:0));
                check("motion.stage-installed-"+stage.id,session.player.GetComponent<HostPixelView>()&&QualityBarMotion.Pixels(true,0,0)!=null,"Actual stage player receives the shared authored motion renderer");
                check("construction.stage-installed-"+stage.id,session.GetComponentsInChildren<QualityBarConstruction>(true).Any(c=>c.Applied),"Physical floor/support-driven construction exists in the stage");
                QualityBarLivingChecks.Stage(session,check);
                QualityBarActorChecks.Stage(session,check);
                int eligible=dressing.EligibleCount,applied=dressing.AppliedCount;
                check("broad.stage-coverage-"+stage.id,eligible>0&&eligible==applied&&dressing.VisualColliders==0,"eligible="+eligible+" applied="+applied+" visualColliders="+dressing.VisualColliders);
                // Newly-created geometry deliberately far outside EVERY review camera. No game state grants.
                var probe=PrimitiveArt.Shape("Broad late-spawn verification solid",session.transform,new Vector2(-1000,-1000),Vector2.one,Color.white);probe.layer=Layers.Terrain;
                var sr=probe.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Tiled;sr.size=new Vector2(3,.5f);probe.AddComponent<BoxCollider2D>().size=sr.size;
                dressing.Scan();yield return null;var skin=probe.GetComponent<BroadSurface>();check("broad.late-spawn-"+stage.id,skin&&skin.Applied&&skin.Profile==BroadArt.Profile(session),"New off-camera object receives the current profile without fixture coordinates");
                if(quality)quality.Scan();yield return null;var fitting=probe.GetComponent<QualityBarSurface>();
                check("quality.offcamera-late-spawn-"+stage.id,fitting&&fitting.Applied&&probe.GetComponentsInChildren<Collider2D>(true).Length==1,"New construction receives the art outside every review camera, without added collision");
                var construction=probe.GetComponent<QualityBarConstruction>();check("construction.offcamera-late-spawn-"+stage.id,construction&&construction.Applied&&construction.PieceCount>0,"Previously unseen support receives dimensional structure without review-coordinate conditions");
                sr.enabled=false;yield return null;
                check("quality.disabled-source-hides-fittings-"+stage.id,probe.GetComponentsInChildren<SpriteRenderer>(true).Where(r=>r.name.StartsWith("QualityBar visual /")).All(r=>!r.enabled),"Inactive terrain must not leave a visible fake ledge");
                UnityEngine.Object.Destroy(probe);yield return null;
            }
            check("broad.all-stage-count",stages.Length==25,"Actual20levels+5bosses enumerated from campaign");
        }
    }
}
