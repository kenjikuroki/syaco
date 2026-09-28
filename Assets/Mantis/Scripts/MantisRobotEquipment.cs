using UnityEngine;
using System.Collections;
using System.IO;
namespace MantisPunch {
public sealed partial class MantisHome {
 bool robotPage,equipmentBoxer;int equipmentSet=1;
 int EquipmentMask=>equipmentSet==3?UnicornEquipment.Mask:equipmentBoxer?BoxerEquipment.Mask:RobotEquipment.Mask;
 void SaveEquipment(int mask){if(equipmentSet==3)UnicornEquipment.Save(mask);else if(equipmentBoxer)BoxerEquipment.Save(mask);else RobotEquipment.Save(mask);}
 void DrawCurrentEquipment(){DrawEquipmentSet(equipmentSet);}
 void DrawBoxerEquipment(){DrawEquipment(true);}
 void DrawRobotEquipment(){DrawEquipment(false);}
 void DrawUnicornEquipment(){DrawEquipmentSet(3);}
 void DrawEquipment(bool boxer){DrawEquipmentSet(boxer?2:1);}
 void DrawEquipmentSet(int set){bool boxer=set==2;if(!TutorialProgress.QA&&!EquipmentCatalog.Owned(set)){DrawEquipmentShop();return;}
  Clear();customVisible=true;robotPage=true;equipmentBoxer=boxer;equipmentSet=set;customPortrait=Screen.safeArea.height>Screen.safeArea.width;customScreen=new Vector2(Screen.width,Screen.height);ApplySafeArea();
  MantisAppearance.Apply(model.GetComponent<MantisModularBody>(),draft);MantisLoadout.Apply(model.GetComponent<MantisModularBody>(),MissionStore.Get().crownEquipped);SetCharacterCamera(customPortrait);
  float w=CustomLayoutSize.x,h=CustomLayoutSize.y;
  Panel("Robot header",0,0,w,80,new Color(.005f,.035f,.045f,.85f));DrawBrand(set==3?"CUSTOM / UNICORN SET":boxer?"CUSTOM / BOXER SET":"CUSTOM / ROBOT SET");Action(MantisDuel.Tr("カスタムへ","Back"),w-162,17,136,46,DrawCustom,false,19);
  SceneLabel(set==3?"UNICORN / 03":boxer?"BOXER / 02":"ROBOT / 01",28,104,490,24,15,cyan);SceneLabel(EquipmentName(set),28,139,490,50,34,Color.white);
  float cy=customPortrait?435:488;Action("↶",30,cy,78,46,()=>model.transform.Rotate(0,45,0,Space.World));Action("パンチ確認",120,cy,240,46,()=>punch=Time.unscaledTime,false,20);Action("↷",372,cy,78,46,()=>model.transform.Rotate(0,-45,0,Space.World));
  float x=customPortrait?28:688,y=customPortrait?502:104,pw=customPortrait?664:484;Plate("Robot equipment console",x,y,pw,customPortrait?590:459);
  Label(MantisDuel.Tr("6部位を自由に着脱","SIX MODULAR PARTS"),x+24,y+16,pw-48,30,22,cyan);
  for(int i=0;i<6;i++){int index=i;float yy=y+60+i*(customPortrait?57:45);bool on=(EquipmentMask&(1<<i))!=0;Label(MantisAppearance.Slots[i],x+24,yy,pw-180,37,21,Color.white);Action(on?MantisDuel.Tr("装備中 ✓","Equipped ✓"):MantisDuel.Tr("装備する","Equip"),x+pw-154,yy,130,37,()=>{SaveEquipment(EquipmentMask^(1<<index));DrawCurrentEquipment();},on,17);}
  float by=y+(customPortrait?424:344);Action(MantisDuel.Tr("全て外す","Remove all"),x+24,by,(pw-60)/2,49,()=>{SaveEquipment(0);DrawCurrentEquipment();},false,19);Action(MantisDuel.Tr("フルセット","Equip full set"),x+36+(pw-60)/2,by,(pw-60)/2,49,()=>{SaveEquipment(63);DrawCurrentEquipment();},true,19);
  Label(EquipmentMicroBonuses.Description(set),x+24,by+61,pw-48,54,15,muted);
  DrawMenuNavigation(w,h,customPortrait,1);
 }
 IEnumerator VerifyRobot(){
  MissionStore.BeginVerification();yield return new WaitForSecondsRealtime(1);var body=model.GetComponent<MantisModularBody>();var original=new Mesh[6];for(int i=0;i<6;i++)original[i]=body.parts[i].sharedMesh;
  RobotEquipment.Save(63);DrawRobotEquipment();yield return new WaitForSecondsRealtime(.5f);bool ok=true;int tris=0;for(int i=0;i<6;i++){ok&=body.parts[i].sharedMesh!=original[i];foreach(var b in body.parts[i].bones)ok&=b;tris+=body.parts[i].sharedMesh.triangles.Length/3;}
  Directory.CreateDirectory("QA/Robot");ReviewCapture.Save(Path.GetFullPath("QA/Robot/landscape.png"),Screen.width,Screen.height);
  model.Sample(0,0,0);var idleMesh=new Mesh();body.parts[1].BakeMesh(idleMesh);model.Sample(17f/41,0,0);var punchMesh=new Mesh();body.parts[1].BakeMesh(punchMesh);float moved=0;for(int i=0;i<idleMesh.vertexCount;i++)moved=Mathf.Max(moved,Vector3.Distance(idleMesh.vertices[i],punchMesh.vertices[i]));ok&=moved>.01f;Destroy(idleMesh);Destroy(punchMesh);ReviewCapture.Save(Path.GetFullPath("QA/Robot/punch.png"),Screen.width,Screen.height);
  Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);ok&=robotPage&&customPortrait;ReviewCapture.Save(Path.GetFullPath("QA/Robot/portrait.png"),Screen.width,Screen.height);
  RobotEquipment.Save(0);DrawRobotEquipment();for(int i=0;i<6;i++)ok&=body.parts[i].sharedMesh==original[i];
  RobotEquipment.Save(2);DrawRobotEquipment();ok&=body.parts[1].sharedMesh!=original[1]&&body.parts[0].sharedMesh==original[0];
  File.WriteAllText("QA/Robot/report.txt",(ok?"PASS":"FAIL")+": six weighted replacements, removal, independent arm, portrait layout retained. Triangles="+tris);Application.Quit(ok?0:1);
 }
}
}


