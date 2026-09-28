using System.Collections;
using UnityEngine;
namespace MantisPunch {
public sealed class CrownVerification:MonoBehaviour {
 IEnumerator Start(){if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-crownTest")<0)yield break;yield return new WaitForSecondsRealtime(1);var d=GetComponent<MantisDuel>();d.Testing=true;var body=d.player.visual.GetComponent<MantisModularBody>();bool ok=body.GetComponent<MantisLoadout>().WearingCrown&&body.parts[0].sharedMesh.subMeshCount==2&&!d.cpu.visual.GetComponent<MantisLoadout>();d.player.visual.Sample(.5f,0,0);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath("QA/CrownVerified/battle.png"));yield return new WaitForSecondsRealtime(1);System.IO.File.AppendAllText("QA/CrownVerified/report.txt",ok?"\nPASS: equipped crown transfers to duel; CPU remains unchanged":"\nFAIL: equipped crown in duel");Application.Quit(ok?0:1);}
}
}
