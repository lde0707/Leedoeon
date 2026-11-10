// Copy to Assets/Editor in an isolated FlyEditor project with the source hashes in the report.
// Unity -batchmode -nographics -quit -projectPath <copy> -executeMethod PortfolioRegression.Run -portfolioOutput <results>
using System;
using System.IO;
using System.Diagnostics;
using UnityEngine;

public static class PortfolioRegression
{
    public static void Run()
    {
        string output = Path.Combine(Application.temporaryCachePath, "portfolio-regression");
        string[] args = Environment.GetCommandLineArgs();
        for (int a = 0; a < args.Length - 1; a++)
            if (args[a] == "-portfolioOutput") output = Path.GetFullPath(args[a + 1]);
        Directory.CreateDirectory(output);
        string csv = "iteration,suite,status,elapsed_ms,detail\n";
        for (int i = 1; i <= 10; i++)
        foreach (string suite in new[] { "ResearchEconomy", "PaperFolding" })
        {
            var timer = Stopwatch.StartNew();
            string status = "PASS", detail = "";
            try
            {
                if (suite == "ResearchEconomy") FlyEditor.Editor.ResearchEconomyVerification.Run();
                else FlyEditor.Editor.PaperFoldingVerification.Run();
            }
            catch (Exception error)
            {
                status = "FAIL";
                detail = error.GetBaseException().Message.Replace("\"", "\"\"");
            }
            timer.Stop();
            csv += $"{i},{suite},{status},{timer.ElapsedMilliseconds},\"{detail}\"\n";
            UnityEngine.Debug.Log($"PORTFOLIO_REGRESSION {i} {suite} {status} {timer.ElapsedMilliseconds}ms {detail}");
        }
        File.WriteAllText(Path.Combine(output, "fly-repeats.csv"), csv);
        UnityEngine.Debug.Log("PORTFOLIO_REGRESSION_DONE " + output);
    }
}
