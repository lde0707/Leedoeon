using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Object=UnityEngine.Object;

public class RewardProbe : MonoBehaviour
{
    [Serializable] public class Observation
    {
        public string id,scenario,status,resultText,sceneAfter;
        public int repetition,initialMinerals,kills,unitReward,earnedBefore,expectedMinerals,walletAfter,walletAfterSecondCall,savedMinerals,afterReturn,unbankedAfterReturn;
        public bool resultOpened;
    }
    [Serializable] public class RunReport
    {
        public string variant,commit,startedAt,finishedAt,unity,gpu,method,phase,error;
        public List<Observation> cases=new List<Observation>();
        public int reloadMinerals,reloadExpected;
        public string reloadStatus;
    }
    private static RewardProbe instance;
    private RunReport report=new RunReport();
    private string output,phase,label="Preparing",activeCase="",info="";
    private int deaths;
    private bool capture;
    private static string Arg(string key,string fallback){var a=Environment.GetCommandLineArgs();for(int i=0;i<a.Length-1;i++)if(a[i]==key)return a[i+1];return fallback;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        if(instance)return;
        if(Application.companyName!="PortfolioValidation"||!Application.productName.StartsWith("PearlRewardRebuild1110-"))throw new Exception("Refusing to use a player profile");
        var go=new GameObject("Reward QA runner");DontDestroyOnLoad(go);instance=go.AddComponent<RewardProbe>();
        SceneManager.sceneLoaded+=(scene,mode)=>{
            foreach(var wave in Object.FindObjectsByType<WaveSystem>(FindObjectsSortMode.None)){wave.enabled=false;wave.StopAllCoroutines();}
        };
    }
    private void Start()
    {
        if(Arg("-qaPhase","")=="gold")return;
        Application.runInBackground=true;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
        Screen.SetResolution(1920,1080,false);
        output=Path.GetFullPath(Arg("-qaOutput",Path.Combine(Application.persistentDataPath,"results")));
        phase=Arg("-qaPhase","suite");capture=Arg("-qaCapture","1")=="1";
        Directory.CreateDirectory(output);
        var version=Resources.Load<TextAsset>("RewardQA/version").text.Split('|');
        report.variant=version[0];report.commit=version[1];report.phase=phase;
        report.startedAt=DateTime.UtcNow.ToString("o");report.unity=Application.unityVersion;report.gpu=SystemInfo.graphicsDeviceName;
        report.method="Windows development player; original Stage1 scene and enemy prefab; deterministic damage fixture; automatic waves disabled; external SDK stripped; isolated PlayerPrefs";
        Entity.OnAnyDeath+=OnDeath;
        StartCoroutine(Guard(Execute()));
    }
    private void OnDeath(Entity entity,Entity attacker){if(entity is EnemyEntity&&!entity.CompareTag("EnemyCastle"))deaths++;}
    private IEnumerator Guard(IEnumerator inner)
    {
        while(true){object value=null;bool next=false;try{next=inner.MoveNext();if(next)value=inner.Current;}catch(Exception e){report.error=e.ToString();Debug.LogException(e);Save();Application.Quit(2);yield break;}if(!next)yield break;yield return value;}
    }
    private void Require(bool value,string message){if(!value)throw new Exception("FIXTURE_BLOCKED: "+message);}
    private IEnumerator Execute()
    {
        yield return new WaitForSecondsRealtime(1);
        if(phase=="reload")
        {
            label="RESTART / saved wallet";
            report.reloadExpected=int.Parse(Arg("-qaExpected","145"));
            report.reloadMinerals=PersistentCurrencyWallet.Minerals;
            report.reloadStatus=report.reloadMinerals==report.reloadExpected?"PASS":"FAIL";
            info="Expected "+report.reloadExpected+" | Actual "+report.reloadMinerals+" | "+report.reloadStatus;
            SceneManager.LoadScene("SelectStage");yield return new WaitForSecondsRealtime(1);
            yield return new WaitForEndOfFrame();Shot("reload");Save();Application.Quit(0);yield break;
        }
        var scenarios=new[]{"clear","exit","zero-fail","repeat-fail","fail"};
        for(int repetition=1;repetition<=3;repetition++)foreach(string scenario in scenarios)
        {
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("Stage1");yield return null;yield return new WaitForSecondsRealtime(.25f);
            foreach(var wave in Object.FindObjectsByType<WaveSystem>(FindObjectsSortMode.None)){wave.enabled=false;wave.StopAllCoroutines();}
            foreach(var enemy in Object.FindObjectsByType<EnemyEntity>(FindObjectsSortMode.None))if(!enemy.CompareTag("EnemyCastle"))Object.Destroy(enemy.gameObject);
            yield return null;
            PersistentCurrencyWallet.Set(100,10);deaths=0;
            var result=Object.FindFirstObjectByType<StageResultUI>();
            var castle=Object.FindObjectsByType<CastleEntity>(FindObjectsSortMode.None).FirstOrDefault(e=>e.CompareTag("PlayerCastle"));
            var enemyCastle=Object.FindObjectsByType<EnemyEntity>(FindObjectsSortMode.None).FirstOrDefault(e=>e.CompareTag("EnemyCastle"));
            Require(result&&castle&&enemyCastle,"Stage1 must contain both castles and result UI");
            Require(StageRewardSystem.EarnedMinerals==0,"Stage entry must reset unbanked rewards");
            activeCase=scenario+" / repeat "+repetition;label="ACCUMULATE / actual enemy death events";info="Initial wallet 100 | Expected enemy reward 15 x 3";
            int expectedEarned=scenario=="zero-fail"?0:45;
            if(expectedEarned>0)
            {
                var prefab=Resources.Load<GameObject>("RewardQA/Enemy");Require(prefab,"Missing original enemy prefab fixture");
                for(int n=0;n<3;n++)
                {
                    var go=Object.Instantiate(prefab,new Vector3(-1+n*1.5f,0,0),Quaternion.identity);
                    var enemy=go.GetComponent<EnemyEntity>();yield return null;
                    Require(enemy&&enemy.GetStats()!=null&&enemy.GetStats().scoreValue==15,"Enemy fixture reward must be 15");
                    enemy.TakeDamage(1000000f,castle);
                }
                float until=Time.realtimeSinceStartup+12;
                while(deaths<3&&Time.realtimeSinceStartup<until)yield return null;
                Require(deaths==3,"Three actual death callbacks required");
            }
            Require(StageRewardSystem.EarnedMinerals==expectedEarned,"Accumulated reward mismatch");
            Require(PersistentCurrencyWallet.Minerals==100,"Wallet must remain 100 before settlement");
            if(capture&&repetition==1&&scenario=="fail"){yield return new WaitForEndOfFrame();Shot("accumulated");}
            var row=new Observation{id="PE-BUILD-"+scenario,scenario=scenario,repetition=repetition,initialMinerals=100,kills=deaths,unitReward=15,earnedBefore=StageRewardSystem.EarnedMinerals,expectedMinerals=100+(scenario=="exit"?0:expectedEarned)};
            label="RESULT / original game end path";
            if(scenario=="exit")SceneManager.LoadScene("SelectStage");
            else if(scenario=="clear")enemyCastle.TakeDamage(1000000f,castle);
            else castle.TakeDamage(1000000f,enemyCastle);
            yield return new WaitForSecondsRealtime(.5f);
            row.walletAfter=PersistentCurrencyWallet.Minerals;row.resultOpened=StageResultUI.IsOpened;
            if(scenario!="exit")
            {
                var field=typeof(StageResultUI).GetField("mineralRewardText",BindingFlags.Instance|BindingFlags.NonPublic);
                var text=field.GetValue(result) as TMP_Text;row.resultText=text?text.text:"MISSING";
                Require(StageResultUI.IsOpened,"Original result UI did not open");
                Require(row.resultText==expectedEarned.ToString(),"Result UI must show earned minerals");
            }
            if(scenario=="repeat-fail")result.ShowFail();
            row.walletAfterSecondCall=PersistentCurrencyWallet.Minerals;row.savedMinerals=PlayerPrefs.GetInt("STAGE_MINERAL",-1);
            info="Earned "+row.earnedBefore+" | Expected wallet "+row.expectedMinerals+" | Actual "+row.walletAfter;
            if(capture&&repetition==1){yield return new WaitForEndOfFrame();Shot(scenario+"-result");}
            if(scenario!="exit")result.GoToStageSelect();
            yield return new WaitForSecondsRealtime(.6f);
            row.sceneAfter=SceneManager.GetActiveScene().name;row.afterReturn=PersistentCurrencyWallet.Minerals;row.unbankedAfterReturn=StageRewardSystem.EarnedMinerals;
            row.status=row.walletAfter==row.expectedMinerals&&row.walletAfterSecondCall==row.expectedMinerals&&row.savedMinerals==row.expectedMinerals&&row.afterReturn==row.expectedMinerals?"PASS":"FAIL";
            report.cases.Add(row);Debug.Log("REWARD_QA_CASE "+JsonUtility.ToJson(row));
            label="RETURN TO STAGE SELECT / "+row.status;
            if(capture&&repetition==1&&scenario=="fail"){yield return new WaitForEndOfFrame();Shot("fail-wallet");}
            Save();
        }
        Save();Application.Quit(0);
    }
    private void Shot(string name)
    {
        var image=SceneCapture.Capture();Require(image,"Screenshot unavailable");
        File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());Object.Destroy(image);
    }
    private void Save(){report.finishedAt=DateTime.UtcNow.ToString("o");File.WriteAllText(Path.Combine(output,phase+".json"),JsonUtility.ToJson(report,true));}
    private void OnGUI()
    {
        GUI.depth=-100;GUI.color=new Color(0.03f,.06f,.12f,.96f);GUI.DrawTexture(new Rect(20,20,1080,138),Texture2D.whiteTexture);GUI.color=Color.white;
        var style=new GUIStyle(GUI.skin.label){fontSize=23,fontStyle=FontStyle.Bold};
        GUI.Label(new Rect(36,28,1050,32),"QA PROBE | "+report.variant+" | "+activeCase,style);
        GUI.Label(new Rect(36,64,1050,32),label,style);GUI.Label(new Rect(36,100,1050,42),info,style);
    }
}
