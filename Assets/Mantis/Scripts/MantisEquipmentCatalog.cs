using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
namespace MantisPunch {
public static class EquipmentCatalog {
 
 public const int Total=4;
 public static bool Owned(int item){if(item<0||item>=Total)return false;if(item==0)return MissionStore.Get().crownOwned;if((MissionStore.Get().purchasedEquipment&(1<<item))!=0)return true;if(TutorialProgress.QA)return false;string key="Mantis.OwnedSet."+item;if(PlayerPrefs.GetInt(key,0)==1)return true;if((item==1?RobotEquipment.Mask:item==2?BoxerEquipment.Mask:UnicornEquipment.Mask)==0)return false;PlayerPrefs.SetInt(key,1);PlayerPrefs.Save();return true;}
 public static int Count=>(Owned(0)?1:0)+(Owned(1)?1:0)+(Owned(2)?1:0)+(Owned(3)?1:0);
 public static bool Claim(int item){if(item<0||item>=Total||Owned(item))return false;return MissionStore.BuyEquipment(item);}
}
public sealed partial class MantisHome {
 bool catalogVisible,catalogOwnedOnly;int catalogSelection=-1,catalogPurchase=-1;
 string EquipmentName(int i)=>i==0?MantisDuel.Tr("リーフ・クラウン","Reef Crown"):i==1?MantisDuel.Tr("ロボフルセット","Robot set"):i==2?MantisDuel.Tr("ボクサーフルセット","Boxer set"):MantisDuel.Tr("ユニコーンフルセット","Unicorn set");
 void DrawEquipmentShop(){DrawEquipmentCatalog(false);}
 void OpenOwnedEquipment(int i){if(!EquipmentCatalog.Owned(i))return;slot=0;customColors=false;DrawCustom();}
 void DrawEquipmentCatalog(bool ownedOnly){
  if(!catalogVisible||catalogOwnedOnly!=ownedOnly)catalogPurchase=-1;
  if(ownedOnly&&(!catalogVisible||!catalogOwnedOnly))catalogSelection=-1;
  Clear();catalogVisible=true;catalogOwnedOnly=ownedOnly;customVisible=true;customPortrait=Screen.safeArea.height>Screen.safeArea.width;customScreen=new Vector2(Screen.width,Screen.height);ApplySafeArea();SetCharacterCamera(customPortrait);
  float w=CustomLayoutSize.x,h=CustomLayoutSize.y;
  if(!ownedOnly)DrawShopChrome(w,customPortrait,false);else {Panel("Equipment header",0,0,w,80,new Color(.005f,.035f,.045f,.85f));DrawBrand(ownedOnly?"COLLECTION / EQUIPMENT":"SHOP / EQUIPMENT");Action("カスタムへ",w-162,17,136,46,DrawCustom,false,18);
  SceneLabel(MantisDuel.Tr("所持装備","YOUR EQUIPMENT"),28,104,620,28,18,cyan);}
  if(ownedOnly&&catalogSelection>=0&&!EquipmentCatalog.Owned(catalogSelection))catalogSelection=-1;
  if(!ownedOnly&&catalogSelection<0)catalogSelection=0;
  SceneLabel(catalogSelection<0?MantisDuel.Tr("現在の装備","CURRENT LOADOUT"):EquipmentName(catalogSelection),28,ownedOnly?144:210,620,47,34,Color.white);
  var body=model.GetComponent<MantisModularBody>();MantisAppearance.Apply(body,MantisAppearance.Load());var loadout=MantisLoadout.Apply(body,MissionStore.Get().crownEquipped);if(catalogSelection>=0){loadout.SetCrown(catalogSelection==0);if(catalogSelection==1)loadout.SetRobot(63);if(catalogSelection==2)loadout.SetBoxer(63);if(catalogSelection==3)loadout.SetUnicorn(63);}
  float cy=customPortrait?435:488;Action("↶",30,cy,78,46,()=>model.transform.Rotate(0,45,0,Space.World));Action("パンチ確認",120,cy,240,46,()=>punch=Time.unscaledTime,false,20);Action("↷",372,cy,78,46,()=>model.transform.Rotate(0,-45,0,Space.World));
  float x=customPortrait?28:688,y=customPortrait?502:ownedOnly?104:212,pw=customPortrait?664:484;Plate("Equipment catalog",x,y,pw,customPortrait?590:ownedOnly?459:351);
  if(ownedOnly)Label(MantisDuel.Tr("所持：","Owned: ")+EquipmentCatalog.Count+" / "+EquipmentCatalog.Total,x+22,y+14,pw-44,30,20,cyan);
  int row=0;for(int i=0;i<EquipmentCatalog.Total;i++){
   int item=i;bool owned=EquipmentCatalog.Owned(i);
   float yy=y+(ownedOnly?58:12)+row*(customPortrait?95:ownedOnly?75:68);row++;
   float rowHeight=customPortrait?89:ownedOnly?69:62,actionY=yy+rowHeight-38;
   Plate("Equipment item "+i,x+18,yy,pw-36,rowHeight,false,i==catalogSelection);
   Label(EquipmentName(i),x+32,yy+3,pw-64,21,20,Color.white);
   Label(owned?MantisDuel.Tr("入手済み","Owned"):ownedOnly?MantisDuel.Tr("未所持","Not owned"):ShellEconomy.Price(i).ToString("N0")+MantisDuel.Tr(" 枚"," shells"),x+32,actionY,125,28,17,cyan);
   Action(MantisDuel.Tr("試着","Preview"),x+pw-272,actionY,90,28,()=>{catalogPurchase=-1;catalogSelection=item;DrawEquipmentCatalog(ownedOnly);},false,16);
   Action(owned?MantisDuel.Tr("カスタムへ","Customize"):ownedOnly?MantisDuel.Tr("ショップへ","Shop"):(MissionStore.Get().shells<ShellEconomy.Price(i)?MantisDuel.Tr("貝殻不足","Need shells"):catalogPurchase==i?MantisDuel.Tr("購入を確定","Confirm buy"):MantisDuel.Tr("購入する","Buy")),x+pw-170,actionY,138,28,()=>{if(EquipmentCatalog.Owned(item))OpenOwnedEquipment(item);else{if(ownedOnly){catalogSelection=item;DrawEquipmentShop();return;}if(MissionStore.Get().shells<ShellEconomy.Price(item)){DrawShellShop();return;}if(catalogPurchase!=item){catalogPurchase=item;catalogSelection=item;DrawEquipmentCatalog(ownedOnly);return;}bool bought=EquipmentCatalog.Claim(item);catalogPurchase=-1;catalogSelection=item;DrawEquipmentCatalog(ownedOnly);if(bought)Message(MantisDuel.Tr("購入しました。カスタムで装備できます。","Purchased. Equip it in Customize."));}},true,18).name="Catalog acquire "+i;
  }
  if(ownedOnly&&row==0)Label(MantisDuel.Tr("ショップで装備を購入できます。","Find equipment in the shop."),x+24,y+94,pw-48,70,23,Color.white);
  Action(ownedOnly?MantisDuel.Tr("ショップへ →","Go to shop →"):MantisDuel.Tr("カスタムで装備する →","Equip in Customize →"),x+24,y+(customPortrait?500:ownedOnly?394:294),pw-48,45,()=>{if(ownedOnly)DrawEquipmentShop();else{customColors=false;DrawCustom();}},false,20);
  notice=Label(catalogSelection>=0?EquipmentMicroBonuses.Description(catalogSelection):MantisDuel.Tr("試着は保存されません。装備の変更はカスタムで行えます。","Preview is temporary. Change your equipment in Customize."),28,customPortrait?1124:565,w-56,40,16,cyan);
  DrawMenuNavigation(w,h,customPortrait,ownedOnly?1:3);
 }
 IEnumerator VerifyFreeEquipment(){
  MissionStore.BeginVerification();MissionStore.Get().shells=20000;yield return new WaitForSecondsRealtime(.7f);int shells=MissionStore.Get().shells;bool ok=true;
  Directory.CreateDirectory("QA/FreeEquipment");DrawEquipmentShop();yield return new WaitForSecondsRealtime(.4f);ReviewCapture.Save(Path.GetFullPath("QA/FreeEquipment/free-items.png"),Screen.width,Screen.height);
  for(int i=0;i<EquipmentCatalog.Total;i++){root.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Catalog acquire "+i).onClick.Invoke();ok&=!EquipmentCatalog.Owned(i)&&MissionStore.Get().shells==shells;root.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="Catalog acquire "+i).onClick.Invoke();shells-=ShellEconomy.Price(i);ok&=!EquipmentCatalog.Claim(i)&&EquipmentCatalog.Owned(i)&&MissionStore.Get().shells==shells;}
  DrawEquipmentShop();yield return new WaitForSecondsRealtime(.4f);Directory.CreateDirectory("QA/FreeEquipment");ReviewCapture.Save(Path.GetFullPath("QA/FreeEquipment/shop.png"),Screen.width,Screen.height);
  OpenOwnedEquipment(2);ok&=customVisible&&!catalogVisible&&!robotPage;BoxerEquipment.Save(63);DrawCustom();var body=model.GetComponent<MantisModularBody>();var equipped=body.parts.Select(r=>r.sharedMesh).ToArray();
  DrawEquipmentCatalog(true);ok&=EquipmentCatalog.Count==EquipmentCatalog.Total&&catalogSelection==-1;for(int i=0;i<6;i++)ok&=body.parts[i].sharedMesh==equipped[i];
  catalogSelection=0;DrawEquipmentCatalog(true);ok&=body.GetComponent<MantisLoadout>().WearingCrown;
  DrawHome();DrawEquipmentCatalog(true);ok&=catalogSelection==-1;for(int i=0;i<6;i++)ok&=body.parts[i].sharedMesh==equipped[i];
  UnicornEquipment.Save(63);RobotEquipment.Save(1);BoxerEquipment.Save(2);DrawCustom();equipped=body.parts.Select(r=>r.sharedMesh).ToArray();DrawEquipmentCatalog(true);for(int i=0;i<6;i++)ok&=body.parts[i].sharedMesh==equipped[i];
  Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);ok&=catalogVisible&&catalogOwnedOnly&&customPortrait;ReviewCapture.Save(Path.GetFullPath("QA/FreeEquipment/collection.png"),Screen.width,Screen.height);
  for(int i=0;i<6;i++)ok&=body.parts[i].sharedMesh==equipped[i];DrawHome();ok&=BoxerEquipment.Mask==2&&RobotEquipment.Mask==1&&UnicornEquipment.Mask==60;File.WriteAllText("QA/FreeEquipment/report.txt",ok?"PASS: all four priced; confirmation; exact debit once; duplicate rejected; equip route; collection; rotation; collection preserves mixed meshes; reentry clears preview; preview restores loadout":"FAIL");Application.Quit(ok?0:1);
 }
}
}





