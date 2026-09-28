using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
namespace MantisPunch {
[DefaultExecutionOrder(2000)] public sealed partial class BattleHud:MonoBehaviour {
 Transform playerEyeLeft,playerEyeRight,enemyEyeLeft,enemyEyeRight;
 readonly System.Collections.Generic.Dictionary<Transform,Vector3> eyeCenters=new System.Collections.Generic.Dictionary<Transform,Vector3>();
 System.Collections.Generic.List<GameObject> matchHeader=new System.Collections.Generic.List<GameObject>();Text decision,roundMarks;System.Collections.Generic.List<GameObject> combatControls=new System.Collections.Generic.List<GameObject>(); public MantisDuel duel;public bool Paused{get;private set;}
 RectTransform root,knob;Font font;Text score,range,playerStatus,enemyStatus;Image playerBar,enemyBar;GameObject modal;bool resultShown;Vector2 screen;Rect safe;RenderTexture[] portraits=new RenderTexture[2];
 Color cyan=new Color(.16f,.93f,.95f),orange=new Color(1,.43f,.08f);float S=>BattleLayout.Scale;float W=>BattleLayout.W;float H=>BattleLayout.H;
 RectTransform Rect(string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(root,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
 HomePlate Plate(string name,Rect r,bool hot=false){var p=Rect(name,r.x,r.y,r.width,r.height).gameObject.AddComponent<HomePlate>();p.color=hot?new Color(1,.55f,.08f):new Color(.02f,.21f,.25f,.95f);p.bottom=hot?new Color(.94f,.20f,.03f):new Color(.005f,.06f,.085f,.94f);p.edge=hot?new Color(1,.77f,.22f):cyan*.75f;p.cut=9;p.raycastTarget=false;return p;}
 Text Label(string value,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft){var t=Rect(value,x,y,w,h).gameObject.AddComponent<Text>();t.text=System.Text.RegularExpressions.Regex.Replace(value," {2,}"," ");t.text=MantisLanguage.T(t.text);t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(11,Mathf.FloorToInt(size*.65f));t.resizeTextMaxSize=size;t.font=font;if(value=="ATTACK"||value=="FEINT"||value=="PARRY"||value=="PAUSED"||value=="FIRST TO 3"||value.StartsWith("FINAL ")){if(!displayFont)displayFont=Resources.Load<Font>("Fonts/BarlowCondensed-BlackItalic");if(displayFont)t.font=displayFont;}t.lineSpacing=1.08f;t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;return t;}
 void Icon(string kind,float x,float y,float size,Color color){var p=Rect(kind,x,y,size,size).gameObject.AddComponent<HomeIcon>();p.kind=kind;p.color=color;p.raycastTarget=false;}
 Button Button(string name,Rect rect,UnityEngine.Events.UnityAction action,bool hot=false){var p=Plate(name,rect,hot);p.raycastTarget=true;var b=p.gameObject.AddComponent<Button>();b.targetGraphic=p;b.onClick.AddListener(action);Label(name,rect.x+7,rect.y,rect.width-14,rect.height,21,Color.white,TextAnchor.MiddleCenter);return b;}
 Image Bar(float x,float y,float w,Color color){var bg=Rect("Guard track",x,y,w,14).gameObject.AddComponent<Image>();bg.color=new Color(.04f,.12f,.15f);bg.raycastTarget=false;var bar=Rect("Guard fill",x+2,y+2,w-4,10).gameObject.AddComponent<Image>();bar.color=color;bar.raycastTarget=false;return bar;}
 IEnumerator Start(){
  font=Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","sans-serif"},24);
  var canvas=new GameObject("Battle UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().sortingOrder=20;
  root=new GameObject("Battle safe area",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(canvas.transform,false);
  if(!FindFirstObjectByType<EventSystem>())new GameObject("Battle UI input",typeof(EventSystem),typeof(InputSystemUIInputModule),typeof(XboxMenuInput));
  yield return null;portraits[0]=Portrait(duel.player.visual);portraits[1]=Portrait(duel.cpu.visual);Build();
  if(Array.IndexOf(Environment.GetCommandLineArgs(),"-battleUiTest")>=0)StartCoroutine(Verify());
  if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cpuNamesTest")>=0)StartCoroutine(VerifyCpuNames());
 }
 RenderTexture Portrait(MantisVisual source){
  var clone=Instantiate(source.gameObject,new Vector3(1000,1000,1000),source.transform.rotation);foreach(var t in clone.GetComponentsInChildren<Transform>())t.gameObject.layer=30;var v=clone.GetComponent<MantisVisual>();v.Sample(.18f,0,0);
  Transform eye=null,tail=null;foreach(var t in clone.GetComponentsInChildren<Transform>()){if(t.name=="eye_L")eye=t;if(t.name=="tail")tail=t;}
  var cam=new GameObject("Portrait camera").AddComponent<Camera>();cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.015f,.12f,.15f);cam.orthographic=true;cam.orthographicSize=.29f;
  Vector3 forward=eye.position-tail.position;forward.y=0;forward.Normalize();Vector3 target=eye.position-Vector3.up*.12f;cam.transform.position=target+forward*.95f+Vector3.up*.24f+Vector3.Cross(Vector3.up,forward)*.22f;cam.transform.LookAt(target);cam.enabled=false;
  var rt=new RenderTexture(128,128,24,RenderTextureFormat.ARGB32);rt.Create();RenderPipeline.SubmitRenderRequest(cam,new RenderPipeline.StandardRequest{destination=rt});Destroy(cam.gameObject);Destroy(clone);return rt;
 }
 void Build(){
  foreach(Transform c in root){c.gameObject.SetActive(false);Destroy(c.gameObject);}screen=new Vector2(Screen.width,Screen.height);safe=Screen.safeArea;duel.ResetInput();
  root.anchorMin=root.anchorMax=new Vector2(0,1);root.pivot=new Vector2(0,1);root.anchoredPosition=new Vector2(safe.xMin,-(Screen.height-safe.yMax));root.sizeDelta=new Vector2(W,H);root.localScale=Vector3.one*S;
  bool p=BattleLayout.Portrait;float header=p?174:103;Plate("Battle header",new Rect(0,0,W,header));
  float bw=p?(W-234)/2:(W-680)/2;float by=p?117:47;float leftX=p?88:109,rightX=p?W/2+29:W-bw-343;
  if(p){Button("Ⅱ",new Rect(20,15,61,61),()=>Pause(false));Button("戦闘終了",new Rect(W-174,15,154,61),()=>Pause(true));}
  else{Button("Ⅱ",new Rect(W-207,17,58,59),()=>Pause(false));Button("戦闘終了",new Rect(W-137,17,122,59),()=>Pause(true));}
  float avatar=p?62:77;float ay=p?91:13;var a=Rect("YOU portrait",p?16:16,ay,avatar,avatar).gameObject.AddComponent<RawImage>();a.texture=portraits[0];a.raycastTarget=false;var b=Rect("CPU portrait",p?W-avatar-16:W-326,ay,avatar,avatar).gameObject.AddComponent<RawImage>();b.texture=portraits[1];b.raycastTarget=false;
  Label(p?"YOU":"YOU / GUARD",leftX,by-30,bw,25,17,Color.white);
  var opponent=Label(duel.OpponentName,rightX,by-30,bw-38,25,17,Color.white);opponent.name="Opponent name";opponent.supportRichText=false;opponent.horizontalOverflow=HorizontalWrapMode.Overflow;
  Label(OnlineMatch.Current&&OnlineMatch.Current.Active?"PVP":"BOT",rightX+bw-33,by-30,33,25,11,cyan,TextAnchor.MiddleRight);
  playerBar=Bar(leftX,by,bw,GuardColor(duel.Player.Guard));enemyBar=Bar(rightX,by,bw,GuardColor(duel.Enemy.Guard));

  float scoreX=p?W/2-98:W/2-220;score=Label("",scoreX,p?5:7,196,37,29,Color.white,TextAnchor.MiddleCenter);Label(duel.Tutorial?"BASICS":"FIRST TO 3",scoreX,p?44:44,196,23,13,Color.white,TextAnchor.MiddleCenter);
  matchHeader.Clear();for(int c=0;c<root.childCount;c++)matchHeader.Add(root.GetChild(c).gameObject);int controlsStart=root.childCount;combatControls.Clear(); for(int i=0;i<3;i++){
   var r=BattleLayout.Action(i);Plate("Action "+i,r,i==0);Icon(i==0?"battle":i==1?"feint":"parry",r.center.x-20,r.y+(i==0?22:15),40,i==0?Color.white:cyan);
   Label(i==0?"ATTACK":i==1?"FEINT":"PARRY",r.x,r.y+(i==0?77:66),r.width,30,i==0?26:20,Color.white,TextAnchor.MiddleCenter);
   Label(i==0?"攻撃":i==1?"フェイント":"受け流し",r.x,r.y+(i==0?111:98),r.width,22,13,Color.white,TextAnchor.MiddleCenter);
   if(!p)Label(i==0?"X / J / SPACE":i==1?"Y / K":"B / L",r.x,r.y+r.height-29,r.width,19,12,Color.white,TextAnchor.MiddleCenter);
  }
  var center=BattleLayout.Stick;float radius=BattleLayout.Radius;var disc=Rect("Movement stick",center.x-radius,center.y-radius,radius*2,radius*2).gameObject.AddComponent<BattleDisc>();disc.color=new Color(.025f,.12f,.15f,.83f);disc.raycastTarget=false;
  foreach(var item in new[]{new Vector3(0,-.73f,0),new Vector3(0,.73f,180),new Vector3(-.73f,0,90),new Vector3(.73f,0,-90)}){var t=Label("▲",center.x+item.x*radius-13,center.y+item.y*radius-13,26,26,17,new Color(.38f,.64f,.69f),TextAnchor.MiddleCenter);t.rectTransform.localRotation=Quaternion.Euler(0,0,item.z);}
  knob=Rect("Stick thumb",center.x-36,center.y-36,72,72);var kd=knob.gameObject.AddComponent<BattleDisc>();kd.color=new Color(.52f,.85f,.88f,.92f);kd.rim=new Color(.8f,1,1);kd.raycastTarget=false;
  range=Label("",p?20:W*.28f,H-33,p?W-40:W*.44f,23,p?13:15,cyan,TextAnchor.MiddleCenter);
  playerStatus=Label("",0,0,240,60,23,Color.white,TextAnchor.MiddleCenter);enemyStatus=Label("",0,0,240,60,23,Color.white,TextAnchor.MiddleCenter);
  for(int c=controlsStart;c<root.childCount;c++)combatControls.Add(root.GetChild(c).gameObject); decision=Label("",20,BattleLayout.Portrait?155:111,W-40,105,BattleLayout.Portrait?58:64,Color.white,TextAnchor.MiddleCenter);decision.resizeTextForBestFit=true;decision.resizeTextMinSize=24;roundMarks=Label("",W*.25f,BattleLayout.Portrait?265:210,W*.5f,34,24,cyan,TextAnchor.MiddleCenter); modal=null;resultShown=false;BuildBattleTypography();BuildRoundMedals();BuildTutorial();if(Paused)MakeModal(false);
 }
 void Update(){if(!root||!duel.Player)return;if(screen!=new Vector2(Screen.width,Screen.height)||safe!=Screen.safeArea||resultShown&&!duel.RoundOver)Build();
  foreach(var item in matchHeader)if(item)item.SetActive(!duel.MatchOver);
  score.text=duel.playerWins+" : "+duel.cpuWins;if(rankingResultLabel)rankingResultLabel.text=OnlineRanking.ResultStatus;
  foreach(var item in combatControls)if(item)item.SetActive(!duel.RoundOver);
  foreach(var button in root.GetComponentsInChildren<Button>(true))if(button.name=="Ⅱ"||button.name=="戦闘終了")button.gameObject.SetActive(!duel.RoundOver);
  decision.fontSize=duel.MatchOver?(BattleLayout.Portrait?64:86):(BattleLayout.Portrait?34:48);decision.resizeTextMaxSize=decision.fontSize;decision.fontStyle=FontStyle.Bold;bool ended=duel.RoundOver;decision.text=ended?(duel.ResultAge<.3f?"":duel.MatchOver?(duel.Player.Alive?"PLAYER WIN":duel.OpponentName+" WIN"):(duel.Player.Alive?"ROUND / PLAYER WIN":"ROUND / "+duel.OpponentName+" WIN")):duel.ReadyAge>0?(duel.ReadyAge>MantisDuel.FightDisplayDuration?"READY":"FIGHT"):"";decision.color=duel.Player.Alive?cyan:Color.white;
  float pop=ended?Mathf.Clamp01((duel.ResultAge-.3f)/.22f):1;decision.rectTransform.localScale=Vector3.one*Mathf.Lerp(1.12f,1,Mathf.SmoothStep(0,1,pop));
  if(!ended && duel.ReadyAge>0 && duel.Demo==0){
   bool fight=duel.ReadyAge<=MantisDuel.FightDisplayDuration;bool first=duel.playerWins+duel.cpuWins==0;
   float elapsed=(first?MantisDuel.FirstReadyDuration:MantisDuel.NextReadyDuration)-duel.ReadyAge;
   decision.text=fight?"FIGHT":first?"ROUND 1":"READY";
   decision.fontSize=BattleLayout.Portrait?58:72;
   float alpha=fight?Mathf.Clamp01(duel.ReadyAge/.12f):Mathf.Clamp01(elapsed/.16f);
   decision.color=new Color(fight?1:cyan.r,fight?.55f:cyan.g,fight?.12f:cyan.b,alpha);
   decision.rectTransform.localScale=Vector3.one*Mathf.Lerp(fight?1.16f:.94f,1,Mathf.Clamp01((fight?MantisDuel.FightDisplayDuration-duel.ReadyAge:elapsed)/.18f));
  }
  UpdateBattleTypography();
  roundMarks.text=ended?new string('●',Mathf.Min(3,duel.playerWins))+new string('○',Mathf.Max(0,3-duel.playerWins))+"   :   "+new string('●',Mathf.Min(3,duel.cpuWins))+new string('○',Mathf.Max(0,3-duel.cpuWins)):"";
  roundMarks.rectTransform.anchoredPosition=new Vector2(W*.25f,-(duel.RoundOver?H*(duel.MatchOver?.26f:.29f)+105:(BattleLayout.Portrait?265:210)));
  UpdateRoundMedals();
  float bw=BattleLayout.Portrait?(W-234)/2:(W-680)/2;playerBar.rectTransform.sizeDelta=new Vector2((bw-4)*duel.Player.Guard/100,10);enemyBar.rectTransform.sizeDelta=new Vector2((bw-4)*duel.Enemy.Guard/100,10);
  playerBar.color=GuardColor(duel.Player.Guard);enemyBar.color=GuardColor(duel.Enemy.Guard);
  var c=BattleLayout.Stick+new Vector2(duel.StickInput.x,-duel.StickInput.y)*BattleLayout.Radius*.65f;knob.anchoredPosition=new Vector2(c.x-36,-c.y+36);
  range.text=duel.InReach?"IN RANGE · READ YOUR OPPONENT":"CONTROL THE DISTANCE";

  UpdateTutorial();
  if(duel.ShowResult&&!resultShown&&!Paused){resultShown=true;MakeModal(true);}
 }
 Color GuardColor(float guard)=>guard<=50?new Color(1,.8f,.2f):cyan;
 void LateUpdate(){
  if(!root||!duel.Player||!playerStatus||!enemyStatus)return;
  Status(duel.Player,playerStatus,ref playerEyeLeft,ref playerEyeRight);
  Status(duel.Enemy,enemyStatus,ref enemyEyeLeft,ref enemyEyeRight);
 }
 void Status(DuelFighter f,Text t,ref Transform left,ref Transform right){
  t.text="";
  if(f.State!=DuelState.Stunned||duel.RoundOver)return;
  if(!left||!right)foreach(var bone in f.visual.GetComponentsInChildren<Transform>()){
   if(!left&&bone.name=="eye_L")left=bone;else if(!right&&bone.name=="eye_R")right=bone;
  }
  Vector3 head=left&&right?(EyeCenter(f.visual,left)+EyeCenter(f.visual,right))*.5f:f.transform.position+Vector3.up*.65f;
  var p=duel.arenaCamera.WorldToScreenPoint(head+Vector3.up*.18f);
  if(p.z<=0)return;
  t.rectTransform.anchoredPosition=new Vector2((p.x-safe.xMin)/S-120,-(safe.yMax-p.y)/S+30);
  t.color=new Color(1,.85f,.18f);t.text="✦ ✦ ✦";
 }
 Vector3 EyeCenter(MantisVisual visual,Transform bone){
  if(!eyeCenters.TryGetValue(bone,out var local)){
   Vector3 sum=Vector3.zero;float total=0;
   foreach(var renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>()){
    var mesh=renderer.sharedMesh;if(!mesh||!mesh.isReadable)continue;
    int index=System.Array.IndexOf(renderer.bones,bone);if(index<0)continue;
    var vertices=mesh.vertices;var weights=mesh.boneWeights;var bind=mesh.bindposes[index];
    for(int i=0;i<weights.Length;i++){
     var b=weights[i];float weight=(b.boneIndex0==index?b.weight0:0)+(b.boneIndex1==index?b.weight1:0)+(b.boneIndex2==index?b.weight2:0)+(b.boneIndex3==index?b.weight3:0);
     if(weight>.5f){sum+=bind.MultiplyPoint3x4(vertices[i])*weight;total+=weight;}
    }
   }
   local=total>0?sum/total:Vector3.zero;eyeCenters[bone]=local;
  }
  return bone.TransformPoint(local);
 }
 public void Pause(bool exit){if(OnlineMatch.Current&&OnlineMatch.Current.Active){ShowOnlineExit();return;}if(Paused)return;Paused=true;duel.ResetInput();Time.timeScale=0;MakeModal(false,exit);}
 public void Resume(){Paused=false;Time.timeScale=1;duel.ResetInput();Build();}
 string adResultMessage="";Text rankingResultLabel;
 void MakeModal(bool result,bool exit=false){
  if(result){BuildResultModal();return;}
  var shade=Rect("Modal shade",0,0,W,H).gameObject.AddComponent<Image>();shade.color=new Color(0,.025f,.035f,.83f);modal=shade.gameObject;
  float width=Mathf.Min(W-48,590),x=(W-width)/2,y=H*.32f;Plate("Battle dialog",new Rect(x,y,width,321));Label(result?(duel.Player.Alive?"IPPON! YOU WIN":"CPU WINS"):exit?"戦闘を終了しますか？":"PAUSED",x+20,y+23,width-40,65,29,Color.white,TextAnchor.MiddleCenter);
  Button(result?(duel.MatchOver?"もう一度戦う":"次のラウンド"):"対戦に戻る",new Rect(x+28,y+120,width-56,67),()=>{if(result){if(duel.MatchOver)duel.Restart();else duel.NextRound();Build();}else Resume();},true);
  Button("ホームへ戻る",new Rect(x+28,y+214,width-56,62),()=>{Time.timeScale=1;SceneManager.LoadScene("Home");});
 }
 void OnDestroy(){Time.timeScale=1;foreach(var rt in portraits)if(rt){rt.Release();Destroy(rt);}}
 IEnumerator Verify(){
  const string folder="QA/BattleUIVerified";System.IO.Directory.CreateDirectory(folder);string report="";int failures=0;void Check(bool ok,string text){report+=(ok?"PASS: ":"FAIL: ")+text+"\n";if(!ok)failures++;}
  duel.Testing=true;yield return new WaitForSecondsRealtime(1);
  foreach(bool portrait in new[]{false,true}){Screen.SetResolution(portrait?720:1280,portrait?1280:720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);
   Check(BattleLayout.Portrait==portrait,"Orientation layout "+portrait);
   for(int i=0;i<3;i++)Check(duel.PointerAction(BattleLayout.ScreenPoint(BattleLayout.Action(i).center))==(i==0?DuelAction.Attack:i==1?DuelAction.Feint:DuelAction.Parry),"Visible action hit area "+i);
   var c=new MantisControls{BattleLayoutEnabled=true};c.RoutePointer(41,c.StickCenter,TouchPhase.Began);c.RoutePointer(41,c.StickCenter+Vector2.right*c.Radius,TouchPhase.Moved);c.RoutePointer(42,BattleLayout.ScreenPoint(BattleLayout.Action(0).center),TouchPhase.Began);Check(c.Stick.x>.9f&&c.MovePointer==41,"Second action finger retains movement capture");c.RoutePointer(41,c.StickCenter,TouchPhase.Ended);Check(c.Stick==Vector2.zero,"Stick releases");
   duel.Player.Show("GUARD");yield return null;Check(playerStatus.text=="","No floating combat text");typeof(DuelFighter).GetProperty("State").SetValue(duel.Player,DuelState.Stunned);yield return null;yield return new WaitForEndOfFrame();Check(playerStatus.text=="✦ ✦ ✦"&&playerEyeLeft&&playerEyeRight,"Stars use eye bones without English label");ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath(folder+(portrait?"/stars-native-portrait.png":"/stars-native-landscape.png")));yield return new WaitForSecondsRealtime(.15f);ReviewCapture.Save(System.IO.Path.GetFullPath(folder+(portrait?"/stars-portrait.png":"/stars-landscape.png")),Screen.width,Screen.height);typeof(DuelFighter).GetProperty("State").SetValue(duel.Player,DuelState.Guard);
   yield return new WaitForEndOfFrame();ReviewCapture.Save(System.IO.Path.GetFullPath(folder+(portrait?"/02-portrait.png":"/01-landscape.png")),Screen.width,Screen.height);
  }
  duel.Testing=false;duel.Player.Begin(DuelAction.Attack);Pause(false);float age=duel.Player.Age;yield return new WaitForSecondsRealtime(.2f);Check(Time.timeScale==0&&duel.Player.Age==age,"Pause freezes combat");Resume();duel.Testing=true;Check(Time.timeScale==1&&!Paused,"Resume restores time");
  System.IO.File.WriteAllText(folder+"/report.txt",report+"RESULT: "+(failures==0?"PASS":"FAIL"));Application.Quit(failures==0?0:1);
 }
}
}






