using System.Globalization;
namespace MantisPunch {
public static class BoxerSetBonus {
 public static int Count(int mask){int count=0;for(int i=0;i<6;i++)if((mask&(1<<i))!=0)count++;return count;}
 public static float GuardDamage(int mask){int count=Count(mask);return count==6?1f:count*.1f;}
 public static string Summary(int mask)=>MantisDuel.Tr("ボクサー ","Boxer ")+Count(mask)+"/6 · "+MantisDuel.Tr("ガード削り +","Guard damage +")+GuardDamage(mask).ToString("0.0",CultureInfo.InvariantCulture);
 public static string Description=>MantisDuel.Tr("ボクサー：1部位でガード削り +0.1。6部位で合計 +1.0。","Boxer: +0.1 guard damage per part; +1.0 total with all 6 parts.");
}
}
