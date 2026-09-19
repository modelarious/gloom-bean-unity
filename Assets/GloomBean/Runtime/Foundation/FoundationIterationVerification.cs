using System;
using System.Collections;
using System.Linq;
using UnityEngine;
namespace GloomBean.Foundation
{
    // Fixtures verify reusable iteration features in both the foundation-only and atlas projects.
    public sealed class FoundationIterationVerification:MonoBehaviour
    {
        readonly WaitForFixedUpdate tick=new WaitForFixedUpdate();
        IEnumerator Steps(int n){for(int i=0;i<n;i++)yield return tick;}
        public IEnumerator Run(GameRoot game,FoundationVerification report)
        {
            void C(string name,bool pass,string observation=""){report.Check("iteration."+name,pass,observation);}
            game.SelectSource(0);yield return game.Load(game.AvailableWorlds[0].levels[0],true);yield return Steps(4);
            game.Session.player.disabled=true;game.Session.player.Body.simulated=false;
            var fixture=new GameObject("Independent foundation iteration fixtures");fixture.transform.SetParent(game.Session.transform);
            var b=new StageBuilder(fixture.transform,game.Session);b.Floor(700,0,80);var actor=b.Player(new Vector2(700,.8f));actor.GetComponent<HumanInput>().disabled=true;
            var input=new ScriptedInput();actor.input=input;yield return Steps(5);
            var thin=b.Platform(new Vector2(700,1.8f),new Vector2(5,.4f));thin.AddComponent<OneWaySurface>();yield return Steps(3);
            input.frame=new InputFrame{jump=true,jumpHeld=true};yield return Steps(1);input.frame=new InputFrame{jumpHeld=true};float peak=actor.Feet.y;
            for(int step=0;step<70;step++){yield return tick;peak=Mathf.Max(peak,actor.Feet.y);}
            C("oneway.rise-through-and-land",peak>2.15f&&actor.Grounded&&Mathf.Abs(actor.Feet.y-2)<.12f,"peak="+peak+" feet="+actor.Feet.y);

            var editable=b.Solid("Editable fixture ledge",new Vector2(710,2),new Vector2(3,.4f));var machine=b.Slider(new Vector2(716,2),new Vector2(719,2),Vector2.one);
            var overlay=StageLayoutSnapshot.Install("fixture",fixture.transform,false);var oldPosition=editable.transform.position;
            C("layout.captures-static-positive-control",overlay.Nodes.Any(n=>n.target==editable.transform),"nodes="+overlay.Nodes.Count);
            C("layout.excludes-scripted-machinery",!overlay.Nodes.Any(n=>n.target==machine.transform));
            editable.transform.position+=Vector3.up*2;editable.transform.localScale=new Vector3(1.5f,1.5f,1);
            var authored=JsonUtility.FromJson<LayoutPatch>(JsonUtility.ToJson(overlay.Export()));
            editable.transform.position=oldPosition;editable.transform.localScale=Vector3.one;
            bool applied=overlay.Apply(authored,out string reason);
            C("layout.pose-collider-and-sprite-roundtrip",applied&&Mathf.Abs(editable.transform.position.y-4)<.001f&&Vector2.Distance(editable.GetComponent<BoxCollider2D>().size,new Vector2(4.5f,.6f))<.001f&&editable.GetComponent<SpriteRenderer>().size==editable.GetComponent<BoxCollider2D>().size,"changes="+authored.changes.Count+" "+reason);
            if(authored.changes.Count>0){
                Vector3 acceptedPosition=editable.transform.position;authored.sourceFingerprint="stale";authored.changes[0].position+=Vector2.right*9;
                C("layout.stale-patch-rejected-atomically",!overlay.Apply(authored,out reason)&&editable.transform.position==acceptedPosition);
                authored.sourceFingerprint=overlay.Fingerprint;authored.changes.Add(authored.changes[0]);
                C("layout.duplicate-target-rejected",!overlay.Apply(authored,out reason)&&editable.transform.position==acceptedPosition);
            }else C("layout.export-has-a-real-change",false);
            var added=new GameObject("An unsupported hierarchy edit");added.transform.SetParent(fixture.transform);bool rejected=false;
            try{overlay.Export();}catch(InvalidOperationException){rejected=true;}C("layout.added-hierarchy-is-not-silently-lost",rejected);Destroy(added);

            var cue=GameAudio.Synthesize("turn");C("audio.bounded-finite-original-cue",cue.Length>10000&&cue.All(v=>!float.IsNaN(v)&&!float.IsInfinity(v)&&Mathf.Abs(v)<.3f));
            C("audio.verification-forces-silence",game.GetComponent<GameAudio>()&&game.GetComponent<GameAudio>().Muted);
            var first=new WorldDefinition{id="test1",levels=new[]{new StageDefinition{id="a"},new StageDefinition{id="b"}},boss=new StageDefinition{id="boss1",boss=true}};
            var next=new WorldDefinition{id="test2",levels=new[]{new StageDefinition{id="c"}},boss=new StageDefinition{id="boss2",boss=true}};var data=new SaveData();
            C("progress.later-stage-and-boss-initially-locked",!CampaignProgression.LevelOpen(first,1,data)&&!CampaignProgression.BossOpen(first,data)&&!CampaignProgression.WorldOpen(new[]{first,next},1,data));
            data.cleared.Add("a");C("progress.first-clear-opens-only-next-stage",CampaignProgression.LevelOpen(first,1,data)&&!CampaignProgression.BossOpen(first,data));
            data.cleared.Add("b");C("progress.all-course-clears-open-boss",CampaignProgression.BossOpen(first,data)&&!CampaignProgression.WorldOpen(new[]{first,next},1,data));
            data.cleared.Add("boss1");C("progress.boss-opens-next-world",CampaignProgression.WorldOpen(new[]{first,next},1,data));
            C("progress.invalid-indices-stay-closed",!CampaignProgression.LevelOpen(first,-1,data,true)&&!CampaignProgression.WorldOpen(new[]{first,next},2,data,true));
            input.frame=default;actor.SetStandingSize(new Vector2(1.5f,.32f));yield return Steps(5);
            C("body.wide-capsule-physical-footprint",actor.Shape.direction==CapsuleDirection2D.Horizontal&&actor.Shape.bounds.size.y<.36f&&actor.Shape.bounds.size.x>1.45f,actor.Shape.bounds.size.ToString());
            actor.RestoreShape();yield return Steps(5);C("body.restore-vertical-capsule",actor.Shape.direction==CapsuleDirection2D.Vertical&&actor.Shape.bounds.size.y>1.45f);
            yield return Steps(75);actor.Hit(new HitInfo(null,Vector2.left,1));int damaged=actor.Health;actor.Reposition(new Vector2(702,3));
            C("body.reposition-preserves-damage",damaged<actor.tuning.maximumHealth&&actor.Health==damaged);
            Destroy(fixture);yield return null;
        }
    }
}
