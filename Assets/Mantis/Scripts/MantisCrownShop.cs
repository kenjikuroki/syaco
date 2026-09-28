using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 bool crownPreview=true,confirmCrown,crownCollection,shopVisible,shopPortrait;Vector2 shopScreen;
 Vector2 ShopLayoutSize=>shopPortrait?new Vector2(720,1280):new Vector2(1200,675);
 void DrawCrownShop(bool collection=false){DrawEquipmentCatalog(collection);}
 IEnumerator VerifyCrown(){yield return VerifyFreeEquipment();}
 IEnumerator VerifyShopDesign(){yield return VerifyFreeEquipment();}
}
}
