using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
namespace MantisPunch {
public sealed partial class MantisHome : MonoBehaviour {
 public MantisVisual model;
 Font font; RectTransform root; Text notice; float punch=-10; bool busy; int slot; int[] draft; Quaternion modelRotation; HomeMusic music;
 readonly Color ink=new Color(.035f,.105f,.14f,.96f), mint=new Color(.36f,.94f,.78f), muted=new Color(.78f,.86f,.89f);
 void Start(){
  if(System.Array.Exists(System.Environment.GetCommandLineArgs(),a=>a.StartsWith("-")&&a.EndsWith("Test"))){CharacterProfile.BeginVerification();if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-onboardingTest")<0)CharacterProfile.Save("Test Mantis");}
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-tutorialTest")>=0){TutorialProgress.Requested=true;SceneManager.LoadScene("Duel");return;}
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-cpuVarietyTest")>=0){MissionStore.BeginVerification();RankingStore.BeginVerification();SceneManager.LoadScene("Duel");return;}
  Time.timeScale=1; if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-finishTest")>=0){MissionStore.BeginVerification();RankingStore.BeginVerification();SceneManager.LoadScene("Duel");return;}
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-presentationTest")>=0||System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-battleUiTest")>=0||System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-cpuNamesTest")>=0){MissionStore.BeginVerification();SceneManager.LoadScene("Duel");return;}
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-homeUiTest")>=0)MissionStore.BeginVerification();
  font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Noto Sans CJK JP","sans-serif"},24);
  var canvas=new GameObject("Home UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
  var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1200,750); scaler.matchWidthOrHeight=.5f;
  root=new GameObject("Safe area",typeof(RectTransform)).GetComponent<RectTransform>(); root.SetParent(canvas.transform,false); ApplySafeArea();
  new GameObject("UI input",typeof(EventSystem),typeof(InputSystemUIInputModule),typeof(XboxMenuInput));
  draft=MantisAppearance.Load(); modelRotation=model.transform.rotation; MantisAppearance.Apply(model.GetComponent<MantisModularBody>(),draft); music=GetComponent<HomeMusic>(); CacheHomeCamera(); DrawHome();OfferTutorial();
  if(OnlineMatch.OpenSearchOnHome){OnlineMatch.OpenSearchOnHome=false;ShowBattleOptions();StartOnline(MatchKind.Public);}
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-networkHostTest")>=0||System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-networkClientTest")>=0){MissionStore.BeginVerification();RankingStore.BeginVerification();OnlineMatch.Instance.gameObject.AddComponent<NetworkVerification>();}
  if(System.Array.Exists(System.Environment.GetCommandLineArgs(),a=>a=="-adsPersistSetTest"||a=="-adsPersistReadTest"))StartCoroutine(VerifyAdPersistence());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-rewardAdsTest")>=0)StartCoroutine(VerifyRewardAds());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-onlineUiTest")>=0)StartCoroutine(VerifyOnlineUI());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-shopLayoutTest")>=0)StartCoroutine(VerifyShopLayout());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-boxerBonusTest")>=0)StartCoroutine(VerifyBoxerBonusUI());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-equipmentFlowTest")>=0)StartCoroutine(VerifyEquipmentFlow());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-economyTest")>=0)StartCoroutine(VerifyEconomy());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-freeEquipmentTest")>=0)StartCoroutine(VerifyFreeEquipment());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-unicornTest")>=0)StartCoroutine(VerifyUnicorn());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-boxerTest")>=0)StartCoroutine(VerifyBoxer());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-robotTest")>=0)StartCoroutine(VerifyRobot());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-homeUiTest")>=0)StartCoroutine(VerifyHomeDesign());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-crownTest")>=0)StartCoroutine(VerifyCrown());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-missionUiTest")>=0)StartCoroutine(VerifyMissionDesign());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-missionTest")>=0)StartCoroutine(VerifyMissions());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-collectionTest")>=0)StartCoroutine(VerifyCollection());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-customUiTest")>=0)StartCoroutine(VerifyCustomDesign());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-customTest")>=0)StartCoroutine(VerifyCustom());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-rankingTest")>=0)StartCoroutine(VerifyRanking());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-menuTransitionTest")>=0)StartCoroutine(VerifyMenuTransitions());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-xboxTest")>=0)StartCoroutine(VerifyXbox());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-rewardTest")>=0)StartCoroutine(VerifyRewardReaction());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-shopUiTest")>=0)StartCoroutine(VerifyShopDesign());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-onboardingTest")>=0)StartCoroutine(VerifyOnboardingShop());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-localizationTest")>=0)StartCoroutine(VerifyLocalization());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-uiLayoutTest")>=0)StartCoroutine(VerifyUILayout());
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-homeTest")>=0)StartCoroutine(Verify());
 }
 void DrawHome(){ DrawStyledHome();if(NeedsRegistration)ShowCharacterName(); }
 void Clear([System.Runtime.CompilerServices.CallerMemberName] string page=null){if(catalogVisible){MantisLoadout.Apply(model.GetComponent<MantisModularBody>(),MissionStore.Get().crownEquipped);catalogVisible=false;}robotPage=false;shellShopVisible=false;if(shopVisible){shopVisible=false;RestoreHomeCamera();MantisLoadout.Apply(model.GetComponent<MantisModularBody>(),MissionStore.Get().crownEquipped);}if(collectionVisible){RestoreHomeCamera();collectionVisible=false;MantisLoadout.Apply(model.GetComponent<MantisModularBody>(),MissionStore.Get().crownEquipped);}if(homeVisible||customVisible)RestoreHomeCamera();homeVisible=false;customVisible=false;missionVisible=false;rankingVisible=false;ClearPageUI(page);}
 void DrawCustom(){DrawStyledCustom();}
 IEnumerator VerifyCustom(){
  yield return new WaitForSecondsRealtime(3);System.IO.Directory.CreateDirectory("QA/CustomVerified");
  draft=MantisAppearance.Load();DrawCustom();yield return new WaitForSecondsRealtime(1);
  ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/CustomVerified/custom.png"));
  int[] original=MantisAppearance.Load();draft=new[]{1,2,2,1,3,4};DrawCustom();MantisAppearance.Save(draft);
  yield return new WaitForSecondsRealtime(.5f);punch=Time.unscaledTime;yield return new WaitForSecondsRealtime(.35f);
  ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/CustomVerified/punch.png"));yield return new WaitForSecondsRealtime(1);
  bool saved=string.Join(",",MantisAppearance.Load())==string.Join(",",draft);
  DrawHome();yield return new WaitForSecondsRealtime(1);ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/CustomVerified/home.png"));
  bool applied=CheckAppearance(model.GetComponent<MantisModularBody>(),draft);
  System.IO.File.WriteAllText("QA/CustomVerified/report.txt",saved&&applied&&music.Playing&&music.loop.name=="DeepReefLoop"?"PASS: six-part colors save, apply at home, and separate home loop plays":"FAIL: appearance or home music");
  // Restore the user's saved appearance before leaving automated verification.
  MantisAppearance.Save(original);yield return new WaitForSecondsRealtime(1);EnterBattle();
 }
 public static bool CheckAppearance(MantisModularBody body,int[] values){for(int i=0;i<6;i++){var b=new MaterialPropertyBlock();body.parts[i].GetPropertyBlock(b);if(b.GetColor("_BaseColor")!=MantisAppearance.Colors[values[i]])return false;}return true;}
 void ApplySafeArea(){var s=Screen.safeArea;float scale=root.GetComponentInParent<Canvas>().scaleFactor;root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.pivot=new Vector2(.5f,.5f);Vector2 layout=homeVisible?HomeLayoutSize:customVisible?CustomLayoutSize:collectionVisible?CollectionLayoutSize:missionVisible?MissionLayoutSize:rankingVisible?RankingLayoutSize:shopVisible?ShopLayoutSize:new Vector2(1200,750);root.sizeDelta=layout;root.anchoredPosition=(s.center-new Vector2(Screen.width,Screen.height)*.5f)/scale;root.localScale=Vector3.one*(Mathf.Min(s.width/layout.x,s.height/layout.y)/scale);}
 void Update(){UpdateRewardAdUI();UpdateHomeLayout();if(shopVisible&&shopScreen!=new Vector2(Screen.width,Screen.height))DrawCrownShop(crownCollection);if(rankingVisible&&(rankingScreen!=new Vector2(Screen.width,Screen.height)||rankingDay!=System.DateTime.UtcNow.ToString("yyyy-MM-dd")))DrawRanking();if(collectionVisible&&collectionScreen!=new Vector2(Screen.width,Screen.height))DrawCollection();if(customVisible&&customScreen!=new Vector2(Screen.width,Screen.height)){if(catalogVisible)DrawEquipmentCatalog(catalogOwnedOnly);else if(robotPage)DrawCurrentEquipment();else DrawCustom();}if(missionVisible&&(missionDate!=System.DateTime.Now.ToString("yyyy-MM-dd")||missionScreen!=new Vector2(Screen.width,Screen.height)))DrawMissions();if(root)ApplySafeArea(); if(model){float t=Time.unscaledTime-punch;model.Sample(t>=0&&t<.9f?t/.9f:0,.13f,Time.unscaledTime);}}
 RectTransform Rect(string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(root,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
 void Panel(string name,float x,float y,float w,float h,Color c){Rect(name,x,y,w,h).gameObject.AddComponent<Image>().color=c;}
 Text Label(string value,float x,float y,float w,float h,int size,Color c){var t=Rect(value,x,y,w,h).gameObject.AddComponent<Text>();t.font=font;t.text=System.Text.RegularExpressions.Regex.Replace(value,@"\b(?:[A-Z] ){2,}[A-Z]\b",m=>m.Value.Replace(" ",""));t.text=System.Text.RegularExpressions.Regex.Replace(t.text," {2,}"," ");t.text=MantisLanguage.T(t.text);t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(11,Mathf.FloorToInt(size*.65f));t.resizeTextMaxSize=size;t.fontSize=size;t.alignment=t.text.Contains("\n")?TextAnchor.UpperLeft:TextAnchor.MiddleLeft;t.lineSpacing=1.08f;if(size>=30)t.fontStyle=FontStyle.Bold;t.color=c;t.raycastTarget=false;return t;}
 Button Button(string value,float x,float y,float w,float h,UnityEngine.Events.UnityAction action,Color c){var r=Rect(value,x,y,w,h);var img=r.gameObject.AddComponent<Image>();img.color=c;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=img;b.onClick.AddListener(action);var t=Label(value,x,y,w,h,22,Color.white);t.alignment=TextAnchor.MiddleCenter;return b;}
 void Message(string text){if(notice)notice.text=MantisLanguage.T(text);}
 public void EnterBattle(){if(busy)return;if(!TutorialProgress.QA&&TutorialProgress.Completed){ShowBattleOptions();return;}if(!TutorialProgress.QA&&!TutorialProgress.Completed)TutorialProgress.Requested=true;busy=true;Time.timeScale=1; if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-finishTest")>=0){MissionStore.BeginVerification();RankingStore.BeginVerification();SceneManager.LoadScene("Duel");return;}MenuSceneFade.Load("Duel");}
 IEnumerator Verify(){yield return new WaitForSecondsRealtime(3);System.IO.Directory.CreateDirectory("QA/HomeVerified");ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/HomeVerified/home.png"));yield return new WaitForSecondsRealtime(1);bool ready=model&&model.Ready;punch=Time.unscaledTime;yield return new WaitForSecondsRealtime(.35f);ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/HomeVerified/punch.png"));yield return new WaitForSecondsRealtime(1);System.IO.File.WriteAllText("QA/HomeVerified/home.txt",ready?"PASS: home model and animation ready; UI created":"FAIL: model not ready");EnterBattle();}
}
public sealed class HomeReturn : MonoBehaviour {
 IEnumerator Start(){var duel=FindFirstObjectByType<MantisDuel>();MantisAppearance.Apply(duel.player.visual.GetComponent<MantisModularBody>(),MantisAppearance.Load());MantisLoadout.Apply(duel.player.visual.GetComponent<MantisModularBody>(),MissionStore.Get().crownEquipped); if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-customTest")>=0){yield return new WaitForSecondsRealtime(3);bool customOk=MantisHome.CheckAppearance(duel.player.visual.GetComponent<MantisModularBody>(),MantisAppearance.Load())&&duel.GetComponent<ReefMusic>().Playing&&!FindFirstObjectByType<HomeMusic>();System.IO.File.AppendAllText("QA/CustomVerified/report.txt",customOk?"\nPASS: player appearance in combat; battle music starts; home music removed":"\nFAIL: combat transition");yield return new WaitForSecondsRealtime(1);Application.Quit(customOk?0:1);yield break;} if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-homeTest")<0)yield break;yield return new WaitForSecondsRealtime(3);var d=FindFirstObjectByType<MantisDuel>();bool ok=d&&d.Player&&d.Enemy;System.IO.File.AppendAllText("QA/HomeVerified/home.txt",ok?"\nPASS: home to playable CPU duel":"\nFAIL: duel transition");ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/HomeVerified/battle.png"));yield return new WaitForSecondsRealtime(1);Application.Quit(ok?0:1);}

}
}




















