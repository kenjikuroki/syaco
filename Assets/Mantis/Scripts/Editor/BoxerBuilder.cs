using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
public static class BoxerBuilder {
 public static void Build(){
  const string path="Assets/Mantis/Art/Boxer/Boxer_Set.fbx";
  AssetDatabase.Refresh();var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;importer.isReadable=true;importer.optimizeGameObjects=false;importer.SaveAndReimport();
  Directory.CreateDirectory("Assets/Mantis/Resources/Equipment/Boxer");AssetDatabase.Refresh();
  string[] names={"Boxer_Cream","Boxer_Green","Boxer_Navy","Boxer_Red","Boxer_Gold","Boxer_Teal"};
  Color[] colors={new Color(.83f,.78f,.65f),new Color(.08f,.31f,.13f),new Color(.018f,.05f,.10f),new Color(.62f,.014f,.025f),new Color(.63f,.52f,.26f),new Color(.015f,.22f,.24f)};
  var materials=new Material[6];
  for(int i=0;i<6;i++){
   string mp="Assets/Mantis/Resources/Equipment/Boxer/"+names[i]+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(mp);
   if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,mp);}
   m.SetColor("_BaseColor",colors[i]);m.SetFloat("_Metallic",0);m.SetFloat("_Smoothness",i==3?.48f:.30f);m.SetFloat("_Surface",0);EditorUtility.SetDirty(m);materials[i]=m;
  }
  var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
  var rs=go.GetComponentsInChildren<SkinnedMeshRenderer>();if(rs.Length!=6)throw new System.Exception("Boxer requires six slots");
  foreach(var r in rs){r.sharedMaterials=r.sharedMaterials.Select(m=>{int i=System.Array.FindIndex(names,n=>m.name.StartsWith(n));if(i<0)throw new System.Exception("Unknown boxer material "+m.name);return materials[i];}).ToArray();if(r.bones.Any(b=>!b))throw new System.Exception("Unmapped boxer bones");}
  PrefabUtility.SaveAsPrefabAsset(go,"Assets/Mantis/Resources/Equipment/BoxerSet.prefab");Object.DestroyImmediate(go);AssetDatabase.SaveAssets();
 }
}
