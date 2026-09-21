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
                int eligible=dressing.EligibleCount,applied=dressing.AppliedCount;
                check("broad.stage-coverage-"+stage.id,eligible>0&&eligible==applied&&dressing.VisualColliders==0,"eligible="+eligible+" applied="+applied+" visualColliders="+dressing.VisualColliders);
                // Newly-created geometry deliberately far outside EVERY review camera. No game state grants.
                var probe=PrimitiveArt.Shape("Broad late-spawn verification solid",session.transform,new Vector2(-1000,-1000),Vector2.one,Color.white);probe.layer=Layers.Ground;
                var sr=probe.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Tiled;sr.size=new Vector2(3,.5f);probe.AddComponent<BoxCollider2D>().size=sr.size;
                dressing.Scan();yield return null;var skin=probe.GetComponent<BroadSurface>();check("broad.late-spawn-"+stage.id,skin&&skin.Applied&&skin.Profile==BroadArt.Profile(session),"New off-camera object receives the current profile without fixture coordinates");
                UnityEngine.Object.Destroy(probe);yield return null;
            }
            check("broad.all-stage-count",stages.Length==25,"Actual20levels+5bosses enumerated from campaign");
        }
    }
}
