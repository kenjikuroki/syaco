using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
// Dedicated display treatment; normal HUD labels keep their readable UI font.
public sealed class BattleLetterGradient:BaseMeshEffect {
 public Color top=new Color(1,.96f,.72f),bottom=new Color(1,.32f,.035f);
 public override void ModifyMesh(VertexHelper vh){if(!IsActive())return;UIVertex v=new UIVertex();float min=99999,max=-99999;for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref v,i);min=Mathf.Min(min,v.position.y);max=Mathf.Max(max,v.position.y);}for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref v,i);var c=Color.Lerp(bottom,top,Mathf.InverseLerp(min,max,v.position.y));c.a=v.color.a;v.color=c;vh.SetUIVertex(v,i);}}
}
public sealed class BattlePressureRing:MaskableGraphic {
 public float phase;
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();Vector2 center=rectTransform.rect.center;float r=Mathf.Lerp(35,230,phase),thickness=Mathf.Lerp(5,1,phase);for(int i=0;i<72;i++){float a=i*Mathf.PI*2/72,b=(i+1)*Mathf.PI*2/72;int n=vh.currentVertCount;foreach(var p in new[]{new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(r-thickness),new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(r-thickness)})vh.AddVert(center+new Vector2(p.x,p.y*.38f),color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}}
}
public sealed partial class BattleHud {
 Text displayWord,winnerCaption;RectTransform displayGroup;CanvasGroup displayAlpha;BattlePressureRing pressureRing;Image flashRule;Font displayFont;
 readonly System.Collections.Generic.HashSet<string> typographyShots=new System.Collections.Generic.HashSet<string>();
 void BuildBattleTypography(){
  displayFont=Resources.Load<Font>("Fonts/BarlowCondensed-BlackItalic");
  displayGroup=Rect("Battle announcement",W*.5f,H*.3f,0,0);displayGroup.pivot=new Vector2(.5f,.5f);displayAlpha=displayGroup.gameObject.AddComponent<CanvasGroup>();displayAlpha.blocksRaycasts=false;
  var ringRect=Rect("Pressure ring",-280,-90,560,180);ringRect.SetParent(displayGroup,false);pressureRing=ringRect.gameObject.AddComponent<BattlePressureRing>();pressureRing.raycastTarget=false;
  displayWord=Label("",-W*.42f,-88,W*.84f,176,BattleLayout.Portrait?116:148,Color.white,TextAnchor.MiddleCenter);displayWord.rectTransform.SetParent(displayGroup,false);displayWord.font=displayFont?displayFont:font;displayWord.fontStyle=FontStyle.Normal;displayWord.gameObject.AddComponent<BattleLetterGradient>();var outline=displayWord.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.015f,.035f,.065f,1);outline.effectDistance=new Vector2(3,-3);var shadow=displayWord.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.005f,.025f,.055f,.85f);shadow.effectDistance=new Vector2(6,-8);
  winnerCaption=Label("",-W*.4f,-119,W*.8f,31,22,Color.white,TextAnchor.MiddleCenter);winnerCaption.supportRichText=false;winnerCaption.rectTransform.SetParent(displayGroup,false);
  var line=Rect("Impact light",-180,65,360,3);line.SetParent(displayGroup,false);flashRule=line.gameObject.AddComponent<Image>();flashRule.raycastTarget=false;
  displayGroup.gameObject.SetActive(false);
 }
 void UpdateBattleTypography(){
  bool victory=duel.MatchOver&&duel.ResultAge>=.3f;
  bool roundWin=duel.RoundOver&&!duel.MatchOver&&duel.ResultAge>=.3f;
  bool ready=!duel.RoundOver&&duel.ReadyAge>MantisDuel.FightDisplayDuration&&duel.Demo==0;
  bool fight=!duel.RoundOver&&duel.ReadyAge>0&&duel.ReadyAge<=MantisDuel.FightDisplayDuration&&duel.Demo==0;
  displayGroup.gameObject.SetActive(victory||roundWin||ready||fight);if(!victory&&!roundWin&&!ready&&!fight)return;
  bool first=duel.playerWins+duel.cpuWins==0;
  decision.text="";float age=victory||roundWin?duel.ResultAge-.3f:ready?(first?MantisDuel.FirstReadyDuration:MantisDuel.NextReadyDuration)-duel.ReadyAge:MantisDuel.FightDisplayDuration-duel.ReadyAge;
  displayWord.text=victory?"WIN":roundWin?"ROUND WIN":ready?(first?"ROUND 1":"READY"):"FIGHT";
  int size=ready?(BattleLayout.Portrait?72:94):roundWin?(BattleLayout.Portrait?82:108):(BattleLayout.Portrait?116:148);displayWord.fontSize=size;displayWord.resizeTextMaxSize=size;
  var gradient=displayWord.GetComponent<BattleLetterGradient>();gradient.top=ready?Color.white:new Color(1,.96f,.72f);gradient.bottom=ready?new Color(.18f,.83f,.94f):new Color(1,.32f,.035f);displayWord.SetVerticesDirty();
  winnerCaption.text=victory||roundWin?(duel.Player.Alive?CharacterProfile.Name:duel.OpponentName):"";
  float entry=Mathf.Clamp01(age/(fight?.12f:.24f));float scale=1+(ready?.12f:roundWin?.25f:.65f)*Mathf.Pow(1-entry,3);float shake=!ready&&age>.08f&&age<.21f?Mathf.Sin(age*150)*2*(1-age/.21f):0;
  displayGroup.anchoredPosition=new Vector2(W*.5f+shake,-H*(victory?.26f:roundWin?.29f:.36f));displayGroup.localScale=Vector3.one*scale;displayGroup.localRotation=Quaternion.Euler(0,0,-3);
  displayAlpha.alpha=fight?Mathf.Min(Mathf.Clamp01(age/.025f),Mathf.Clamp01(duel.ReadyAge/.07f)):ready?Mathf.Min(Mathf.Clamp01(age/.12f),Mathf.Clamp01((duel.ReadyAge-MantisDuel.FightDisplayDuration)/.1f)):Mathf.Clamp01(age/.05f);
  pressureRing.phase=Mathf.Clamp01(age/.48f);pressureRing.color=new Color(.35f,.95f,1,Mathf.Max(0,1-age/.48f)*.65f);pressureRing.SetVerticesDirty();
  flashRule.color=new Color(1,.88f,.45f,Mathf.Clamp01(1-age/.5f));flashRule.rectTransform.localScale=new Vector3(Mathf.Lerp(.2f,1.5f,Mathf.Clamp01(age/.4f)),1,1);
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-finishTest")>=0&&age>(victory?1.3f:.09f)){string shot=(victory?"win-type":roundWin?"round-win-type":ready?"ready-type":"fight-type")+(BattleLayout.Portrait?"-portrait":"-landscape");if(typographyShots.Add(shot))StartCoroutine(CaptureTypography(shot));}
 }
 System.Collections.IEnumerator CaptureTypography(string name){yield return new WaitForEndOfFrame();System.IO.Directory.CreateDirectory("QA/FinishVerified");ReviewCapture.Save(System.IO.Path.GetFullPath("QA/FinishVerified/"+name+".png"),Screen.width,Screen.height);}
}
}
