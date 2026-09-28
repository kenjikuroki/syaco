using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public static class MantisContrastAudit {
 static float L(Color c){return .2126f*Mathf.GammaToLinearSpace(c.r)+.7152f*Mathf.GammaToLinearSpace(c.g)+.0722f*Mathf.GammaToLinearSpace(c.b);}
 public static float Ratio(Text text,RectTransform root){
  var bounds=text.rectTransform.rect;var sample=bounds.center;int alignment=(int)text.alignment%3;if(alignment==0)sample.x=bounds.xMin+Mathf.Min(12,bounds.width*.5f);else if(alignment==2)sample.x=bounds.xMax-Mathf.Min(12,bounds.width*.5f);Vector3 point=text.rectTransform.TransformPoint(sample);Color background=Color.white;
  foreach(var graphic in root.GetComponentsInChildren<Graphic>()){
   if(graphic==text)break;if(graphic is Text||graphic is HomeIcon||!graphic.enabled)continue;
   var local=graphic.rectTransform.InverseTransformPoint(point);if(!graphic.rectTransform.rect.Contains(local))continue;
   Color c=graphic.color;
   if(graphic is HomePlate plate)c=Color.Lerp(plate.bottom,plate.color,Mathf.InverseLerp(plate.rectTransform.rect.yMin,plate.rectTransform.rect.yMax,local.y));
   c*=graphic.canvasRenderer.GetColor();background=Color.Lerp(background,c,c.a);
  }
  Color foreground=Color.Lerp(background,text.color,text.color.a);float a=L(foreground),b=L(background);return (Mathf.Max(a,b)+.05f)/(Mathf.Min(a,b)+.05f);
 }
}
}
