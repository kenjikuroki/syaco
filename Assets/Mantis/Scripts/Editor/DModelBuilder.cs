using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MantisPunch;
public static class DModelBuilder {
 public const string Root="Assets/Mantis/Art/D/";
 public static void BuildModel(){
  AssetDatabase.Refresh();var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"D_Mantis_Game.fbx");
  importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.optimizeGameObjects=false;importer.importAnimation=true;
  importer.animationCompression=ModelImporterAnimationCompression.Off;importer.isReadable=true;
  var clips=importer.defaultClipAnimations;
  foreach(var clip in clips){clip.name="D_Underhand_RearUp_Punch";clip.loopTime=false;clip.lockRootRotation=true;clip.lockRootPositionXZ=true;clip.lockRootHeightY=true;clip.keepOriginalOrientation=true;clip.keepOriginalPositionXZ=true;clip.keepOriginalPositionY=true;}
  importer.clipAnimations=clips;importer.SaveAndReimport();
  var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"D_Mantis_Game.fbx"));instance.name="Mantis D Modular";
  var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"DShell.mat");
  if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,Root+"DShell.mat");}
  material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"D_Character_Albedo.png"));material.SetColor("_BaseColor",Color.white);material.SetFloat("_Smoothness",.25f);material.SetFloat("_Surface",0);material.SetFloat("_Cull",2);
  var body=instance.AddComponent<MantisModularBody>();string[] names={"01_Cephalothorax","02_PunchArm_L","03_PunchArm_R","04_Abdomen","05_Tail","06_WalkingLegs"};
  var renderers=instance.GetComponentsInChildren<SkinnedMeshRenderer>();if(renderers.Length!=6)throw new Exception("Expected six D slots");
  for(int i=0;i<6;i++){var r=renderers.Single(x=>x.name==names[i]);body.parts[i]=r;r.sharedMaterial=material;r.updateWhenOffscreen=true;if(r.bones.Any(b=>!b))throw new Exception("Missing D bone");}
  var visual=instance.AddComponent<MantisVisual>();visual.animator=instance.GetComponent<Animator>();if(!visual.animator)visual.animator=instance.AddComponent<Animator>();
  visual.punchClip=AssetDatabase.LoadAllAssetsAtPath(Root+"D_Mantis_Game.fbx").OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview__"));
  var bones=instance.GetComponentsInChildren<Transform>();visual.leftClub=bones.Single(b=>b.name=="dactyl_L");visual.rightClub=bones.Single(b=>b.name=="dactyl_R");visual.leftStrike=bones.Single(b=>b.name=="strike_L");visual.rightStrike=bones.Single(b=>b.name=="strike_R");
  if(bones.Count(b=>b.name.StartsWith("pleopod_"))!=10)throw new Exception("Missing swimmerets");
  PrefabUtility.SaveAsPrefabAsset(instance,Root+"Mantis_D_Modular.prefab");UnityEngine.Object.DestroyImmediate(instance);AssetDatabase.SaveAssets();
 }
 public static MantisVisual ReplaceVisual(MantisPlayer owner){
  var old=owner.visual;var oldBody=old.GetComponent<MantisModularBody>();
  // Match body length, excluding antenna tips, so combat distances stay unchanged.
  float oldLength=BodyLength(old.transform,oldBody);var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Mantis_D_Modular.prefab"));
  go.transform.SetParent(owner.transform,false);go.transform.localPosition=old.transform.localPosition;go.transform.localRotation=old.transform.localRotation;go.transform.localScale=old.transform.localScale;
  var body=go.GetComponent<MantisModularBody>();go.transform.localScale*=oldLength/BodyLength(go.transform,body);
  // Align the walking surface, preserving the previous actor root and arena placement.
  float floor=oldBody?oldBody.parts[5].bounds.min.y:old.GetComponentInChildren<Renderer>().bounds.min.y;
  go.transform.position+=Vector3.up*(floor-body.parts[5].bounds.min.y);
  owner.visual=go.GetComponent<MantisVisual>();UnityEngine.Object.DestroyImmediate(old.gameObject);return owner.visual;
 }
 static float BodyLength(Transform root,MantisModularBody body){
  if(!body)return root.GetComponentInChildren<Renderer>().bounds.size.z;
  var eye=root.GetComponentsInChildren<Transform>().First(t=>t.name=="eye_L");var tail=body.parts[4].bounds;
  return Vector3.Distance(eye.position,tail.center)+tail.extents.magnitude;
 }
 [MenuItem("Mantis/Integrate D character")]
 public static void Build(){
  BuildModel();EditorSceneManager.OpenScene("Assets/Mantis/Generated/Duel.unity");
  var duel=UnityEngine.Object.FindFirstObjectByType<MantisDuel>();
  foreach(var owner in new[]{duel.player,duel.cpu}){
   if(!owner.visual.leftStrike)ReplaceVisual(owner);
  }
  var cpuMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Mantis/Generated/CpuShell.mat");EditorUtility.CopySerialized(AssetDatabase.LoadAssetAtPath<Material>(Root+"DShell.mat"),cpuMat);cpuMat.SetColor("_BaseColor",new Color(1,.57f,.34f));
  foreach(var r in duel.cpu.visual.GetComponentsInChildren<SkinnedMeshRenderer>())r.sharedMaterial=cpuMat;
  if(!duel.GetComponent<DModelVerification>())duel.gameObject.AddComponent<DModelVerification>();
  EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();HomeBuilder.Build();
 }
}
