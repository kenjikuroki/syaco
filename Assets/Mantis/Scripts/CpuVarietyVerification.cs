using UnityEngine;
using System.Collections;
using System.IO;
namespace MantisPunch {
public sealed partial class MantisDuel {
 IEnumerator VerifyCpuVariety(){
  Testing=true;yield return new WaitForSecondsRealtime(.8f);Directory.CreateDirectory("QA/CpuVariety");bool ok=true;string report="";var oldRandom=Random.state;Random.InitState(9173);
  var combinations=new bool[7,8];for(int i=0;i<2000;i++){var identity=DrawOpponentIdentity();combinations[(int)identity.personality,identity.palette]=true;}for(int a=0;a<7;a++)for(int b=0;b<8;b++)ok&=combinations[a,b];
  int palette=OpponentPalette;var personality=OpponentPersonality;NextRound();ok&=OpponentPalette==palette&&OpponentPersonality==personality;
  ok&=CpuAdaptation.Choose(1,1,4,0)==CpuPersonality.Feint&&CpuAdaptation.Choose(1,4,1,0)==CpuPersonality.Aggressive&&CpuAdaptation.Choose(4,1,1,0)==CpuPersonality.Parry&&CpuAdaptation.Choose(1,1,1,7)==CpuPersonality.Pressure;
  for(int i=0;i<7;i++){
   brain.BeginMatch((CpuPersonality)i);Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(new Vector3(0,0,2.45f),Quaternion.Euler(0,180,0));brain.ResetRound(Player.Actions);
   int attacks=0,feints=0,parries=0,changes=0;float retreat=0;var last=brain.Strategy;
   for(int tick=0;tick<1800;tick++){
    const float dt=1f/60;if(tick%90==0&&Player.Available)Player.Begin(DuelAction.Parry);
    brain.Decide(dt,Enemy,Player,out var movement,out var action);
    if(action==DuelAction.Attack)attacks++;if(action==DuelAction.Feint)feints++;if(action==DuelAction.Parry)parries++;
    var away=(Enemy.transform.position-Player.transform.position).normalized;if(Vector2.Dot(movement,new Vector2(away.x,away.z))>0)retreat+=dt;
    Enemy.Begin(action);Player.Step(dt,Vector2.zero,Enemy);Enemy.Step(dt,movement,Player);
    // Isolated decision exercise: consume impacts so the run lasts beyond a one-hit knockout.
    Player.SpendStrike();Enemy.SpendStrike();
    if(last!=brain.Strategy){changes++;last=brain.Strategy;}
   }
   bool active=attacks+feints>0;ok&=active;if(i==6)ok&=changes>0;report+=((CpuPersonality)i)+": attacks="+attacks+", feints="+feints+", parries="+parries+", retreatSeconds="+retreat.ToString("F1")+", strategyChanges="+changes+"\n";
  }
  int[] reactionParries=new int[2];
  for(int style=0;style<2;style++){
   Random.InitState(442);brain.BeginMatch(style==0?CpuPersonality.Balanced:CpuPersonality.Parry);
   for(int attempt=0;attempt<200;attempt++){
    Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(new Vector3(0,0,2),Quaternion.Euler(0,180,0));brain.ResetRound(Player.Actions);Player.Begin(DuelAction.Attack);
    for(int tick=0;tick<30;tick++){brain.Decide(1f/60,Enemy,Player,out var movement,out var action);if(action==DuelAction.Parry){reactionParries[style]++;break;}Player.Step(1f/60,Vector2.zero,Enemy);Player.SpendStrike();}
   }
  }
  ok&=reactionParries[1]>reactionParries[0];report+="Reactive parries / 200: balanced="+reactionParries[0]+", parry type="+reactionParries[1]+"\n";
  Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(new Vector3(0,0,2.2f),Quaternion.Euler(0,180,0));
  for(int i=0;i<8;i++){
   var material=Resources.Load<Material>("CpuPalettes/Palette"+i);ok&=material&&material.mainTexture;
   if(material)foreach(var r in cpu.visual.GetComponentsInChildren<SkinnedMeshRenderer>())r.sharedMaterial=material;
   yield return null;ReviewCapture.Save(Path.GetFullPath("QA/CpuVariety/palette-"+i+".png"),Screen.width,Screen.height);
  }
  Random.state=oldRandom;report+=(ok?"PASS":"FAIL")+": independent palette/personality combinations; round identity stable; adaptive mapping; all styles act; adaptive changes after observed parries; eight material assets.";File.WriteAllText("QA/CpuVariety/report.txt",report);Application.Quit(ok?0:1);
 }
}
}
