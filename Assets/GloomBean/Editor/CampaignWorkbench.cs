#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using GloomBean.Foundation;
using System;
using System.IO;
namespace GloomBean.Editor
{
    public sealed class CampaignWorkbench:EditorWindow
    {
        Vector2 scroll,layoutScroll;int source;bool layoutMode;string message;
        [MenuItem("Gloom Bean/Campaign workbench")]
        static void Open(){GetWindow<CampaignWorkbench>("Gloom Bean");}
        [InitializeOnLoadMethod]
        static void Hook(){EditorApplication.playModeStateChanged-=State;EditorApplication.playModeStateChanged+=State;}
        static void State(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode)return;
            string pending=SessionState.GetString("GloomBean.PracticeStage","");if(string.IsNullOrEmpty(pending))return;
            bool edit=SessionState.GetBool("GloomBean.LayoutEdit",false);SessionState.EraseString("GloomBean.PracticeStage");
            EditorApplication.delayCall+=()=>{if(GameRoot.Instance){if(edit)ArmLayoutPause(GameRoot.Instance);if(!GameRoot.Instance.OpenPractice(pending))Debug.LogError("Unknown stage "+pending);}};
        }
        static void ArmLayoutPause(GameRoot game)
        {
            Action<StageDefinition,StageBuilder> hook=null;
            hook=(d,b)=>{game.StageBuilt-=hook;EditorApplication.isPaused=true;Selection.activeGameObject=b.root.gameObject;SceneView.lastActiveSceneView?.FrameSelected();};
            game.StageBuilt+=hook;
        }
        void Launch(string id,bool edit=false)
        {
            if(EditorApplication.isPlaying){EditorApplication.isPaused=false;if(edit)ArmLayoutPause(GameRoot.Instance);GameRoot.Instance?.OpenPractice(id);return;}
            if(!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            SessionState.SetString("GloomBean.PracticeStage",id);SessionState.SetBool("GloomBean.LayoutEdit",edit);BuildTools.Open();EditorApplication.EnterPlaymode();
        }
        void SaveLayout(StageLayoutSnapshot snapshot)
        {
            try{
                var patch=snapshot.Export();string directory="Assets/GloomBean/Resources/LevelLayouts";Directory.CreateDirectory(directory);string path=directory+"/"+snapshot.StageId+".json";
                string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(patch,true));if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
                AssetDatabase.ImportAsset(path);message="Saved "+patch.changes.Count+" static geometry overrides. Reload and test the route before committing.";
            }catch(Exception e){message="Not saved: "+e.Message;Debug.LogWarning(message);}
        }
        void Row(StageDefinition d)
        {
            EditorGUILayout.BeginHorizontal();if(GUILayout.Button(d.id+"  "+d.title,GUILayout.Height(25)))Launch(d.id);
            if(GUILayout.Button("Edit layout",GUILayout.Width(88),GUILayout.Height(25))){layoutMode=true;Launch(d.id,true);}EditorGUILayout.EndHorizontal();
        }
        void OnGUI()
        {
            GUILayout.Label("GLOOM BEAN / ITERATION WORKBENCH",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Practice does not write progress. Edit layout pauses a rebuilt course before play: move or scale eligible static boxes in Scene view, save their ordinary JSON overlay, then reload. Moving mechanisms, gates and possession rules remain authored in code.",MessageType.Info);
            if(Type.GetType("GloomBean.Campaign.AtlasCampaign, Assembly-CSharp")!=null)source=GUILayout.Toolbar(source,new[]{"Foundation","Host Cycle"});else{source=0;GUILayout.Label("Foundation-only edition",EditorStyles.miniBoldLabel);}
            if(GUILayout.Button("Inspect movement-tuning asset")){BuildTools.Bootstrap();Selection.activeObject=AssetDatabase.LoadAssetAtPath<MovementTuning>("Assets/GloomBean/Resources/MovementTuning.asset");}
            var game=GameRoot.Instance;var session=game?game.Session:null;var snapshot=session?session.GetComponent<StageLayoutSnapshot>():null;
            if(EditorApplication.isPlaying&&snapshot){
                layoutMode=EditorGUILayout.Foldout(layoutMode,"Current course: "+snapshot.StageId+" / static layout editor");
                if(layoutMode){
                    GUILayout.Label(snapshot.Nodes.Count+" editable static boxes. Transform tools: W move, E rotate, R scale.",EditorStyles.wordWrappedLabel);
                    EditorGUILayout.BeginHorizontal();if(GUILayout.Button(EditorApplication.isPaused?"Resume practice":"Pause for editing"))EditorApplication.isPaused=!EditorApplication.isPaused;
                    using(new EditorGUI.DisabledScope(!EditorApplication.isPaused||!game.Practice||session.Phase!=RunPhase.Explore)){if(GUILayout.Button("Save static layout overrides"))SaveLayout(snapshot);}
                    if(GUILayout.Button("Reload saved layout"))Launch(snapshot.StageId,true);EditorGUILayout.EndHorizontal();
                    if(!string.IsNullOrEmpty(message))EditorGUILayout.HelpBox(message,MessageType.Info);
                    layoutScroll=EditorGUILayout.BeginScrollView(layoutScroll,GUILayout.MaxHeight(220));
                    foreach(var n in snapshot.Nodes){if(GUILayout.Button(n.index+"  "+n.name+"  "+n.originalPosition,GUILayout.Height(20))){Selection.activeGameObject=n.target.gameObject;SceneView.lastActiveSceneView?.FrameSelected();}}
                    EditorGUILayout.EndScrollView();
                }
            }
            scroll=EditorGUILayout.BeginScrollView(scroll);
            ICampaignSource campaign=new FoundationCampaign();var atlas=Type.GetType("GloomBean.Campaign.AtlasCampaign, Assembly-CSharp");if(source==1&&atlas!=null)campaign=(ICampaignSource)Activator.CreateInstance(atlas);
            foreach(var world in campaign.Worlds()){GUILayout.Space(10);GUILayout.Label(world.title,EditorStyles.boldLabel);foreach(var stage in world.levels)Row(stage);Row(world.boss);}
            EditorGUILayout.EndScrollView();GUILayout.Label("F1 controls | F2 atlas notes | U / I possession actions. Overlays reject changed source layouts instead of silently moving the wrong object.",EditorStyles.wordWrappedLabel);
        }
    }
}
#endif
