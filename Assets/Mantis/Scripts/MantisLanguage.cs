using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
namespace MantisPunch {
public static class MantisLanguage {
 public static bool Japanese=>Array.IndexOf(Environment.GetCommandLineArgs(),"-lang-ja")>=0 || Array.IndexOf(Environment.GetCommandLineArgs(),"-lang-en")<0 && Application.systemLanguage==SystemLanguage.Japanese;
 public static string Normalize(string value)=>Regex.Replace(Regex.Replace(value??"",@"\b(?:[A-Z] ){2,}[A-Z]\b",m=>m.Value.Replace(" ",""))," {2,}"," ");
 static readonly KeyValuePair<string,string>[] translations=Build();
 static KeyValuePair<string,string>[] Build(){return Data.Split('\n').Where(l=>l.Contains("|")).Select(l=>{var p=l.TrimEnd('\r').Split('|');return new KeyValuePair<string,string>(Normalize(p[0].Replace("\\n","\n")),p[1].Replace("\\n","\n"));}).OrderByDescending(p=>p.Key.Length).ToArray();}
 public static string T(string value){
  string text=Normalize(value);if(Japanese)return text;
  foreach(var p in translations)if(text==p.Key)return p.Value;
  if(Regex.IsMatch(text,@"^あと \d+ 貝殻$"))return "Need "+Regex.Match(text,@"\d+").Value+" more shells";
  if(Regex.IsMatch(text,@"^受取可能：\d+ 件$"))return "Ready to claim: "+Regex.Match(text,@"\d+").Value;
  if(Regex.IsMatch(text,@"^期間終了まで \d+日")){text=Regex.Replace(text,@"^期間終了まで (\d+)日","Ends in $1 days");}
  foreach(var p in translations)text=text.Replace(p.Key,p.Value);
  return text;
 }
 const string Data=@"一撃に、すべてを。|Everything in one punch.
深海から、一撃で。|From the deep. One decisive strike.
一撃勝負|ONE HIT. ONE WIN.
一撃で決めろ。|Make your strike count.
名前を変更 ›|Edit name ›
CPU 対戦|VS CPU
シャコをタップしてパンチ ≫|Tap your mantis to punch ≫
ホーム|Home
ホームへ|Home
ホームへ戻る|Back to home
カスタム|Customize
カスタムへ|Customize
カスタムへ →|Customize →
コレクション|Collection
ミッション|Missions
戦う|Fight
戦う →|Fight →
設定|Settings
ホームBGM：ON|Home music: ON
ホームBGM：OFF|Home music: OFF
画面の揺れ：ON|Screen shake: ON
画面の揺れ：OFF|Screen shake: OFF
閉じる|Close
C U S T O M / 自分だけの一撃を。|CUSTOM / Make your mark.
C O L L E C T I O N / 手に入れた装備|COLLECTION / Your equipment.
M I S S I O N S / 一戦ずつ、貝殻を集めよう。|MISSIONS / Fight. Earn. Customize.
R A N K I N G / 読み合いを制し、頂点へ。|RANKING / Read your rival. Rise to the top.
SHOP / 貝殻で、お気に入りの装備を。|SHOP / Shells for your favorite gear.
01 / 部位を選ぶ|01 / Select a body part
03 / 所持装備|03 / Owned gear
頭胸部|Head & thorax
左パンチ腕|Left arm
右パンチ腕|Right arm
腹部|Abdomen
尻尾|Tail
脚|Legs
 のカラー| color
オリジナル|Original
アクア|Aqua
コーラル|Coral
ゴールド|Gold
ミッドナイト|Midnight
パール|Pearl
初期色に戻す|Reset colors
カラーを保存|Save colors
カラーを保存しました。ホーム・対戦に反映されます。|Colors saved for home and battle.
色は「カラーを保存」で反映。装備の着け外しは自動保存。|Save colors to apply. Equipment changes save automatically.
ショップ →|Shop →
所持装備はまだありません|No equipment yet
ショップで装備を入手できます|Get equipment from the shop.
パンチ確認|Test punch
リーフ・クラウン|Reef Crown
装備中|Equipped
未装備|Not equipped
装備する|Equip
外す|Unequip
装備を保存しました。|Equipment saved.
「所持装備」から装備・取り外しを選べます。|Equip or remove gear under Owned gear.
頭装備 / 装備中|Headgear / Equipped
頭装備 / 未装備|Headgear / Not equipped
頭装備 / |Headgear / 
所持している装備|Owned equipment
所持カラー|Owned colors
カスタムで装備する →|Equip in Customize →
カスタムで装備を変更 →|Change equipment →
確認用プレビューです。現在の装備は変更されません。|Preview only. Your equipped gear stays the same.
使用中の装備です。着け外しはカスタム画面で。|Currently equipped. Change it in Customize.
装備を集めよう|Start your collection
カスタム画面のショップで装備を入手すると、\nここで見た目や装備状態を確認できます。|Get equipment from the shop in Customize.\nView your collection here.
入手した装備が並びます。着け外しはカスタム画面で。|Your collected gear. Equip it in Customize.
入手先：カスタム内のショップ|Available in the Customize shop
王冠なしと比較|Compare without crown
王冠を試着|Try on crown
王冠を外す|Remove crown
所持 / 装備中|Owned / Equipped
所持 / 未装備|Owned / Not equipped
未所持 / 無料で試着できます|Not owned / Try it on for free
6つの頂点を持つ、金色の王冠。\n頭胸部と一緒に動く装備です。\n見た目だけが変わり、性能は変わりません。|A golden crown with six points.\nMoves with your head.\nCosmetic only. No combat advantage.
価格 / 貝殻 300|Price / 300 shells
購入後の残高：|Balance after purchase: 
購入を確定|Confirm purchase
戻る|Back
貝殻300で購入へ|Buy for 300 shells
ミッションで貝殻を集める →|Earn shells in Missions →
購入しました。「装備する」で使用できます。|Purchased! Select Equip to wear it.
残高を確認してください。|Please check your shell balance.
購入済み / 再購入は不要です|Already owned. No need to buy again.
試着では装備は変わりません。購入後に装備を選べます。|Try it on without changing your gear. Equip after purchase.
貝殻購入 ＋|Buy shells +
貝殻購入|Buy shells
装備ショップ|Gear shop
装備・見た目のカスタムに使えます。|Use shells for equipment and cosmetic customization.
おためしパック|Starter pack
基本パック|Standard pack
まとめ買いパック|Value pack
大容量パック|Large pack
準備中|Coming soon
価格は仮設定です。現在は購入できません。|Proposed prices. Purchases are not available yet.
（仮）| (draft)
所持 |Balance: 
 枚| shells
貝殻 |Shells 
デイリー|Daily
ウィークリー|Weekly
ログイン|Login
ログインボーナス|Daily login bonus
毎日プレイして貝殻を集めよう|Play daily to earn shells.
本日の報酬を受け取りました|Today's reward claimed.
受取可能|Available
受取済|Claimed
受取済み|Claimed
受け取る|Claim
挑戦中|In progress
まとめて受け取る|Claim all
月曜 0:00 に更新|Resets Monday at 00:00
毎日 0:00 に更新|Resets daily at 00:00
ラウンドを戦い抜く|Finish rounds
一撃を決めて勝利|Land a winning hit
パリィで攻撃を弾く|Parry attacks
勝敗を問わず、最後まで戦う|Finish a round, win or lose.
ラウンドで相手を倒す|Defeat your rival in a round.
相手のパンチをパリィする|Parry your rival's punch.
今日のログイン報酬|Today's login reward
毎日1回受け取れます。\n連続ログインは不要です。|Claim once a day.\nNo login streak required.
本日は受取済み|Claimed today
今日の報酬を受け取る|Claim today's reward
CPU戦も対象・デモは対象外。貝殻はカスタム内のショップで使用できます。|CPU battles count; demos do not. Spend shells in the Customize shop.
ランキング|Rankings
ルール / 戦績|Rules / Record
シーズンレート|Season rating
最高連勝|Best win streak
レート|Rating
順位|Rank
プレイヤー|Player
未ランク|Unranked
毎月1日 0:00 UTC 更新|Resets monthly on day 1 at 00:00 UTC
オンライン対戦準備中|Online battles coming soon
対人対戦が利用可能になると、\nここにランキングが表示されます。|Rankings will appear here\nwhen online battles become available.
対人戦のみ集計。BOT戦ではレート・対人連勝は変わりません。|PvP only. BOT battles do not affect your rating or PvP win streak.
ランキングのルール / 戦績|Ranking rules / Record
・1シーズンは1か月（UTC基準）\n・3本先取の1試合で勝敗を集計\n・レートは対人戦の勝敗と相手の強さで変動\n・最高連勝は、その月のベスト記録\n・BOT戦は対人レート・対人連勝の対象外|• One season lasts one calendar month (UTC).\n• A match is first to three round wins.\n• PvP results and rival strength affect your rating.\n• Win streak shows your best streak this month.\n• BOT battles do not affect PvP rankings.
この端末のCPU戦績|CPU record on this device
 勝 / | wins / 
 敗| losses
この機能の追加後に完了した試合を記録します。\n対人戦績・過去シーズンの順位はオンライン対応後に表示します。|Tracks matches completed since this feature was added.\nPvP records and past seasons will arrive with online play.
あなたのシャコに名前を付けよう|Name your mantis
キャラ名を変更|Change character name
ランキングにも表示する名前です（1〜12文字）|Shown in rankings. Use 1–12 characters.
名前をタップして入力してください|Tap the field to enter your name.
1〜12文字で入力してください（改行・< > は不可）|Use 1–12 characters. No line breaks or < >.
この名前ではじめる|Start with this name
キャンセル|Cancel
保存|Save
戦闘終了|Exit
戦闘を終了しますか？|Leave this battle?
対戦に戻る|Resume battle
もう一度戦う|Fight again
次のラウンド|Next round
攻撃|Attack
フェイント|Feint
受け流し|Parry
";
}
}
