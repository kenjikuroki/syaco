using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 bool shellShopVisible;
 void DrawShellShop(){
  if(NeedsRegistration){DrawHome();return;}
  Clear();homeVisible=true;shellShopVisible=true;homePortrait=Screen.safeArea.height>Screen.safeArea.width;previousHomeScreen=new Vector2(Screen.width,Screen.height);homeDay=DateTime.Now.ToString("yyyy-MM-dd");ApplySafeArea();SetCharacterCamera(homePortrait);
  float w=HomeLayoutSize.x,h=HomeLayoutSize.y;
  Panel("Shell shop background",0,0,w,h,new Color(.005f,.045f,.065f,.96f));
  DrawShopChrome(w,homePortrait,true);
  DrawRewardAdCard(w);
  int[] amounts=ShellEconomy.PackAmounts;string[] prices=ShellEconomy.ProvisionalYen;
  for(int i=0;i<4;i++){
   float cw=homePortrait?w-56:(w-72)/2,ch=homePortrait?152:110;
   float x=homePortrait?28:28+(i%2)*(cw+16),y=(homePortrait?356:324)+(homePortrait?i:i/2)*(ch+12);
   Plate("Shell pack "+amounts[i],x,y,cw,ch);Icon("shell",x+22,y+25,42,new Color(.95f,.83f,.52f));
   Label(amounts[i].ToString("N0")+" 枚",x+83,y+18,cw-260,47,29,Color.white);
   Label(i==0?"おためしパック":i==1?"基本パック":i==2?"まとめ買いパック":"大容量パック",x+24,y+(homePortrait?80:72),cw-200,26,17,muted);
   var price=Label(prices[i]+"（仮）",x+cw-187,y+8,163,28,22,orange);price.alignment=TextAnchor.MiddleRight;
   int pack=i;Action(MantisDuel.Tr("詳細を見る","View pack"),x+cw-172,y+ch-64,148,43,()=>ShowApplePurchase(pack),false,18);
  }
  notice=Label("価格は仮設定です。現在は購入できません。",28,homePortrait?1028:566,w-56,homePortrait?45:28,17,muted);
  DrawMenuNavigation(w,h,homePortrait,3);
 }
 IEnumerator VerifyOnboardingShop(){
  MissionStore.BeginVerification();CharacterProfile.BeginVerification();Directory.CreateDirectory("QA/Identity");yield return new WaitForSecondsRealtime(.5f);DrawHome();
  bool ok=!NeedsRegistration&&CharacterProfile.Name==MantisDuel.Tr("ゲスト","Guest")&&root.GetComponentsInChildren<InputField>().Length==0;
  ShowHomeSettings();ok&=!root.GetComponentsInChildren<Button>().Any(b=>b.name=="Change character name from settings");
  yield return new WaitForSecondsRealtime(.4f);ReviewCapture.Save(Path.GetFullPath("QA/Identity/settings.png"),Screen.width,Screen.height);
  DrawHome();var property=typeof(GameCenterIdentity).GetProperty("DisplayName");property.SetValue(null,"Game Center Player With Long Name");yield return null;
  ok&=CharacterProfile.Name=="Game Center Player With Long Name"&&root.GetComponentsInChildren<PlayerDisplayName>().All(n=>n.GetComponent<Text>().text==CharacterProfile.Name);
  property.SetValue(null,"");yield return null;ok&=CharacterProfile.Name==MantisDuel.Tr("ゲスト","Guest");
  yield return new WaitForSecondsRealtime(.4f);ReviewCapture.Save(Path.GetFullPath("QA/Identity/guest-home.png"),Screen.width,Screen.height);
  DrawShellShop();ok&=shellShopVisible;File.WriteAllText("QA/Identity/report.txt",ok?"PASS: no registration; guest fallback; no rename action; identity refresh and sign-out fallback; shop accessible":"FAIL");Application.Quit(ok?0:1);
 }
}
}