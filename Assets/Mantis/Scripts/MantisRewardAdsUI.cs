using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 Button rewardAdButton;Text rewardAdButtonText,rewardAdInfo;
 void DrawRewardAdCard(float w){
  var ads=RewardAdService.Instance;ads.Prepare();float x=28,y=212,cw=w-56,ch=homePortrait?128:96;Plate("Reward ad card",x,y,cw,ch);
  Label(MantisDuel.Tr("広告で貝殻 +30","Watch an ad · +30 shells"),x+20,y+10,cw-40,30,23,Color.white);
  rewardAdInfo=Label("",x+20,y+46,cw-270,homePortrait?68:38,16,cyan);
  rewardAdButton=Action("",x+cw-242,y+ch-61,222,46,()=>{int before=MissionStore.Get().shells;ads.Show((earned,message)=>{if(!this)return;if(shellShopVisible){DrawShellShop();Message(message);if(earned)rewardAnimation=StartCoroutine(AnimateReward(before,MissionStore.Get().shells,new Vector2(x+cw-131,y+ch-38)));}else Message(message);});},true,18);rewardAdButton.name="Watch rewarded ad";rewardAdButtonText=root.GetChild(root.childCount-1).GetComponent<Text>();UpdateRewardAdUI();
 }
 void UpdateRewardAdUI(){if(!rewardAdButton||!rewardAdButtonText||!rewardAdInfo)return;var ads=RewardAdService.Instance;int remaining=MissionStore.AdsRemaining;
  rewardAdButton.interactable=remaining>0&&!ads.Busy&&ads.Ready;
  rewardAdButtonText.text=remaining==0?MantisDuel.Tr("本日は受取済み","Daily limit reached"):ads.Busy?MantisDuel.Tr("視聴中…","Ad in progress…"):ads.Ready?MantisDuel.Tr("広告を見る +30","Watch ad +30"):MantisDuel.Tr("広告を準備中…","Preparing ad…");
  rewardAdInfo.text=MantisDuel.Tr("本日あと ","Remaining today: ")+remaining+" / 2"+(RewardAdSettings.TestAds?" · TEST":"")+"\n"+(string.IsNullOrEmpty(ads.Status)?MantisDuel.Tr("毎日 UTC 0:00 更新","Resets daily at 00:00 UTC"):ads.Status);if(remaining>0&&!ads.Ready&&!ads.Busy)ads.Prepare();
 }
 void RewardAdSecretTap(){if(RewardAdSettings.TapSecret()){DrawHome();ShowHomeSettings();Message(MantisDuel.Tr("テスト広告固定を保存しました。再起動後も有効です。","Test ads enabled permanently for this installation."));}}
}
}
