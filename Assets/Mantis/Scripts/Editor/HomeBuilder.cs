using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using MantisPunch;
using System.IO;
public static class HomeBuilder {
 [MenuItem("Mantis/Build home prototype")]
 public static void Build(){ CrownBuilder.Build();RobotBuilder.Build();BoxerBuilder.Build();UnicornBuilder.Build();CpuPaletteBuilder.Build();
  PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
  PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
  PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
  EditorSceneManager.OpenScene("Assets/Mantis/Generated/Duel.unity");
  var duel=Object.FindFirstObjectByType<MantisDuel>(); if(!duel.GetComponent<CrownVerification>())duel.gameObject.AddComponent<CrownVerification>(); if(!duel.GetComponent<MissionCombatVerification>())duel.gameObject.AddComponent<MissionCombatVerification>();
  if(!duel.GetComponent<HomeReturn>())duel.gameObject.AddComponent<HomeReturn>();
  EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
  var player=duel.player; var cpu=duel.cpu; var camera=duel.arenaCamera;
  var visual=player.visual; visual.transform.SetParent(null,true);visual.transform.position=Vector3.zero;
  Object.DestroyImmediate(cpu.gameObject);Object.DestroyImmediate(player.gameObject);Object.DestroyImmediate(duel.gameObject);
  foreach(var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))if(listener.gameObject!=camera.gameObject)Object.DestroyImmediate(listener);
  camera.transform.position=new Vector3(2.2f,1.8f,3.1f);camera.transform.LookAt(new Vector3(0,.25f,0));camera.fieldOfView=38;
  // Offset the subject into the left preview area without changing its proportions.
  camera.lensShift=new Vector2(.19f,0);camera.usePhysicalProperties=true;camera.fieldOfView=38;
  var fill=new GameObject("Home soft key").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=1.3f;fill.color=new Color(.8f,.94f,1);fill.transform.rotation=Quaternion.Euler(40,-40,0);
  var home=new GameObject("Home controller").AddComponent<MantisHome>();home.model=visual; home.gameObject.AddComponent<HomeMusic>().loop=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Mantis/Audio/DeepReefLoop.wav");
  home.gameObject.AddComponent<DModelVerification>();
  EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Mantis/Generated/Home.unity");
  AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds/HomeUIDuel");
  var scenes=new[]{"Assets/Mantis/Generated/Home.unity","Assets/Mantis/Generated/Duel.unity"};EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenes[0],true),new EditorBuildSettingsScene(scenes[1],true)};
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName="Builds/HomeUIDuel/ShakoHomeUI.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Home build failed");
 }
}







