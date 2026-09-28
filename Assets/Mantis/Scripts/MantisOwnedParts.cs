using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
namespace MantisPunch {
public sealed partial class MantisHome {
 bool customColors;
 readonly Dictionary<int,RenderTexture> equipmentThumbs=new Dictionary<int,RenderTexture>();
 int EquippedPart(){int bit=1<<slot;if((UnicornEquipment.Mask&bit)!=0)return 3;if((BoxerEquipment.Mask&bit)!=0)return 2;if((RobotEquipment.Mask&bit)!=0)return 1;return slot==0&&MissionStore.Get().crownEquipped?0:-1;}
 string PartSetName(int item)=>item<0?MantisDuel.Tr("標準","Standard"):item==0?MantisDuel.Tr("王冠","Crown"):item==1?MantisDuel.Tr("ロボ","Robot"):item==2?MantisDuel.Tr("ボクサー","Boxer"):MantisDuel.Tr("ユニコーン","Unicorn");
 void EquipCustomPart(int item){
  if(item>=0&&!EquipmentCatalog.Owned(item))return;if(item==0&&slot!=0)return;
  int bit=1<<slot;RobotEquipment.ClearBits(bit);BoxerEquipment.ClearBits(bit);UnicornEquipment.ClearBits(bit);
  if(slot==0)MissionStore.EquipCrown(item==0);
  if(item==1)RobotEquipment.Save(RobotEquipment.Mask|bit);if(item==2)BoxerEquipment.Save(BoxerEquipment.Mask|bit);if(item==3)UnicornEquipment.Save(UnicornEquipment.Mask|bit);
  DrawCustom();
 }
 void DrawOwnedPartChoices(float x,float y,float w,float footer){
  Label(EquipmentMicroBonuses.Summary(BoxerEquipment.Mask,RobotEquipment.Mask,UnicornEquipment.Mask,model.GetComponent<MantisLoadout>().WearingCrown),x,y,w,25,17,cyan);
  int selected=EquippedPart(),row=0;float cw=(w-16)/3,ch=customPortrait?96:73;
  for(int i=-1;i<EquipmentCatalog.Total;i++){
   if(i>=0&&(!EquipmentCatalog.Owned(i)||(i==0&&slot!=0)))continue;int item=i;float xx=x+(row%3)*(cw+8),yy=y+31+(row/3)*(ch+8);row++;
   Plate("Owned part card "+i,xx,yy,cw,ch,false,selected==i);
   var image=Rect("Equipment thumbnail "+i,xx+6,yy+5,48,48).gameObject.AddComponent<RawImage>();image.texture=EquipmentThumbnail(i,slot);image.raycastTarget=false;
   Label(PartSetName(i),xx+58,yy+3,cw-62,48,customPortrait?19:15,Color.white);
   Label(selected==i?MantisDuel.Tr("装備中 ✓","Equipped ✓"):MantisDuel.Tr("装備する","Equip"),xx+8,yy+ch-23,cw-16,21,14,selected==i?cyan:muted);
   Touch("Equip owned part "+i,xx,yy,cw,ch,()=>EquipCustomPart(item));
  }
  if(row==1)Label(MantisDuel.Tr("この部位の所持装備はありません。","No owned gear for this part yet."),x,y+125,w,40,18,muted);
  Action(MantisDuel.Tr("この部位を標準に","Reset this part"),x,footer,(w-12)/2,customPortrait?62:51,()=>EquipCustomPart(-1),false,18);
  var full=Action(MantisDuel.Tr("セット一括装備","Equip full set"),x+(w+12)/2,footer,(w-12)/2,customPortrait?62:51,()=>{if(selected<1||!EquipmentCatalog.Owned(selected))return;MissionStore.EquipCrown(false);if(selected==1)RobotEquipment.Save(63);if(selected==2)BoxerEquipment.Save(63);if(selected==3)UnicornEquipment.Save(63);DrawCustom();},true,18);full.name="Equip current full set";full.interactable=selected>0;
 }
 RenderTexture EquipmentThumbnail(int item,int part){
  int key=(item+1)*6+part;if(equipmentThumbs.TryGetValue(key,out var cached))return cached;
  var body=model.GetComponent<MantisModularBody>();var loadout=MantisLoadout.Apply(body,false);loadout.SetCrown(item==0);if(item==1)loadout.SetRobot(63);if(item==2)loadout.SetBoxer(63);if(item==3)loadout.SetUnicorn(63);
  var source=body.parts[part];var mesh=new Mesh();source.BakeMesh(mesh);mesh.RecalculateBounds();var obj=new GameObject("Equipment thumbnail mesh");obj.layer=30;obj.transform.position=Vector3.one*1000;obj.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterials=source.sharedMaterials;
  for(int i=0;i<source.sharedMaterials.Length;i++){var block=new MaterialPropertyBlock();source.GetPropertyBlock(block,i);renderer.SetPropertyBlock(block,i);}
  var cam=new GameObject("Equipment thumbnail camera").AddComponent<Camera>();cam.enabled=false;cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.025f,.10f,.13f);cam.orthographic=true;
  Vector3 target=obj.transform.position+mesh.bounds.center;float radius=Mathf.Max(.05f,mesh.bounds.extents.magnitude);cam.orthographicSize=radius*1.05f;cam.transform.position=target+new Vector3(1,.7f,1.5f).normalized*(radius*3+1);cam.transform.LookAt(target);cam.nearClipPlane=.01f;cam.farClipPlane=radius*8+10;
  var rt=new RenderTexture(128,128,24,RenderTextureFormat.ARGB32);rt.Create();RenderPipeline.SubmitRenderRequest(cam,new RenderPipeline.StandardRequest{destination=rt});obj.SetActive(false);Destroy(cam.gameObject);Destroy(obj);Destroy(mesh);equipmentThumbs[key]=rt;MantisLoadout.Apply(body,MissionStore.Get().crownEquipped);return rt;
 }
 void OnDestroy(){foreach(var rt in equipmentThumbs.Values)if(rt){rt.Release();Destroy(rt);}}
 void DrawShopTabs(float x,float y,float width,bool shells){
  float bw=(width-8)/2;Choice(MantisDuel.Tr("装備","Equipment"),x,y,bw,36,!shells,DrawEquipmentShop);Choice(MantisDuel.Tr("貝殻","Shells"),x+bw+8,y,bw,36,shells,DrawShellShop);
 }
}
}


