using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 bool homeVisible,homePortrait;Vector2 previousHomeScreen;string homeDay;Camera homeCamera;Vector3 cameraPosition;Quaternion cameraRotation;Vector2 cameraShift;float cameraFov;Camera.GateFitMode cameraGateFit;
 readonly Color cyan=new Color(.1f,.91f,.94f),orange=new Color(1,.43f,.08f),plateTop=new Color(.025f,.12f,.16f,1);
 Vector2 HomeLayoutSize=>homePortrait?new Vector2(720,1280):new Vector2(1200,675);
 void CacheHomeCamera(){homeCamera=FindFirstObjectByType<Camera>();cameraPosition=homeCamera.transform.position;cameraRotation=homeCamera.transform.rotation;cameraShift=homeCamera.lensShift;cameraFov=homeCamera.fieldOfView;cameraGateFit=homeCamera.gateFit;}
 void RestoreHomeCamera(){if(!homeCamera)return;homeCamera.transform.SetPositionAndRotation(cameraPosition,cameraRotation);homeCamera.lensShift=cameraShift;homeCamera.fieldOfView=cameraFov;homeCamera.gateFit=cameraGateFit;}
 void UpdateHomeLayout(){if(homeVisible&&(previousHomeScreen!=new Vector2(Screen.width,Screen.height)||homeDay!=DateTime.Now.ToString("yyyy-MM-dd"))){if(shellShopVisible)DrawShellShop();else DrawHome();}}
 HomePlate Plate(string name,float x,float y,float w,float h,bool hot=false,bool active=false){
  var p=Rect(name,x,y,w,h).gameObject.AddComponent<HomePlate>();p.color=hot?new Color(1,.55f,.10f):active?new Color(.025f,.23f,.27f,1):plateTop;p.bottom=hot?new Color(.94f,.20f,.025f):new Color(.01f,.055f,.075f,1);p.edge=hot?new Color(1,.73f,.22f):active?cyan:new Color(.11f,.58f,.64f,.8f);p.cut=hot?9:13;p.raycastTarget=false;return p;
 }
 void Icon(string kind,float x,float y,float size,Color c){var icon=Rect("Icon "+kind,x,y,size,size).gameObject.AddComponent<HomeIcon>();icon.kind=kind;icon.color=c;icon.raycastTarget=false;}
 Button Touch(string name,float x,float y,float w,float h,UnityEngine.Events.UnityAction action){var r=Rect(name,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=Color.clear;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);return b;}
 Button Action(string text,float x,float y,float w,float h,UnityEngine.Events.UnityAction action,bool hot=false,int size=25){var p=Plate(text,x,y,w,h,hot);p.raycastTarget=true;var b=p.gameObject.AddComponent<Button>();b.targetGraphic=p;b.onClick.AddListener(action);var colors=b.colors;colors.highlightedColor=new Color(1.1f,1.1f,1.1f);colors.pressedColor=new Color(.65f,.85f,.9f);colors.disabledColor=new Color(.62f,.70f,.73f,1);b.colors=colors;var label=Label(text,x+10,y,w-20,h,size,Color.white);label.alignment=TextAnchor.MiddleCenter;return b;}
 void Line(float x,float y,float w,Color c){Panel("Accent rule",x,y,w,2,c);root.GetChild(root.childCount-1).GetComponent<Image>().raycastTarget=false;}
 void Dot(float x,float y){var p=Rect("Reward available",x,y,8,8).gameObject.AddComponent<HomePlate>();p.cut=4;p.border=0;p.color=p.bottom=orange;p.raycastTarget=false;}
 void DrawStyledHome(){
  Clear();homeVisible=true;homePortrait=Screen.safeArea.height>Screen.safeArea.width;previousHomeScreen=new Vector2(Screen.width,Screen.height);homeDay=DateTime.Now.ToString("yyyy-MM-dd");ApplySafeArea();
  MantisAppearance.Apply(model.GetComponent<MantisModularBody>(),MantisAppearance.Load());MantisLoadout.Apply(model.GetComponent<MantisModularBody>(),MissionStore.Get().crownEquipped);
  SetCharacterCamera(homePortrait);
  float w=HomeLayoutSize.x,h=HomeLayoutSize.y;var profile=MissionStore.Get();
  Panel("Header shade",0,0,w,homePortrait?150:80,new Color(.005f,.035f,.045f,.79f));
  DrawBrand("一撃に、すべてを。");
  float py=homePortrait?84:16,px=homePortrait?28:552;
  Plate("Player identity",px,py,homePortrait?272:248,51);Icon("profile",px+13,py+9,31,cyan);CharacterName(px+56,py+7,homePortrait?195:107,22,16);Label("RANKING  ≫",px+56,py+29,150,17,11,cyan);Touch("Open ranking",px,py,homePortrait?272:248,51,DrawRanking);
  if(!homePortrait){Line(px+169,py+11,2,cyan);Label("CPU 対戦",px+181,py+18,76,22,13,orange);}
  float sx=homePortrait?312:812;Plate("Shell wallet",sx,py,homePortrait?208:204,51);Icon("shell",sx+12,py+12,27,new Color(.91f,.86f,.66f));Label("貝殻 "+profile.shells,sx+49,py+14,119,25,18,Color.white).name="Wallet value";Label("＋",sx+164,py+9,33,30,23,cyan);Touch("Wallet shell shop",sx,py,homePortrait?208:204,51,DrawShellShop);
  float soundX=homePortrait?532:1028,soundW=homePortrait?72:64;Plate("Sound",soundX,py,soundW,51);Icon("sound",soundX+(soundW-27)/2,py+12,27,music.Muted?muted:Color.white);Touch("Toggle home music",soundX,py,soundW,51,()=>{music.Toggle();DrawHome();});
  float settingsX=homePortrait?616:1104,settingsW=homePortrait?76:68;Plate("Settings",settingsX,py,settingsW,51);Icon("settings",settingsX+(settingsW-27)/2,py+12,27,Color.white);Touch("Open settings",settingsX,py,settingsW,51,ShowHomeSettings);
  float titleY=homePortrait?184:104;
  SceneLabel("Y O U R  F I G H T E R",28,homePortrait?165:titleY,500,22,13,cyan);if(!homePortrait){SceneLabel("深海から、一撃で。",28,titleY+92,480,25,17,new Color(.8f,.91f,.91f));Line(28,titleY+126,168,new Color(.035f,.25f,.3f,1));}
  ApplySceneTextShadow(CharacterName(28,homePortrait?194:titleY+32,homePortrait?460:540,57,38));
  Touch("Tap mantis to punch",20,homePortrait?322:254,homePortrait?680:685,homePortrait?310:234,()=>punch=Time.unscaledTime);
  float hintY=homePortrait?620:522;var hint=Label("シャコをタップしてパンチ  ≫",48,hintY,350,42,homePortrait?17:15,cyan);float hintWidth=Mathf.Ceil(hint.preferredWidth)+32;hint.rectTransform.sizeDelta=new Vector2(hintWidth-32,42);hint.alignment=TextAnchor.MiddleCenter;var hintPlate=Plate("Punch hint",32,hintY,hintWidth,42);hintPlate.transform.SetSiblingIndex(hint.transform.GetSiblingIndex());Touch("Punch hint action",32,hintY,hintWidth,42,()=>punch=Time.unscaledTime);
  float cx=homePortrait?28:688,cy=homePortrait?700:104,cw=homePortrait?664:484,ch=homePortrait?185:251;
  Plate("One hit battle",cx,cy,cw,ch);Label("·  ONE HIT. ONE WIN.",cx+24,cy+24,cw-48,22,14,cyan);Line(cx+24,cy+56,176,cyan);
  if(homePortrait){Label("一撃勝負",cx+24,cy+72,340,61,42,Color.white);Action("戦う",cx+363,cy+72,cw-391,89,EnterBattle,true,31);}
  else{Label("一撃勝負",cx+24,cy+79,cw-48,64,47,Color.white);Action("戦う",cx+24,cy+165,cw-48,64,EnterBattle,true,32);}
  int complete=0;float progress=0;for(int i=0;i<3;i++){if(profile.daily[i]>=MissionStore.Target(0,i))complete++;progress+=Mathf.Clamp01((float)profile.daily[i]/MissionStore.Target(0,i))/3;}
  float dy=cy+ch+16,dh=homePortrait?86:74;
  Plate("Daily mission",cx,dy,cw,dh);Icon("mission",cx+21,dy+20,34,cyan);Label("D A I L Y  M I S S I O N S",cx+77,dy+7,cw-115,16,11,cyan);Label("ミッション",cx+76,dy+25,200,24,20,Color.white);Label(complete+" / 3",cx+cw-104,dy+31,70,28,20,Color.white);
  Panel("Mission progress track",cx+78,dy+dh-21,cw-202,7,new Color(.14f,.31f,.34f));if(progress>0)Panel("Mission progress fill",cx+78,dy+dh-21,(cw-202)*progress,7,cyan);Icon("arrow",cx+cw-27,dy+31,15,cyan);Touch("Open daily missions",cx,dy,cw,dh,DrawMissions);if(MissionStore.Ready(0)>0)Dot(cx+cw-17,dy+9);
  float ly=dy+dh+16,lh=dh;Plate("Login bonus",cx,ly,cw,lh);Icon("gift",cx+22,ly+(lh-32)/2,32,orange);Label("ログインボーナス",cx+77,ly+(lh-48)/2,cw-190,26,19,Color.white);Label(profile.loginClaimed?"本日の報酬を受け取りました":"毎日プレイして貝殻を集めよう",cx+77,ly+(lh-48)/2+29,cw-115,19,11,muted);Label(profile.loginClaimed?"受取済":"受取可能",cx+cw-103,ly+(lh-26)/2,85,26,16,profile.loginClaimed?muted:orange);Touch("Claim login bonus",cx,ly,cw,lh,()=>{if(!MissionStore.Get().loginClaimed)ClaimWithReaction(2,0,new Vector2(cx+cw*.5f,ly+lh*.5f),false,true);else DrawMissions();});if(!profile.loginClaimed)Dot(cx+cw-17,ly+9);
  notice=Label("",36,homePortrait?1139:576,w-72,24,14,cyan);
  DrawMenuNavigation(w,h,homePortrait,0);
 }
 void ShowHomeSettings(){
  float w=HomeLayoutSize.x,h=HomeLayoutSize.y;var dim=Rect("Settings backdrop",0,0,w,h).gameObject.AddComponent<Image>();dim.color=new Color(0,.025f,.03f,.88f);float x=(w-530)/2,y=(h-564)/2;Plate("Settings dialog",x,y,530,564);Label("設定",x+26,y+24,420,44,30,Color.white);Touch("Reward ad secret",x+26,y+24,420,44,RewardAdSecretTap);Action(music.Muted?"ホームBGM：OFF":"ホームBGM：ON",x+26,y+91,478,58,()=>{music.Toggle();DrawHome();ShowHomeSettings();},false,21);float shake=PlayerPrefs.GetFloat("Mantis.Shake",.6f);Action(shake>.01f?"画面の揺れ：ON":"画面の揺れ：OFF",x+26,y+165,478,58,()=>{PlayerPrefs.SetFloat("Mantis.Shake",shake>.01f?0:.6f);PlayerPrefs.Save();DrawHome();ShowHomeSettings();},false,21);Label(MantisDuel.Tr("プレイヤー名はGame Centerで変更できます。","Change your player name in Game Center."),x+26,y+239,478,58,18,muted);Action(MantisDuel.Tr("チュートリアル","Tutorial"),x+26,y+313,478,58,StartTutorial,false,21);if(RewardAdService.Instance.PrivacyRequired)Action(MantisDuel.Tr("広告のプライバシー設定","Ad privacy options"),x+26,y+387,478,44,()=>RewardAdService.Instance.Privacy(()=>{if(this){DrawHome();ShowHomeSettings();}}),false,19);Label(RewardAdSettings.PermanentTest?"TEST ADS · SAVED":RewardAdSettings.TestAds?"TEST ADS":"",x+26,y+439,478,26,16,cyan);Action("閉じる",x+26,y+479,478,57,DrawHome,false,21);
 }
}
}



