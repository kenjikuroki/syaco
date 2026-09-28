using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
namespace MantisPunch {
public sealed class XboxMenuInput:MonoBehaviour {
 float nextMove;Button selected;Outline outline;
 void Update(){
  var es=GetComponent<EventSystem>();es.sendNavigationEvents=false;var pad=Gamepad.current;if(pad==null){if(outline)outline.enabled=false;return;}
  var duel=FindFirstObjectByType<MantisDuel>();var hud=duel?duel.Hud:null;
  if(hud&&pad.startButton.wasPressedThisFrame){if(hud.Paused)hud.Resume();else if(!duel.RoundOver)hud.Pause(false);return;}
  if(hud&&pad.buttonEast.wasPressedThisFrame&&hud.Paused){hud.Resume();return;}
  if(hud&&!hud.Paused&&!duel.ShowResult){es.SetSelectedGameObject(null);if(outline)outline.enabled=false;selected=null;return;}
  if(pad.buttonEast.wasPressedThisFrame&&!hud){var home=FindFirstObjectByType<MantisHome>();if(home)home.SendMessage("DrawHome");return;}
  var buttons=FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.isActiveAndEnabled&&b.IsInteractable()&&b.GetComponentsInParent<CanvasGroup>().All(g=>g.interactable)).ToArray();
  // Modal controls are siblings after their full-screen backdrop in these runtime menus.
  var backdrop=FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.isActiveAndEnabled&&(i.name=="Modal shade"||i.name.EndsWith("backdrop")&&i.name!="Navigation backdrop")).OrderByDescending(i=>i.transform.GetSiblingIndex()).FirstOrDefault();
  if(backdrop)buttons=buttons.Where(b=>b.transform.parent==backdrop.transform.parent&&b.transform.GetSiblingIndex()>backdrop.transform.GetSiblingIndex()).ToArray();
  if(buttons.Length==0)return;
  Vector2 move=pad.dpad.ReadValue();if(move.sqrMagnitude<.25f)move=pad.leftStick.ReadValue();
  bool active=move.magnitude>.5f||pad.buttonSouth.wasPressedThisFrame;
  if(!selected||!buttons.Contains(selected)){if(!active)return;Select(buttons.OrderByDescending(b=>b.transform.position.y).ThenBy(b=>b.transform.position.x).First(),es);}
  if(move.magnitude>.5f&&Time.unscaledTime>=nextMove){nextMove=Time.unscaledTime+.20f;Vector2 dir=Mathf.Abs(move.x)>Mathf.Abs(move.y)?new Vector2(Mathf.Sign(move.x),0):new Vector2(0,Mathf.Sign(move.y));var origin=(Vector2)selected.transform.position;var target=buttons.Where(b=>b!=selected&&Vector2.Dot((Vector2)b.transform.position-origin,dir)>5).OrderBy(b=>{Vector2 d=(Vector2)b.transform.position-origin;return d.magnitude+Mathf.Abs(d.x*dir.y-d.y*dir.x)*3;}).FirstOrDefault();if(target)Select(target,es);}else if(move.magnitude<.3f)nextMove=0;
  if(pad.buttonSouth.wasPressedThisFrame&&selected)selected.onClick.Invoke();
 }
 void Select(Button button,EventSystem es){if(outline)outline.enabled=false;selected=button;es.SetSelectedGameObject(button.gameObject);outline=button.GetComponent<Outline>();if(!outline)outline=button.gameObject.AddComponent<Outline>();outline.useGraphicAlpha=false;outline.effectColor=new Color(.1f,1,1);outline.effectDistance=new Vector2(3,-3);outline.enabled=true;}
}
}
namespace MantisPunch {
public sealed partial class MantisHome {
 System.Collections.IEnumerator VerifyXbox(){
  yield return new UnityEngine.WaitForSecondsRealtime(2);var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();bool ok=true;var controls=new MantisControls();var go=new GameObject("Input verification");var duel=go.AddComponent<MantisDuel>();duel.enabled=false;var poll=typeof(MantisDuel).GetMethod("Poll",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
  var state=new UnityEngine.InputSystem.LowLevel.GamepadState{leftStick=new Vector2(.8f,0)};UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,state);yield return null;controls.Poll(new Rect());Debug.Log("XBOX MOVE "+controls.Move+" PAD "+pad.leftStick.ReadValue());ok&=controls.Move.x>.5f;
  foreach(var pair in new[]{(UnityEngine.InputSystem.LowLevel.GamepadButton.West,DuelAction.Attack),(UnityEngine.InputSystem.LowLevel.GamepadButton.North,DuelAction.Feint),(UnityEngine.InputSystem.LowLevel.GamepadButton.East,DuelAction.Parry)}){UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState());yield return null;UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(pair.Item1));yield return null;var actual=(DuelAction)poll.Invoke(duel,null);Debug.Log("XBOX ACTION "+pair.Item2+" actual "+actual+" current "+Gamepad.current);ok&=actual==pair.Item2;}
  UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);controls.Poll(new Rect());Debug.Log("XBOX DISCONNECT "+controls.Move);ok&=controls.Move==Vector2.zero;Destroy(go);System.IO.Directory.CreateDirectory("QA/XboxVerified");System.IO.File.WriteAllText("QA/XboxVerified/report.txt",ok?"PASS: virtual Xbox stick, X attack, Y feint, B parry, disconnect clears movement. Physical controller not tested.":"FAIL");Application.Quit(ok?0:1);
 }
}
}