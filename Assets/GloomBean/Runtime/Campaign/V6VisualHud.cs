using System;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Entire HUD uses the same logical pixel grid as the world, not desktop-sized typography.
    public sealed class V6VisualHud:MonoBehaviour
    {
        GameRoot game;static readonly Color Bone=new Color(1,.94f,.77f),Gold=new Color(1,.75f,.36f),Panel=new Color(.055f,.04f,.10f,.94f);
        void Start(){game=GetComponent<GameRoot>();game.GameplayHud=Draw;}
        static void Image(Rect rect,Sprite sprite,Color tint)=>GbaPixels.Sprite(rect,sprite,tint);
        public void Draw()
        {
            if(!game||!game.Session)return;var prior=GUI.matrix;GUI.matrix=GbaDisplay.PixelMatrix;
            var s=game.Session;var host=s.player.GetComponent<HostController>();
            GbaPixels.Fill(new Rect(0,0,240,20),Panel);GbaPixels.Fill(new Rect(0,19,240,1),new Color(.50f,.32f,.37f));
            Image(new Rect(1,1,18,18),HostPixelArt.Host(HostKind.None,game.IsCorrupted),Color.white);
            for(int i=0;i<6;i++)Image(new Rect(21+i*9,2,9,8),V6Art.Sprite("heart",64),i<s.player.Health?Color.white:new Color(.25f,.20f,.30f));
            Image(new Rect(80,2,8,8),V6Art.Sprite("coin",64),Gold);GbaPixels.Text(new Rect(90,2,30,8),s.Coins.ToString("000"),Bone);
            Image(new Rect(125,2,9,9),V6Art.Sprite("key",64),s.HasKey?Color.white:new Color(.32f,.29f,.36f));
            Image(new Rect(139,1,10,10),V6Art.Sprite("mercy",64),s.Mercies.Count>0?Color.white:new Color(.32f,.29f,.36f));
            string stage=s.definition.worldId.Substring(1)+"-"+(s.definition.boss?"B":(((s.definition.course-1)%4)+1).ToString());
            GbaPixels.Text(new Rect(21,11,111,8),stage+(game.Practice?" PRACTICE":" HOST CYCLE"),new Color(.76f,.68f,.80f));
            bool returning=s.Phase==RunPhase.Returning;string phase=returning?(s.definition.timed?TimeSpan.FromSeconds(Mathf.Max(0,s.Remaining)).ToString(@"mm\:ss"):"THE TURN"):"EXPLORE";
            GbaPixels.Text(new Rect(183,2,55,8),phase,returning?Gold:Bone);GbaPixels.Text(new Rect(177,11,61,8),"F1 HELP",new Color(.70f,.64f,.77f));
            if(host&&host.Forms.Count>0){
                GbaPixels.Fill(new Rect(2,147,128,12),Panel);GbaPixels.Fill(new Rect(2,147,1,12),Gold);
                Image(new Rect(4,146,13,13),HostPixelArt.Host(host.Primary,true),Color.white);
                GbaPixels.Text(new Rect(19,150,109,8),HostController.Display(host.Primary),Bone);
                if(host.Forms.Count>1)Image(new Rect(130,146,13,13),HostPixelArt.Host(host.Forms[1-host.focus].Kind,true),Color.white);
            }
            var boss=s.GetComponentInChildren<AtlasBoss>();var mass=s.GetComponentInChildren<ColossusMass>();
            if(boss&&!boss.defeated){GbaPixels.Fill(new Rect(150,147,88,12),Panel);GbaPixels.Text(new Rect(154,150,82,8),mass?"ALT "+mass.Altitude.ToString("0.0"):"ACT "+(boss.phase+1)+"/"+boss.phases,Gold);}
            if(!string.IsNullOrEmpty(s.Message)){GbaPixels.Fill(new Rect(7,24,226,27),Panel);GbaPixels.Text(new Rect(11,27,217,23),s.Message,Bone);}
            GUI.matrix=prior;
        }
        void OnDestroy(){if(game)game.GameplayHud=null;}
    }
}
