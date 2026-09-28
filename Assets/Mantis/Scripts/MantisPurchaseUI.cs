using UnityEngine;
namespace MantisPunch {
public sealed partial class MantisHome {
 void ShowApplePurchase(int pack=0){
  DrawShellShop();
  var service=ApplePurchases.Instance;service.Select(pack);service.Prepare();float w=HomeLayoutSize.x,h=HomeLayoutSize.y,dw=Mathf.Min(w-48,620),x=(w-dw)/2,y=(h-370)/2;
  Panel("Purchase backdrop",0,0,w,h,new Color(0,.025f,.035f,.98f));Plate("Apple purchase",x,y,dw,370);
  Label(service.Config.shells.ToString("N0")+NetText(" 枚"," shells"),x+24,y+20,dw-48,42,29,Color.white);Label(service.Config.id,x+24,y+74,dw-48,32,21,cyan);
  var price=Label(service.Price=="—"?ShellEconomy.ProvisionalYen[pack]+NetText("（仮）"," (draft)"):service.Price,x+24,y+122,dw-48,44,26,Color.white);
  var buy=Action(NetText("購入する","Purchase"),x+24,y+184,(dw-60)/2,48,()=>{service.Buy();},true,21);buy.interactable=service.Ready;
  Action(NetText("購入を復元","Restore purchases"),x+36+(dw-60)/2,y+184,(dw-60)/2,48,()=>{service.RestorePurchases();ShowApplePurchase(pack);},false,19).interactable=service.Catalog.paymentsEnabled;
  notice=Label(service.Status,x+24,y+242,dw-48,58,17,cyan);Action(NetText("戻る","Back"),x+24,y+310,dw-48,42,DrawShellShop,false,20);
  StartCoroutine(UpdatePurchasePanel(service,price,buy,notice));
 }
 System.Collections.IEnumerator UpdatePurchasePanel(ApplePurchases service,UnityEngine.UI.Text price,UnityEngine.UI.Button buy,UnityEngine.UI.Text state){while(price&&buy&&state){if(service.Price!="—")price.text=service.Price;buy.interactable=service.Ready;state.text=service.Status;yield return new WaitForSecondsRealtime(.2f);}}
}
}
