using UnityEngine;
namespace MantisPunch {
// Fictional handles; independent of appearance, personality and device language.
public static class CpuNames {
 public static readonly string[] English={
  "ReefRider","CoralRush","TideRunner","AquaFang","DeepBlue",
  "WaveBreaker","ShellShock","OceanPulse","NeonReef","SaltSpark",
  "BlueCurrent","ReefRookie","TidalAce","CoralKnight","AquaDash",
  "DeepDrifter","FoamFury","PearlPunch","ReefGhost","SeaSprint",
  "CobaltClaw","WaveDancer","LagoonFox","BrineBolt","CoralComet",
  "TideWatcher","AzureFin","SeaGlider","ReefEcho","OceanNomad",
  "BubbleRush","KelpKnight","AquaBlitz","PearlStorm","TidalFlash",
  "BlueRipple","ShellRacer","SeaStriker","ReefOrbit","CoralShade"
 };
 public static readonly string[] Japanese={"しおまる","波のりパンチ","さんごもち","海底の達人","あおしお","えびじゃない","一撃丸","うみねこ","貝がら集め","深海さんぽ"};
 public static int Count=>English.Length+Japanese.Length;
 public static string At(int index)=>index<English.Length?English[index]:Japanese[index-English.Length];
 public static string Draw(string previous,string playerName){
  // Uniform over eligible names, excluding an immediate repeat or the player's handle.
  int eligible=0;for(int i=0;i<Count;i++)if(At(i)!=previous&&At(i)!=playerName)eligible++;
  int pick=Random.Range(0,eligible);
  for(int i=0;i<Count;i++)if(At(i)!=previous&&At(i)!=playerName&&pick--==0)return At(i);
  return English[0];
 }
}
}
