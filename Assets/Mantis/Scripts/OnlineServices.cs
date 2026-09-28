using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
namespace MantisPunch {
public static class OnlineServices {
 static Task login;
 public static Task SignIn(){if(login==null||login.IsFaulted||login.IsCanceled)login=Connect();return login;}
 static async Task Connect(){
  await UnityServices.InitializeAsync();
  if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
  await AuthenticationService.Instance.UpdatePlayerNameAsync(CharacterProfile.Name);
 }
 public static string PlayerId=>UnityServices.State==ServicesInitializationState.Initialized&&AuthenticationService.Instance.IsSignedIn?AuthenticationService.Instance.PlayerId:"";
}
public enum MatchKind { Cpu, Public, Friend }
public static class MatchRules {
 public static bool CanRematch(MatchKind kind,bool connected)=>kind==MatchKind.Friend&&connected;
 public static bool Ranked(MatchKind kind)=>kind==MatchKind.Public;
}
}
