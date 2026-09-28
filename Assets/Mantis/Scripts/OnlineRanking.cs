using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.CloudCode;
using UnityEngine;
namespace MantisPunch {
public static class OnlineRanking {
 public static readonly string[] BoardIds={"shako_monthly_rating","shako_monthly_streak"};
 public static List<LeaderboardEntry> Entries=new List<LeaderboardEntry>();public static LeaderboardEntry Mine;
 public static string Status="";public static bool Loading;static int request;
 public static string ResultStatus="";
 [Serializable] public sealed class ResultReply {public string status;}
 public static async Task Load(int tab){
  int version=++request;Loading=true;Entries=new List<LeaderboardEntry>();Mine=null;Status=MantisDuel.Tr("読み込み中…","Loading…");
  try{await OnlineServices.SignIn();string id=BoardIds[Mathf.Clamp(tab,0,1)];var scores=await LeaderboardsService.Instance.GetScoresAsync(id,new GetScoresOptions{Offset=0,Limit=10});LeaderboardEntry mine=null;
   try{mine=await LeaderboardsService.Instance.GetPlayerScoreAsync(id);}catch(LeaderboardsException ex)when(ex.Reason==LeaderboardsExceptionReason.EntryNotFound){}
   if(version!=request)return;Entries=scores.Results;Mine=mine;Status=Entries.Count==0?MantisDuel.Tr("まだ記録がありません。","No scores yet."):"";
  }catch(Exception ex){if(version==request)Status=MantisDuel.Tr("ランキングに接続できません。","Could not load the leaderboard.");Debug.LogWarning("Leaderboard: "+ex.GetType().Name);}
  finally{if(version==request)Loading=false;}
 }
 public static async void Submit(OnlineMatch match,int own,int opponent){
  if(!MatchRules.Ranked(match.Kind)||match.Direct)return;
  string session=match.SessionId,id=match.MatchId,other=match.Remote.id;
  ResultStatus=MantisDuel.Tr("ランキングを更新中…","Updating ranking…");
  try{await OnlineServices.SignIn();for(int attempt=0;attempt<5;attempt++){var reply=await CloudCodeService.Instance.CallEndpointAsync<ResultReply>("ShakoSubmitResult",new Dictionary<string,object>{{"sessionId",session},{"matchId",id},{"opponentId",other},{"wins",own},{"losses",opponent}});if(reply?.status=="confirmed"){ResultStatus=MantisDuel.Tr("ランキング更新済み","Ranking updated");return;}await Task.Delay(8000);}ResultStatus=MantisDuel.Tr("対戦結果の確認待ちです。","Result confirmation pending.");}
  catch(Exception ex){ResultStatus=MantisDuel.Tr("ランキングを更新できませんでした。","Ranking update unavailable.");Debug.LogWarning("Rank result was not confirmed: "+ex.GetType().Name);}
 }
}
}
