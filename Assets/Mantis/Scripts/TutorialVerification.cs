using System.Collections;
using System.IO;
using UnityEngine;
namespace MantisPunch {
public sealed partial class MantisDuel {
 IEnumerator VerifyTutorial(){
  Testing=true;MissionStore.BeginVerification();RankingStore.BeginVerification();
  string folder="QA/Tutorial";Directory.CreateDirectory(folder);string report="";bool ok=true;
  yield return new WaitForSecondsRealtime(1);
  foreach(bool portrait in new[]{false,true}){
   Screen.SetResolution(portrait?720:1280,portrait?1280:720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);
   Lesson=0;ResetLesson();yield return null;ReviewCapture.Save(Path.GetFullPath(folder+(portrait?"/portrait.png":"/landscape.png")),Screen.width,Screen.height);
   for(int lesson=0;lesson<4;lesson++){
    if(Lesson!=lesson){ok=false;break;}
    TutorialNext();float timeout=Time.realtimeSinceStartup+18;
    while(!LessonPassed&&Time.realtimeSinceStartup<timeout){
     DuelAction action=DuelAction.None;
     if(lesson==1&&Player.Available)action=DuelAction.Attack;
     if(lesson==2){if(!baited&&lessonAge>.5f)action=DuelAction.Feint;else if(baited&&Enemy.State==DuelState.Recovery&&Player.Available)action=DuelAction.Attack;}
     if(lesson==3){if(Player.SuccessfulParries>parriesAtStart)action=DuelAction.Attack;else if(enemyAt-lessonAge<.12f&&enemyAt>lessonAge)action=DuelAction.Parry;}
     TickTutorial(action,Time.deltaTime);yield return null;
    }
    bool passed=LessonPassed&&playerWins==0&&cpuWins==0;ok&=passed;report+=(passed?"PASS":"FAIL")+": lesson "+lesson+" portrait="+portrait+"\n";
    yield return null;ReviewCapture.Save(Path.GetFullPath(folder+"/lesson-"+lesson+"-"+portrait+".png"),Screen.width,Screen.height);
    if(!passed)break;
    if(lesson<3)TutorialNext();
   }
  }
  report+="RESULT: "+(ok?"PASS":"FAIL");File.WriteAllText(folder+"/report.txt",report);Application.Quit(ok?0:1);
 }
}
}
