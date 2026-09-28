using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 bool rankingVisible,rankingPortrait;int rankingTab;Vector2 rankingScreen;string rankingDay;
 Vector2 RankingLayoutSize=>rankingPortrait?new Vector2(720,1280):new Vector2(1200,675);
 void DrawRanking(){DrawLiveRanking(true);}
 async void DrawLiveRanking(bool refresh){
  Clear();rankingVisible=true;rankingPortrait=Screen.safeArea.height>Screen.safeArea.width;rankingScreen=new Vector2(Screen.width,Screen.height);rankingDay=DateTime.UtcNow.ToString("yyyy-MM-dd");ApplySafeArea();
  float w=RankingLayoutSize.x,h=RankingLayoutSize.y;bool p=rankingPortrait;
  Panel("Ranking backdrop",0,0,w,h,new Color(.005f,.045f,.065f,.97f));Panel("Ranking header",0,0,w,80,new Color(.005f,.035f,.045f,.9f));DrawBrand("RANKING / 読み合いを制し、頂点へ。");Action("ホームへ",w-162,17,136,46,DrawHome,false,19);
  Icon("crown",29,104,39,new Color(1,.73f,.22f));Label("ランキング",86,101,380,47,32,Color.white);
  float bx=p?28:396,bw=p?w-56:776,by=p?282:226,row=p?48:24;
  Plate("Season",28,162,p?w-56:344,p?54:100);Label("MONTHLY / "+RankingStore.Season(DateTime.UtcNow),48,174,p?w-96:304,32,22,cyan);
  if(!p)Label(MantisDuel.Tr("毎月1日 0:00 UTC 更新","Resets monthly · 00:00 UTC"),48,218,304,26,14,muted);
  for(int i=0;i<2;i++){int n=i;float tw=(bw-8)/2,x=bx+i*(tw+8),y=p?225:162;Plate("Ranking tab "+i,x,y,tw,48,false,rankingTab==i);var t=Label(i==0?"シーズンレート":"最高連勝",x,y,tw,48,21,rankingTab==i?cyan:Color.white);t.alignment=TextAnchor.MiddleCenter;Touch("Ranking tab "+i,x,y,tw,48,()=>{rankingTab=n;DrawRanking();});}
  Plate("Leaderboard",bx,by,bw,p?563:325);Label("順位",bx+20,by+10,70,28,16,muted);Label("プレイヤー",bx+100,by+10,bw-250,28,16,muted);Label(rankingTab==0?"RATE":"STREAK",bx+bw-130,by+10,110,28,16,cyan);Line(bx+20,by+46,bw-40,cyan);
  if(!refresh&&!OnlineRanking.Loading&&OnlineRanking.Entries.Count>0){for(int i=0;i<OnlineRanking.Entries.Count;i++){var e=OnlineRanking.Entries[i];float y=by+55+i*row;Label((e.Rank+1).ToString(),bx+20,y,70,row,18,i<3?orange:Color.white);var name=Label(e.PlayerName??"Player",bx+100,y,bw-250,row,19,Color.white);name.supportRichText=false;Label(e.Score.ToString("0"),bx+bw-130,y,110,row,18,cyan);}}
  else {var label=Label(refresh?MantisDuel.Tr("読み込み中…","Loading…"):OnlineRanking.Status,bx+24,by+100,bw-48,100,22,Color.white);label.alignment=TextAnchor.MiddleCenter;}
  float my=p?865:283,mx=28,mw=p?w-56:344;Plate("My ranked position",mx,my,mw,p?146:194,false,true);CharacterName(mx+20,my+12,mw-40,32,24);var me=OnlineRanking.Mine;Label(MantisDuel.Tr("順位  ","Rank  ")+(me==null?"—":(me.Rank+1).ToString()),mx+20,my+60,mw-40,30,23,Color.white);Label((rankingTab==0?"RATE  ":"STREAK  ")+(me==null?"—":me.Score.ToString("0")),mx+20,my+104,mw-40,30,23,cyan);
  Action("ルール / 戦績",28,p?1031:496,p?w-56:344,45,ShowRankingInfo,false,18);notice=Label(MantisDuel.Tr("通常の対人戦のみ集計。BOT戦・友達対戦は対象外です。","Public PvP only. BOT and friend matches are unranked."),28,p?1090:561,w-56,p?60:30,17,muted);DrawMenuNavigation(w,h,p,-1);
  if(refresh){int tab=rankingTab;await OnlineRanking.Load(tab);if(this&&rankingVisible&&rankingTab==tab)DrawLiveRanking(false);}
 }
 void ShowRankingInfo(){
  float w=RankingLayoutSize.x,h=RankingLayoutSize.y;var dim=Rect("Ranking rules backdrop",0,0,w,h).gameObject.AddComponent<Image>();dim.color=new Color(0,.025f,.03f,.97f);float dw=rankingPortrait?664:840,dh=rankingPortrait?900:526,x=(w-dw)/2,y=(h-dh)/2;Plate("Ranking rules",x,y,dw,dh);Label("ランキングのルール / 戦績",x+24,y+22,dw-48,44,rankingPortrait?27:29,Color.white);
  Label("・1シーズンは1か月（UTC基準）\n・3本先取の1試合で勝敗を集計\n・レートは対人戦の勝敗と相手の強さで変動\n・最高連勝は、その月のベスト記録\n・BOT戦は対人レート・対人連勝の対象外",x+27,y+88,dw-54,rankingPortrait?245:174,rankingPortrait?22:19,Color.white);
  var p=RankingStore.Get();float ry=y+(rankingPortrait?380:279);Line(x+26,ry,dw-52,cyan);Label("この端末のCPU戦績",x+27,ry+17,dw-54,30,22,cyan);Label(p.cpuWins+" 勝   /   "+p.cpuLosses+" 敗",x+27,ry+57,dw-54,40,29,Color.white);Label("この機能の追加後に完了した試合を記録します。\n対人戦績・過去シーズンの順位はオンライン対応後に表示します。",x+27,ry+107,dw-54,rankingPortrait?132:56,rankingPortrait?20:15,muted);Action("閉じる",x+24,y+dh-78,dw-48,54,DrawRanking,false,23);
 }
 IEnumerator VerifyRanking(){
  const string folder="QA/RankingVerified";Directory.CreateDirectory(folder);RankingStore.BeginVerification();MissionStore.BeginVerification();yield return new WaitForSecondsRealtime(2);
  bool ok=RankingStore.SeasonEnd(new DateTime(2026,12,31,23,59,59,DateTimeKind.Utc))==new DateTime(2027,1,1,0,0,0,DateTimeKind.Utc)&&RankingStore.SeasonEnd(new DateTime(2028,2,1))==new DateTime(2028,3,1,0,0,0,DateTimeKind.Utc);
  RankingStore.RecordCpuMatch(true);RankingStore.RecordCpuMatch(false);var saved=JsonUtility.FromJson<RankingProfile>(JsonUtility.ToJson(RankingStore.Get()));ok&=saved.cpuWins==1&&saved.cpuLosses==1;
  DrawHome();root.GetComponentsInChildren<Button>().Single(b=>b.name=="Open ranking").onClick.Invoke();yield return new WaitForEndOfFrame();ReviewCapture.Save(Path.GetFullPath(folder+"/landscape.png"));ok&=rankingVisible&&!homeVisible;root.GetComponentsInChildren<Button>().Single(b=>b.name=="Ranking tab 1").onClick.Invoke();Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(2);yield return new WaitForEndOfFrame();ok&=rankingPortrait&&rankingTab==1;ReviewCapture.Save(Path.GetFullPath(folder+"/portrait.png"),Screen.width,Screen.height);ShowRankingInfo();yield return new WaitForEndOfFrame();ReviewCapture.Save(Path.GetFullPath(folder+"/rules.png"),Screen.width,Screen.height);DrawRanking();Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(2);ok&=!rankingPortrait;DrawHome();ok&=homeVisible&&!rankingVisible;File.WriteAllText(folder+"/report.txt",ok?"PASS: home entry; both ranking tabs; portrait/landscape; navigation; UTC month/year/leap boundaries; separate persisted CPU statistics; no fabricated standings":"FAIL");Application.Quit(ok?0:1);
 }
}
}
