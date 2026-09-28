using UnityEngine;
namespace MantisPunch {
public sealed partial class MantisDuel {
 public CpuPersonality OpponentPersonality=>brain.Personality;
 public CpuPersonality OpponentStrategy=>brain.Strategy;
 public static (CpuPersonality personality,int palette) DrawOpponentIdentity()=>((CpuPersonality)Random.Range(0,7),Random.Range(0,8));
 public int OpponentPalette {get;private set;}
 public string OpponentName {get;private set;}="ReefRider";
 void RollOpponent(){
  if(OnlineMatch.Current&&OnlineMatch.Current.Active){OpponentName=OnlineMatch.Current.Remote.name;return;}
  // Independent draws: no palette-to-personality lookup or shared index.
  var identity=DrawOpponentIdentity();var personality=identity.personality;OpponentPalette=identity.palette;
  OpponentName=CpuNames.Draw(OpponentName,CharacterProfile.Name);
  brain.BeginMatch(personality);
  var material=Resources.Load<Material>("CpuPalettes/Palette"+OpponentPalette);
  if(material)foreach(var renderer in cpu.visual.GetComponentsInChildren<SkinnedMeshRenderer>()){renderer.sharedMaterial=material;renderer.SetPropertyBlock(null);}
  if(Hud)Hud.RefreshOpponentPortrait();
 }
}
public sealed partial class BattleHud {
 public void RefreshOpponentPortrait(){if(!root)return;if(portraits[1]){portraits[1].Release();Destroy(portraits[1]);}portraits[1]=Portrait(duel.cpu.visual);Build();}
}
}
