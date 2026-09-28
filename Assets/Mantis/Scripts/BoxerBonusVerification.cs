using System.Collections;
using System.IO;
using UnityEngine;
namespace MantisPunch {
public sealed partial class MantisHome {
 IEnumerator VerifyBoxerBonusUI(){
  MissionStore.BeginVerification();RankingStore.BeginVerification();MissionStore.Get().shells=20000;EquipmentCatalog.Claim(2);BoxerEquipment.Save(63);yield return new WaitForSecondsRealtime(.5f);customColors=false;DrawCustom();Directory.CreateDirectory("QA/BoxerBonus");yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/BoxerBonus/custom.png"),Screen.width,Screen.height);
  catalogSelection=2;DrawEquipmentShop();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/BoxerBonus/shop.png"),Screen.width,Screen.height);
  for(int i=0;i<4;i++)EquipmentCatalog.Claim(i);RobotEquipment.Save(63);DrawCustom();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/BoxerBonus/robot.png"),Screen.width,Screen.height);
  UnicornEquipment.Save(63);Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);DrawCustom();yield return new WaitForSecondsRealtime(.3f);ReviewCapture.Save(Path.GetFullPath("QA/BoxerBonus/unicorn.png"),Screen.width,Screen.height);
  BoxerEquipment.Save(63);
  UnityEngine.SceneManagement.SceneManager.LoadScene("Duel");
 }
}
public sealed partial class MantisDuel {
 IEnumerator VerifyBoxerCombatBonus(){
  Testing=true;yield return null;bool ok=Mathf.Abs(Player.EquipmentGuardDamage-1)<.0001f&&Enemy.EquipmentGuardDamage==0;string report="";
  var body=player.visual.GetComponent<MantisModularBody>();
  for(int mask=0;mask<64;mask++){
   BoxerEquipment.Save(mask);MantisLoadout.Apply(body,false);int count=0;for(int bit=0;bit<6;bit++)if((mask&(1<<bit))!=0)count++;float expected=count==6?1f:count/10f;ok&=Mathf.Abs(Player.EquipmentGuardDamage-expected)<.0001f;
   Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(Vector3.forward,Quaternion.Euler(0,180,0));
   Enemy.Receive(Player);ok&=Mathf.Abs(Enemy.Guard-(100-100f/3-expected))<.001f;
   Enemy.Receive(Player);ok&=Enemy.State==DuelState.Guard&&Enemy.Guard>0;
   Enemy.Receive(Player);ok&=Enemy.State==DuelState.Stunned&&Enemy.Guard==0;
  }
  BoxerEquipment.Save(63);var loadout=MantisLoadout.Apply(body,false);loadout.SetRobot(1);ok&=Mathf.Abs(Player.EquipmentGuardDamage-.5f)<.0001f;loadout.SetCrown(false);ok&=Player.EquipmentGuardDamage==0;
  MantisLoadout.Apply(body,false);Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(Vector3.forward,Quaternion.Euler(0,180,0));Player.Begin(DuelAction.Attack);ok&=Player.Guard==90;Enemy.Begin(DuelAction.Parry);Enemy.Receive(Player);ok&=Enemy.Guard==100&&Player.State==DuelState.Recovery;
  Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(Vector3.forward,Quaternion.Euler(0,180,0));Enemy.Begin(DuelAction.Feint);Enemy.Receive(Player);ok&=!Enemy.Alive;
  Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(Vector3.forward,Quaternion.Euler(0,180,0));Player.Receive(Enemy);ok&=Mathf.Abs(Player.Guard-(100-100f/3))<.001f;
  BoxerEquipment.Save(0);MantisLoadout.Apply(body,false);ok&=Player.EquipmentGuardDamage==0;
  for(int mask=0;mask<64;mask++){
   int count=0;for(int bit=0;bit<6;bit++)if((mask&(1<<bit))!=0)count++;float tier=count==6?1f:count/10f;
   loadout.SetCrown(false);loadout.SetRobot(mask);Player.ResetRound(Vector3.zero,Quaternion.identity);Player.Begin(DuelAction.Attack);ok&=Mathf.Abs(Player.Guard-(90+tier))<.001f;
   Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(Vector3.forward,Quaternion.Euler(0,180,0));Player.Receive(Enemy);Player.Receive(Enemy);ok&=Player.State==DuelState.Guard;Player.Receive(Enemy);ok&=Player.State==DuelState.Stunned;
   loadout.SetCrown(false);loadout.SetUnicorn(mask);ok&=Mathf.Abs(Player.NormalMoveSpeed-1.65f*(1+tier*.01f))<.0001f&&Player.EffectiveAttackCost==10;
  }
  loadout.SetCrown(false);Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(Vector3.forward*4,Quaternion.identity);Player.Step(.2f,Vector2.right,Enemy);float normalDistance=Player.transform.position.x;
  loadout.SetUnicorn(63);Player.ResetRound(Vector3.zero,Quaternion.identity);Player.Step(.2f,Vector2.right,Enemy);ok&=Mathf.Abs(Player.transform.position.x/normalDistance-1.01f)<.002f;
  loadout.SetCrown(true);Player.ResetRound(Vector3.zero,Quaternion.identity);Enemy.ResetRound(Vector3.forward*4,Quaternion.identity);Player.Receive(Enemy);float before=Player.Guard;Player.Step(2,Vector2.zero,Enemy);ok&=Player.Guard==before;Player.Step(.1f,Vector2.zero,Enemy);ok&=Mathf.Abs(Player.Guard-before-(100f/9+.1f)*.1f)<.001f;
  loadout.SetRobot(2);loadout.SetBoxer(4);loadout.SetUnicorn(56);ok&=Mathf.Abs(Player.EffectiveAttackCost-9.9f)<.001f&&Mathf.Abs(Player.EquipmentGuardDamage-.1f)<.001f&&Mathf.Abs(Player.NormalMoveSpeed-1.65f*1.003f)<.0001f&&Mathf.Abs(Player.GuardRecoveryRate-(100f/9+.1f))<.001f;
  loadout.SetRobot(1);ok&=!loadout.WearingCrown&&Mathf.Abs(Player.GuardRecoveryRate-100f/9)<.001f;
  loadout.SetCrown(false);ok&=Player.EquipmentGuardDamage==0&&Player.EffectiveAttackCost==10&&Player.NormalMoveSpeed==1.65f&&Enemy.EffectiveAttackCost==10&&Enemy.NormalMoveSpeed==1.65f;
  report=ok?"PASS: home-to-combat equipped bonus; all 64 masks; actual guard loss; 3 blocks to break for every mask; replacing/removing parts clears bonus; CPU independent; attack cost 10 unchanged; parry no extra drain; vulnerable hit remains lethal; robot all 64 masks actual cost and 3-block break; unicorn all masks and actual +1% movement; crown actual recovery and unchanged delay; mixed effects; reset removes all":"FAIL";File.WriteAllText("QA/BoxerBonus/report.txt",report);Application.Quit(ok?0:1);
 }
}
}


