using UnityEngine;
namespace MantisPunch {
 public enum CpuPersonality { Aggressive, Parry, Feint, Retreat, Balanced, Pressure, Adaptive }
 public struct CpuStyle {
  public float attack,feint,parry,initiative,spacing,commit,retreat;
  public static CpuStyle For(CpuPersonality type){
   switch(type){
    case CpuPersonality.Aggressive:return new CpuStyle{attack=1.8f,feint=.32f,parry=.17f,initiative=1.45f,spacing=.12f,commit=.44f,retreat=.13f};
    case CpuPersonality.Parry:return new CpuStyle{attack=.85f,feint=.35f,parry=.56f,initiative=3.2f,spacing=.08f,commit=.19f,retreat=.19f};
    case CpuPersonality.Feint:return new CpuStyle{attack=.9f,feint=1.1f,parry=.23f,initiative=2.1f,spacing=.22f,commit=.35f,retreat=.20f};
    case CpuPersonality.Retreat:return new CpuStyle{attack=1.2f,feint=.35f,parry=.19f,initiative=3.6f,spacing=.42f,commit=.17f,retreat=.40f};
    case CpuPersonality.Pressure:return new CpuStyle{attack=1.7f,feint=.4f,parry=.18f,initiative=1.8f,spacing=.15f,commit=.38f,retreat=.13f};
    default:return new CpuStyle{attack=1.05f,feint=.44f,parry=.28f,initiative=2.4f,spacing=.24f,commit=.28f,retreat=.22f};
   }
  }
 }
 public static class CpuAdaptation {
  // Use accumulated, already-observed behavior, not the opponent's current input.
  public static CpuPersonality Choose(float attacks,float feints,float parries,float passive){
   if(parries>attacks+.6f&&parries>feints)return CpuPersonality.Feint;
   if(feints>attacks+.6f)return CpuPersonality.Aggressive;
   if(attacks>parries+.6f)return CpuPersonality.Parry;
   return passive>4?CpuPersonality.Pressure:CpuPersonality.Balanced;
  }
 }
}
