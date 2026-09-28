using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed partial class MantisHome {
 readonly Color sceneInk=Color.white;
 void ApplySceneTextShadow(Text text){
  text.color=Color.white;
  var shadow=text.GetComponent<Shadow>();if(!shadow)shadow=text.gameObject.AddComponent<Shadow>();
  shadow.effectColor=new Color(.005f,.02f,.03f,.85f);
  shadow.effectDistance=new Vector2(1.2f,-1.5f);shadow.useGraphicAlpha=true;
 }
 Text SceneLabel(string value,float x,float y,float w,float h,int size,Color unused){var text=Label(value,x,y,w,h,size,Color.white);ApplySceneTextShadow(text);return text;}
}
}
