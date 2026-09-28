using System.Collections;
using UnityEngine;
namespace MantisPunch {
public sealed class MissionCombatVerification:MonoBehaviour {
 IEnumerator Start(){if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-missionTest")<0)yield break;yield return null;var duel=GetComponent<MantisDuel>();duel.Testing=true;
  duel.Restart();Position(duel);duel.Player.Begin(DuelAction.Parry);duel.Enemy.Begin(DuelAction.Attack);Advance(duel);bool ok=MissionStore.Get().daily[2]==1;
  duel.NextRound();Position(duel);duel.Enemy.Begin(DuelAction.Feint);duel.Player.Begin(DuelAction.Attack);Advance(duel);ok&=MissionStore.Get().daily[0]==1&&MissionStore.Get().daily[1]==1;Advance(duel);ok&=MissionStore.Get().daily[0]==1;
  duel.StartDemo(2);Position(duel);duel.Player.Begin(DuelAction.Parry);duel.Enemy.Begin(DuelAction.Attack);Advance(duel);ok&=MissionStore.Get().daily[2]==1;
  MissionStore.BeginVerification();duel.Restart();
  for(int r=0;r<3;r++){if(r>0)duel.NextRound();Position(duel);duel.Enemy.Begin(DuelAction.Feint);duel.Player.Begin(DuelAction.Attack);Advance(duel);ok&=MissionStore.Get().shells==(r==2?50:0);}
  Advance(duel);ok&=MissionStore.Get().shells==50&&duel.MatchShellReward==50;
  duel.Restart();ok&=duel.MatchShellReward==0;
  for(int r=0;r<3;r++){if(r>0)duel.NextRound();Position(duel);duel.Player.Begin(DuelAction.Feint);duel.Enemy.Begin(DuelAction.Attack);Advance(duel);}
  ok&=MissionStore.Get().shells==55&&duel.MatchShellReward==5;Advance(duel);ok&=MissionStore.Get().shells==55;
  duel.Restart();typeof(MantisDuel).GetProperty("Tutorial").SetValue(duel,true);Position(duel);duel.Enemy.Begin(DuelAction.Feint);duel.Player.Begin(DuelAction.Attack);Advance(duel);ok&=MissionStore.Get().shells==55;typeof(MantisDuel).GetProperty("Tutorial").SetValue(duel,false);
  System.IO.File.AppendAllText("QA/MissionsVerified/report.txt",ok?"\nPASS: actual parry/round progress; first-to-three payout once; loss reward; restart reset; demo/tutorial excluded; isolated data":"\nFAIL: combat progress integration");yield return new WaitForSecondsRealtime(.3f);Application.Quit(ok?0:1);
 }
 void Position(MantisDuel d){d.Player.ResetRound(Vector3.zero,Quaternion.identity);d.Enemy.ResetRound(new Vector3(0,0,2),Quaternion.Euler(0,180,0));}
 void Advance(MantisDuel d){d.Testing=false;for(int i=0;i<35;i++)d.Advance(.01f,Vector2.zero,Vector2.zero);d.Testing=true;}
}
}


