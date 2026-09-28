using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
namespace MantisPunch {
public static class TutorialProgress {
 const string Key="Mantis.Tutorial.v1";
 public static bool Requested, Offered;
 public static bool Completed=>PlayerPrefs.GetInt(Key,0)==1;
 public static bool QA=>System.Array.Exists(System.Environment.GetCommandLineArgs(),a=>a.EndsWith("Test"));
 public static void Complete(){if(!QA){PlayerPrefs.SetInt(Key,1);PlayerPrefs.Save();}}
}
public sealed partial class MantisHome {
 void StartTutorial(){TutorialProgress.Requested=true;TutorialProgress.Offered=true;EnterBattle();}
 void OfferTutorial(){if(!TutorialProgress.QA&&!NeedsRegistration&&!TutorialProgress.Completed&&!TutorialProgress.Offered)StartTutorial();}
}
public sealed partial class MantisDuel {
 public bool Tutorial {get;private set;}
 public int Lesson {get;private set;}
 public bool LessonStarted {get;private set;}
 public bool LessonPassed {get;private set;}
 float lessonAge,enemyAt,retryAt;int guardHits,parriesAtStart;bool baited,broken;
 string lessonHint="";
 public string TutorialTitle=>Tr("基本操作","TRAINING")+"  "+(Lesson+1)+" / 4 · "+new[]{Tr("オートガード","AUTO GUARD"),Tr("攻撃","ATTACK"),Tr("フェイント → 攻撃","FEINT → ATTACK"),Tr("パリィ → 攻撃","PARRY → ATTACK")}[Lesson];
 public static string Tr(string ja,string en)=>MantisLanguage.Japanese?ja:en;
 public string TutorialMessage=>LessonPassed?Tr("成功！ 次の練習へ進もう。","Success! Ready for the next lesson."):!LessonStarted?new[]{
 Tr("操作しない間は自動でガード。連続3回でゲージが0になり、気絶します。攻撃を受けずにいると回復します。まずは何も押さずに見てみよう。","You guard automatically while idle. Three consecutive hits break your guard and stun you. The bar recovers when you avoid hits. Watch without pressing anything."),
 Tr("ATTACKでパンチ。隙のある相手には一撃で勝てます。攻撃にもゲージを使います。相手の隙に攻撃してみよう。","Press ATTACK to punch. One hit defeats an exposed opponent. Punches also spend guard energy. Strike when the opponent is open."),
 Tr("FEINTで攻撃するふり。相手のパリィを誘い、空振り後の隙にATTACK！ フェイント中は無防備です。","Use FEINT to bait a parry. Wait for it to miss, then ATTACK! You are exposed during your feint."),
 Tr("相手のパンチに合わせてPARRY。成功すると相手が崩れます。すぐATTACKで反撃！ 空振りには隙があります。","Use PARRY just before a punch lands. A successful parry exposes the opponent. ATTACK to counter! A missed parry leaves you open.")}[Lesson]:lessonHint;
 public void TutorialNext(){
  if(!LessonStarted){LessonStarted=true;return;}
  if(!LessonPassed)return;
  if(Lesson==3){TutorialProgress.Complete();Tutorial=false;Time.timeScale=1;SceneManager.LoadScene("Home");return;}
  Lesson++;ResetLesson();
 }
 void ResetLesson(){
  NextRound();readyLeft=0;missionDemo=true;rankingMatchDemo=true;
  Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(new Vector3(0,0,1.65f),Quaternion.Euler(0,180,0));
  LessonStarted=LessonPassed=false;lessonAge=0;enemyAt=1.3f;retryAt=0;guardHits=0;baited=broken=false;parriesAtStart=Player.SuccessfulParries;
  lessonHint=Tr("準備できたら開始を押してください。","Press Start when you are ready.");
 }
 void TickTutorial(DuelAction input,float realDt){
  if(!LessonStarted||LessonPassed)return;
  if(retryAt>0){retryAt-=realDt;if(retryAt<=0){ResetLesson();LessonStarted=true;}return;}
  float dt=realDt*effects.CombatSpeed;
  if(effects.HitStop>0)return;
  lessonAge+=dt;
  if(Lesson==0){
   lessonHint=broken?Tr("ガードブレイク！ 気絶中は無防備。回復を待とう。","Guard broken! You are exposed while stunned. Wait for recovery."):Tr("何も押さずにガード。上の自分のゲージに注目！","Stay idle to guard. Watch your guard bar above!");
   if(guardHits<3&&lessonAge>=enemyAt&&Enemy.Begin(DuelAction.Attack)){guardHits++;enemyAt=lessonAge+.8f;}
  }else{
   if(Lesson==1){
    lessonHint=Tr("相手は今、無防備。ATTACKで一撃！","The opponent is exposed. Press ATTACK!");
    if(Enemy.Available)Enemy.Begin(DuelAction.Feint);
    if(input==DuelAction.Attack)Player.Begin(input);
   }
   if(Lesson==2){
    if(input==DuelAction.Feint&&Player.Begin(input)){baited=true;Enemy.Begin(DuelAction.Parry);}
    if(input==DuelAction.Attack)Player.Begin(input);
    lessonHint=!baited?Tr("FEINTで相手のパリィを誘おう。","Press FEINT to bait a parry."):Enemy.ParryActive?Tr("まだ待って！ 相手がパリィ中。","Wait! The opponent is still parrying."):Tr("今！ ATTACKで隙を突こう。","Now! ATTACK while the opponent is open.");
   }
   if(Lesson==3){
    if(Player.SuccessfulParries>parriesAtStart){
     lessonHint=Tr("パリィ成功！ 今すぐATTACK！","Parry successful! ATTACK now!");
     if(input==DuelAction.Attack)Player.Begin(input);
     if(Enemy.Available){parriesAtStart=Player.SuccessfulParries;enemyAt=lessonAge+1.3f;}
    }else{
     float until=enemyAt-lessonAge;
     lessonHint=until>.55f&&Enemy.State!=DuelState.Attack?Tr("相手が構えた。パンチを待とう。","The opponent is preparing. Watch for the punch."):Tr("今、PARRY！","PARRY now!");
     if(input==DuelAction.Parry)Player.Begin(input);
     if(lessonAge>=enemyAt&&Enemy.Begin(DuelAction.Attack))enemyAt=lessonAge+2.3f;
    }
   }
  }
  Advance(dt,Vector2.zero,Vector2.zero);
  if(Player.State==DuelState.Stunned)broken=true;
  if(Lesson==0&&broken&&Player.Available)LessonPassed=true;
  if(Lesson>0&&!Enemy.Alive&&(Lesson!=2||baited)&&(Lesson!=3||Player.SuccessfulParries>parriesAtStart))LessonPassed=true;
  if(!Player.Alive||(!Enemy.Alive&&!LessonPassed)||lessonAge>25){lessonHint=Tr("もう一度やってみよう。失敗しても大丈夫！","Let's try again. No penalty for mistakes!");retryAt=1.5f;}
 }
}
public sealed partial class BattleHud {
 Text tutorialTitle,tutorialText,tutorialButtonText;Button tutorialNext;
 void BuildTutorial(){
  if(!duel.Tutorial)return;
  float y=BattleLayout.Portrait?184:111;float width=Mathf.Min(W-32,830),x=(W-width)/2;
  Plate("Tutorial instructions",new Rect(x,y,width,184));
  tutorialTitle=Label("",x+18,y+8,width-36,30,22,cyan);
  tutorialText=Label("",x+18,y+43,width-36,86,19,Color.white,TextAnchor.UpperLeft);
  tutorialText.resizeTextForBestFit=false;tutorialText.horizontalOverflow=HorizontalWrapMode.Wrap;tutorialText.verticalOverflow=VerticalWrapMode.Truncate;
  tutorialNext=Button("",new Rect(x+width-192,y+137,174,39),duel.TutorialNext,true);
  tutorialNext.name="Tutorial next";
  tutorialButtonText=root.GetChild(root.childCount-1).GetComponent<Text>();
 }

 void UpdateTutorial(){
  if(!duel.Tutorial||!tutorialTitle)return;
  decision.text=roundMarks.text="";score.text="TRAINING";range.text=MantisDuel.Tr("練習中は間合いを固定しています","Distance is fixed during training");
  tutorialTitle.text=duel.TutorialTitle;tutorialText.text=duel.TutorialMessage;
  bool show=!duel.LessonStarted||duel.LessonPassed;
  tutorialNext.gameObject.SetActive(show);tutorialButtonText.gameObject.SetActive(show);
  tutorialButtonText.text=!duel.LessonStarted?MantisDuel.Tr("開始","START"):duel.Lesson==3?MantisDuel.Tr("完了","FINISH"):MantisDuel.Tr("次へ","NEXT");
 }
}
}
