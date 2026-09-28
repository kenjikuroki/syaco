using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 Coroutine rewardAnimation;AudioSource rewardSource;AudioClip rewardChime;
 void ClaimWithReaction(int tab,int id,Vector2 origin,bool all=false,bool fromHome=false){
  int before=MissionStore.Get().shells;if(all){for(int i=0;i<3;i++)MissionStore.Claim(tab,i);}else MissionStore.Claim(tab,id);
  int after=MissionStore.Get().shells;if(fromHome)DrawHome();else DrawMissions();if(after<=before)return;
  if(rewardAnimation!=null)StopCoroutine(rewardAnimation);rewardAnimation=StartCoroutine(AnimateReward(before,after,origin));
 }
 IEnumerator AnimateReward(int before,int after,Vector2 origin){
  var wallet=System.Array.Find(root.GetComponentsInChildren<RectTransform>(),r=>r.name=="Shell wallet");var number=System.Array.Find(root.GetComponentsInChildren<Text>(),t=>t.name=="Wallet value");if(!wallet||!number)yield break;
  Vector2 target=new Vector2(wallet.anchoredPosition.x+27,-wallet.anchoredPosition.y+wallet.sizeDelta.y*.5f);
  var glow=Plate("Reward wallet glow",wallet.anchoredPosition.x,-wallet.anchoredPosition.y,wallet.sizeDelta.x,wallet.sizeDelta.y,false,true);glow.color=glow.bottom=Color.clear;glow.edge=new Color(1,.78f,.25f,0);glow.raycastTarget=false;
  var amount=Label("+"+(after-before),origin.x-90,origin.y-48,180,42,29,new Color(1,.84f,.38f));amount.alignment=TextAnchor.MiddleCenter;
  var shells=new RectTransform[5];var graphics=new HomeIcon[5];for(int i=0;i<5;i++){Icon("shell",origin.x-13,origin.y-13,26,new Color(1,.83f,.38f,0));shells[i]=root.GetChild(root.childCount-1) as RectTransform;graphics[i]=shells[i].GetComponent<HomeIcon>();}
  number.text=MantisLanguage.T("貝殻 ")+before;float age=0;bool sounded=false;var numberPos=number.rectTransform.anchoredPosition;
  while(age<.85f&&wallet&&number){
   age+=Time.unscaledDeltaTime;for(int i=0;i<5;i++){float t=Mathf.Clamp01((age-i*.045f)/.46f);Vector2 control=(origin+target)*.5f+new Vector2((i-2)*20,-100-i*9);Vector2 p=(1-t)*(1-t)*origin+2*(1-t)*t*control+t*t*target;shells[i].anchoredPosition=new Vector2(p.x-13,-p.y+13);graphics[i].color=new Color(1,.83f,.38f,t<=0||t>=1?0:1);shells[i].localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI)*(i-2)*12);}
   float arrival=Mathf.Clamp01((age-.35f)/.45f),pulse=Mathf.Sin(arrival*Mathf.PI);glow.edge=new Color(1,.78f,.25f,pulse);number.color=Color.Lerp(Color.white,new Color(1,.84f,.36f),pulse);number.rectTransform.localScale=Vector3.one*(1+.07f*pulse);number.rectTransform.anchoredPosition=numberPos+new Vector2(Mathf.Sin(arrival*Mathf.PI*4)*2*pulse,0);number.text=MantisLanguage.T("貝殻 ")+Mathf.RoundToInt(Mathf.Lerp(before,after,Mathf.SmoothStep(0,1,arrival)));
   amount.rectTransform.anchoredPosition=new Vector2(origin.x-90,-origin.y+48+age*35);amount.color=new Color(1,.84f,.38f,1-Mathf.Clamp01((age-.30f)/.4f));if(!sounded&&age>=.42f){sounded=true;PlayRewardChime();}yield return null;
  }
  if(number){number.text=MantisLanguage.T("貝殻 ")+after;number.color=Color.white;number.rectTransform.localScale=Vector3.one;number.rectTransform.anchoredPosition=numberPos;}
  if(glow)Destroy(glow.gameObject);if(amount)Destroy(amount.gameObject);foreach(var shell in shells)if(shell)Destroy(shell.gameObject);rewardAnimation=null;
 }
 void PlayRewardChime(){
  if(music&&music.Muted)return;if(!rewardSource){rewardSource=gameObject.AddComponent<AudioSource>();rewardSource.spatialBlend=0;rewardSource.volume=.13f;}
  if(!rewardChime){const int rate=24000;var data=new float[7200];for(int i=0;i<data.Length;i++){float t=(float)i/rate;data[i]=Mathf.Sin(2*Mathf.PI*1320*t)*Mathf.Exp(-t*23)*.45f+Mathf.Sin(2*Mathf.PI*1980*t)*Mathf.Exp(-t*35)*.2f;}rewardChime=AudioClip.Create("Shell reward",data.Length,1,rate,false);rewardChime.SetData(data,0);}rewardSource.PlayOneShot(rewardChime);
 }
 IEnumerator VerifyRewardReaction(){
  MissionStore.BeginVerification();yield return new WaitForSecondsRealtime(2);var p=MissionStore.Get();p.daily=new[]{3,1,2};DrawMissions();yield return new WaitForSecondsRealtime(.25f);ClaimWithReaction(0,0,new Vector2(270,430));yield return new WaitForSecondsRealtime(.48f);System.IO.Directory.CreateDirectory("QA/RewardVerified");ReviewCapture.Save(System.IO.Path.GetFullPath("QA/RewardVerified/reaction.png"));yield return new WaitForSecondsRealtime(.5f);bool ok=p.shells==20&&root.Find("Wallet value").GetComponent<Text>().text.EndsWith("20");ClaimWithReaction(0,0,Vector2.zero);ok&=p.shells==20;ClaimWithReaction(0,0,new Vector2(1000,526),true);yield return new WaitForSecondsRealtime(1);ok&=p.shells==70;ClaimWithReaction(2,0,new Vector2(980,530),false,true);DrawCustom();yield return new WaitForSecondsRealtime(1);ok&=p.shells==80&&customVisible;System.IO.File.WriteAllText("QA/RewardVerified/report.txt",ok?"PASS: individual claim, count-up settles, duplicate rejected, bulk sum, login, navigation during reaction":"FAIL");Application.Quit(ok?0:1);
 }
}
}

