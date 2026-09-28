using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyAdPersistence(){
  MissionStore.BeginVerification();yield return null;Directory.CreateDirectory("QA/RewardAds");bool read=Array.IndexOf(Environment.GetCommandLineArgs(),"-adsPersistReadTest")>=0;bool ok;
  if(read){ok=RewardAdSettings.PermanentTest&&RewardAdSettings.UnitId==RewardAdSettings.IOSTestUnitId;RewardAdSettings.ResetQA();}else{RewardAdSettings.ResetQA();for(int i=0;i<50;i++)RewardAdSettings.TapSecret();ok=RewardAdSettings.PermanentTest;}
  File.AppendAllText("QA/RewardAds/persistence.txt",(ok?"PASS":"FAIL")+(read?": enabled after fresh process restart\n":": saved by 50 taps\n"));Application.Quit(ok?0:1);
 }
 IEnumerator VerifyRewardAds(){
  MissionStore.BeginVerification();RewardAdSettings.ResetQA();Directory.CreateDirectory("QA/RewardAds");yield return new WaitForSecondsRealtime(.5f);bool ok=true;
  var p=new MissionProfile();var date=new DateTime(2026,9,27,12,0,0,DateTimeKind.Utc);ok&=RewardAdLedger.Remaining(p,date)==2;
  ok&=!RewardAdLedger.Grant(p,"",date)&&RewardAdLedger.Grant(p,"first",date)&&!RewardAdLedger.Grant(p,"first",date)&&p.shells==30;
  p=JsonUtility.FromJson<MissionProfile>(JsonUtility.ToJson(p));ok&=RewardAdLedger.Remaining(p,date)==1&&!RewardAdLedger.Grant(p,"first",date);
  ok&=RewardAdLedger.Grant(p,"second",date)&&!RewardAdLedger.Grant(p,"third",date)&&p.shells==60&&RewardAdLedger.Remaining(p,date.AddDays(-1))==0;
  ok&=RewardAdLedger.Remaining(p,date.AddDays(1))==2&&!RewardAdLedger.Grant(p,"first",date.AddDays(1));
  DrawHome();ShowHomeSettings();var secret=root.GetComponentsInChildren<Button>().Single(b=>b.name=="Reward ad secret");for(int i=0;i<49;i++)secret.onClick.Invoke();ok&=!RewardAdSettings.PermanentTest;secret.onClick.Invoke();ok&=RewardAdSettings.PermanentTest&&RewardAdSettings.TestAds&&RewardAdSettings.UnitId==RewardAdSettings.IOSTestUnitId;
  yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/RewardAds/test-mode.png"),Screen.width,Screen.height);
  DrawShellShop();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/RewardAds/shop-landscape.png"),Screen.width,Screen.height);var ads=RewardAdService.Instance;
  root.GetComponentsInChildren<Button>().Single(b=>b.name=="Watch rewarded ad").onClick.Invoke();ok&=ads.Busy&&Time.timeScale==0;ads.CompleteSimulation(false);ok&=!ads.Busy&&MissionStore.AdsRemaining==2&&MissionStore.Get().shells==0&&Time.timeScale==1;
  root.GetComponentsInChildren<Button>().Single(b=>b.name=="Watch rewarded ad").onClick.Invoke();ads.CompleteSimulation(true);ok&=ads.Busy&&MissionStore.Get().shells==0;
  yield return new WaitForSecondsRealtime(3.1f);ReviewCapture.Save(Path.GetFullPath("QA/RewardAds/simulation.png"),Screen.width,Screen.height);ads.CompleteSimulation(true);ads.CompleteSimulation(true);ok&=MissionStore.Get().shells==30&&MissionStore.AdsRemaining==1&&!AudioListener.pause;
  yield return new WaitForSecondsRealtime(1);bool busyRejected=true;ads.Show((earned,message)=>{});ads.Show((earned,message)=>busyRejected=false);yield return new WaitForSecondsRealtime(3.1f);ads.CompleteSimulation(true);ok&=busyRejected&&MissionStore.Get().shells==60&&MissionStore.AdsRemaining==0;
  bool limitRejected=false;ads.Show((earned,message)=>limitRejected=!earned);ok&=limitRejected&&!ads.Busy;
  Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);DrawShellShop();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/RewardAds/shop-portrait.png"),Screen.width,Screen.height);ok&=!rewardAdButton.interactable;
  File.WriteAllText("QA/RewardAds/report.txt",ok?"PASS: 30 shells once per receipt; 2/day; serialization; UTC rollover and rollback; 49/50 tap activation; test ID selection; cancel/no reward; early completion blocked; rewarded completion; duplicate callback; busy lock; quota shared; audio/time restored; responsive UI":"FAIL");RewardAdSettings.ResetQA();Application.Quit(ok?0:1);
 }
}
}
