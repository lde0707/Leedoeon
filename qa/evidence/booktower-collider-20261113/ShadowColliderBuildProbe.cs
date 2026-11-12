using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using BookTower.Interaction;
using BookTower.Puzzles;
using BookTower.Rooms;
using BookTower.PlayerSystem;
using BookTower.CameraSystem;
using BookTower.UI;
public sealed class ShadowColliderBuildProbe:MonoBehaviour {
 StringBuilder report=new StringBuilder();int failures;string folder;
 PlayerController player;StorybookLightPuzzle puzzle;Camera cam;Mouse mouse;Keyboard keys;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 static void Storage(){if(Environment.GetCommandLineArgs().Contains("-colliderQA"))PaperSettingsRuntime.TestStoragePath=Path.GetFullPath("collider/settings.json");}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 public static void Boot(){if(Environment.GetCommandLineArgs().Contains("-colliderQA"))new GameObject("Collider regression").AddComponent<ShadowColliderBuildProbe>();}
 void Check(bool ok,string label){report.AppendLine((ok?"PASS ":"FAIL ")+label);if(!ok)failures++;File.WriteAllText(folder+"/checks.txt",report.ToString());}
 void Log(string text){report.AppendLine("DATA "+text);File.WriteAllText(folder+"/checks.txt",report.ToString());}
 void Place(Vector3 p){player.Controller.enabled=false;player.transform.position=p;player.Controller.enabled=true;Physics.SyncTransforms();}
 IEnumerator Start(){
 folder=Path.GetFullPath("collider");Directory.CreateDirectory(folder);Application.runInBackground=true;
 yield return new WaitForSecondsRealtime(1);
 var story=FindFirstObjectByType<BookTower.Story.OpeningStoryDirector>();if(story){if(story.companion)story.companion.transform.SetParent(null,true);story.StopAllCoroutines();story.enabled=false;}
 PaperUI.Instance.EndStory();PaperUI.Instance.Resume();BookTower.Core.GameFlow.EnsureExists().Resume();Time.timeScale=1;
 var manager=FindFirstObjectByType<RoomManager>();manager.enabled=false;
 puzzle=FindObjectsByType<StorybookLightPuzzle>(FindObjectsSortMode.None).First(p=>p.realShadows);
 var room=puzzle.GetComponentInParent<Room>();foreach(var r in FindObjectsByType<Room>(FindObjectsSortMode.None))r.SetVisible(r==room);
 player=FindFirstObjectByType<PlayerController>();player.enabled=false;cam=Camera.main;cam.GetComponent<PaperCamera>().enabled=false;
 var settings=Instantiate(InputSystem.settings);settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings=settings;
 mouse=InputSystem.AddDevice<Mouse>();keys=InputSystem.AddDevice<Keyboard>();mouse.MakeCurrent();keys.MakeCurrent();
 var drag=player.GetComponent<PaperShadowDrag>();var collisions=player.gameObject.AddComponent<CollisionContacts>();
 puzzle.SelectLight(2);yield return new WaitForSecondsRealtime(.3f);
 foreach(int index in new[]{1,3}){
  var part=index==1?puzzle.ears:puzzle.Duet.foxTail;var handle=part.GetComponent<PaperShadowHandle>();
  foreach(int flip in new[]{0,1}){
   bool desired=flip==1;bool current=index==1?puzzle.Flipped:puzzle.Duet.TailFlipped;if(current!=desired)puzzle.TurnPiece(index);
   puzzle.DragPiece(index,new Vector3(index==1?2:-2,1,6));yield return new WaitForSeconds(.8f);Physics.SyncTransforms();
   var col=part.GetComponents<Collider>().First(c=>c.enabled);var box=col as BoxCollider;
   Check(!(box && part.lossyScale.x<0),"C01 supported collider under mirror piece="+index+" flip="+flip);
   Log("piece="+index+" flip="+flip+" type="+col.GetType().Name+" scale="+part.lossyScale.ToString("F3")+" bounds="+col.bounds);
   int hits=0;foreach(var direction in new[]{puzzle.stage.right,-puzzle.stage.right,puzzle.stage.forward,-puzzle.stage.forward,Vector3.up,-Vector3.up}){
    var ray=new Ray(col.bounds.center+direction*5,-direction);if(col.Raycast(ray,out var hit,10))hits++;
   }
   Check(hits==6,"C02 six face physics rays piece="+index+" flip="+flip+" hits="+hits);
   var centre=col.bounds.center;
   Place(centre-puzzle.stage.forward*4-puzzle.stage.up);
   cam.transform.position=centre-puzzle.stage.forward*4+puzzle.stage.up*.3f;cam.transform.LookAt(centre);Physics.SyncTransforms();yield return new WaitForSeconds(.25f);
   Check(PaperShadowDrag.Aimed==handle,"C03 centre aim resolves handle piece="+index+" flip="+flip);
   InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1});yield return new WaitForSeconds(.15f);
   Check(drag.Held==handle,"C04 left click grabs piece="+index+" flip="+flip);
   var before=puzzle.PiecePosition(index);InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1,delta=new Vector2(35,30)});yield return new WaitForSeconds(.2f);
   var after=puzzle.PiecePosition(index);Check(Mathf.Abs(after.x-before.x)>.01f&&Mathf.Abs(after.z-before.z)>.01f,"C05 pointer delta moves both axes piece="+index+" flip="+flip+" delta="+(after-before).ToString("F3"));
   bool oldFlip=index==1?puzzle.Flipped:puzzle.Duet.TailFlipped;InputSystem.QueueStateEvent(mouse,new MouseState{buttons=3});yield return new WaitForSeconds(.2f);
   Check((index==1?puzzle.Flipped:puzzle.Duet.TailFlipped)!=oldFlip&&drag.Held==handle,"C06 right click flips held piece="+index+" flip="+flip);
   InputSystem.QueueStateEvent(mouse,new MouseState());yield return new WaitForSeconds(.2f);Check(!PaperShadowDrag.Holding,"C07 release ends drag piece="+index+" flip="+flip);
   // Reset orientation for the collision comparison; movement uses the real character capsule.
   current=index==1?puzzle.Flipped:puzzle.Duet.TailFlipped;if(current!=desired)puzzle.TurnPiece(index);
   yield return new WaitForSeconds(.2f);Physics.SyncTransforms();centre=col.bounds.center;
   var from=centre-puzzle.stage.forward*3-player.Controller.center;
   Place(from);collisions.Target=col;collisions.Count=0;
   for(int step=0;step<80;step++)player.Controller.Move(puzzle.stage.forward*.075f);
   Check(collisions.Count>0,"C08 character capsule contacts piece="+index+" flip="+flip+" contacts="+collisions.Count);
   Place(centre-puzzle.stage.forward*4-puzzle.stage.up);cam.transform.position=centre-puzzle.stage.forward*4+Vector3.up*.3f;cam.transform.LookAt(centre);yield return new WaitForSeconds(.2f);Capture("piece-"+index+"-"+flip);
  }
 }
 Check(!puzzle.Complete,"C09 off-target input never completes puzzle");
 // Freshly manipulated parts still satisfy the existing projection puzzle.
 puzzle.SelectLight(0);puzzle.DragPiece(2,ShadowDuet.FoxBodySolution);puzzle.DragPiece(3,ShadowDuet.FoxTailSolution);if(!puzzle.Duet.TailFlipped)puzzle.TurnPiece(3);yield return new WaitForSeconds(3);
 Check(puzzle.Duet.FoxRecorded&&!puzzle.Complete,"C10 fox records after input and collision checks");
 puzzle.SelectLight(1);puzzle.DragPiece(0,new Vector3(-1.8f,1,12.5f/1.5f));puzzle.DragPiece(1,new Vector3(-.9f,1,12.5f/3));if(!puzzle.Flipped)puzzle.TurnPiece(1);yield return new WaitForSeconds(3);
 Check(puzzle.Duet.AllRecorded&&puzzle.Complete,"C11 both records complete after mirrored collider interaction");
 report.AppendLine("RESULT: "+(failures==0?"PASS":"FAIL")+"; failures="+failures);File.WriteAllText(folder+"/checks.txt",report.ToString());InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(keys);Application.Quit(failures==0?0:1);
 }
 void Capture(string name){var target=cam.transform.position+cam.transform.forward*4;cam.transform.position=target+puzzle.stage.right*4-puzzle.stage.forward*6+Vector3.up*3;cam.transform.LookAt(target);var rt=RenderTexture.GetTemporary(1600,900,24);var old=cam.targetTexture;var active=RenderTexture.active;float aspect=cam.aspect;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);try{cam.targetTexture=rt;cam.aspect=16f/9;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(folder+"/"+name+".png",tex.EncodeToPNG());}finally{cam.targetTexture=old;cam.aspect=aspect;RenderTexture.active=active;Destroy(tex);RenderTexture.ReleaseTemporary(rt);}}
}
public sealed class CollisionContacts:MonoBehaviour{public Collider Target;public int Count;void OnControllerColliderHit(ControllerColliderHit hit){if(hit.collider==Target)Count++;}}
