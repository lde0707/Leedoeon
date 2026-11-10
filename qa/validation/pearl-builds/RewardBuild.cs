using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RewardBuild
{
    public static string Arg(string key, string fallback)
    { var a=Environment.GetCommandLineArgs();for(int i=0;i<a.Length-1;i++)if(a[i]==key)return a[i+1];return fallback; }
    public static void Run()
    {
        SessionState.SetBool("AutoSetupStagesDone",true);
        string output=Path.GetFullPath(Arg("-qaOutput",Path.Combine(Application.dataPath,"../../build-before")));
        string variant=Arg("-qaVariant","before");
        Directory.CreateDirectory(output);
        PlayerSettings.companyName="PortfolioValidation";
        PlayerSettings.productName="PearlRewardRebuild1110-"+variant;
        PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.runInBackground=true;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        Directory.CreateDirectory("Assets/Resources/RewardQA");
        AssetDatabase.Refresh();
        if(!File.Exists("Assets/Resources/RewardQA/Enemy.prefab"))
            if(!AssetDatabase.CopyAsset("Assets/Prefabs/Enemy/eWarrior.prefab","Assets/Resources/RewardQA/Enemy.prefab"))throw new Exception("Enemy fixture copy failed");
        File.WriteAllText("Assets/Resources/RewardQA/version.txt",variant+"|"+Arg("-qaCommit","unknown"));
        AssetDatabase.Refresh();
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=new[]{"Assets/Scenes/StageScenes/Stage/Stage1.unity","Assets/Scenes/StageScenes/SelectStage.unity"},
            locationPathName=Path.Combine(output,"PearlReward.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development
        });
        Debug.Log("REWARD_QA_BUILD "+variant+" "+result.summary.result);
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+result.summary.result);
    }
}

// Identical test infrastructure in both builds: no external SDK initialization or score submission.
public sealed class RewardOfflineBuild : IProcessSceneWithReport
{
    public int callbackOrder=>1000;
    public void OnProcessScene(Scene scene,BuildReport report)
    {
        foreach(var root in scene.GetRootGameObjects())
            foreach(var manager in root.GetComponentsInChildren<StoveManager>(true))
                UnityEngine.Object.DestroyImmediate(manager);
    }
}
