using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
namespace MantisPunch {
public sealed class RewardAdService:MonoBehaviour {
 static RewardAdService instance;
 public static RewardAdService Instance {get{if(!instance){instance=new GameObject("Reward ads").AddComponent<RewardAdService>();DontDestroyOnLoad(instance.gameObject);}return instance;}}
 RewardedAd ad;bool initialized,initializing,loading;int generation;float loadedAt,retryAt;Action<bool,string> completion;string receipt;bool rewarded;float oldTime;bool oldAudio;
 public bool Busy{get;private set;}
 public bool Simulated=>!RewardAdSettings.DeviceSupported||Application.isEditor;
 public bool Ready=>!Busy&&(Simulated||(ad!=null&&ad.CanShowAd()&&Time.realtimeSinceStartup-loadedAt<3300));
 public string Status{get;private set;}="";
 static string T(string ja,string en)=>MantisDuel.Tr(ja,en);
 void Main(Action action)=>MobileAdsEventExecutor.ExecuteInUpdate(action);
 public void Prepare(){
  if(Simulated||Busy||MissionStore.AdsRemaining==0||Time.realtimeSinceStartup<retryAt)return;
  if(initialized){Load();return;}if(initializing)return;initializing=true;
  Status=T("広告を準備中…","Preparing ad…");
  MobileAds.SetRequestConfiguration(new RequestConfiguration{AgeRestrictedTreatment=AgeRestrictedTreatment.Child,MaxAdContentRating=MaxAdContentRating.G});
  ConsentInformation.Update(new ConsentRequestParameters{TagForUnderAgeOfConsent=RewardAdSettings.ChildTreatment},error=>Main(()=>{
   if(error!=null){initializing=false;FailLoading();return;}
   ConsentForm.LoadAndShowConsentFormIfRequired(formError=>Main(()=>{
    if(formError!=null||!ConsentInformation.CanRequestAds()){initializing=false;FailLoading();return;}
    MobileAds.Initialize(status=>Main(()=>{initializing=false;if(status==null){FailLoading();return;}initialized=true;Load();}));
   }));
  }));
 }
 void FailLoading(){Status=T("広告を取得できません。後でもう一度お試しください。","Ad unavailable. Please try again later.");retryAt=Time.realtimeSinceStartup+30;}
 void Load(){
  if(loading||Busy||Ready||Time.realtimeSinceStartup<retryAt)return;
  ad?.Destroy();ad=null;loading=true;int version=generation;Status=T("広告を読み込み中…","Loading ad…");
  RewardedAd.Load(RewardAdSettings.UnitId,new AdRequest(),(loaded,error)=>Main(()=>{
   if(version!=generation){loaded?.Destroy();return;}loading=false;
   if(error!=null||loaded==null){loaded?.Destroy();FailLoading();return;}
   ad=loaded;loadedAt=Time.realtimeSinceStartup;Status="";
  }));
 }
 public void TestModeChanged(){generation++;loading=false;ad?.Destroy();ad=null;retryAt=0;if(!Busy)Prepare();}
 public void Show(Action<bool,string> done){
  if(Busy)return;if(MissionStore.AdsRemaining==0){done?.Invoke(false,T("本日の広告報酬は受取済みです。","Daily ad rewards claimed."));return;}
  if(!Ready){Prepare();done?.Invoke(false,T("広告を準備しています。少し待ってお試しください。","Preparing ad. Please try again shortly."));return;}
  Busy=true;completion=done;rewarded=false;receipt=Guid.NewGuid().ToString("N");oldTime=Time.timeScale;oldAudio=AudioListener.pause;Time.timeScale=0;AudioListener.pause=true;
  if(Simulated){ShowSimulation();return;}
  var showing=ad;ad=null;bool settled=false;string token=receipt;
  showing.OnAdFullScreenContentClosed+=()=>Main(()=>{if(settled)return;settled=true;StartCoroutine(CloseAfterCallbacks(showing));});
  showing.OnAdFullScreenContentFailed+=error=>Main(()=>{if(settled)return;settled=true;showing.Destroy();Finish(T("広告を表示できませんでした。回数は消費しません。","Ad could not be shown. No attempt used."));});
  try{showing.Show(reward=>Main(()=>{if(Busy&&receipt==token&&!rewarded)rewarded=MissionStore.GrantAdReward(token);}));}
  catch(Exception ex){Debug.LogWarning("Reward ad show failed: "+ex.GetType().Name);if(!settled){settled=true;showing.Destroy();Finish(T("広告を表示できませんでした。","Ad could not be shown."));}}
 }
 IEnumerator CloseAfterCallbacks(RewardedAd showing){yield return new WaitForSecondsRealtime(.25f);showing.Destroy();Finish("");}
 void Finish(string error){
  if(!Busy)return;bool earned=rewarded;Busy=false;Time.timeScale=oldTime;AudioListener.pause=oldAudio;var done=completion;completion=null;
  done?.Invoke(earned,earned?T("貝殻 +30 を獲得しました。","Received 30 shells."):string.IsNullOrEmpty(error)?T("視聴は完了しませんでした。回数は消費しません。","Viewing was not completed. No attempt used."):error);
  Prepare();
 }
 public bool PrivacyRequired=>!Simulated&&ConsentInformation.PrivacyOptionsRequirementStatus==PrivacyOptionsRequirementStatus.Required;
 public void Privacy(Action done){if(Busy)return;if(!PrivacyRequired){done?.Invoke();return;}ConsentForm.ShowPrivacyOptionsForm(error=>Main(()=>{TestModeChanged();done?.Invoke();}));}
 GameObject simulation;Button completeButton;float simulationAt;
 void ShowSimulation(){
  simulationAt=Time.realtimeSinceStartup;simulation=new GameObject("Simulated rewarded ad",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=simulation.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30000;var scaler=simulation.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(720,1280);scaler.matchWidthOrHeight=.5f;
  var shade=new GameObject("Shade",typeof(RectTransform),typeof(Image));shade.transform.SetParent(simulation.transform,false);var full=shade.GetComponent<RectTransform>();full.anchorMin=Vector2.zero;full.anchorMax=Vector2.one;full.offsetMin=full.offsetMax=Vector2.zero;shade.GetComponent<Image>().color=new Color(.005f,.035f,.05f,.98f);
  SimLabel(T("テスト広告の動作確認","REWARDED AD SIMULATION"),-150,30);
  SimLabel(T("PC上の疑似表示です。実際の広告ではありません。\niOSではGoogleのテスト広告を表示します。","PC simulation, not an advertisement.\niOS uses Google's test ads."),-55,20);
  completeButton=SimButton(T("視聴完了を再現（貝殻 +30）","Simulate completion (+30 shells)"),65,()=>CompleteSimulation(true));completeButton.interactable=false;
  SimButton(T("閉じる（報酬なし）","Close without reward"),145,()=>CompleteSimulation(false));
 }
 RectTransform SimRect(string name,float y,float height){var obj=new GameObject(name,typeof(RectTransform));obj.transform.SetParent(simulation.transform,false);var r=obj.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(620,height);r.anchoredPosition=new Vector2(0,-y);return r;}
 void SimLabel(string value,float y,int size){var t=SimRect(value,y,90).gameObject.AddComponent<Text>();t.font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},size);t.text=value;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;}
 Button SimButton(string value,float y,Action action){var r=SimRect(value,y,60);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.02f,.3f,.35f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(()=>action());var label=new GameObject("Label",typeof(RectTransform),typeof(Text));label.transform.SetParent(r,false);var lr=label.GetComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;var t=label.GetComponent<Text>();t.font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},20);t.text=value;t.fontSize=20;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.raycastTarget=false;return b;}
 void Update(){if(completeButton)completeButton.interactable=Time.realtimeSinceStartup-simulationAt>=3;}
 public void CompleteSimulation(bool completed){if(!Simulated||!Busy||!simulation)return;if(completed&&Time.realtimeSinceStartup-simulationAt<3)return;if(completed)rewarded=MissionStore.GrantAdReward(receipt);Destroy(simulation);simulation=null;completeButton=null;Finish("");}
 void OnDestroy(){ad?.Destroy();if(Busy){Time.timeScale=oldTime;AudioListener.pause=oldAudio;}if(instance==this)instance=null;}
}
}
