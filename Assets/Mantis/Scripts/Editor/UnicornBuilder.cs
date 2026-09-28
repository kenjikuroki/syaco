using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
public static class UnicornBuilder {
 public static void Build(){
  const string path="Assets/Mantis/Art/Unicorn/Unicorn_Set.fbx";
  AssetDatabase.Refresh();var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;importer.isReadable=true;importer.optimizeGameObjects=false;importer.SaveAndReimport();
  Directory.CreateDirectory("Assets/Mantis/Resources/Equipment/Unicorn");AssetDatabase.Refresh();
  string[] names={"Unicorn_Pearl","Unicorn_Gold","Unicorn_Pink","Unicorn_Lilac","Unicorn_Sky"};
  Color[] colors={new Color(.88f,.82f,.76f),new Color(.72f,.40f,.12f),new Color(.80f,.24f,.49f),new Color(.43f,.28f,.74f),new Color(.24f,.59f,.83f)};
  var materials=new Material[5];
  for(int i=0;i<5;i++){
   string mp="Assets/Mantis/Resources/Equipment/Unicorn/"+names[i]+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(mp);
   if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,mp);}
   m.SetColor("_BaseColor",colors[i]);m.SetFloat("_Metallic",i==1?.65f:.12f);m.SetFloat("_Smoothness",.40f);m.SetFloat("_Surface",0);EditorUtility.SetDirty(m);materials[i]=m;
  }
  var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
  var rs=go.GetComponentsInChildren<SkinnedMeshRenderer>();if(rs.Length!=6)throw new System.Exception("Unicorn requires six slots");
  foreach(var r in rs){r.sharedMaterials=r.sharedMaterials.Select(m=>{int i=System.Array.FindIndex(names,n=>m.name.StartsWith(n));if(i<0)throw new System.Exception("Unknown boxer material "+m.name);return materials[i];}).ToArray();if(r.bones.Any(b=>!b))throw new System.Exception("Unmapped boxer bones");}
  PrefabUtility.SaveAsPrefabAsset(go,"Assets/Mantis/Resources/Equipment/UnicornSet.prefab");Object.DestroyImmediate(go);AssetDatabase.SaveAssets();
 }
}
