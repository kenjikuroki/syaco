using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MantisPunch {
public sealed class DModelVerification:MonoBehaviour {
 const string Folder="QA/DModelVerified";static int failures;
 void Check(bool ok,string text){File.AppendAllText(Folder+"/report.txt",(ok?"PASS: ":"FAIL: ")+text+"\n");if(!ok)failures++;}
 void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception)Check(false,message);}
 IEnumerator Shot(string name){yield return new WaitForEndOfFrame();ReviewCapture.Save(Path.GetFullPath(Folder+"/"+name+".png"));yield return null;}
 IEnumerator Start(){
  if(Array.IndexOf(Environment.GetCommandLineArgs(),"-dModelTest")<0)yield break;
  Directory.CreateDirectory(Folder);Application.logMessageReceived+=Log;yield return new WaitForSecondsRealtime(1);
  var home=GetComponent<MantisHome>();
  if(home){
   failures=0;File.WriteAllText(Folder+"/report.txt","");MissionStore.BeginVerification();
   var visual=home.model;var body=visual.GetComponent<MantisModularBody>();
   Check(visual.Ready&&body.parts.Length==6&&visual.leftStrike&&visual.rightStrike,"Home uses D rig with contact markers and six slots");
   var fins=visual.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("pleopod_")).ToArray();Check(fins.Length==10,"Ten independent swimmerets");
   Quaternion before=fins[2].localRotation;yield return new WaitForSecondsRealtime(.2f);Check(Quaternion.Angle(before,fins[2].localRotation)>.2f,"Swimmerets animate at rest");
   yield return Shot("01-home");var loadout=MantisLoadout.Apply(body,true);Check(loadout.WearingCrown&&body.parts[0].sharedMesh.subMeshCount==2,"New crown fits D head");
   yield return Shot("02-crown");loadout.SetCrown(false);Check(body.parts[0].sharedMesh.subMeshCount==1,"Unequip restores D head");
   home.enabled=false;visual.Sample(15f/41,0,0);Vector3 folded=visual.StrikePoint;visual.Sample(16f/41,0,0);Vector3 mid=visual.StrikePoint;visual.Sample(17f/41,0,0);Vector3 strike=visual.StrikePoint;
   Check(strike.y>mid.y&&Vector3.Distance(strike,folded)>.2f,"Punch extends through a lower arc");yield return Shot("03-home-punch");
   visual.Sample(15f/41,0,0);Check(Vector3.Distance(visual.StrikePoint,folded)<.001f,"Reverse sampling restores folded pose");
   Application.logMessageReceived-=Log;SceneManager.LoadScene("Duel");yield break;
  }
  var duel=GetComponent<MantisDuel>();duel.Testing=true;
  Check(duel.Player.visual.leftStrike&&duel.Enemy.visual.leftStrike,"Both combatants use D model");Check(duel.Player.visual.Ready&&duel.Enemy.visual.Ready,"Both animation graphs ready");yield return Shot("04-duel");
  duel.Player.ResetRound(Vector3.zero,Quaternion.identity);duel.Enemy.ResetRound(Vector3.forward*1.6f,Quaternion.Euler(0,180,0));
  duel.Player.Begin(DuelAction.Attack);duel.Advance(.245f,Vector2.zero,Vector2.zero);Check(duel.Enemy.Alive&&duel.Enemy.Guard<70,"Guard still blocks a D punch");yield return Shot("05-guard");
  duel.Restart();duel.Player.ResetRound(Vector3.zero,Quaternion.identity);duel.Enemy.ResetRound(Vector3.forward*1.6f,Quaternion.Euler(0,180,0));
  duel.Player.Begin(DuelAction.Parry);duel.Enemy.Begin(DuelAction.Attack);duel.Advance(.245f,Vector2.zero,Vector2.zero);Check(duel.Enemy.State==DuelState.Recovery&&duel.Player.SuccessfulParries>0,"Parry exposes attacker");yield return new WaitForSecondsRealtime(.1f);yield return Shot("06-parry");
  duel.Player.Begin(DuelAction.Attack);duel.Advance(.245f,Vector2.zero,Vector2.zero);Check(!duel.Enemy.Alive,"Counter still kills in one hit");yield return new WaitForSecondsRealtime(.45f);yield return Shot("07-knockout");
  duel.Restart();duel.Player.Begin(DuelAction.Feint);duel.Advance(.1f,Vector2.zero,Vector2.zero);Check(duel.Player.State==DuelState.Feint&&duel.Player.Vulnerable,"Feint retains vulnerable window");yield return Shot("08-feint");
  File.AppendAllText(Folder+"/report.txt","RESULT: "+(failures==0?"PASS":"FAIL")+"\n");Application.logMessageReceived-=Log;yield return new WaitForSecondsRealtime(.5f);Application.Quit(failures==0?0:1);
 }
 void OnDestroy(){Application.logMessageReceived-=Log;}
}
}
