using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class AtlasBoss:MonoBehaviour
    {
        public string title;public int phase;public bool defeated;public float phaseClock;
        public string objective;public Func<int,bool> Solve;public Action<int> EnterPhase;public int phases=3;public bool combat=true;
        public Transform body;public Vector2 arenaCenter;public float attackInterval=3.8f;float attackClock;public event Action<int> PhaseChanged;
        public void Configure(string name,Func<int,bool> solve,Action<int> enter){title=name;Solve=solve;EnterPhase=enter;phase=0;phaseClock=0;EnterPhase?.Invoke(0);}
        public void Advance(){if(defeated)return;phase++;phaseClock=0;PhaseChanged?.Invoke(phase);RuntimeEvents.Emit("boss-phase",title+":"+phase);if(phase>=phases){defeated=true;StageSession.Current?.BossClear();return;}EnterPhase?.Invoke(phase);}
        void Update()
        {
            if(defeated||!StageSession.Current||StageSession.Current.Phase==RunPhase.Failed)return;
            float dt=Time.deltaTime*LocalTime.Scale(body?body.position:transform.position,true);phaseClock+=dt;attackClock+=dt;
            if(phaseClock>.4f&&Solve!=null&&Solve(phase)){Advance();return;}
            if(combat&&attackClock>attackInterval){attackClock=0;var actor=StageSession.Current.player;if(actor)WarnLane(actor.Body.position.x,phase>0?1.1f:1.35f);}
        }
        void WarnLane(float x,float wait)
        {
            var obj=PrimitiveArt.Shape("Boss commitment tell",transform.parent,new Vector2(x,arenaCenter.y),Vector2.one,new Color(.94f,.55f,.29f,.25f),PrimitiveArt.Icon.Stripe,2);
            var sr=obj.GetComponent<SpriteRenderer>();sr.drawMode=SpriteDrawMode.Sliced;sr.size=new Vector2(2.4f,18);
            var attack=obj.AddComponent<BossLaneAttack>();attack.delay=wait;attack.floor=arenaCenter.y-8;
        }
        void OnGUI()
        {
            if(!StageSession.Current||defeated||!GameRoot.Instance||GameRoot.Instance.CurrentScreen!="Play")return;
            if(GameRoot.Instance.GameplayHud!=null)return; // The native V6 HUD owns only the encounter's presentation.

            var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;
            GUI.depth=10; // The controls card and pause menu stay above the encounter HUD.
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/960f,Screen.height/600f,1));
            GUI.color=new Color(.025f,.018f,.045f,.91f);GUI.DrawTexture(new Rect(18,166,924,78),Texture2D.whiteTexture);GUI.color=Color.white;
            var style=new GUIStyle(GUI.skin.label){fontSize=15,wordWrap=true,normal={textColor=Color.white}};
            GUI.Label(new Rect(30,172,900,65),title+"  |  ACT "+(phase+1)+" / "+phases+"\n"+objective,style);
            GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;
        }
    }
}