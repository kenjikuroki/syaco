using UnityEngine;
using UnityEditor;
using System.IO;
public static class CrownBuilder {
 public static void Build(){
 const string path="Assets/Mantis/Art/D/D_CrownHead.fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;importer.SaveAndReimport();
 var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));var head=go.GetComponentInChildren<SkinnedMeshRenderer>();
 Directory.CreateDirectory("Assets/Mantis/Resources/Equipment");AssetDatabase.Refresh();
 var gold=new Material(Shader.Find("Universal Render Pipeline/Lit"));gold.SetColor("_BaseColor",new Color(.95f,.61f,.1f));gold.SetFloat("_Metallic",.65f);gold.SetFloat("_Smoothness",.55f);
 const string matPath="Assets/Mantis/Resources/Equipment/CrownGold.mat";var previous=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(previous){EditorUtility.CopySerialized(gold,previous);Object.DestroyImmediate(gold);gold=previous;}else AssetDatabase.CreateAsset(gold,matPath);
 head.sharedMaterials=new[]{AssetDatabase.LoadAssetAtPath<Material>("Assets/Mantis/Art/D/DShell.mat"),gold};
 if(head.sharedMesh.subMeshCount!=2)throw new System.Exception("Crown must have shell and gold submeshes");
 PrefabUtility.SaveAsPrefabAsset(go,"Assets/Mantis/Resources/Equipment/CrownHead.prefab");Object.DestroyImmediate(go);AssetDatabase.SaveAssets();
 }
}
