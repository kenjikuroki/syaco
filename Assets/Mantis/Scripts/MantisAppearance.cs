using UnityEngine;
namespace MantisPunch {
public static class MantisAppearance {
 public static readonly string[] Slots={"頭胸部","左パンチ腕","右パンチ腕","腹部","尻尾","脚"};
 public static readonly string[] Names={"オリジナル","アクア","コーラル","ゴールド","ミッドナイト","パール"};
 public static readonly Color[] Colors={Color.white,new Color(.35f,1.45f,1.5f),new Color(1.65f,.65f,.43f),new Color(1.65f,1.3f,.45f),new Color(.38f,.42f,.65f),new Color(1.5f,1.5f,1.5f)};
 public static int[] Load(){var values=new int[6];for(int i=0;i<6;i++)values[i]=Mathf.Clamp(PlayerPrefs.GetInt("Mantis.Appearance."+i,0),0,Colors.Length-1);return values;}
 public static void Save(int[] values){for(int i=0;i<6;i++)PlayerPrefs.SetInt("Mantis.Appearance."+i,values[i]);PlayerPrefs.Save();}
 public static void Apply(MantisModularBody body,int[] values){if(!body)return;for(int i=0;i<6;i++){if(!body.parts[i])continue;var block=new MaterialPropertyBlock();body.parts[i].GetPropertyBlock(block);block.SetColor("_BaseColor",Colors[Mathf.Clamp(values[i],0,Colors.Length-1)]);body.parts[i].SetPropertyBlock(block);}}
}
}
