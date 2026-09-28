using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 void DrawBrand(string subtitle){
  var brand=Label((MantisLanguage.Japanese?"SHAKO":"MANTIS")+" <color=#A9CDD3>/</color> <color=#1AE8F0>PUNCH</color>",28,12,400,32,24,Color.white);brand.name="Brand title";brand.fontStyle=FontStyle.Bold;brand.supportRichText=true;
  Label(subtitle,28,48,490,20,13,muted).name="Page subtitle";
 }
 void DrawMenuNavigation(float w,float h,bool portrait,int selected){
  float height=portrait?72:60,padding=12,y=h-height-padding,gap=8,margin=28,width=(w-margin*2-gap*4)/5;
  if(notice&&!portrait){notice.rectTransform.anchoredPosition=new Vector2(28,-565);notice.rectTransform.sizeDelta=new Vector2(w-56,18);notice.fontSize=12;}
  if(notice&&(customVisible||collectionVisible||shopVisible||homeVisible&&!shellShopVisible))ApplySceneTextShadow(notice);
  Panel("Navigation backdrop",0,y-padding,w,height+padding*2,new Color(.005f,.035f,.045f,.94f));Line(0,y-padding,w,new Color(.1f,.6f,.65f,.8f));
  string[] names={"ホーム","カスタム","戦う",MantisDuel.Tr("ショップ","Shop"),"ミッション"},icons={"home","custom","battle","shop","mission"};
  for(int i=0;i<5;i++){int tab=i;float x=margin+i*(width+gap);Plate("Nav "+names[i],x,y,width,height,i==2,i==selected);
   if(portrait){Icon(icons[i],x+width*.5f-12,y+8,24,i==selected?cyan:Color.white);Label(names[i],x+4,y+39,width-8,25,i==3?13:15,Color.white).alignment=TextAnchor.MiddleCenter;}
   else{float textWidth=!MantisLanguage.Japanese?(i==1||i==3?100:80):i==3||i==4?90:60,groupWidth=24+12+textWidth,start=x+(width-groupWidth)*.5f;Icon(icons[i],start,y+18,24,i==selected?cyan:Color.white);Label(names[i],start+36,y+14,textWidth,32,18,Color.white).alignment=TextAnchor.MiddleCenter;}
   Touch("Navigate "+names[i],x,y,width,height,()=>{if(tab==selected)return;if(tab==0)DrawHome();else if(tab==1){draft=MantisAppearance.Load();customColors=false;DrawCustom();}else if(tab==2)EnterBattle();else if(tab==3)DrawEquipmentShop();else DrawMissions();});
   if(i==4&&MissionStore.Ready(0)+MissionStore.Ready(1)+MissionStore.Ready(2)>0)Dot(x+width-17,y+8);
  }
 }
 IEnumerator VerifyUILayout(){
  MissionStore.BeginVerification();RankingStore.BeginVerification();var p=MissionStore.Get();p.shells=350;yield return new WaitForSecondsRealtime(2);const string folder="QA/UILayoutVerified";Directory.CreateDirectory(folder);bool ok=true;
  Action[] pages={DrawHome,DrawCustom,DrawCollection,DrawMissions,DrawRanking,()=>DrawCrownShop()};string[] names={"home","custom","collection","missions","ranking","shop"};
  for(int orientation=0;orientation<2;orientation++){
   Screen.SetResolution(orientation==0?1280:720,orientation==0?720:1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);
   for(int i=0;i<pages.Length;i++){pages[i]();yield return new WaitForSecondsRealtime(.3f);var brand=root.GetComponentsInChildren<Text>().Single(t=>t.name=="Brand title");ok&=brand.rectTransform.anchoredPosition==new Vector2(28,-12)&&!brand.text.Contains("S H A");ok&=root.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Navigate "))==5;
    ReviewCapture.Save(Path.GetFullPath(folder+"/"+names[i]+"-"+orientation+".png"),Screen.width,Screen.height);
   }
  }
  string clean;ok &= CharacterProfile.Validate("  シャコ丸  ",out clean)&&clean=="シャコ丸"&&!CharacterProfile.Validate("   ",out clean)&&!CharacterProfile.Validate("<b>名前</b>",out clean)&&!CharacterProfile.Validate("あいうえおかきくけこさしす",out clean);DrawHome();ShowCharacterName();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath(folder+"/character-name.png"),Screen.width,Screen.height);
  DrawHome();ShowHomeSettings();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath(folder+"/settings.png"),Screen.width,Screen.height);DrawRanking();ShowRankingInfo();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath(folder+"/rules.png"),Screen.width,Screen.height);missionTab=2;DrawMissions();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath(folder+"/login.png"),Screen.width,Screen.height);
  p.crownOwned=true;DrawCustom();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath(folder+"/custom-owned.png"),Screen.width,Screen.height);DrawCollection();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath(folder+"/collection-owned.png"),Screen.width,Screen.height);File.WriteAllText(folder+"/report.txt",ok?"PASS: six pages in both orientations; shared brand position and five navigation targets; empty and owned previews":"FAIL");Application.Quit(ok?0:1);
 }
}
}
