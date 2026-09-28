#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
public static class GameCenterIdentityBuild {
 [PostProcessBuild(120)] public static void Configure(BuildTarget target,string path){
  if(target!=BuildTarget.iOS)return;
  string projectPath=PBXProject.GetPBXProjectPath(path);var project=new PBXProject();project.ReadFromFile(projectPath);string main=project.GetUnityMainTargetGuid();project.AddFrameworkToProject(project.GetUnityFrameworkTargetGuid(),"GameKit.framework",false);project.WriteToFile(projectPath);
  string entitlement=project.GetBuildPropertyForAnyConfig(main,"CODE_SIGN_ENTITLEMENTS");if(string.IsNullOrEmpty(entitlement))entitlement="Shako.entitlements";
  var capabilities=new ProjectCapabilityManager(projectPath,entitlement,null,main);capabilities.AddGameCenter();capabilities.WriteToFile();
 }
}
#endif
