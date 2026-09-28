using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace MantisPunch {
public sealed partial class MantisHome {
 string previousPage;Coroutine uiFade;GameObject outgoingUI;CanvasGroup incomingUI;
 IEnumerator VerifyMenuTransitions(){
  const string folder="QA/MenuTransitionsVerified";System.IO.Directory.CreateDirectory(folder);MissionStore.BeginVerification();yield return new WaitForSecondsRealtime(2);bool ok=true;
  for(int orientation=0;orientation<2;orientation++){
   Screen.SetResolution(orientation==0?1280:720,orientation==0?720:1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);DrawHome();yield return new WaitForSecondsRealtime(.25f);
   var pos=homeCamera.transform.position;var rot=homeCamera.transform.rotation;var shift=homeCamera.lensShift;float fov=homeCamera.fieldOfView;model.transform.Rotate(0,30,0,Space.World);var modelRot=model.transform.rotation;
   ReviewCapture.Save(System.IO.Path.GetFullPath(folder+"/home-"+orientation+".png"),Screen.width,Screen.height);
   DrawCustom();ok&=incomingUI.alpha==0&&outgoingUI;yield return new WaitForSecondsRealtime(.25f);ok&=incomingUI.alpha==1&&!outgoingUI&&homeCamera.transform.position==pos&&homeCamera.transform.rotation==rot&&homeCamera.lensShift==shift&&homeCamera.fieldOfView==fov&&model.transform.rotation==modelRot;
   ReviewCapture.Save(System.IO.Path.GetFullPath(folder+"/custom-"+orientation+".png"),Screen.width,Screen.height);DrawCustom();ok&=incomingUI.alpha==1;
   DrawCollection();yield return new WaitForSecondsRealtime(.25f);ok&=homeCamera.transform.position==pos&&homeCamera.fieldOfView==fov&&homeCamera.lensShift==shift&&model.transform.rotation==modelRot;ReviewCapture.Save(System.IO.Path.GetFullPath(folder+"/collection-"+orientation+".png"),Screen.width,Screen.height);
   DrawHome();DrawMissions();DrawRanking();DrawCustom();yield return new WaitForSecondsRealtime(.3f);ok&=customVisible&&incomingUI.alpha==1&&!outgoingUI;
  }
  System.IO.File.WriteAllText(folder+"/report.txt",ok?"PASS: identical camera across three pages in both orientations; rotation retained; fade completes; same-page edits immediate; rapid navigation cleans up":"FAIL");if(!ok){Application.Quit(1);yield break;}EnterBattle();
 }
 void SetCharacterCamera(bool portrait){
  homeCamera.gateFit=portrait?Camera.GateFitMode.Vertical:cameraGateFit;
  homeCamera.transform.position=portrait?new Vector3(3,2.1f,4.3f):new Vector3(1.98f,1.51f,2.79f);
  homeCamera.transform.LookAt(new Vector3(0,.30f,0));homeCamera.fieldOfView=portrait?44:38;homeCamera.lensShift=portrait?new Vector2(0,-.28f):new Vector2(.22f,-.03f);
 }
 void ClearPageUI(string page){
  if(uiFade!=null){StopCoroutine(uiFade);uiFade=null;}if(outgoingUI){outgoingUI.SetActive(false);Destroy(outgoingUI);outgoingUI=null;}
  if(!incomingUI)incomingUI=root.gameObject.AddComponent<CanvasGroup>();incomingUI.alpha=1;incomingUI.interactable=true;incomingUI.blocksRaycasts=true;
  bool transition=previousPage!=null&&previousPage!=page;previousPage=page;
  if(transition){
   var old=new GameObject("Outgoing page",typeof(RectTransform),typeof(CanvasGroup));outgoingUI=old;var rect=old.GetComponent<RectTransform>();rect.SetParent(root.parent,false);rect.anchorMin=root.anchorMin;rect.anchorMax=root.anchorMax;rect.pivot=root.pivot;rect.sizeDelta=root.sizeDelta;rect.anchoredPosition=root.anchoredPosition;rect.localScale=root.localScale;rect.SetSiblingIndex(root.GetSiblingIndex());
   var group=old.GetComponent<CanvasGroup>();group.interactable=false;group.blocksRaycasts=false;
   while(root.childCount>0)root.GetChild(0).SetParent(rect,false);
   incomingUI.alpha=0;uiFade=StartCoroutine(FadePage(group));
  }else foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}
 }
 IEnumerator FadePage(CanvasGroup old){
  float age=0;while(age<.18f){yield return null;age+=Time.unscaledDeltaTime;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.18f));incomingUI.alpha=t;if(old)old.alpha=1-t;}
  incomingUI.alpha=1;if(outgoingUI){Destroy(outgoingUI);outgoingUI=null;}uiFade=null;
 }
}
public sealed class MenuSceneFade:MonoBehaviour {
 public static bool Active {get;private set;}
 public static void Load(string scene){if(Active)return;Active=true;var go=new GameObject("Scene fade");DontDestroyOnLoad(go);go.AddComponent<MenuSceneFade>().StartCoroutine(Transition(go,scene));}
 static IEnumerator Transition(GameObject go,string scene){
  var canvas=go.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32760;go.AddComponent<GraphicRaycaster>();var panel=new GameObject("Darken",typeof(RectTransform),typeof(Image));panel.transform.SetParent(go.transform,false);var rect=panel.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;var img=panel.GetComponent<Image>();img.color=Color.clear;
  for(float t=0;t<.28f;t+=Time.unscaledDeltaTime){img.color=new Color(.005f,.025f,.035f,Mathf.Clamp01(t/.28f));yield return null;}img.color=new Color(.005f,.025f,.035f,1);yield return SceneManager.LoadSceneAsync(scene);yield return null;
  for(float t=0;t<.22f;t+=Time.unscaledDeltaTime){img.color=new Color(.005f,.025f,.035f,1-Mathf.Clamp01(t/.22f));yield return null;}Active=false;Destroy(go);if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-menuTransitionTest")>=0){bool ok=SceneManager.GetActiveScene().name=="Duel"&&Object.FindFirstObjectByType<MantisDuel>();System.IO.File.AppendAllText("QA/MenuTransitionsVerified/report.txt",ok?"\nPASS: scene fade completes in Duel":"\nFAIL: scene fade");Application.Quit(ok?0:1);}
 }
}
}
