using System;
using UnityEngine;
namespace MantisPunch {
[Serializable] public sealed class MissionProfile {
 public string day="",week="";public int shells;public bool crownOwned,crownEquipped;public int[] daily=new int[3],weekly=new int[3];public bool[] dailyClaimed=new bool[3],weeklyClaimed=new bool[3];public bool loginClaimed;
 public string rewardAdDay="";public int rewardAdCount;public string[] rewardAdReceipts=new string[0];
 public string[] purchaseReceipts=new string[0];public bool premium;
 public bool firstWinClaimed;public int purchasedEquipment;
 public bool BuyCrown()=>BuyEquipment(0);
 public bool BuyEquipment(int item){int price=ShellEconomy.Price(item);if(price<0||shells<price||(purchasedEquipment&(1<<item))!=0||(item==0&&crownOwned))return false;shells-=price;purchasedEquipment|=1<<item;if(item==0)crownOwned=true;return true;}
 public int CompleteMatch(bool won,DateTime date){Refresh(date);int reward=won?ShellEconomy.MatchWin:ShellEconomy.MatchLoss;if(won&&!firstWinClaimed){firstWinClaimed=true;reward+=ShellEconomy.FirstWin;}shells+=reward;return reward;}
 public void Refresh(DateTime date){string d=date.ToString("yyyy-MM-dd"),w=date.AddDays(-((int)date.DayOfWeek+6)%7).ToString("yyyy-MM-dd");if(string.CompareOrdinal(d,day)>0){day=d;daily=new int[3];dailyClaimed=new bool[3];loginClaimed=false;firstWinClaimed=false;}if(string.CompareOrdinal(w,week)>0){week=w;weekly=new int[3];weeklyClaimed=new bool[3];}}
 public void Record(int kind,int amount,DateTime date){Refresh(date);daily[kind]=Math.Min(100000,daily[kind]+amount);weekly[kind]=Math.Min(100000,weekly[kind]+amount);}
 public bool Claim(int tab,int id,DateTime date){Refresh(date);if(tab==2){if(loginClaimed)return false;loginClaimed=true;shells+=ShellEconomy.Login;return true;}var progress=tab==0?daily:weekly;var claims=tab==0?dailyClaimed:weeklyClaimed;if(claims[id]||progress[id]<MissionStore.Target(tab,id))return false;claims[id]=true;shells+=MissionStore.Reward(tab,id);return true;}
}
public static class MissionStore {
 const string Key="Mantis.Missions.v1";static MissionProfile profile;static bool isolated; public static void BeginVerification(){isolated=true;profile=new MissionProfile();}
 public static int AdsRemaining=>RewardAdLedger.Remaining(Get(),DateTime.UtcNow);
 public static bool GrantAdReward(string receipt){bool granted=RewardAdLedger.Grant(Get(),receipt,DateTime.UtcNow);if(granted)Save();return granted;}
 public static void GrantPurchase(PurchaseGrant grant){var p=Get();if(!grant.verified||string.IsNullOrEmpty(grant.transactionId))throw new InvalidOperationException("Unverified purchase");if(Array.IndexOf(p.purchaseReceipts??new string[0],grant.transactionId)>=0)return;var receipts=new System.Collections.Generic.List<string>(p.purchaseReceipts??new string[0]);receipts.Add(grant.transactionId);p.shells=checked(p.shells+grant.shells);p.premium|=grant.premium;p.purchaseReceipts=receipts.ToArray();Save();}
 public static bool BuyCrown(){bool bought=Get().BuyCrown();if(bought)Save();return bought;}
 public static bool EquipCrown(bool equip){if(equip&&!Get().crownOwned)return false;Get().crownEquipped=equip;Save();return true;}
 public static int Target(int tab,int id)=>tab==0?new[]{3,1,2}[id]:new[]{20,5,10}[id];
 public static int Reward(int tab,int id)=>tab==0?ShellEconomy.Daily(id):ShellEconomy.Weekly(id);
 public static bool BuyEquipment(int item){bool bought=Get().BuyEquipment(item);if(bought)Save();return bought;}
 public static int CompleteMatch(bool won){int reward=Get().CompleteMatch(won,DateTime.Now);Save();return reward;}
 public static MissionProfile Get(){if(profile==null){try{profile=JsonUtility.FromJson<MissionProfile>(PlayerPrefs.GetString(Key,""));}catch{profile=null;}if(profile==null)profile=new MissionProfile();}string old=profile.day+profile.week;profile.Refresh(DateTime.Now);if(old!=profile.day+profile.week)Save();return profile;}
 static void Save(){if(isolated)return;PlayerPrefs.SetString(Key,JsonUtility.ToJson(profile));PlayerPrefs.Save();}
 public static void Record(int kind,int amount=1){Get().Record(kind,amount,DateTime.Now);Save();}
 public static bool Claim(int tab,int id){bool result=Get().Claim(tab,id,DateTime.Now);if(result)Save();return result;}
 public static int Ready(int tab){var p=Get();if(tab==2)return p.loginClaimed?0:1;int n=0;for(int i=0;i<3;i++)if(!(tab==0?p.dailyClaimed[i]:p.weeklyClaimed[i])&&(tab==0?p.daily[i]:p.weekly[i])>=Target(tab,i))n++;return n;}
}
}




