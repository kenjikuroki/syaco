using UnityEngine;
namespace MantisPunch {
public static class RewardAdSettings {
 public const string IOSAppId="ca-app-pub-3331079517737737~9150305046";
 public const string IOSUnitId="ca-app-pub-3331079517737737/2487770330";
 public const string IOSTestUnitId="ca-app-pub-3940256099942544/1712485313";
 public const string AndroidTestUnitId="ca-app-pub-3940256099942544/5224354917";
 public const int Amount=30, DailyLimit=2;
 // Provisional child treatment until the public audience is decided. No ATT request.
 public const bool ChildTreatment=true;
 static string TestKey=>"Mantis.Ads.TestOnly.v1"+(TutorialProgress.QA?".QA":"");
 static int taps;static float lastTap;
 public static bool PermanentTest=>PlayerPrefs.GetInt(TestKey,0)==1;
 public static bool DeviceSupported=>Application.platform==RuntimePlatform.IPhonePlayer||Application.platform==RuntimePlatform.Android;
 public static bool TestAds=>PermanentTest||Debug.isDebugBuild||Application.isEditor||Application.platform!=RuntimePlatform.IPhonePlayer;
 public static string UnitId=>Application.platform==RuntimePlatform.Android?AndroidTestUnitId:TestAds?IOSTestUnitId:IOSUnitId;
 public static bool TapSecret(){if(Time.realtimeSinceStartup-lastTap>8)taps=0;lastTap=Time.realtimeSinceStartup;if(++taps<50)return false;taps=0;PlayerPrefs.SetInt(TestKey,1);PlayerPrefs.Save();RewardAdService.Instance.TestModeChanged();return true;}
 public static void ResetQA(){if(!TutorialProgress.QA)return;PlayerPrefs.DeleteKey(TestKey);PlayerPrefs.Save();taps=0;}
}
}
