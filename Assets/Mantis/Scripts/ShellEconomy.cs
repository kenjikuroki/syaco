namespace MantisPunch {
// One source of truth for earned currency, cosmetic prices and provisional packs.
public static class ShellEconomy {
 public const int MatchWin=20, MatchLoss=5, FirstWin=30, Login=10;
 public static int Daily(int id)=>new[]{20,25,25}[id];
 public static int Weekly(int id)=>new[]{80,100,120}[id];
 public static int Price(int item)=>item>=0&&item<4?new[]{800,4200,2800,4800}[item]:-1;
 public static readonly int[] PackAmounts={800,2800,6000,16000};
 public static readonly string[] ProvisionalYen={"¥160","¥480","¥960","¥2,400"};
}
}
