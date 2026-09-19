using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class AtlasNotes:MonoBehaviour
    {
        public StageDefinition definition;bool visible;
        void Update(){if(Input.GetKeyDown(KeyCode.F2))visible=!visible;}
        void OnGUI(){if(!visible)return;GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/960f,Screen.height/600f,1));GUI.Box(new Rect(100,170,760,300),"DESIGNER NOTES — F2");var style=new GUIStyle(GUI.skin.label){fontSize=16,wordWrap=true};GUI.Label(new Rect(120,210,720,240),definition.id+" | Atlas p."+definition.atlasPage+"\n\n"+definition.gimmick+"\n\nTURN: "+definition.turn+"\n\nMERCY: "+definition.mercy+"\n\nGraybox implementation; final art, pacing and uncoached completion are not certified.",style);}
    }
}