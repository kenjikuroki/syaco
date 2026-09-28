using System;
using System.Collections.Generic;
namespace MantisPunch {
public static class RewardAdLedger {
 public static void Refresh(MissionProfile p,DateTime utc){string day=utc.ToString("yyyy-MM-dd");if(string.CompareOrdinal(day,p.rewardAdDay)>0){p.rewardAdDay=day;p.rewardAdCount=0;}}
 public static int Remaining(MissionProfile p,DateTime utc){Refresh(p,utc);return Math.Max(0,RewardAdSettings.DailyLimit-p.rewardAdCount);}
 public static bool Grant(MissionProfile p,string receipt,DateTime utc){if(string.IsNullOrEmpty(receipt)||receipt.Length>64)return false;var receipts=new List<string>(p.rewardAdReceipts??new string[0]);if(receipts.Contains(receipt)||Remaining(p,utc)==0)return false;receipts.Add(receipt);if(receipts.Count>32)receipts.RemoveAt(0);p.rewardAdReceipts=receipts.ToArray();p.rewardAdCount++;p.shells+=RewardAdSettings.Amount;return true;}
}
}
