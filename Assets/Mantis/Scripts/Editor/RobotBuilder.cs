using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
public static class RobotBuilder {
 public static void Build(){
  const string path="Assets/Mantis/Art/Robot/Robot_Set.fbx";
  AssetDatabase.Refresh();var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;importer.isReadable=true;importer.optimizeGameObjects=false;importer.SaveAndReimport();
  Directory.CreateDirectory("Assets/Mantis/Resources/Equipment/Robot");AssetDatabase.Refresh();
  string[] names={"Robot_Ivory","Robot_Petrol","Robot_Graphite","Robot_Red","Robot_Brass","Robot_Cyan"};
  Color[] colors={new Color(.82f,.78f,.65f),new Color(.045f,.12f,.135f),new Color(.018f,.026f,.035f),new Color(.58f,.035f,.026f),new Color(.52f,.32f,.10f),new Color(.015f,.58f,.85f)};
  var materials=new Material[6];
  for(int i=0;i<6;i++){
   string mp="Assets/Mantis/Resources/Equipment/Robot/"+names[i]+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(mp);
   if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,mp);}
   m.SetColor("_BaseColor",colors[i]);m.SetFloat("_Metallic",i==4?.78f:.45f);m.SetFloat("_Smoothness",.55f);m.SetFloat("_Surface",0);
   if(i==5){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",colors[i]*1.4f);}EditorUtility.SetDirty(m);materials[i]=m;
  }
  var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
  var rs=go.GetComponentsInChildren<SkinnedMeshRenderer>();if(rs.Length!=6)throw new System.Exception("Robot requires six slots");
  foreach(var r in rs){r.sharedMaterials=r.sharedMaterials.Select(m=>{int i=System.Array.FindIndex(names,n=>m.name.StartsWith(n));if(i<0)throw new System.Exception("Unknown robot material "+m.name);return materials[i];}).ToArray();if(r.bones.Any(b=>!b))throw new System.Exception("Unmapped robot bones");}
  PrefabUtility.SaveAsPrefabAsset(go,"Assets/Mantis/Resources/Equipment/RobotSet.prefab");Object.DestroyImmediate(go);AssetDatabase.SaveAssets();
 }
}
