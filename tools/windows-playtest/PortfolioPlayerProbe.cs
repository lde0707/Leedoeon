#if PORTFOLIO_QA
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Explicit QA builds only. No gameplay commands or files are read in release builds.
[DefaultExecutionOrder(-31000)]
public sealed class PortfolioPlayerProbe : MonoBehaviour
{
    [Serializable] public class Command { public int id; public string op,type,member,name; public string[] keys; public float x,y; public bool down; public int button; }
    [Serializable] public class Reply { public int id,frame,width,height; public string value,error,scene; public float timeScale; }
    [Serializable] public class ButtonInfo { public string name,text; public bool enabled; public float x,y; }
    [Serializable] public class Buttons { public ButtonInfo[] items; }
    string folder; int last; float next; Keyboard keyboard; Mouse mouse;
    static readonly BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-playerQa");
        if(i<0||i+1>=args.Length)return;
        var go=new GameObject("Portfolio player input probe");DontDestroyOnLoad(go);
        var probe=go.AddComponent<PortfolioPlayerProbe>();probe.folder=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(probe.folder);
        Application.runInBackground=true;
        var settings=Instantiate(InputSystem.settings);settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;InputSystem.settings=settings;
        probe.keyboard=InputSystem.AddDevice<Keyboard>();probe.keyboard.MakeCurrent();
        probe.mouse=InputSystem.AddDevice<Mouse>();probe.mouse.MakeCurrent();
        Application.logMessageReceived+=(message,stack,type)=>{if(type==LogType.Error||type==LogType.Exception)File.AppendAllText(Path.Combine(probe.folder,"errors.txt"),message+"\n"+stack+"\n");};
    }
    void Update()
    {
        if(folder==null||Time.realtimeSinceStartup<next)return;next=Time.realtimeSinceStartup+.04f;
        var path=Path.Combine(folder,"command.json");if(!File.Exists(path))return;
        Command c;try{c=JsonUtility.FromJson<Command>(File.ReadAllText(path));}catch{return;}
        if(c==null||c.id<=last)return;last=c.id;
        var reply=new Reply{id=c.id,frame=Time.frameCount,width=Screen.width,height=Screen.height,timeScale=Time.timeScale,scene=SceneManager.GetActiveScene().name};
        try
        {
            switch(c.op)
            {
                case "keys":InputSystem.QueueStateEvent(keyboard,new KeyboardState((c.keys??Array.Empty<string>()).Select(k=>(Key)Enum.Parse(typeof(Key),k,true)).ToArray()));reply.value="queued";break;
                case "mouse":InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(c.x,Screen.height-c.y),buttons=(ushort)(c.down?(1<<c.button):0)});reply.value="queued";break;
                case "look":InputSystem.QueueStateEvent(mouse,new MouseState{position=mouse.position.ReadValue(),delta=new Vector2(c.x,c.y)});reply.value="queued";break;
                case "capture":ScreenCapture.CaptureScreenshot(Path.Combine(folder,c.name+".png"));reply.value="scheduled";break;
                case "objects":reply.value=string.Join("\n",FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(o=>o&& (string.IsNullOrEmpty(c.type)||o.GetType().FullName.Contains(c.type))).Select(o=>o.GetType().FullName+" | "+o.name));break;
                case "read":reply.value=Value(Read(c.type,c.member));break;
                case "members":var obj=Root(c.type);reply.value=string.Join("\n",obj.GetType().GetFields(Flags).Select(f=>"field "+f.Name+" = "+Safe(()=>f.GetValue(obj))).Concat(obj.GetType().GetProperties(Flags).Where(p=>p.GetIndexParameters().Length==0).Select(p=>"property "+p.Name+" = "+Safe(()=>p.GetValue(obj)))));break;
                case "buttons":reply.value=JsonUtility.ToJson(new Buttons{items=FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy).Select(b=>{var canvas=b.GetComponentInParent<Canvas>();var p=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:(canvas.worldCamera?canvas.worldCamera:Camera.main),((RectTransform)b.transform).TransformPoint(((RectTransform)b.transform).rect.center));return new ButtonInfo{name=b.name,text=string.Join(" ",b.GetComponentsInChildren<Text>().Select(t=>t.text)),enabled=b.interactable,x=p.x,y=Screen.height-p.y};}).ToArray()});break;
                case "quit":Application.Quit();break;
                default:reply.value="ready";break;
            }
        }catch(Exception ex){reply.error=ex.ToString();}
        File.WriteAllText(Path.Combine(folder,"reply.json"),JsonUtility.ToJson(reply,true));
    }
    static object Root(string type)
    { var o=FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).FirstOrDefault(x=>x&&x.GetType().FullName==type);if(o)return o;throw new Exception("Object not found: "+type); }
    static object Read(string type,string member)
    { object value=Root(type);foreach(string name in member.Split('.')){if(value==null)return null;var t=value.GetType();var f=t.GetField(name,Flags);if(f!=null)value=f.GetValue(value);else{var property=t.GetProperty(name,Flags);if(property==null)throw new Exception("Member not found: "+t+"."+name);value=property.GetValue(value);}}return value; }
    static string Safe(Func<object> get){try{return Value(get());}catch{return "<unavailable>";}}
    static string Value(object value){if(value==null)return "null";if(value is string||value.GetType().IsPrimitive||value.GetType().IsEnum)return value.ToString();if(value is UnityEngine.Object o)return o?o.GetType().FullName+" | "+o.name:"null";try{return JsonUtility.ToJson(value);}catch{return value.ToString();}}
}
#endif
