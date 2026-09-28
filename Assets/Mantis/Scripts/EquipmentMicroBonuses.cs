using System.Globalization;
using System.Collections.Generic;
namespace MantisPunch {
public static class EquipmentMicroBonuses {
 public static float Tier(int mask)=>BoxerSetBonus.GuardDamage(mask);
 public static float AttackSaving(int mask)=>Tier(mask);
 public static float MoveMultiplier(int mask)=>1+Tier(mask)*.01f;
 public static float Recovery(bool crown)=>crown?.1f:0;
 static string N(float n)=>n.ToString("0.0",CultureInfo.InvariantCulture);
 public static string Description(int item)=>item==0?MantisDuel.Tr("王冠：ガード回復を毎秒 +0.1。回復開始までの時間は同じ。","Crown: +0.1 guard recovery/sec. Recovery delay unchanged."):item==1?MantisDuel.Tr("ロボ：攻撃の消費を1部位 -0.1、全身で -1.0（10→9）。","Robot: attack cost -0.1 per part; -1.0 total with all 6 (10 to 9)."):item==2?BoxerSetBonus.Description:MantisDuel.Tr("ユニコーン：通常移動 +0.1%/部位、全身で合計 +1.0%。","Unicorn: normal movement +0.1% per part; +1.0% total with all 6.");
 public static string Summary(int boxer,int robot,int unicorn,bool crown){var items=new List<string>();if(boxer!=0)items.Add(MantisDuel.Tr("削り +","Damage +")+N(Tier(boxer)));if(robot!=0)items.Add(MantisDuel.Tr("攻撃消費 −","Cost −")+N(Tier(robot)));if(unicorn!=0)items.Add(MantisDuel.Tr("移動 +","Move +")+N(Tier(unicorn))+"%");if(crown)items.Add(MantisDuel.Tr("回復 +0.1/秒","Recovery +0.1/s"));return items.Count==0?MantisDuel.Tr("装備効果なし · 選ぶと自動保存","No bonus · Select to equip"):string.Join(" · ",items);}
}
}
