using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using MantisPunch;
public sealed class RewardAdsBuild:IPreprocessBuildWithReport {
 public int callbackOrder=>-1000;
 public void OnPreprocessBuild(BuildReport report){Configure();}
 [MenuItem("Mantis/Configure rewarded ads")]
 public static void Configure(){
  const string path="Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
  var settings=AssetDatabase.LoadMainAssetAtPath(path);if(!settings){var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("GoogleMobileAds.Editor.GoogleMobileAdsSettings")).FirstOrDefault(t=>t!=null);if(type==null)throw new BuildFailedException("Google Mobile Ads Unity plugin is missing.");Directory.CreateDirectory(Path.GetDirectoryName(path));settings=ScriptableObject.CreateInstance(type);AssetDatabase.CreateAsset(settings,path);}
  var data=new SerializedObject(settings);data.FindProperty("adMobIOSAppId").stringValue=RewardAdSettings.IOSAppId;data.FindProperty("adMobAndroidAppId").stringValue="ca-app-pub-3940256099942544~3347511713";data.FindProperty("userTrackingUsageDescription").stringValue="";data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();
 }
}
