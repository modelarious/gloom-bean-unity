using System;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Draw-only IMGUI skin. It reads state; it never modifies controls, saves or objectives.
    public sealed class V6VisualHud:MonoBehaviour
    {
        GameRoot game;GUIStyle title,small,state,detail;static readonly Color Bone=new Color(.97f,.91f,.76f),Gold=new Color(.87f,.70f,.39f),PanelColor=new Color(.035f,.05f,.075f,.91f);
        void Start(){game=GetComponent<GameRoot>();game.GameplayHud=Draw;}
        static void Fill(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        static void Image(Rect r,Sprite s,Color c){if(!s)return;var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,s.texture,ScaleMode.ScaleToFit,true);GUI.color=old;}
        void Styles(){if(title!=null)return;title=new GUIStyle(GUI.skin.label){fontSize=20,fontStyle=FontStyle.Bold,normal={textColor=Bone}};small=new GUIStyle(GUI.skin.label){fontSize=12,normal={textColor=new Color(.70f,.76f,.77f)}};state=new GUIStyle(title){fontSize=18,alignment=TextAnchor.MiddleRight};detail=new GUIStyle(small){wordWrap=true,fontSize=14};}
        public void Draw()
        {
            if(!game||!game.Session)return;Styles();var s=game.Session;var host=s.player.GetComponent<HostController>();
            Fill(new Rect(0,0,960,61),PanelColor);Fill(new Rect(0,60,960,1),new Color(.48f,.39f,.28f,.85f));
            Image(new Rect(8,7,47,47),HostPixelArt.Host(HostKind.None,game.IsCorrupted),Color.white);
            for(int i=0;i<6;i++)Image(new Rect(61+i*19,9,19,21),V6Art.Sprite("heart",64),i<s.player.Health?Color.white:new Color(.26f,.29f,.34f,.8f));
            Image(new Rect(61,35,18,18),V6Art.Sprite("coin",64),Color.white);GUI.Label(new Rect(81,34,48,20),s.Coins.ToString("000"),small);
            Image(new Rect(126,34,18,19),V6Art.Sprite("key",64),s.HasKey?Color.white:new Color(.45f,.48f,.52f));Image(new Rect(157,33,20,20),V6Art.Sprite("mercy",64),s.Mercies.Count>0?Color.white:new Color(.45f,.48f,.52f));
            GUI.Label(new Rect(207,8,530,28),s.definition.title,title);GUI.Label(new Rect(209,35,540,20),"CHAPTER "+s.definition.worldId.Substring(1)+"  ·  "+(s.definition.boss?"BOSS":"STAGE "+s.definition.course.ToString("00"))+"     "+(game.Practice?"PRACTICE — NOT SAVED":"HOST CYCLE"),small);
            bool returning=s.Phase==RunPhase.Returning;string phase=returning?(s.definition.timed?TimeSpan.FromSeconds(Mathf.Max(0,s.Remaining)).ToString(@"mm\:ss"):"THE TURN"):"OUTWARD";
            GUI.Label(new Rect(748,8,192,30),phase,state);GUI.Label(new Rect(745,38,195,17),"F1 controls   ·   Esc pause",small);
            if(host&&host.Forms.Count>0){Fill(new Rect(8,538,382,54),PanelColor);Fill(new Rect(8,538,2,54),Gold);Image(new Rect(14,540,49,49),HostPixelArt.Host(host.Primary,true),Color.white);
                GUI.Label(new Rect(71,542,310,23),HostController.Display(host.Primary),title);GUI.Label(new Rect(72,567,305,20),host.Forms[host.focus].Status,small);
                if(host.Forms.Count>1)Image(new Rect(347,542,34,34),HostPixelArt.Host(host.Forms[1-host.focus].Kind,true),new Color(.85f,.85f,.85f));
            }
            var boss=s.GetComponentInChildren<AtlasBoss>();
            if(boss&&!boss.defeated){Fill(new Rect(412,538,540,54),PanelColor);Fill(new Rect(412,538,2,54),Gold);GUI.Label(new Rect(424,541,519,18),boss.title+"  ·  ACT "+(boss.phase+1)+" / "+boss.phases,small);GUI.Label(new Rect(424,559,515,31),boss.objective,detail);}
            else if(host&&host.Forms.Count>0){Fill(new Rect(400,538,552,54),PanelColor);GUI.Label(new Rect(413,545,528,44),host.Forms[host.focus].Help,detail);}
            if(!string.IsNullOrEmpty(s.Message)){Fill(new Rect(192,77,576,61),new Color(.035f,.05f,.075f,.94f));Fill(new Rect(192,77,3,61),Gold);GUI.Label(new Rect(207,84,545,52),s.Message,detail);}
        }
        void OnDestroy(){if(game)game.GameplayHud=null;}
    }
}
