using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyEquipmentFlow(){
  MissionStore.BeginVerification();Directory.CreateDirectory("QA/EquipmentFlow");yield return new WaitForSecondsRealtime(.6f);slot=0;customColors=false;DrawCustom();bool ok=customVisible&&!catalogVisible&&root.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Equip owned part "))==1;
  MissionStore.Get().shells=20000;for(int i=0;i<4;i++)EquipmentCatalog.Claim(i);int balance=MissionStore.Get().shells;DrawCustom();
  root.GetComponentsInChildren<Button>().Single(b=>b.name=="Equip owned part 3").onClick.Invoke();ok&=UnicornEquipment.Mask==1&&customVisible&&!catalogVisible;
  slot=1;DrawCustom();root.GetComponentsInChildren<Button>().Single(b=>b.name=="Equip owned part 2").onClick.Invoke();ok&=UnicornEquipment.Mask==1&&BoxerEquipment.Mask==2;
  slot=3;DrawCustom();EquipCustomPart(1);ok&=RobotEquipment.Mask==8&&UnicornEquipment.Mask==1&&BoxerEquipment.Mask==2;
  slot=0;DrawCustom();EquipCustomPart(0);ok&=MissionStore.Get().crownEquipped&&UnicornEquipment.Mask==0&&BoxerEquipment.Mask==2&&RobotEquipment.Mask==8;
  EquipCustomPart(-1);ok&=!MissionStore.Get().crownEquipped&&BoxerEquipment.Mask==2&&RobotEquipment.Mask==8;
  EquipCustomPart(3);root.GetComponentsInChildren<Button>().Single(b=>b.name=="Equip current full set").onClick.Invoke();ok&=UnicornEquipment.Mask==63&&BoxerEquipment.Mask==0&&RobotEquipment.Mask==0;
  yield return new WaitForSecondsRealtime(.5f);ReviewCapture.Save(Path.GetFullPath("QA/EquipmentFlow/custom-landscape.png"),Screen.width,Screen.height);
  DrawCollection();ok&=catalogOwnedOnly;OpenOwnedEquipment(2);ok&=customVisible&&!catalogVisible&&UnicornEquipment.Mask==63;
  DrawEquipmentShop();ok&=catalogVisible&&!catalogOwnedOnly;DrawShellShop();ok&=shellShopVisible;DrawEquipmentShop();ok&=catalogVisible&&!shellShopVisible;yield return new WaitForSecondsRealtime(.4f);ReviewCapture.Save(Path.GetFullPath("QA/EquipmentFlow/shop.png"),Screen.width,Screen.height);
  DrawCustom();Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);ok&=customVisible&&!catalogVisible&&customPortrait&&UnicornEquipment.Mask==63;
  ReviewCapture.Save(Path.GetFullPath("QA/EquipmentFlow/custom-portrait.png"),Screen.width,Screen.height);
  customColors=true;DrawCustom();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/EquipmentFlow/colors.png"),Screen.width,Screen.height);ok&=MissionStore.Get().shells==balance;
  File.WriteAllText("QA/EquipmentFlow/report.txt",ok?"PASS: owned-only choices; per-part mixed loadout; crown/reset; full set; collection opens customization without equipping; shop tabs; portrait; currency unchanged":"FAIL");Application.Quit(ok?0:1);
 }
}
}
