using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public static class QualityBarActorChecks
    {
        static int Visible(Sprite s){if(!s||!s.texture.isReadable)return 0;var r=s.rect;return s.texture.GetPixels((int)r.x,(int)r.y,(int)r.width,(int)r.height).Count(c=>c.a>.1f);}
        public static IEnumerator Run(GameRoot game,Action<string,bool,string> check)
        {
            check("actors.state.walk",QualityBarActorArt.PatrolPose(EnemyState.Patrol,1)==QualityPatrolPose.Walk,"Real velocity selects gait");
            check("actors.state.idle",QualityBarActorArt.PatrolPose(EnemyState.Patrol,0)==QualityPatrolPose.Idle,"No running in place");
            check("actors.state.stunned",QualityBarActorArt.PatrolPose(EnemyState.Stunned,3)==QualityPatrolPose.Stunned,"Stunned pose precedes slide speed");
            check("actors.state.carried",QualityBarActorArt.PatrolPose(EnemyState.Carried,3)==QualityPatrolPose.Carried,"Raised-hand carried pose");
            check("actors.state.thrown",QualityBarActorArt.PatrolPose(EnemyState.Thrown,0)==QualityPatrolPose.Thrown,"Authored tumbling cell");
            check("actors.signed-clock",QualityBarActorArt.Frame(-1.1f)==6&&QualityBarActorArt.Frame(9.1f)==1,"Signed wrap bounded");
            for(int w=1;w<=5;w++){
                for(int armor=0;armor<2;armor++)for(int pose=0;pose<5;pose++){
                    bool valid=true;for(int f=0;f<8;f++){var sprite=QualityBarActorArt.Patrol(w,(QualityPatrolPose)pose,f,armor>0);int count=Visible(sprite);valid&=sprite&&sprite.rect.width==32&&sprite.rect.height==32&&count>100&&count<1024&&sprite.texture.filterMode==FilterMode.Point;}
                    check("actors.cells.patrol."+w+"."+armor+"."+pose,valid,"All8 authored cells imported/transparent");
                }
                for(int phase=0;phase<3;phase++)for(int pose=0;pose<3;pose++){
                    bool valid=true;for(int f=0;f<8;f++){var sprite=QualityBarActorArt.Boss(w,phase,(QualityBossPose)pose,f);int count=Visible(sprite);valid&=sprite&&sprite.rect.width==128&&sprite.rect.height==128&&count>1000&&count<16384&&sprite.texture.filterMode==FilterMode.Point;}
                    check("actors.cells.boss."+w+"."+phase+"."+pose,valid,"All8 fixed-footprint boss cells imported");
                }
                yield return null;
            }
            game.SelectSource(1);yield return game.Load(game.AvailableWorlds[0].levels[0],true);yield return null;var session=game.Session;
            var probe=PrimitiveArt.Shape("Q11 offcamera late patrol",session.transform,new Vector2(-900,900),Vector2.one,Color.white,PrimitiveArt.Icon.Block,9);
            var sr=probe.GetComponent<SpriteRenderer>();var body=probe.AddComponent<Rigidbody2D>();var box=probe.AddComponent<BoxCollider2D>();var actor=probe.AddComponent<CarryableEnemy>();actor.patrolSpeed=0;body.gravityScale=0;
            session.GetComponent<QualityBarWorld>().Scan();yield return null;var view=probe.GetComponent<PatrolPixelView>();
            check("actors.late-offcamera-patrol",view&&view.Picture&&view.Picture.sprite.name.StartsWith("Q11 authored"),"Component scan outside all sampled cameras");
            var position=body.position;var velocity=body.linearVelocity;var dimensions=box.size;int direction=actor.direction;var state=actor.state;
            view.Refresh();check("actors.refresh-read-only",body.position==position&&body.linearVelocity==velocity&&box.size==dimensions&&actor.direction==direction&&actor.state==state,"No body/input/collision/state mutation");
            sr.enabled=false;yield return null;check("actors.original-visibility",!view.Picture.enabled,"Disabled source hides new art");sr.enabled=true;
            actor.enabled=false;yield return null;check("actors.owner-disabled",!view.Picture.enabled,"Disabled owner hides art");actor.enabled=true;
            view.enabled=false;yield return null;check("actors.visual-disable-restores",!sr.forceRenderingOff&&!view.Picture.enabled,"Fallback restored");view.enabled=true;yield return null;
            check("actors.no-extra-colliders",probe.GetComponentsInChildren<Collider2D>().Length==1,"Only original collision");UnityEngine.Object.Destroy(probe);yield return null;
        }
        public static void Stage(StageSession session,Action<string,bool,string> check)
        {
            var patrols=session.GetComponentsInChildren<CarryableEnemy>(true);
            bool valid=patrols.Where(e=>e.gameObject.activeInHierarchy).All(e=>{var v=e.GetComponent<PatrolPixelView>();return v&&v.Picture&&v.Picture.sprite&&v.Picture.sprite.name.StartsWith("Q11 authored");});
            var bosses=session.GetComponentsInChildren<AtlasBoss>(true);
            valid&=bosses.Where(b=>b.gameObject.activeInHierarchy&&b.body).All(b=>{var v=b.body.GetComponent<BossPixelView>();return v&&v.Picture&&v.Picture.sprite&&v.Picture.sprite.name.StartsWith("Q11 authored")&&Vector2.Distance(v.Picture.bounds.size,b.body.GetComponent<SpriteRenderer>().bounds.size)<.01f;});
            check("actors.all25."+session.definition.id,valid,"All active actual patrol/boss components; patrols="+patrols.Length+",bosses="+bosses.Length);
        }
    }
}
