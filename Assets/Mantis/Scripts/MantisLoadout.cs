using UnityEngine;
namespace MantisPunch {
public sealed class MantisLoadout:MonoBehaviour {
 Mesh[] originals;Material[][] materials;Transform[][] bones;Transform[] roots;Bounds[] bounds;MantisModularBody body;
 public bool WearingCrown {get;private set;} public int EquippedBoxerMask {get;private set;} public int EquippedRobotMask {get;private set;} public int EquippedUnicornMask {get;private set;}
 void Cache(){if(body)return;body=GetComponent<MantisModularBody>();originals=new Mesh[6];materials=new Material[6][];bones=new Transform[6][];roots=new Transform[6];bounds=new Bounds[6];for(int i=0;i<6;i++){var r=body.parts[i];originals[i]=r.sharedMesh;materials[i]=r.sharedMaterials;bones[i]=r.bones;roots[i]=r.rootBone;bounds[i]=r.localBounds;}}
 public void SetCrown(bool enabled){
  Cache();for(int i=0;i<6;i++){var r=body.parts[i];for(int m=0;m<r.sharedMaterials.Length;m++)r.SetPropertyBlock(null,m);r.sharedMesh=originals[i];r.sharedMaterials=materials[i];r.bones=bones[i];r.rootBone=roots[i];r.localBounds=bounds[i];}
  WearingCrown=false;EquippedBoxerMask=EquippedRobotMask=EquippedUnicornMask=0;
  if(enabled){var donor=Resources.Load<GameObject>("Equipment/CrownHead");if(donor&&body.Replace(MantisBodySlot.Cephalothorax,donor.GetComponentInChildren<SkinnedMeshRenderer>())){var r=body.parts[0];var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",r.sharedMaterials[1].GetColor("_BaseColor"));r.SetPropertyBlock(block,1);WearingCrown=true;}}
 }
 public void SetRobot(int mask){SetEquipment(mask,"RobotSet");}
 public void SetUnicorn(int mask){SetEquipment(mask,"UnicornSet");}
 public void SetBoxer(int mask){SetEquipment(mask,"BoxerSet");}
 void SetEquipment(int mask,string set){
  if(mask==0)return;var prefab=Resources.Load<GameObject>("Equipment/"+set);if(!prefab)return;
  var donors=prefab.GetComponentsInChildren<SkinnedMeshRenderer>();string[] names={"01_Cephalothorax","02_PunchArm_L","03_PunchArm_R","04_Abdomen","05_Tail","06_WalkingLegs"};
  for(int i=0;i<6;i++)if((mask&(1<<i))!=0){var donor=System.Array.Find(donors,r=>r.name==names[i]);if(!body.Replace((MantisBodySlot)i,donor))continue;EquippedBoxerMask&=~(1<<i);EquippedRobotMask&=~(1<<i);EquippedUnicornMask&=~(1<<i);if(set=="RobotSet")EquippedRobotMask|=1<<i;if(set=="UnicornSet")EquippedUnicornMask|=1<<i;if(set=="BoxerSet")EquippedBoxerMask|=1<<i;var r=body.parts[i];for(int m=0;m<r.sharedMaterials.Length;m++){var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",r.sharedMaterials[m].GetColor("_BaseColor"));r.SetPropertyBlock(block,m);}if(i==0)WearingCrown=false;}
 }
 public static MantisLoadout Apply(MantisModularBody body,bool crown){var loadout=body.GetComponent<MantisLoadout>();if(!loadout)loadout=body.gameObject.AddComponent<MantisLoadout>();loadout.SetCrown(crown);loadout.SetRobot(RobotEquipment.Mask);loadout.SetBoxer(BoxerEquipment.Mask);loadout.SetUnicorn(UnicornEquipment.Mask);return loadout;}
}
public static class RobotEquipment {
 static int testMask;
 public static int Mask=>TutorialProgress.QA?testMask:PlayerPrefs.GetInt("Mantis.RobotEquipment.v1",0)&63;
 public static void Save(int mask){BoxerEquipment.ClearBits(mask);UnicornEquipment.ClearBits(mask);Write(mask);}
 public static void ClearBits(int mask){Write(Mask&~mask);}
 static void Write(int mask){if(TutorialProgress.QA)testMask=mask&63;else{PlayerPrefs.SetInt("Mantis.RobotEquipment.v1",mask&63);PlayerPrefs.Save();}}
}
}

namespace MantisPunch {
public static class BoxerEquipment {
 static int testMask;
 public static int Mask=>TutorialProgress.QA?testMask:UnityEngine.PlayerPrefs.GetInt("Mantis.BoxerEquipment.v1",0)&63;
 public static void Save(int mask){RobotEquipment.ClearBits(mask);UnicornEquipment.ClearBits(mask);Write(mask);}
 public static void ClearBits(int mask){Write(Mask&~mask);}
 static void Write(int mask){if(TutorialProgress.QA)testMask=mask&63;else{UnityEngine.PlayerPrefs.SetInt("Mantis.BoxerEquipment.v1",mask&63);UnityEngine.PlayerPrefs.Save();}}
}
}
namespace MantisPunch {
public static class UnicornEquipment {
 static int testMask;
 public static int Mask=>TutorialProgress.QA?testMask:UnityEngine.PlayerPrefs.GetInt("Mantis.UnicornEquipment.v1",0)&63;
 public static void Save(int mask){RobotEquipment.ClearBits(mask);BoxerEquipment.ClearBits(mask);Write(mask);}
 public static void ClearBits(int mask){Write(Mask&~mask);}
 static void Write(int mask){if(TutorialProgress.QA)testMask=mask&63;else{UnityEngine.PlayerPrefs.SetInt("Mantis.UnicornEquipment.v1",mask&63);UnityEngine.PlayerPrefs.Save();}}
}
}

