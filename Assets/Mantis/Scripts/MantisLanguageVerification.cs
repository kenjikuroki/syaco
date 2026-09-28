using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyLocalization(){
  MissionStore.BeginVerification();RankingStore.BeginVerification();CharacterProfile.Save("OceanStriker");
  var profile=MissionStore.Get();profile.shells=12340;
  string folder="QA/Localization/"+(MantisLanguage.Japanese?"ja":"en");Directory.CreateDirectory(folder);
  var report=new List<string>();int failures=0;
  Action<string> inspect=page=>{Canvas.ForceUpdateCanvases();foreach(var text in root.GetComponentsInChildren<Text>()){
   if(string.IsNullOrWhiteSpace(text.text))continue;
   Transform top=text.transform;while(top.parent!=root&&top.parent)top=top.parent;int modalIndex=-1;foreach(Transform child in root)if(child.name.EndsWith("backdrop")&&child.name!="Navigation backdrop")modalIndex=child.GetSiblingIndex();
   if(top.GetSiblingIndex()>modalIndex){float contrast=MantisContrastAudit.Ratio(text,root);if(contrast<4.5f){report.Add("CONTRAST "+page+" "+contrast.ToString("F2")+": "+text.text);failures++;}}
   if(!MantisLanguage.Japanese&&Regex.IsMatch(text.text,@"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]")){report.Add("UNTRANSLATED "+page+": "+text.text);failures++;}
   var size=text.rectTransform.rect.size;var settings=text.GetGenerationSettings(size);settings.resizeTextForBestFit=false;
   settings.fontSize=text.resizeTextForBestFit?text.cachedTextGenerator.fontSizeUsedForBestFit:text.fontSize;
   if(settings.fontSize<=0)settings.fontSize=text.fontSize;
   settings.verticalOverflow=VerticalWrapMode.Overflow;
   var generator=new TextGenerator();float height=generator.GetPreferredHeight(text.text,settings)/text.pixelsPerUnit;
   if(height>size.y+3){report.Add("OVERFLOW "+page+" "+height.ToString("F1")+">"+size.y+" @"+settings.fontSize+": "+text.text);failures++;}
  }};
  Action[] pages={DrawHome,DrawCustom,DrawCollection,DrawMissions,()=>{missionTab=1;DrawMissions();},()=>{missionTab=2;DrawMissions();},DrawRanking,()=>{DrawRanking();ShowRankingInfo();},()=>DrawCrownShop(),()=>{confirmCrown=true;DrawCrownShop();},DrawShellShop,()=>{DrawHome();ShowHomeSettings();},()=>{DrawHome();ShowCharacterName();},()=>{profile.crownOwned=true;DrawCustom();},()=>{profile.crownOwned=true;DrawCollection();},()=>{profile.crownOwned=false;profile.shells=0;DrawCrownShop();}};
  string[] names={"home","custom","collection-empty","missions-daily","missions-weekly","login","ranking","ranking-rules","gear-shop","gear-confirm","shell-shop","settings","rename","custom-owned","collection-owned","insufficient"};
  for(int orientation=0;orientation<2;orientation++){
   Screen.SetResolution(orientation==0?1280:720,orientation==0?720:1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
   profile.crownOwned=false;profile.shells=12340;missionTab=0;confirmCrown=false;
   for(int i=0;i<pages.Length;i++){pages[i]();yield return new WaitForSecondsRealtime(.3f);inspect(names[i]+orientation);ReviewCapture.Save(Path.GetFullPath(folder+"/"+names[i]+"-"+orientation+".png"),Screen.width,Screen.height);}
  }
  CharacterProfile.BeginVerification();DrawHome();yield return new WaitForSecondsRealtime(.3f);inspect("registration");ReviewCapture.Save(Path.GetFullPath(folder+"/registration.png"),Screen.width,Screen.height);
  report.Add(failures==0?"PASS: 16 menu states in both orientations; initial registration; no untranslated labels, vertical text overflow or text contrast below 4.5:1":"FAIL: "+failures+" issues");File.WriteAllLines(folder+"/report.txt",report);Application.Quit(failures==0?0:1);
 }
}
}
