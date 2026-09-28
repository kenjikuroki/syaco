using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public static class CharacterProfile {
 const string Key="Mantis.CharacterName.v1";
 static bool verification;static string testName;
 public static void BeginVerification(){verification=true;testName=null;}
 public static bool Registered=>true;
 public static string Name=>verification&&testName!=null?testName:GameCenterIdentity.SignedIn?GameCenterIdentity.DisplayName:MantisDuel.Tr("ゲスト","Guest");
 public static bool Validate(string value,out string clean){
  clean=(value??"").Trim().Normalize();
  if(new StringInfo(clean).LengthInTextElements<1||new StringInfo(clean).LengthInTextElements>12)return false;
  foreach(char c in clean)if(char.IsControl(c)||c=='<'||c=='>')return false;
  return true;
 }
 public static bool Save(string value){string clean;if(!Validate(value,out clean))return false;if(verification){testName=clean;return true;}return false;}
}
public sealed partial class MantisHome {
 bool NeedsRegistration=>false;
 Text CharacterName(float x,float y,float w,float h,int size){var t=Label(CharacterProfile.Name,x,y,w,h,size,Color.white);t.text=CharacterProfile.Name;t.supportRichText=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=12;t.resizeTextMaxSize=size;t.gameObject.AddComponent<PlayerDisplayName>();return t;}
 // Kept for older QA entry points; there is no editable player-name screen.
 void ShowCharacterName(){}
 void ShowCharacterName(bool fromSettings){}
}
}