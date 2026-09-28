using System;
using UnityEngine;
namespace MantisPunch {
// Ranked results must eventually come from an authenticated server, never CPU simulation.
[Serializable] public sealed class RankingProfile {
 public int cpuWins,cpuLosses;
 public void RecordCpu(bool won){if(won)cpuWins++;else cpuLosses++;}
}
public static class RankingStore {
 const string Key="Mantis.Ranking.v1";static RankingProfile profile;static bool isolated;
 public static string Season(DateTime utc)=>utc.ToString("yyyy-MM");
 public static DateTime SeasonEnd(DateTime utc)=>new DateTime(utc.Year,utc.Month,1,0,0,0,DateTimeKind.Utc).AddMonths(1);
 public static void BeginVerification(){isolated=true;profile=new RankingProfile();}
 public static RankingProfile Get(){if(profile==null){try{profile=JsonUtility.FromJson<RankingProfile>(PlayerPrefs.GetString(Key,""));}catch{profile=null;}if(profile==null)profile=new RankingProfile();}return profile;}
 public static void RecordCpuMatch(bool won){Get().RecordCpu(won);if(!isolated){PlayerPrefs.SetString(Key,JsonUtility.ToJson(profile));PlayerPrefs.Save();}}
}
}
