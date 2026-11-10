using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TMPro;

// Same runner for the unmodified and candidate builds. Calls the original component in Stage1.
public sealed class GoldProbe : MonoBehaviour
{
    [Serializable] public class Row {
        public string id,cost,expectedBalance,actualBalance,status,uiText;
        public int repetition;
        public float initial;
        public bool expectedReturn,actualReturn;
    }
    [Serializable] public class Report {
        public string version,startedAt,finishedAt,unity,gpu,method,error;
        public List<Row> cases=new List<Row>();
    }
    Report report=new Report(); string output,detail="Preparing",active="";
    static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();for(int i=0;i<a.Length-1;i++)if(a[i]==key)return a[i+1];return fallback;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot(){if(Arg("-qaPhase","")!="gold")return;var o=new GameObject("Gold QA runner");DontDestroyOnLoad(o);o.AddComponent<GoldProbe>();}
    IEnumerator Start()
    {
        if(Application.companyName!="PortfolioValidation")throw new Exception("Not a QA build");
        output=Arg("-qaOutput",Application.persistentDataPath);Directory.CreateDirectory(output);
        Application.runInBackground=true;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
        Screen.SetResolution(1920,1080,false);
        report.version=Resources.Load<TextAsset>("RewardQA/version").text;
        report.startedAt=DateTime.UtcNow.ToString("o");report.unity=Application.unityVersion;report.gpu=SystemInfo.graphicsDeviceName;
        report.method="Original Stage1 GoldSystem and GoldUI; direct method boundary inputs; time income disabled; no ordinary UI exploit claim";
        yield return new WaitForSecondsRealtime(1);
        var gold=FindFirstObjectByType<GoldSystem>();var ui=FindFirstObjectByType<GoldUI>();
        if(!gold||!ui){report.error="Missing original GoldSystem or GoldUI";Save();Application.Quit(2);yield break;}
        gold.enabled=false;gold.SetGoldPerSecond(0);Time.timeScale=0;
        var field=typeof(GoldSystem).GetField("currentGold",BindingFlags.Instance|BindingFlags.NonPublic);
        var uiField=typeof(GoldUI).GetField("goldText",BindingFlags.Instance|BindingFlags.NonPublic);
        var uiGold=typeof(GoldUI).GetField("goldSystem",BindingFlags.Instance|BindingFlags.NonPublic);
        if(uiGold.GetValue(ui)!=gold){report.error="UI references a different wallet";Save();Application.Quit(2);yield break;}
        string[] ids={"negative","zero","exact","insufficient","normal","repeat","nan","negative-infinity","positive-infinity"};
        float[] starts={0,15,15,0,20,30,15,15,15};
        float[] costs={-5,0,15,15,5,15,float.NaN,float.NegativeInfinity,float.PositiveInfinity};
        float[] expected={0,15,0,0,15,0,15,15,15};
        bool[] returns={false,true,true,false,true,true,false,false,false};
        for(int rep=1;rep<=3;rep++)for(int i=0;i<ids.Length;i++){
            field.SetValue(gold,starts[i]);yield return null;
            bool actual=gold.TrySpendGold(costs[i]);if(ids[i]=="repeat")actual=gold.TrySpendGold(costs[i])&&actual;
            yield return null;yield return null;
            float balance=gold.GetGold();var text=(TMP_Text)uiField.GetValue(ui);
            var row=new Row{id="DEF-001-"+ids[i],repetition=rep,initial=starts[i],cost=costs[i].ToString(System.Globalization.CultureInfo.InvariantCulture),expectedBalance=expected[i].ToString(),actualBalance=balance.ToString(),expectedReturn=returns[i],actualReturn=actual,uiText=text.text};
            row.status=actual==returns[i]&&balance==expected[i]&&text.text==Mathf.FloorToInt(expected[i]).ToString()?"PASS":"FAIL";
            report.cases.Add(row);Debug.Log("GOLD_QA_CASE "+JsonUtility.ToJson(row));
            active=ids[i]+" / repeat "+rep;detail="Initial "+starts[i]+" | Cost "+row.cost+" | Expected "+row.expectedBalance+" | Actual "+row.actualBalance+" | "+row.status;
            if(rep==1&&(i==0||i==2||i==5)){yield return new WaitForEndOfFrame();var shot=SceneCapture.Capture();File.WriteAllBytes(Path.Combine(output,ids[i]+".png"),shot.EncodeToPNG());Destroy(shot);}
            Save();
        }
        Save();Application.Quit(0);
    }
    void Save(){report.finishedAt=DateTime.UtcNow.ToString("o");File.WriteAllText(Path.Combine(output,"gold.json"),JsonUtility.ToJson(report,true));}
    void OnGUI(){GUI.depth=-200;GUI.color=new Color(.04f,.06f,.12f,.98f);GUI.DrawTexture(new Rect(20,20,1350,130),Texture2D.whiteTexture);GUI.color=Color.white;var s=new GUIStyle(GUI.skin.label){fontSize=23,fontStyle=FontStyle.Bold};GUI.Label(new Rect(35,25,1310,35),"QA BOUNDARY PROBE | "+report.version.Split('|')[0]+" | "+active,s);GUI.Label(new Rect(35,66,1310,70),detail,s);}
}
