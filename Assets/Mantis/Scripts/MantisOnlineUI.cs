using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 GameObject onlineDialog;Text onlineStatus;InputField roomCode;
 static string NetText(string ja,string en)=>MantisDuel.Tr(ja,en);
 void ShowBattleOptions(){
  DrawHome();float w=HomeLayoutSize.x,h=HomeLayoutSize.y,dw=Mathf.Min(w-48,620),x=(w-dw)/2,y=(h-480)/2;
  var dim=Rect("Battle options",0,0,w,h).gameObject.AddComponent<Image>();dim.color=new Color(0,.025f,.035f,.97f);onlineDialog=dim.gameObject;
  // The page redraw removes the entire overlay, including all sibling controls.
  Plate("Battle choice",x,y,dw,480);Label(NetText("対戦を選ぶ","CHOOSE A MATCH"),x+24,y+20,dw-48,42,29,Color.white);
  Action(NetText("通常対戦","Find a match"),x+24,y+83,dw-48,52,()=>StartOnline(MatchKind.Public),true,23);
  Action(NetText("友達のルームを作る","Create a friend room"),x+24,y+149,dw-48,52,()=>StartOnline(MatchKind.Friend),false,21);
  var field=Rect("Room code",x+24,y+215,dw-230,52).gameObject.AddComponent<Image>();field.color=new Color(.025f,.16f,.19f);roomCode=field.gameObject.AddComponent<InputField>();roomCode.characterLimit=12;var text=Label("",x+34,y+215,dw-250,52,23,Color.white);text.transform.SetParent(field.transform,true);roomCode.textComponent=text;roomCode.contentType=InputField.ContentType.Alphanumeric;
  Action(NetText("コードで参加","Join code"),x+dw-192,y+215,168,52,()=>{if(!string.IsNullOrWhiteSpace(roomCode.text))StartOnline(MatchKind.Friend,roomCode.text);},false,19);
  Action(NetText("CPUと練習","Practice vs BOT"),x+24,y+281,dw-48,52,async()=>{await OnlineMatch.Instance.Leave();TutorialProgress.Requested=false;busy=true;MenuSceneFade.Load("Duel");},false,20);
  onlineStatus=Label(NetText("通常対戦は相手が見つからなければBOTと対戦します。","Public matches fall back to a BOT if no opponent is found."),x+24,y+340,dw-48,65,18,cyan);
  Action(NetText("キャンセル / 戻る","Cancel / Back"),x+24,y+410,dw-48,52,async()=>{await OnlineMatch.Instance.Leave();if(this)DrawHome();},false,20);
 }
 async void StartOnline(MatchKind kind,string code=null){var net=OnlineMatch.Instance;if(net.Busy||net.Active)return;StartCoroutine(OnlineStatusLoop(net));await net.StartSearch(kind,code);if(onlineStatus)onlineStatus.text=net.Status;}
 IEnumerator OnlineStatusLoop(OnlineMatch net){yield return null;while(this&&onlineDialog&&onlineDialog.activeInHierarchy){if(onlineStatus)onlineStatus.text=net.Status;yield return new WaitForSecondsRealtime(.15f);}}
}
public sealed partial class BattleHud {
 void BuildResultModal(){
  var backdrop=Rect("Modal shade",0,0,W,H).gameObject.AddComponent<Image>();backdrop.color=Color.clear;modal=backdrop.gameObject;
  float width=Mathf.Min(W-48,540),x=(W-width)/2,y=H-(BattleLayout.Portrait?232:164);var net=OnlineMatch.Current;bool friendly=net&&net.Active&&net.Kind==MatchKind.Friend;
  Plate("Match result",new Rect(x,y,width,142));Label("FINAL "+duel.playerWins+" : "+duel.cpuWins,x+16,y+4,width-32,26,23,Color.white,TextAnchor.MiddleCenter);
  string reward=friendly?MantisDuel.Tr("友達対戦 / ランク対象外","FRIEND MATCH / UNRANKED"):MantisDuel.Tr("貝殻 +","Shells +")+duel.MatchShellReward+(duel.MatchShellReward>ShellEconomy.MatchWin?MantisDuel.Tr("（初勝利ボーナス込み）"," (first win bonus)"):"");
  Label(reward,x+16,y+31,width-32,25,18,cyan,TextAnchor.MiddleCenter);
  if(!friendly){var button=Button(MantisDuel.Tr("広告で貝殻 +30","Watch ad · +30 shells")+" ("+MissionStore.AdsRemaining+"/2)",new Rect(x+55,y-48,width-110,40),()=>RewardAdService.Instance.Show((earned,message)=>{if(this){adResultMessage=message;Build();}}));button.interactable=MissionStore.AdsRemaining>0&&!RewardAdService.Instance.Busy;RewardAdService.Instance.Prepare();}
  if(net&&net.Active&&net.Kind==MatchKind.Public)rankingResultLabel=Label(OnlineRanking.ResultStatus,x,y-112,width,25,16,cyan,TextAnchor.MiddleCenter);
  if(!string.IsNullOrEmpty(adResultMessage))Label(adResultMessage,x,y-82,width,28,17,cyan,TextAnchor.MiddleCenter);
  var next=Button(NextMatchLabel,new Rect(x+18,y+62,(width-48)*.56f,59),NextMatch,true);next.interactable=!friendly||!net.LocalRematch;
  Button("ホームへ",new Rect(x+30+(width-48)*.56f,y+62,(width-48)*.44f,59),ResultHome);
 }
 public bool OnlineOverlay{get;private set;}
 public void ShowNetworkDisconnect(){OnlineOverlay=true;Plate("Disconnected",new Rect(30,H*.35f,W-60,210));Label(OnlineMatch.Current.Status,55,H*.35f+20,W-110,85,25,Color.white,TextAnchor.MiddleCenter);Button(MantisDuel.Tr("ホームへ","Home"),new Rect(W*.25f,H*.35f+126,W*.5f,55),()=>OnlineMatch.Current.GoHome());}
 void ShowOnlineExit(){if(OnlineOverlay)return;OnlineOverlay=true;Plate("Leave online",new Rect(30,H*.35f,W-60,210));Label(MantisDuel.Tr("対戦を終了しますか？ 通信対戦中は一時停止できません。","Leave this match? Online play cannot be paused."),55,H*.35f+20,W-110,75,23,Color.white,TextAnchor.MiddleCenter);Button(MantisDuel.Tr("対戦に戻る","Resume"),new Rect(55,H*.35f+120,(W-140)/2,55),()=>{OnlineOverlay=false;Build();});Button(MantisDuel.Tr("ホームへ","Home"),new Rect(W/2+15,H*.35f+120,(W-140)/2,55),()=>OnlineMatch.Current.GoHome());}
 string NextMatchLabel=>OnlineMatch.Current&&OnlineMatch.Current.Active&&OnlineMatch.Current.Kind==MatchKind.Friend?MantisDuel.Tr("再戦を希望","Request rematch"):MantisDuel.Tr("次の対戦へ","Next match");
 void NextMatch(){var net=OnlineMatch.Current;if(net&&net.Active&&net.Kind==MatchKind.Friend){net.VoteRematch();adResultMessage=MantisDuel.Tr("相手の再戦希望を待っています…","Waiting for the opponent's rematch vote…");Build();return;}OnlineMatch.OpenSearchOnHome=true;if(net)net.GoHome();else{Time.timeScale=1;UnityEngine.SceneManagement.SceneManager.LoadScene("Home");}}
 void ResultHome(){if(OnlineMatch.Current)OnlineMatch.Current.GoHome();else{Time.timeScale=1;UnityEngine.SceneManagement.SceneManager.LoadScene("Home");}}
}
}
