using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyShopLayout(){
  MissionStore.BeginVerification();MissionStore.Get().shells=123456;yield return new WaitForSecondsRealtime(.5f);Directory.CreateDirectory("QA/ShopLayout");bool ok=true;
  var catalog=ApplePurchases.Instance.Catalog;ok&=catalog.products.Length==4&&catalog.products.Select(p=>p.id).Distinct().Count()==4;
  for(int i=0;i<4;i++)ok&=catalog.products[i].id=="shako_shells_"+ShellEconomy.PackAmounts[i]&&catalog.products[i].shells==ShellEconomy.PackAmounts[i]&&catalog.products[i].type==UnityEngine.Purchasing.ProductType.Consumable;
  foreach(bool portrait in new[]{false,true}){
   Screen.SetResolution(portrait?720:1280,portrait?1280:720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
   System.Action[] pages={DrawHome,DrawCustom,DrawCollection,DrawMissions,DrawEquipmentShop,DrawShellShop};string[] names={"home","custom","collection","missions","equipment","shells"};Vector2 wallet=Vector2.zero;
   for(int i=0;i<pages.Length;i++){pages[i]();yield return new WaitForSecondsRealtime(.15f);var backdrop=root.Find("Navigation backdrop").GetComponent<RectTransform>();var nav=root.Find("Nav ホーム").GetComponent<RectTransform>();float top=-nav.anchoredPosition.y+backdrop.anchoredPosition.y;float bottom=backdrop.sizeDelta.y-top-nav.sizeDelta.y;ok&=Mathf.Abs(top-bottom)<.01f;
    if(i==4)wallet=root.Find("Shell wallet").GetComponent<RectTransform>().anchoredPosition;if(i==5)ok&=wallet==root.Find("Shell wallet").GetComponent<RectTransform>().anchoredPosition;
    ReviewCapture.Save(Path.GetFullPath("QA/ShopLayout/"+names[i]+(portrait?"-portrait":"-landscape")+".png"),Screen.width,Screen.height);
   }
   ShowApplePurchase(3);yield return new WaitForSecondsRealtime(.2f);ok&=ApplePurchases.Instance.Config.id=="shako_shells_16000"&&!ApplePurchases.Instance.Ready;ReviewCapture.Save(Path.GetFullPath("QA/ShopLayout/pack"+(portrait?"-portrait":"-landscape")+".png"),Screen.width,Screen.height);
  }
  File.WriteAllText("QA/ShopLayout/report.txt",ok?"PASS: four unique consumable SKUs and amounts; matching shop wallet placement; equal navigation padding on six pages in both orientations; selected pack; payments remain disabled":"FAIL");Application.Quit(ok?0:1);
 }
}
}
