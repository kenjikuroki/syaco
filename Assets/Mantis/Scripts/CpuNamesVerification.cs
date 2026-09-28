using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace MantisPunch {
public sealed partial class BattleHud {
 IEnumerator VerifyCpuNames(){
  const string folder="QA/CpuNames";Directory.CreateDirectory(folder);duel.Testing=true;yield return null;
  bool ok=CpuNames.English.Length==40&&CpuNames.Japanese.Length==10;
  var names=new HashSet<string>();for(int i=0;i<CpuNames.Count;i++)ok&=names.Add(CpuNames.At(i));
  var seen=new HashSet<string>();string last="";
  for(int i=0;i<2000;i++){string next=CpuNames.Draw(last,CharacterProfile.Name);ok&=next!=last&&next!=CharacterProfile.Name&&names.Contains(next);seen.Add(next);last=next;}
  ok&=seen.Count==50;
  string matchName=duel.OpponentName;var style=duel.OpponentPersonality;int palette=duel.OpponentPalette;
  duel.NextRound();ok&=duel.OpponentName==matchName&&duel.OpponentPersonality==style&&duel.OpponentPalette==palette;
  duel.Restart();ok&=duel.OpponentName!=matchName;
  var property=typeof(MantisDuel).GetProperty("OpponentName");
  foreach(bool portrait in new[]{false,true}){
   Screen.SetResolution(portrait?720:1280,portrait?1280:720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.3f);
   foreach(string name in new[]{"WaveBreaker","波のりパンチ"}){
    property.SetValue(duel,name);Build();yield return new WaitForSecondsRealtime(.2f);
    var label=root.Find("Opponent name").GetComponent<UnityEngine.UI.Text>();Canvas.ForceUpdateCanvases();ok&=label.text==name&&label.cachedTextGenerator.lineCount==1;
    ReviewCapture.Save(Path.GetFullPath(folder+"/"+(portrait?"portrait-":"landscape-")+(name=="WaveBreaker"?"en":"ja")+".png"),Screen.width,Screen.height);
   }
  }
  File.WriteAllText(folder+"/report.txt",ok?"PASS: 40 English + 10 Japanese unique names; all names reachable; no immediate repeat/player collision; identity retained across rounds and rerolled per match; single-line labels in portrait/landscape":"FAIL");Application.Quit(ok?0:1);
 }
}
}
