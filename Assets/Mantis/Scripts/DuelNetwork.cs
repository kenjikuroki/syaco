using System;
using UnityEngine;
namespace MantisPunch {
[Serializable] public sealed class NetFighter {
 public Vector3 position,velocity,direction;public Quaternion rotation;public int state,action,actions,reaction,beat,parries;public float guard,age,gait,sinceGuard;public bool blocked;
}
[Serializable] public sealed class NetDuel {public NetFighter a,b;public int aWins,bWins;public float resultAge,ready;}
public sealed partial class DuelFighter {
 int receivedReaction;
 NetFighter remoteFrame;float receivedAt;
 public NetFighter CaptureNetwork()=>new NetFighter{position=transform.position,rotation=transform.rotation,velocity=velocity,state=(int)State,action=(int)LastAction,actions=Actions,guard=Guard,age=Age,gait=gait,sinceGuard=TimeSinceGuard,blocked=attackWasBlocked,reaction=presentation.NetworkReaction,beat=(int)presentation.NetworkBeat,direction=presentation.NetworkDirection,parries=SuccessfulParries};
 public void ApplyNetwork(NetFighter frame){
  if(frame==null||frame.state<0||frame.state>6)return;
  bool actionChanged=Actions!=frame.actions;
  remoteFrame=frame;receivedAt=Time.unscaledTime;body.enabled=false;if(Vector3.Distance(transform.position,frame.position)>1)transform.SetPositionAndRotation(frame.position,frame.rotation);
  State=(DuelState)frame.state;LastAction=(DuelAction)frame.action;Actions=frame.actions;Guard=Mathf.Clamp(frame.guard,0,100);Age=frame.age;gait=frame.gait;velocity=frame.velocity;TimeSinceGuard=frame.sinceGuard;attackWasBlocked=frame.blocked;SuccessfulParries=frame.parries;
  if(actionChanged)presentation.ActionStarted(LastAction);
  if(frame.reaction!=receivedReaction){receivedReaction=frame.reaction;presentation.React((CombatBeat)frame.beat,frame.direction);if((CombatBeat)frame.beat==CombatBeat.GuardBreak)GuardBreakAt=Time.unscaledTime;}
  Pose();
 }
 public void RenderNetwork(){if(remoteFrame==null)return;float blend=1-Mathf.Exp(-Time.unscaledDeltaTime*30);transform.SetPositionAndRotation(Vector3.Lerp(transform.position,remoteFrame.position,blend),Quaternion.Slerp(transform.rotation,remoteFrame.rotation,blend));Age=remoteFrame.age+Mathf.Min(.05f,Time.unscaledTime-receivedAt);gait=remoteFrame.gait+Mathf.Min(.05f,Time.unscaledTime-receivedAt);Pose();}
}
public sealed partial class MantisDuel {
 bool networkEnded;
 public void SetOnlineName(string value){OpponentName=value;Hud?.RefreshOpponentPortrait();}
 public void NetworkDisconnected(){networkEnded=true;controls.Reset();Hud?.ShowNetworkDisconnect();}
 public NetDuel CaptureNetwork()=>new NetDuel{a=Player.CaptureNetwork(),b=Enemy.CaptureNetwork(),aWins=playerWins,bWins=cpuWins,resultAge=resultAge,ready=readyLeft};
 public void ApplyNetwork(NetDuel frame){
  if(frame==null||frame.a==null||frame.b==null)return;
  bool reset=RoundOver&&frame.a.state!=(int)DuelState.Dead&&frame.b.state!=(int)DuelState.Dead;
  if(reset){Player.presentation.ResetPose();Enemy.presentation.ResetPose();effects.Clear();}
  bool ownParry=frame.b.parries>Player.SuccessfulParries,otherParry=frame.a.parries>Enemy.SuccessfulParries;
  Player.ApplyNetwork(frame.b);Enemy.ApplyNetwork(frame.a);if(ownParry)effects.FocusParry(Player,Enemy);if(otherParry)effects.FocusParry(Enemy,Player);playerWins=frame.bWins;cpuWins=frame.aWins;resultAge=frame.resultAge;readyLeft=frame.ready;
  if(MatchOver&&!rankingMatchRecorded){rankingMatchRecorded=true;RecordOnlineResult();}
 }
 void TickNetwork(){
  var net=OnlineMatch.Current;if(!net.GameReady)return;
  var input=Hud&&Hud.OnlineOverlay?DuelAction.None:Poll();if(Hud&&Hud.OnlineOverlay)controls.Reset();Vector3 forward=arenaCamera.transform.forward;forward.y=0;forward.Normalize();Vector3 right=arenaCamera.transform.right;right.y=0;right.Normalize();Vector3 world=forward*controls.Move.y+right*controls.Move.x;
  if(!net.Host){net.Input(new Vector2(world.x,world.z),input);Player.RenderNetwork();Enemy.RenderNetwork();return;}
  if(RoundOver){Score();resultAge+=Time.deltaTime;if(!MatchOver&&resultAge>=1.65f)NextRound();return;}
  if(readyLeft>0){readyLeft=Mathf.Max(0,readyLeft-Time.deltaTime);net.TakeAction();controls.Reset();return;}
  if(effects.HitStop>0)return;
  Player.Begin(input);Enemy.Begin(net.TakeAction());
  Advance(Mathf.Min(Time.deltaTime,.05f)*effects.CombatSpeed,new Vector2(world.x,world.z),net.RemoteMove);
 }
 void RecordOnlineResult(){
  var net=OnlineMatch.Current;
  if(net.Kind==MatchKind.Public){MatchShellReward=MissionStore.CompleteMatch(playerWins>=3);OnlineRanking.Submit(net,playerWins,cpuWins);}
  // Private rematches award no currency or rating, avoiding unlimited friend farming.
 }
}
}
