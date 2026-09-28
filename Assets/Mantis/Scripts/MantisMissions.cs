using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 int missionTab;bool missionVisible,missionPortrait;string missionDate;Vector2 missionScreen;
 Vector2 MissionLayoutSize=>missionPortrait?new Vector2(720,1280):new Vector2(1200,675);
 void DrawMissions(){
  Clear();missionVisible=true;missionDate=DateTime.Now.ToString("yyyy-MM-dd");missionPortrait=Screen.safeArea.height>Screen.safeArea.width;missionScreen=new Vector2(Screen.width,Screen.height);ApplySafeArea();
  MantisAppearance.Apply(model.GetComponent<MantisModularBody>(),MantisAppearance.Load());MantisLoadout.Apply(model.GetComponent<MantisModularBody>(),MissionStore.Get().crownEquipped);
  float w=MissionLayoutSize.x,h=MissionLayoutSize.y;var p=MissionStore.Get();
  Panel("Mission backdrop",0,0,w,h,new Color(.005f,.045f,.065f,.95f));Panel("Mission header shade",0,0,w,80,new Color(.005f,.035f,.045f,.85f));DrawBrand("M I S S I O N S  /  一戦ずつ、貝殻を集めよう。");Action("ホームへ",w-162,17,136,46,DrawHome,false,19);
  Icon("mission",30,104,37,cyan);Label("ミッション",83,102,370,47,32,Color.white);Plate("Shell wallet",w-225,99,197,53);Icon("shell",w-211,113,27,new Color(.91f,.86f,.66f));Label("貝殻  "+p.shells,w-169,114,136,27,20,Color.white).name="Wallet value";
  float tabY=missionPortrait?184:169,tabW=missionPortrait?(w-72)/3:214;
  string[] tabs={"デイリー","ウィークリー","ログイン"};for(int i=0;i<3;i++){int tab=i;float tx=28+i*(tabW+8);Plate("Mission tab "+i,tx,tabY,tabW,51,false,missionTab==i);var label=Label(tabs[i],tx,tabY,tabW,51,20,missionTab==i?cyan:Color.white);label.alignment=TextAnchor.MiddleCenter;Touch("Mission tab "+i,tx,tabY,tabW,51,()=>{missionTab=tab;DrawMissions();});if(MissionStore.Ready(i)>0)Dot(tx+tabW-19,tabY+9);}
  Label(missionTab==1?"月曜 0:00 に更新":"毎日 0:00 に更新",missionPortrait?30:854,missionPortrait?253:185,310,27,16,muted);
  if(missionTab<2){
   string[] names={"ラウンドを戦い抜く","一撃を決めて勝利","パリィで攻撃を弾く"},notes={"勝敗を問わず、最後まで戦う","ラウンドで相手を倒す","相手のパンチをパリィする"},icons={"battle","battle","parry"};
   for(int i=0;i<3;i++){int id=i;int value=missionTab==0?p.daily[i]:p.weekly[i],goal=MissionStore.Target(missionTab,i);bool claimed=missionTab==0?p.dailyClaimed[i]:p.weeklyClaimed[i],ready=!claimed&&value>=goal;float cw=missionPortrait?w-56:(w-88)/3,x=missionPortrait?28:28+i*(cw+16),y=missionPortrait?297+i*219:239;
    Plate("Mission card "+i,x,y,cw,missionPortrait?200:245,false,ready);Icon(icons[i],x+16,y+19,30,claimed?muted:cyan);Label(names[i],x+60,y+16,missionPortrait?540:cw-76,33,missionPortrait?25:22,Color.white);Label(notes[i],missionPortrait?88:x+20,y+59,missionPortrait?540:cw-40,24,15,muted);
    float px=x+21,py=y+(missionPortrait?91:104),pw=cw-42;Label(Mathf.Min(value,goal)+" / "+goal,px,py,pw,23,16,claimed?muted:cyan);Panel("Progress track",px,py+31,pw,7,new Color(.01f,.065f,.085f));float progress=Mathf.Clamp01((float)value/goal);if(progress>0)Panel("Progress fill",px,py+31,pw*progress,7,claimed?muted:cyan);
    float rx=x+23,ry=y+(missionPortrait?151:187);Icon("shell",rx,ry,23,new Color(.91f,.86f,.66f));Label("+"+MissionStore.Reward(missionTab,i),rx+33,ry,99,27,21,Color.white);
    float bx=missionPortrait?w-239:x+cw-183,by=y+(missionPortrait?143:177),bw=missionPortrait?190:162;var button=Action(claimed?"受取済み":ready?"受け取る":"挑戦中",bx,by,bw,missionPortrait?44:49,()=>ClaimWithReaction(missionTab,id,new Vector2(bx+bw*.5f,by+24)),ready,19);button.name="Claim mission "+i;button.interactable=ready;
   }
   float fy=missionPortrait?987:502;Label("受取可能："+MissionStore.Ready(missionTab)+" 件",30,fy+9,missionPortrait?270:600,28,19,cyan);var all=Action("まとめて受け取る",missionPortrait?330:843,fy,missionPortrait?362:329,49,()=>ClaimWithReaction(missionTab,0,new Vector2(missionPortrait?511:1007,fy+24),true),MissionStore.Ready(missionTab)>0,21);all.name="Claim all missions";all.interactable=MissionStore.Ready(missionTab)>0;
  }else{
   float y=missionPortrait?297:239,ph=missionPortrait?650:280;Plate("Login reward",28,y,w-56,ph);Icon("gift",missionPortrait?w*.5f-45:67,y+(missionPortrait?43:37),missionPortrait?90:67,p.loginClaimed?muted:orange);
   float tx=missionPortrait?52:162;Label("今日のログイン報酬",tx,y+(missionPortrait?173:23),w-tx-55,45,missionPortrait?30:27,Color.white);Label(MantisDuel.Tr("貝殻  +","Shells  +")+ShellEconomy.Login,tx,y+(missionPortrait?246:83),w-tx-55,65,48,cyan);Label("毎日1回受け取れます。\n連続ログインは不要です。",tx,y+(missionPortrait?351:159),w-tx-55,64,missionPortrait?22:18,muted);
   var claim=Action(p.loginClaimed?"本日は受取済み":"今日の報酬を受け取る",missionPortrait?52:784,y+(missionPortrait?529:195),missionPortrait?w-104:364,60,()=>ClaimWithReaction(2,0,new Vector2(missionPortrait?w*.5f:966,y+(missionPortrait?559:225))),!p.loginClaimed,missionPortrait?25:21);claim.name="Claim login mission";claim.interactable=!p.loginClaimed;
  }
  notice=Label("CPU戦も対象・デモは対象外。貝殻はカスタム内のショップで使用できます。",30,missionPortrait?1076:565,w-60,missionPortrait?65:20,missionPortrait?18:13,muted);
  DrawMenuNavigation(w,h,missionPortrait,4);
 }
 IEnumerator VerifyMissionDesign(){
  const string folder="QA/MissionDesignVerified";Directory.CreateDirectory(folder);MissionStore.BeginVerification();yield return new WaitForSecondsRealtime(2);var p=MissionStore.Get();p.daily[0]=3;p.daily[1]=0;p.daily[2]=1;DrawMissions();yield return new WaitForEndOfFrame();ReviewCapture.Save(Path.GetFullPath(folder+"/landscape.png"));
  bool ok=!root.GetComponentsInChildren<Button>().Single(b=>b.name=="Claim mission 1").interactable;root.GetComponentsInChildren<Button>().Single(b=>b.name=="Claim mission 0").onClick.Invoke();ok&=p.shells==20&&p.dailyClaimed[0]&&!MissionStore.Claim(0,0);
  Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(2);yield return new WaitForEndOfFrame();ok&=missionPortrait&&missionVisible;ReviewCapture.Save(Path.GetFullPath(folder+"/portrait.png"),Screen.width,Screen.height);
  root.GetComponentsInChildren<Button>().Single(b=>b.name=="Mission tab 1").onClick.Invoke();p.weekly=new[]{20,5,10};DrawMissions();root.GetComponentsInChildren<Button>().Single(b=>b.name=="Claim all missions").onClick.Invoke();ok&=p.shells==320&&MissionStore.Ready(1)==0;
  root.GetComponentsInChildren<Button>().Single(b=>b.name=="Mission tab 2").onClick.Invoke();yield return new WaitForEndOfFrame();ReviewCapture.Save(Path.GetFullPath(folder+"/login.png"),Screen.width,Screen.height);root.GetComponentsInChildren<Button>().Single(b=>b.name=="Claim login mission").onClick.Invoke();ok&=p.shells==330&&p.loginClaimed&&!MissionStore.Claim(2,0);
  Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(2);ok&=!missionPortrait&&missionTab==2;yield return new WaitForEndOfFrame();ReviewCapture.Save(Path.GetFullPath(folder+"/login-landscape.png"));root.GetComponentsInChildren<Button>().Single(b=>b.name=="Navigate ホーム").onClick.Invoke();ok&=homeVisible&&!missionVisible;File.WriteAllText(folder+"/report.txt",ok?"PASS: responsive daily/weekly/login; progress and disabled states; individual and bulk rewards exactly once; tab retained on rotation; home navigation":"FAIL");Application.Quit(ok?0:1);
 }
 IEnumerator VerifyMissions(){yield return new WaitForSecondsRealtime(3);System.IO.Directory.CreateDirectory("QA/MissionsVerified");
  // Exercise an isolated profile so verification never awards currency to the user.
  var p=new MissionProfile();var monday=new DateTime(2026,9,21);bool ok=!p.Claim(0,0,monday);p.Record(0,3,monday);ok&=p.Claim(0,0,monday)&&!p.Claim(0,0,monday)&&p.shells==20;
  p=JsonUtility.FromJson<MissionProfile>(JsonUtility.ToJson(p));ok&=!p.Claim(0,0,monday);p.Refresh(monday.AddDays(1));ok&=p.daily[0]==0&&p.weekly[0]==3&&p.shells==20;
  ok&=p.Claim(2,0,monday.AddDays(1))&&!p.Claim(2,0,monday.AddDays(1));p.Refresh(monday.AddDays(7));ok&=p.weekly[0]==0&&!p.loginClaimed;
  DrawMissions();yield return new WaitForSecondsRealtime(1);ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/MissionsVerified/daily.png"));yield return new WaitForSecondsRealtime(1);missionTab=2;DrawMissions();yield return new WaitForSecondsRealtime(1);ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/MissionsVerified/login.png"));yield return new WaitForSecondsRealtime(1);
  System.IO.File.WriteAllText("QA/MissionsVerified/report.txt",ok?"PASS: incomplete claim rejected; reward paid once; serialization preserves claim; daily/weekly rollovers; daily login once; user progress untouched":"FAIL");if(!ok){Application.Quit(1);yield break;}MissionStore.BeginVerification();EnterBattle();
 }
}
}



