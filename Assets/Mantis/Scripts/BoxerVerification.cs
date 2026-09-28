using UnityEngine;
using System.Collections;
using System.IO;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyBoxer(){
  MissionStore.BeginVerification();MissionStore.Get().shells=20000;yield return new WaitForSecondsRealtime(1);var body=model.GetComponent<MantisModularBody>();var original=new Mesh[6];for(int i=0;i<6;i++)original[i]=body.parts[i].sharedMesh;
  BoxerEquipment.Save(63);DrawBoxerEquipment();yield return new WaitForSecondsRealtime(.5f);bool ok=BoxerEquipment.Mask==63&&RobotEquipment.Mask==0;int tris=0;for(int i=0;i<6;i++){ok&=body.parts[i].sharedMesh!=original[i];foreach(var b in body.parts[i].bones)ok&=b;tris+=body.parts[i].sharedMesh.triangles.Length/3;}
  Directory.CreateDirectory("QA/Boxer");ReviewCapture.Save(Path.GetFullPath("QA/Boxer/landscape.png"),Screen.width,Screen.height);
  model.Sample(0,0,0);var idle=new Mesh();body.parts[1].BakeMesh(idle);model.Sample(17f/41,0,0);var posed=new Mesh();body.parts[1].BakeMesh(posed);float moved=0;var a=idle.vertices;var bVerts=posed.vertices;for(int i=0;i<a.Length;i++)moved=Mathf.Max(moved,Vector3.Distance(a[i],bVerts[i]));ok&=moved>.01f;Destroy(idle);Destroy(posed);
  Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.7f);ok&=robotPage&&equipmentBoxer&&customPortrait;ReviewCapture.Save(Path.GetFullPath("QA/Boxer/portrait.png"),Screen.width,Screen.height);
  var boxerHead=body.parts[0].sharedMesh;RobotEquipment.Save(1);DrawBoxerEquipment();ok&=BoxerEquipment.Mask==62&&RobotEquipment.Mask==1&&body.parts[0].sharedMesh!=boxerHead&&body.parts[1].sharedMesh!=original[1];
  BoxerEquipment.Save(63);DrawBoxerEquipment();ok&=RobotEquipment.Mask==0&&body.parts[0].sharedMesh==boxerHead;
  BoxerEquipment.Save(0);DrawBoxerEquipment();for(int i=0;i<6;i++)ok&=body.parts[i].sharedMesh==original[i];
  DrawCustom();yield return null;ReviewCapture.Save(Path.GetFullPath("QA/Boxer/custom-entry.png"),Screen.width,Screen.height);
  File.WriteAllText("QA/Boxer/report.txt",(ok?"PASS":"FAIL")+": six replacements; mapped bones; punch deformation; orientation; mixed robot head and boxer body; mutual exclusion; full restoration. Triangles="+tris);Application.Quit(ok?0:1);
 }
}
}

