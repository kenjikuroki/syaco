using UnityEngine;
namespace MantisPunch {
public sealed partial class MantisHome {
 void DrawShopChrome(float w,bool portrait,bool shells){
  Panel("Shop header",0,0,w,80,new Color(.005f,.035f,.045f,.95f));DrawBrand("SHOP / 貝殻で、お気に入りの装備を。");Action("カスタムへ",w-162,17,136,46,DrawCustom,false,18);
  Icon("shell",28,107,37,new Color(.91f,.86f,.66f));Label(MantisDuel.Tr("ショップ","SHOP"),80,99,300,52,32,Color.white);
  float x=w-276;Plate("Shell wallet",x,104,248,46);Icon("shell",x+16,116,22,new Color(.91f,.86f,.66f));
  var balance=Label(MantisDuel.Tr("所持 ","Shells ")+MissionStore.Get().shells.ToString("N0"),x+50,111,180,32,21,cyan);balance.alignment=TextAnchor.MiddleRight;balance.name="Wallet value";
  DrawShopTabs(28,160,portrait?w-56:484,shells);
 }
}
}
