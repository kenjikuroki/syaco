using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
// Small procedural UI mesh: no bitmap dependency or per-frame texture generation.
public sealed class HomePlate : MaskableGraphic {
 public Color bottom=new Color(.01f,.06f,.08f,.94f),edge=new Color(.1f,.7f,.75f,.7f);
 public float cut=12,border=1.2f;
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();var r=rectTransform.rect;float c=Mathf.Min(cut,Mathf.Min(r.width,r.height)*.3f);
  Vector2[] points={new Vector2(r.xMin+c,r.yMax),new Vector2(r.xMax-c,r.yMax),new Vector2(r.xMax,r.yMax-c),new Vector2(r.xMax,r.yMin+c),new Vector2(r.xMax-c,r.yMin),new Vector2(r.xMin+c,r.yMin),new Vector2(r.xMin,r.yMin+c),new Vector2(r.xMin,r.yMax-c)};
  vh.AddVert(r.center,Color.Lerp(bottom,color,.5f),Vector2.zero);
  foreach(var p in points)vh.AddVert(p,Color.Lerp(bottom,color,Mathf.InverseLerp(r.yMin,r.yMax,p.y)),Vector2.zero);
  for(int i=0;i<8;i++)vh.AddTriangle(0,1+i,1+(i+1)%8);
  if(border<=0)return;
  for(int i=0;i<8;i++){var a=points[i];var b=points[(i+1)%8];var n=(r.center-(a+b)*.5f).normalized*border;int k=vh.currentVertCount;vh.AddVert(a,edge,Vector2.zero);vh.AddVert(b,edge,Vector2.zero);vh.AddVert(b+n,edge,Vector2.zero);vh.AddVert(a+n,edge,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);}
 }
}
}
