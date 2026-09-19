#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using GloomBean.Foundation;
using System;
namespace GloomBean.Editor
{
    public sealed class CampaignWorkbench:EditorWindow
    {
        Vector2 scroll;int source;
        [MenuItem("Gloom Bean/Campaign workbench")]
        static void Open(){GetWindow<CampaignWorkbench>("Gloom Bean");}
        [InitializeOnLoadMethod]
        static void Hook(){EditorApplication.playModeStateChanged-=State;EditorApplication.playModeStateChanged+=State;}
        static void State(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode)return;
            string pending=SessionState.GetString("GloomBean.PracticeStage","");if(string.IsNullOrEmpty(pending))return;
            SessionState.EraseString("GloomBean.PracticeStage");
            EditorApplication.delayCall+=()=>{if(GameRoot.Instance&&!GameRoot.Instance.OpenPractice(pending))Debug.LogError("Unknown stage "+pending);};
        }
        void Launch(string id)
        {
            if(EditorApplication.isPlaying){GameRoot.Instance?.OpenPractice(id);return;}
            if(!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            SessionState.SetString("GloomBean.PracticeStage",id);BuildTools.Open();EditorApplication.EnterPlaymode();
        }
        void OnGUI()
        {
            GUILayout.Label("GLOOM BEAN / ITERATION WORKBENCH",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Preview any course without writing progression or Mercy completion. Play geometry is constructed from the campaign's authoring code; it is not a baked scene or final art release.",MessageType.Info);
            source=GUILayout.Toolbar(source,new[]{"Foundation","Host Cycle"});
            if(GUILayout.Button("Inspect movement-tuning asset"))
            {
                BuildTools.Bootstrap();Selection.activeObject=AssetDatabase.LoadAssetAtPath<MovementTuning>("Assets/GloomBean/Resources/MovementTuning.asset");
            }
            scroll=EditorGUILayout.BeginScrollView(scroll);
            ICampaignSource campaign=new FoundationCampaign();
            var atlas=Type.GetType("GloomBean.Campaign.AtlasCampaign, Assembly-CSharp");
            if(source==1&&atlas!=null)campaign=(ICampaignSource)Activator.CreateInstance(atlas);
            foreach(var world in campaign.Worlds())
            {
                GUILayout.Space(10);GUILayout.Label(world.title,EditorStyles.boldLabel);
                foreach(var stage in world.levels)if(GUILayout.Button(stage.id+"  "+stage.title,GUILayout.Height(26)))Launch(stage.id);
                if(GUILayout.Button(world.boss.id+"  BOSS: "+world.boss.title,GUILayout.Height(28)))Launch(world.boss.id);
            }
            EditorGUILayout.EndScrollView();GUILayout.Space(6);
            GUILayout.Label("F1: controls   F2: atlas notes   U / I: possession actions",EditorStyles.wordWrappedLabel);
        }
    }
}
#endif
