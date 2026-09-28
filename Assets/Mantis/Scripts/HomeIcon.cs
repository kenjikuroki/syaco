using UnityEngine;
using UnityEngine.UI;
namespace MantisPunch {
public sealed class HomeIcon:MaskableGraphic {
 public string kind="home";
 void Line(VertexHelper v,Vector2 a,Vector2 b,float thickness=2){var r=rectTransform.rect;a=new Vector2(r.xMin+a.x*r.width,r.yMin+a.y*r.height);b=new Vector2(r.xMin+b.x*r.width,r.yMin+b.y*r.height);var n=new Vector2(-(b-a).y,(b-a).x).normalized*thickness*.5f;int k=v.currentVertCount;v.AddVert(a-n,color,Vector2.zero);v.AddVert(a+n,color,Vector2.zero);v.AddVert(b+n,color,Vector2.zero);v.AddVert(b-n,color,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}
 void Path(VertexHelper v,params float[] p){for(int i=0;i<p.Length-2;i+=2)Line(v,new Vector2(p[i],p[i+1]),new Vector2(p[i+2],p[i+3]));}
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();switch(kind){
 case "home":Path(v,.08f,.5f,.5f,.87f,.92f,.5f);Path(v,.22f,.52f,.22f,.12f,.42f,.12f,.42f,.4f,.59f,.4f,.59f,.12f,.78f,.12f,.78f,.52f);break;
 case "crown":Path(v,.16f,.22f,.07f,.82f,.32f,.57f,.5f,.94f,.68f,.57f,.93f,.82f,.84f,.22f,.16f,.22f);Path(v,.16f,.1f,.84f,.1f);break;
 case "custom":for(int i=0;i<3;i++){float y=.25f+i*.25f,x=i==1?.65f:.35f;Path(v,.1f,y,.9f,y);Path(v,x,y-.1f,x,y+.1f);}break;
 case "battle":Path(v,.15f,.12f,.83f,.85f,.78f,.62f,.15f,.12f);Path(v,.85f,.12f,.17f,.85f,.22f,.62f,.85f,.12f);Path(v,.08f,.35f,.32f,.1f);Path(v,.68f,.1f,.92f,.35f);break;
 case "parry":Path(v,.5f,.94f,.87f,.79f,.82f,.38f,.5f,.08f,.18f,.38f,.13f,.79f,.5f,.94f);Path(v,.5f,.79f,.69f,.7f,.65f,.43f,.5f,.28f,.35f,.43f,.31f,.7f,.5f,.79f);break;
 case "feint":Path(v,.55f,.87f,.65f,.96f,.76f,.86f,.67f,.76f,.55f,.87f);Path(v,.58f,.7f,.44f,.47f,.65f,.33f,.81f,.08f);Path(v,.44f,.47f,.31f,.27f,.12f,.18f);Path(v,.58f,.7f,.76f,.55f,.9f,.63f);Path(v,.56f,.68f,.32f,.73f,.18f,.53f);Path(v,.05f,.85f,.3f,.85f);break;
 case "shop":Path(v,.18f,.12f,.12f,.68f,.88f,.68f,.82f,.12f,.18f,.12f);Path(v,.32f,.63f,.32f,.83f,.42f,.94f,.58f,.94f,.68f,.83f,.68f,.63f);break;
 case "collection":for(int i=0;i<3;i++){float y=.65f-i*.2f;Path(v,.12f,y,.5f,y-.2f,.88f,y);}Path(v,.12f,.65f,.5f,.85f,.88f,.65f);break;
 case "mission":Path(v,.36f,.85f,.36f,.94f,.64f,.94f,.64f,.85f,.36f,.85f);Path(v,.3f,.85f,.15f,.85f,.15f,.06f,.85f,.06f,.85f,.85f,.7f,.85f);Path(v,.3f,.47f,.45f,.32f,.73f,.62f);break;
 case "gift":Path(v,.12f,.55f,.12f,.75f,.88f,.75f,.88f,.55f,.12f,.55f);Path(v,.2f,.55f,.2f,.1f,.8f,.1f,.8f,.55f);Path(v,.5f,.1f,.5f,.75f,.32f,.97f,.22f,.89f,.5f,.75f,.68f,.97f,.78f,.89f,.5f,.75f);break;
 case "shell":Path(v,.4f,.08f,.07f,.65f,.17f,.88f,.38f,.97f,.62f,.97f,.83f,.88f,.93f,.65f,.6f,.08f,.4f,.08f);Path(v,.5f,.1f,.5f,.95f);Path(v,.43f,.1f,.23f,.89f);Path(v,.57f,.1f,.77f,.89f);break;
 case "sound":Path(v,.12f,.35f,.32f,.35f,.58f,.16f,.58f,.84f,.32f,.65f,.12f,.65f,.12f,.35f);Path(v,.7f,.3f,.8f,.5f,.7f,.7f);Path(v,.82f,.16f,.97f,.5f,.82f,.84f);break;
 case "settings":for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;float ra=i%2==0?.4f:.3f,rb=i%2==0?.3f:.4f;Line(v,new Vector2(.5f+Mathf.Cos(a)*ra,.5f+Mathf.Sin(a)*ra),new Vector2(.5f+Mathf.Cos(b)*rb,.5f+Mathf.Sin(b)*rb));}for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;Line(v,new Vector2(.5f+Mathf.Cos(a)*.12f,.5f+Mathf.Sin(a)*.12f),new Vector2(.5f+Mathf.Cos(b)*.12f,.5f+Mathf.Sin(b)*.12f));}break;
 case "profile":Path(v,.3f,.7f,.35f,.9f,.65f,.9f,.7f,.7f,.6f,.53f,.4f,.53f,.3f,.7f);Path(v,.12f,.1f,.2f,.36f,.4f,.48f,.6f,.48f,.8f,.36f,.88f,.1f);break;
 default:Path(v,.3f,.2f,.7f,.5f,.3f,.8f);break;
 }}
}
}
