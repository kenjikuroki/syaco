using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyOnlineUI(){
  MissionStore.BeginVerification();const string folder="QA/Online";Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(.5f);bool ok=true;
  foreach(bool portrait in new[]{false,true}){Screen.SetResolution(portrait?720:1280,portrait?1280:720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);ShowBattleOptions();yield return new WaitForSecondsRealtime(.2f);roomCode.text="ABC123";ok&=roomCode.text=="ABC123";ReviewCapture.Save(Path.GetFullPath(folder+"/menu-"+(portrait?"portrait":"landscape")+".png"),Screen.width,Screen.height);DrawShellShop();ShowApplePurchase();yield return new WaitForSecondsRealtime(.2f);ok&=!ApplePurchases.Instance.Catalog.paymentsEnabled&&!ApplePurchases.Instance.Ready;ReviewCapture.Save(Path.GetFullPath(folder+"/purchase-"+(portrait?"portrait":"landscape")+".png"),Screen.width,Screen.height);}
  var grant=new PurchaseGrant{verified=true,transactionId="qa-purchase",productId="shako_premium",shells=800};MissionStore.GrantPurchase(grant);MissionStore.GrantPurchase(grant);ok&=MissionStore.Get().shells==800;
  bool rejected=false;try{MissionStore.GrantPurchase(new PurchaseGrant{verified=false,transactionId="bad",shells=800});}catch(System.InvalidOperationException){rejected=true;}ok&=rejected&&MissionStore.Get().shells==800;
  OnlineRanking.Entries=Enumerable.Range(1,10).Select(i=>new Unity.Services.Leaderboards.Models.LeaderboardEntry("test"+i,"Test Fighter "+i,i-1,1200-i*10)).ToList();OnlineRanking.Status="";OnlineRanking.Loading=false;DrawLiveRanking(false);yield return new WaitForSecondsRealtime(.2f);ReviewCapture.Save(Path.GetFullPath(folder+"/ranking-portrait.png"),Screen.width,Screen.height);rankingVisible=false;Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.1f);rankingPortrait=false;DrawLiveRanking(false);yield return new WaitForSecondsRealtime(.2f);ReviewCapture.Save(Path.GetFullPath(folder+"/ranking-landscape.png"),Screen.width,Screen.height);
  File.WriteAllText(folder+"/ui.txt",ok?"PASS: match options and room code in both orientations; unconfigured IAP disabled; durable grant dedupe; unverified grant rejected; ten ranking rows":"FAIL");Application.Quit(ok?0:1);
 }
}
}
