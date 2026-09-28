using UnityEngine;
namespace MantisPunch {
// One layout definition shared by visible controls and pointer hit testing.
public static class BattleLayout {
 public static bool Portrait=>Screen.safeArea.height>Screen.safeArea.width;
 public static float Scale=>Mathf.Min(Screen.safeArea.width/(Portrait?720f:1280f),Screen.safeArea.height/(Portrait?1280f:720f));
 public static float W=>Screen.safeArea.width/Scale;
 public static float H=>Screen.safeArea.height/Scale;
 public static Rect Action(int i){float w=W,h=H;if(Portrait)return i==0?new Rect(w-372,h-186,344,142):new Rect(w-372+(i==1?178:0),h-335,166,134);return i==0?new Rect(w-216,h-253,190,203):new Rect(w-216-(i==1?149:298),h-217,135,167);}
 public static Vector2 Stick=>new Vector2(Portrait?145:151,H-(Portrait?177:161));
 public static float Radius=>Portrait?111:119;
 public static Vector2 ScreenPoint(Vector2 design)=>new Vector2(Screen.safeArea.xMin+design.x*Scale,Screen.safeArea.yMax-design.y*Scale);
 public static Rect GuiRect(Rect r)=>new Rect(Screen.safeArea.xMin+r.x*Scale,Screen.height-Screen.safeArea.yMax+r.y*Scale,r.width*Scale,r.height*Scale);
}
}
