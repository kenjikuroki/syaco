using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed class GameCenterIdentity:MonoBehaviour {
 public static string DisplayName{get;private set;}="";
 public static bool SignedIn=>!string.IsNullOrWhiteSpace(DisplayName);
 static GameCenterIdentity instance;float refreshAt;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Boot(){DisplayName="";if(instance)return;instance=new GameObject("Game Center identity").AddComponent<GameCenterIdentity>();DontDestroyOnLoad(instance.gameObject);}
#if UNITY_IOS && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void ShakoGCAuthenticate();
 [DllImport("__Internal")]static extern IntPtr ShakoGCDisplayName();
 [DllImport("__Internal")]static extern void ShakoGCFree(IntPtr value);
 void Start(){ShakoGCAuthenticate();}
 void Update(){if(Time.unscaledTime<refreshAt)return;refreshAt=Time.unscaledTime+.5f;Refresh();}
 void OnApplicationFocus(bool focused){if(focused)Refresh();}
 void Refresh(){var ptr=ShakoGCDisplayName();try{DisplayName=Marshal.PtrToStringUTF8(ptr)??"";}finally{ShakoGCFree(ptr);}}
#endif
}
public sealed class PlayerDisplayName:MonoBehaviour {
 Text label;void Awake(){label=GetComponent<Text>();}void Update(){if(label&&label.text!=CharacterProfile.Name)label.text=CharacterProfile.Name;}
}
}
