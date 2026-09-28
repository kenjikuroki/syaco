using UnityEngine;
using System.Collections;
using System.IO;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyUnicorn(){
  MissionStore.BeginVerification();MissionStore.Get().shells=20000;yield return new WaitForSecondsRealtime(1);var body=model.GetComponent<MantisModularBody>();var original=new Mesh[6];for(int i=0;i<6;i++)original[i]=body.parts[i].sharedMesh;
  EquipmentCatalog.Claim(3);UnicornEquipment.Save(63);DrawUnicornEquipment();yield return new WaitForSecondsRealtime(.5f);bool ok=UnicornEquipment.Mask==63&&RobotEquipment.Mask==0;int tris=0;for(int i=0;i<6;i++){ok&=body.parts[i].sharedMesh!=original[i];foreach(var b in body.parts[i].bones)ok&=b;tris+=body.parts[i].sharedMesh.triangles.Length/3;}
  Directory.CreateDirectory("QA/Unicorn");ReviewCapture.Save(Path.GetFullPath("QA/Unicorn/landscape.png"),Screen.width,Screen.height);
  model.Sample(0,0,0);var idle=new Mesh();body.parts[1].BakeMesh(idle);model.Sample(17f/41,0,0);var posed=new Mesh();body.parts[1].BakeMesh(posed);float moved=0;var a=idle.vertices;var bVerts=posed.vertices;for(int i=0;i<a.Length;i++)moved=Mathf.Max(moved,Vector3.Distance(a[i],bVerts[i]));ok&=moved>.01f;Destroy(idle);Destroy(posed);
  Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.7f);ok&=robotPage&&equipmentSet==3&&customPortrait;ReviewCapture.Save(Path.GetFullPath("QA/Unicorn/portrait.png"),Screen.width,Screen.height);
  var boxerHead=body.parts[0].sharedMesh;RobotEquipment.Save(1);DrawUnicornEquipment();ok&=UnicornEquipment.Mask==62&&RobotEquipment.Mask==1&&body.parts[0].sharedMesh!=boxerHead&&body.parts[1].sharedMesh!=original[1];
  UnicornEquipment.Save(63);DrawUnicornEquipment();ok&=RobotEquipment.Mask==0&&body.parts[0].sharedMesh==boxerHead;
  UnicornEquipment.Save(0);DrawUnicornEquipment();for(int i=0;i<6;i++)ok&=body.parts[i].sharedMesh==original[i];
  DrawCustom();yield return null;ReviewCapture.Save(Path.GetFullPath("QA/Unicorn/custom-entry.png"),Screen.width,Screen.height);
  UnicornEquipment.Save(63);BoxerEquipment.Save(2);DrawUnicornEquipment();ok&=UnicornEquipment.Mask==61&&BoxerEquipment.Mask==2&&RobotEquipment.Mask==0;
  File.WriteAllText("QA/Unicorn/report.txt",(ok?"PASS":"FAIL")+": six replacements; mapped bones; punch deformation; orientation; mixed robot head and boxer body; mutual exclusion; full restoration. Triangles="+tris);Application.Quit(ok?0:1);
 }
}
}

