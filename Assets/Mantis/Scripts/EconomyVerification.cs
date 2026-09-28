using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyEconomy(){
  MissionStore.BeginVerification();Directory.CreateDirectory("QA/Economy");bool ok=true;var date=new DateTime(2026,9,21);var p=new MissionProfile();
  ok&=p.CompleteMatch(false,date)==5&&!p.firstWinClaimed;
  ok&=p.CompleteMatch(true,date)==50&&p.CompleteMatch(true,date)==20;
  p=JsonUtility.FromJson<MissionProfile>(JsonUtility.ToJson(p));ok&=p.CompleteMatch(true,date)==20;
  ok&=p.CompleteMatch(true,date.AddDays(1))==50;
  var purchase=new MissionProfile{shells=799};ok&=!purchase.BuyEquipment(0)&&purchase.shells==799&&!purchase.crownOwned;
  purchase.shells=800;ok&=purchase.BuyEquipment(0)&&purchase.shells==0&&purchase.crownOwned;
  purchase.shells=800;ok&=!purchase.BuyEquipment(0)&&purchase.shells==800&&!purchase.BuyEquipment(-1)&&!purchase.BuyEquipment(4);
  purchase=JsonUtility.FromJson<MissionProfile>(JsonUtility.ToJson(purchase));ok&=!purchase.BuyEquipment(0);
  var sim=new MissionProfile();for(int d=0;d<7;d++){var day=date.AddDays(d);for(int m=0;m<10;m++)sim.CompleteMatch(m<5,day);sim.Record(0,30,day);sim.Record(1,15,day);sim.Record(2,2,day);for(int i=0;i<3;i++)sim.Claim(0,i,day);sim.Claim(2,0,day);for(int i=0;i<3;i++)sim.Claim(1,i,day);}
  ok&=sim.shells==1945;File.WriteAllText("QA/Economy/report.txt",(ok?"PASS":"FAIL")+": first win once/day; serialized bonus and ownership; insufficient/exact/duplicate/invalid purchase; 7 days x 10 matches at 50% plus all missions = "+sim.shells);
  if(!ok){Application.Quit(1);yield break;}
  DrawEquipmentShop();yield return new UnityEngine.WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/Economy/empty-wallet.png"),Screen.width,Screen.height);
  MissionStore.Get().shells=5000;DrawEquipmentShop();yield return new UnityEngine.WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/Economy/prices.png"),Screen.width,Screen.height);
  Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new UnityEngine.WaitForSecondsRealtime(.5f);DrawShellShop();yield return new UnityEngine.WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/Economy/packs.png"),Screen.width,Screen.height);
  yield return VerifyFreeEquipment();
 }
}
}
