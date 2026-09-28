using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyHomeDesign(){
  const string folder="QA/HomeDesignVerified";Directory.CreateDirectory(folder);File.WriteAllText(folder+"/report.txt","");int failures=0;
  void Check(bool ok,string label){File.AppendAllText(folder+"/report.txt",(ok?"PASS: ":"FAIL: ")+label+"\n");if(!ok)failures++;}
  void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception)Check(false,message);}
  Application.logMessageReceived+=Log;
  yield return new WaitForSecondsRealtime(2);yield return new WaitForEndOfFrame();
  Check(homeVisible&&!homePortrait&&model.Ready,"Landscape home and animated model ready");ReviewCapture.Save(Path.GetFullPath(folder+"/01-landscape.png"),Screen.width,Screen.height);
  var login=root.GetComponentsInChildren<Button>().Single(b=>b.name=="Claim login bonus");login.onClick.Invoke();Check(MissionStore.Get().shells==ShellEconomy.Login&&MissionStore.Get().loginClaimed,"Login card claims actual reward once");
  root.GetComponentsInChildren<Button>().Single(b=>b.name=="Claim login bonus").onClick.Invoke();Check(MissionStore.Get().shells==ShellEconomy.Login,"Claimed login reward cannot be duplicated");DrawHome();
  foreach(string tab in new[]{"カスタム","コレクション","ミッション"}){root.GetComponentsInChildren<Button>().Single(b=>b.name=="Navigate "+tab).onClick.Invoke();yield return null;Check(!homeVisible,"Navigation opens "+tab);DrawHome();yield return null;}
  ShowHomeSettings();yield return new WaitForEndOfFrame();ReviewCapture.Save(Path.GetFullPath(folder+"/02-settings.png"),Screen.width,Screen.height);DrawHome();
  Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(2);yield return new WaitForEndOfFrame();
  Check(homePortrait&&root.sizeDelta.y>root.sizeDelta.x,"Portrait layout responds to orientation change");
  Check(root.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Navigate "))==5,"Portrait keeps all five navigation actions");
  Vector3[] corners=new Vector3[4];root.GetWorldCorners(corners);Check(corners[0].x>=-1&&corners[0].y>=-1&&corners[2].x<=Screen.width+1&&corners[2].y<=Screen.height+1,"UI stays inside display safe area");
  ReviewCapture.Save(Path.GetFullPath(folder+"/03-portrait.png"),Screen.width,Screen.height);
  Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);Check(!homePortrait&&homeVisible,"Returning to landscape rebuilds home correctly");
  File.AppendAllText(folder+"/report.txt","RESULT: "+(failures==0?"PASS":"FAIL")+"\n");Application.logMessageReceived-=Log;Application.Quit(failures==0?0:1);
 }
}
}

