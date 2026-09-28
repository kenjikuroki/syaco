using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 bool collectionVisible,collectionPortrait;Vector2 collectionScreen;
 Vector2 CollectionLayoutSize=>collectionPortrait?new Vector2(720,1280):new Vector2(1200,675);
 void SetPreviewOffset(float y){var camera=Object.FindFirstObjectByType<Camera>();if(camera)camera.lensShift=new Vector2(.19f,y);}
 void CollectionToCustom(){draft=MantisAppearance.Load();slot=0;DrawCustom();Message("「所持装備」から装備・取り外しを選べます。");}
 void DrawCollection(){DrawEquipmentCatalog(true);}
 IEnumerator VerifyCollection(){yield return VerifyFreeEquipment();}
}
}
