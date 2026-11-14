#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using UnityEditor;using UnityEditor.Build.Reporting;
public static class PortfolioQaBuild
{
    public static void Build()
    {
        string company=PlayerSettings.companyName,product=PlayerSettings.productName;
        int code=1;
        try
        {
            if(Environment.GetEnvironmentVariable("PORTFOLIO_WINDOWS_D3D11")=="1") {
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
            }
            PlayerSettings.companyName="PortfolioIsolatedQA20261114";
            var options=new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=Environment.GetEnvironmentVariable("PORTFOLIO_QA_EXE"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None,extraScriptingDefines=new[]{"PORTFOLIO_QA"}};
            var report=BuildPipeline.BuildPlayer(options);code=report.summary.result==BuildResult.Succeeded?0:1;
        }
        finally{PlayerSettings.companyName=company;PlayerSettings.productName=product;AssetDatabase.SaveAssets();}
        EditorApplication.Exit(code);
    }
}
#endif
