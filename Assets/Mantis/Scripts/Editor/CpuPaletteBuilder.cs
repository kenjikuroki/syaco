using UnityEngine;
using UnityEditor;
using System.IO;
public static class CpuPaletteBuilder {
 public static void Build(){
  const string folder="Assets/Mantis/Resources/CpuPalettes";Directory.CreateDirectory(folder);
  Color[] shells={new Color(.15f,.31f,.17f),new Color(.055f,.12f,.30f),new Color(.42f,.13f,.075f),new Color(.27f,.10f,.33f),new Color(.68f,.71f,.64f),new Color(.055f,.10f,.08f),new Color(.49f,.38f,.20f),new Color(.055f,.35f,.33f)};
  Color[] trim={new Color(.74f,.62f,.30f),new Color(.32f,.73f,.81f),new Color(.82f,.75f,.57f),new Color(.74f,.60f,.35f),new Color(.12f,.45f,.42f),new Color(.55f,.32f,.14f),new Color(.26f,.14f,.06f),new Color(.84f,.76f,.56f)};
  Color[] arms={new Color(.70f,.12f,.065f),new Color(.88f,.35f,.08f),new Color(.15f,.30f,.13f),new Color(.64f,.12f,.16f),new Color(.83f,.34f,.25f),new Color(.70f,.17f,.075f),new Color(.10f,.28f,.48f),new Color(.35f,.16f,.44f)};
  var original=new Texture2D(2,2);original.LoadImage(File.ReadAllBytes("Assets/Mantis/Art/D/D_Character_Albedo.png"));var pixels=original.GetPixels();
  for(int i=0;i<8;i++){
   string png=folder+"/Palette"+i+".png";
   if(!File.Exists(png)){
    var result=(Color[])pixels.Clone();int tileSize=original.width/4;
    for(int y=0;y<original.height;y++)for(int x=0;x<original.width;x++){
     int index=y*original.width+x,tile=x/tileSize+(y/tileSize)*4;Color c=pixels[index];float brightness=Mathf.Max(c.r,c.g,c.b);Color tint;
     if(tile==0||tile==1||tile==13)tint=c.r>.45f&&c.g>.40f?trim[i]:shells[i];else if(tile==2||tile==15)tint=trim[i];else if(tile==4)tint=c.g>.4f?trim[i]:arms[i];else continue;
     float shade=Mathf.Clamp(brightness/(tile==4?.72f:tile==2||tile==15?.74f:.32f),.45f,1.2f);result[index]=new Color(Mathf.Clamp01(tint.r*shade),Mathf.Clamp01(tint.g*shade),Mathf.Clamp01(tint.b*shade),1);
    }
    var texture=new Texture2D(original.width,original.height,TextureFormat.RGB24,true);texture.SetPixels(result);texture.Apply();File.WriteAllBytes(png,texture.EncodeToPNG());Object.DestroyImmediate(texture);
   }
   AssetDatabase.ImportAsset(png);var importer=(TextureImporter)AssetImporter.GetAtPath(png);importer.maxTextureSize=1024;importer.mipmapEnabled=true;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
   string path=folder+"/Palette"+i+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
   material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(png));material.SetColor("_BaseColor",Color.white);material.SetFloat("_Smoothness",.25f);EditorUtility.SetDirty(material);
  }
  Object.DestroyImmediate(original);AssetDatabase.SaveAssets();
 }
}
