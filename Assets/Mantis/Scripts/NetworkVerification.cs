using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace MantisPunch {
public sealed class NetworkVerification:MonoBehaviour {
 IEnumerator Start(){
  bool host=Array.IndexOf(Environment.GetCommandLineArgs(),"-networkHostTest")>=0;var net=OnlineMatch.Instance;CharacterProfile.Save(host?"Host Tester":"Guest Tester");Directory.CreateDirectory("QA/Online");string path="QA/Online/"+(host?"host":"client")+".txt";
  net.StartDirect(host);float deadline=Time.realtimeSinceStartup+30;
  while(!net.GameReady&&Time.realtimeSinceStartup<deadline)yield return null;
  if(!net.GameReady){File.WriteAllText(path,"FAIL: handshake timeout "+net.Status);Application.Quit(1);yield break;}
  var duel=FindFirstObjectByType<MantisDuel>();bool ok=net.Remote.name==(host?"Guest Tester":"Host Tester")&&!MatchRules.CanRematch(MatchKind.Public,true)&&MatchRules.CanRematch(MatchKind.Friend,true)&&!MatchRules.Ranked(MatchKind.Friend);
  if(host){
   yield return new WaitForSecondsRealtime(2);duel.Testing=true;duel.Enemy.Begin(DuelAction.Attack);duel.Advance(.1f,Vector2.zero,Vector2.zero);yield return new WaitForSecondsRealtime(1);
   duel.playerWins=2;typeof(DuelFighter).GetProperty("State").SetValue(duel.Enemy,DuelState.Dead);duel.Enemy.presentation.React(CombatBeat.Knockout,Vector3.forward);duel.Advance(.01f,Vector2.zero,Vector2.zero);typeof(MantisDuel).GetField("resultAge",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(duel,4f);yield return new WaitForSecondsRealtime(1);ok&=duel.playerWins==3;net.VoteRematch();
   deadline=Time.realtimeSinceStartup+12;while(duel.MatchOver&&Time.realtimeSinceStartup<deadline)yield return null;ok&=!duel.MatchOver&&duel.playerWins==0&&duel.cpuWins==0;
  }else{
   deadline=Time.realtimeSinceStartup+12;bool attackSeen=false;while(!duel.MatchOver&&Time.realtimeSinceStartup<deadline){attackSeen|=duel.Player.LastAction==DuelAction.Attack;yield return null;}ok&=attackSeen&&duel.cpuWins==3&&duel.playerWins==0;
   ReviewCapture.Save(Path.GetFullPath("QA/Online/client-result.png"),Screen.width,Screen.height);net.VoteRematch();deadline=Time.realtimeSinceStartup+12;while(duel.MatchOver&&Time.realtimeSinceStartup<deadline)yield return null;ok&=!duel.MatchOver&&duel.playerWins==0&&duel.cpuWins==0;
  }
  File.WriteAllText(path,ok?"PASS: two-process connection, names, state/score perspective, bilateral friend rematch, public rematch disabled":"FAIL: network assertions");yield return new WaitForSecondsRealtime(1);Application.Quit(ok?0:1);
 }
}
}
