#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using GloomBean.Foundation;

namespace GloomBean.Editor
{
    public static class BuildTools
    {
        const string Boot="Assets/GloomBean/Scenes/Boot.unity";
        [MenuItem("Gloom Bean/Create or refresh boot scene")]
        public static void Bootstrap()
        {
            Directory.CreateDirectory("Assets/GloomBean/Scenes");Directory.CreateDirectory("Assets/GloomBean/Resources");
            if(!File.Exists(Boot))
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                new GameObject("Gloom Bean").AddComponent<GameRoot>();
                EditorSceneManager.SaveScene(scene,Boot);
            }
            if(!AssetDatabase.LoadAssetAtPath<MovementTuning>("Assets/GloomBean/Resources/MovementTuning.asset"))
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MovementTuning>(),"Assets/GloomBean/Resources/MovementTuning.asset");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Boot,true)};
            PlayerSettings.companyName="Oddworks";PlayerSettings.productName="Gloom Bean";
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=800;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.colorSpace=ColorSpace.Gamma;QualitySettings.vSyncCount=0;
            AssetDatabase.SaveAssets();Debug.Log("GLOOM_BOOTSTRAP_PASS");
        }
        [MenuItem("Gloom Bean/Build Windows x64")]
        public static void BuildWindows()
        {
            Bootstrap();Directory.CreateDirectory("Builds/Windows");
            var options=new BuildPlayerOptions{scenes=new[]{Boot},locationPathName="Builds/Windows/GloomBean.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development};
            var report=BuildPipeline.BuildPlayer(options);
            Directory.CreateDirectory("Reports");File.WriteAllText("Reports/build-result.json","{\"result\":\""+report.summary.result+"\",\"errors\":"+report.summary.totalErrors+",\"warnings\":"+report.summary.totalWarnings+",\"bytes\":"+report.summary.totalSize+",\"unity\":\""+Application.unityVersion+"\"}");
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);
            Debug.Log("GLOOM_WINDOWS_BUILD_PASS");
        }
        [MenuItem("Gloom Bean/Open boot scene")]
        public static void Open(){Bootstrap();EditorSceneManager.OpenScene(Boot);}
        [MenuItem("Gloom Bean/Export current scene as editable snapshot")]
        public static void Snapshot()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Run a level before taking its authoring snapshot.");
            string p="Assets/GloomBean/Scenes/"+(StageSession.Current?StageSession.Current.definition.id:"Snapshot")+".unity";
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),p,true);
            Debug.Log("Snapshot saved at "+p+". Runtime-generated art is reconstructed; geometry and component fields remain inspectable.");
        }
    }
    [InitializeOnLoad]
    static class FirstImport
    {
        static FirstImport(){EditorApplication.delayCall+=()=>{if(!Application.isBatchMode&&!File.Exists("Assets/GloomBean/Scenes/Boot.unity"))BuildTools.Bootstrap();};}
    }
}
#endif
