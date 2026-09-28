using UnityEngine;
namespace MantisPunch {
public sealed class HomeMusic : MonoBehaviour {
 public AudioClip loop;
 AudioSource source;
 public bool Playing=>source&&source.isPlaying;
 public bool Muted=>PlayerPrefs.GetInt("Mantis.HomeMusicMuted",0)!=0;
 void Start(){source=gameObject.AddComponent<AudioSource>();source.clip=loop;source.loop=true;source.spatialBlend=0;source.volume=0;source.Play();}
 void Update(){if(source)source.volume=Mathf.MoveTowards(source.volume,Muted?0:.18f,Time.unscaledDeltaTime*.15f);}
 public void Toggle(){PlayerPrefs.SetInt("Mantis.HomeMusicMuted",Muted?0:1);PlayerPrefs.Save();}
}
}
