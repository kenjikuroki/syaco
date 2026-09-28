using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed class BattleDisc:MaskableGraphic {
 public Color rim=new Color(.24f,.65f,.7f,.8f);public bool solid;
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;float radius=Mathf.Min(r.width,r.height)*.5f;v.AddVert(r.center,color,Vector2.zero);for(int i=0;i<=64;i++){float a=i*Mathf.PI/32;v.AddVert(r.center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);if(i>0)v.AddTriangle(0,i,i+1);}for(int i=0;i<64;i++){float a=i*Mathf.PI/32,b=(i+1)*Mathf.PI/32;int k=v.currentVertCount;foreach(var p in new[]{new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(radius-2),new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(radius-2)})v.AddVert(r.center+p,rim,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}}
}
}
