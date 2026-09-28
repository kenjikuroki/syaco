using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class BattleHud {
 RectTransform medalGroup;HomePlate[] medals=new HomePlate[6];Text[] medalNumbers=new Text[6];Text medalOpponent;
 void BuildRoundMedals(){
  medalGroup=Rect("Round victories",W*.5f-196,0,392,86);
  var backdrop=Plate("Round score panel",new Rect(0,0,392,86));backdrop.transform.SetParent(medalGroup,false);backdrop.color=new Color(.01f,.07f,.095f,.96f);backdrop.edge=new Color(.15f,.55f,.6f,.6f);
  var own=Label("YOU",16,7,154,20,15,cyan,TextAnchor.MiddleCenter);own.transform.SetParent(medalGroup,false);
  medalOpponent=Label(duel.OpponentName,222,7,154,20,15,new Color(1,.73f,.35f),TextAnchor.MiddleCenter);medalOpponent.supportRichText=false;medalOpponent.transform.SetParent(medalGroup,false);
  var vs=Label("VS",178,34,36,28,17,Color.white,TextAnchor.MiddleCenter);vs.transform.SetParent(medalGroup,false);
  for(int i=0;i<6;i++){float x=(i<3?22:228)+(i%3)*47;var plate=Plate("Victory medal "+i,new Rect(x,34,38,38));plate.transform.SetParent(medalGroup,false);plate.cut=10;plate.border=2;medals[i]=plate;var number=Label((i%3+1).ToString(),x,34,38,38,22,Color.white,TextAnchor.MiddleCenter);number.transform.SetParent(medalGroup,false);medalNumbers[i]=number;}
  medalGroup.gameObject.SetActive(false);
 }
 void UpdateRoundMedals(){
  roundMarks.text="";bool visible=duel.RoundOver&&duel.ResultAge>=.3f;medalGroup.gameObject.SetActive(visible);if(!visible)return;
  medalGroup.localScale=Vector3.one*(duel.MatchOver&&!BattleLayout.Portrait?.8f:1);medalGroup.anchoredPosition=duel.MatchOver?(BattleLayout.Portrait?new Vector2(W*.5f-196,-(H-392)):new Vector2(24,-(H-116))):new Vector2(W*.5f-196,-(H*.29f+100));medalOpponent.text=duel.OpponentName;
  for(int i=0;i<6;i++){int count=i<3?duel.playerWins:duel.cpuWins;bool earned=i%3<count;bool newest=earned&&i%3==count-1&&((i<3)==duel.Player.Alive);float pulse=newest?Mathf.Sin(Mathf.Clamp01((duel.ResultAge-.3f)/.55f)*Mathf.PI):0;Color accent=i<3?cyan:new Color(1,.65f,.22f);var plate=medals[i];plate.color=earned?Color.Lerp(accent,Color.white,pulse*.65f):new Color(.025f,.09f,.12f);plate.bottom=earned?accent*.45f:new Color(.01f,.03f,.05f);plate.edge=earned?Color.Lerp(accent,Color.white,pulse):new Color(.2f,.32f,.35f);plate.border=earned?2.5f:1;plate.SetVerticesDirty();medalNumbers[i].color=earned?Color.white:new Color(.36f,.48f,.5f);plate.rectTransform.localScale=medalNumbers[i].rectTransform.localScale=Vector3.one*(1+pulse*.12f);}
 }
}
}
