using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 bool customVisible,customPortrait; Vector2 customScreen;
 Vector2 CustomLayoutSize=>customPortrait?new Vector2(720,1280):new Vector2(1200,675);
 void Choice(string text,float x,float y,float w,float h,bool selected,UnityEngine.Events.UnityAction action){
  Plate(text,x,y,w,h,false,selected);var t=Label(text,x+4,y,w-8,h,customPortrait?20:17,selected?cyan:Color.white);t.alignment=TextAnchor.MiddleCenter;Touch(text,x,y,w,h,action);
 }
 void DrawStyledCustom(){
  Clear();customVisible=true;customPortrait=Screen.safeArea.height>Screen.safeArea.width;customScreen=new Vector2(Screen.width,Screen.height);ApplySafeArea();
  var body=model.GetComponent<MantisModularBody>();MantisAppearance.Apply(body,draft);MantisLoadout.Apply(body,MissionStore.Get().crownEquipped);
  SetCharacterCamera(customPortrait);
  float w=CustomLayoutSize.x,h=CustomLayoutSize.y;
  Panel("Custom header shade",0,0,w,80,new Color(.005f,.035f,.045f,.85f));
  DrawBrand("C U S T O M  /  自分だけの一撃を。");
  Action(MantisDuel.Tr("ショップ","Shop"),w-320,17,146,46,DrawEquipmentShop,true,19);Action("ホームへ",w-162,17,136,46,DrawHome,false,19);
  SceneLabel("Y O U R  F I G H T E R",28,104,510,23,13,cyan);ApplySceneTextShadow(CharacterName(28,136,customPortrait?600:440,48,customPortrait?35:36));Line(28,192,160,new Color(.035f,.25f,.3f,1));
  float controlsY=customPortrait?435:488;
  Action("↶",30,controlsY,78,46,()=>model.transform.Rotate(0,45,0,Space.World));Action("パンチ確認",120,controlsY,customPortrait?210:240,46,()=>punch=Time.unscaledTime,false,20);Action("↷",customPortrait?342:372,controlsY,78,46,()=>model.transform.Rotate(0,-45,0,Space.World));
  float x=customPortrait?28:688,y=customPortrait?502:104,pw=customPortrait?664:484;
  Plate("Customization console",x,y,pw,customPortrait?601:459);float ix=x+24,iw=pw-48;
  Label("01 / 部位を選ぶ",ix,y+17,iw,26,17,cyan);
  float row=customPortrait?57:45, buttonH=customPortrait?48:37;
  for(int i=0;i<6;i++){int chosen=i;float bw=(iw-16)/3;Choice(MantisAppearance.Slots[i],ix+(i%3)*(bw+8),y+51+(i/3)*row,bw,buttonH,i==slot,()=>{slot=chosen;DrawCustom();});}
  float modeY=y+(customPortrait?177:142);Choice(MantisDuel.Tr("所持装備","Owned gear"),ix,modeY,(iw-10)/2,36,!customColors,()=>{customColors=false;DrawCustom();});Choice(MantisDuel.Tr("カラー","Colors"),ix+(iw+10)/2,modeY,(iw-10)/2,36,customColors,()=>{customColors=true;DrawCustom();});
  float saveY=y+(customPortrait?515:387);
  if(customColors){
   float colorY=modeY+48;Label(MantisDuel.Tr("標準パーツのカラー","Base part color"),ix,colorY,iw,25,17,cyan);
   for(int i=0;i<6;i++){int color=i;float bw=(iw-16)/3;Choice((draft[slot]==i?"✓ ":"")+MantisAppearance.Names[i],ix+(i%3)*(bw+8),colorY+36+(i/3)*row,bw,buttonH,draft[slot]==i,()=>{draft[slot]=color;DrawCustom();});}
   Action("初期色に戻す",ix,saveY,(iw-12)*.5f,customPortrait?62:51,()=>{draft=new int[6];DrawCustom();},false,18);Action("カラーを保存",ix+(iw-12)*.5f+12,saveY,(iw-12)*.5f,customPortrait?62:51,()=>{MantisAppearance.Save(draft);Message("カラーを保存しました。ホーム・対戦に反映されます。");},true,customPortrait?23:21);
  }else DrawOwnedPartChoices(ix,modeY+45,iw,saveY);
  notice=Label(customColors?MantisDuel.Tr("色は「カラーを保存」で反映。装備の着け外しは自動保存。","Save colors to apply. Equipment saves automatically."):(EquippedPart()>=0?EquipmentMicroBonuses.Description(EquippedPart()):MantisDuel.Tr("標準パーツに追加効果はありません。装備は自動保存。","Standard parts have no bonus. Equipment saves automatically.")),customPortrait?28:32,customPortrait?1124:565,w-56,customPortrait?40:23,customPortrait?17:14,cyan);
  DrawMenuNavigation(w,h,customPortrait,1);
 }
 IEnumerator VerifyCustomDesign(){yield return VerifyFreeEquipment();}
}
}




