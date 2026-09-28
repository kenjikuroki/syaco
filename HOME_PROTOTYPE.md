# Home prototype

Open Assets/Mantis/Generated/Home.unity to preview the home screen. Build via Mantis > Build home prototype (HomeBuilder.Build); it reads the existing Duel scene and writes a two-scene standalone build under Builds/HomeDuel. Rebuild the duel first with DuelBuilder when combat generation changes, then run HomeBuilder again.

Home: large animated fighter, tap preview to punch, Battle opens the existing CPU duel, HOME in the duel returns here. The other tabs and settings currently show explanatory notices. No online matchmaking, account economy, rank storage, missions or equipment screens are implemented. Japanese UI currently uses OS font fallback; bundle a redistributable Japanese font before mobile release and check on device.

Validation: standalone -homeTest captures the home/animated punch, verifies rig readiness and transition into initialized CPU combat, then exits. Screenshots use absolute paths based on the working directory. UI scales to the screen safe area; verified desktop 1200x750 and 1280x720. Device touch and mobile hardware remain untested.

## Custom screen and menu music

The Custom tab in Home now provides six independent body-region color choices, preview rotation in 45-degree steps, punch preview, reset draft, and explicit Save. Leaving without saving discards edits. Saved colors use Mantis.Appearance.0..5 PlayerPrefs keys and MaterialPropertyBlock overrides on the six existing skinned meshes; source materials and the CPU are not modified. These are color options, not replacement equipment meshes. The existing MantisModularBody.Replace API remains available for future donor parts.

Home and Custom share DeepReefLoop.wav at low volume via HomeMusic. Home music mute is separate from battle music mute. Entering Duel destroys the home AudioSource; ReefMusic keeps its existing random three-track selection. HomeReturn applies saved appearance to the player only.

Current standalone output: Builds/CustomDuel/ShakoCustom.exe. Run HomeBuilder.Build to rebuild. -customTest verifies color persistence and home application, captures the preview, restores the user's original selection, then verifies combat appearance and music separation. It does not validate physical phone input or audio perception.

## Collection screen

Collection now lists owned equipment only; color choices and color favorites are removed from this screen. Colors and equipment changes belong in Custom. Owned Reef Crown shows its item details and equipped/unequipped state, with a read-only 3D preview and a link to the head slot in Custom. Empty inventory shows an acquisition hint, not unowned catalog entries. Leaving the preview restores the saved equipment. Previewing never purchases or equips an item and preserves saved body colors.

Custom also has an Owned Equipment panel. Crown equip/remove persists immediately; color edits still require their separate Save action. Shop navigation remains available there.

Verification: -collectionTest uses isolated inventory data to check empty, owned, unequipped and equipped states, color/loadout preservation, leaving the preview and the Custom handoff. Screenshots/report: QA/CollectionVerified. Current executable: Builds/HomeUIDuel/ShakoHomeUI.exe.

## Missions

The Missions tab includes daily (3 completed rounds, 1 round win, 2 successful parries), weekly (20 rounds, 5 round wins, 10 parries), and a daily login claim. CPU rounds count; demo rounds and combat with Testing enabled do not. Scoring is counted once per round. Daily rewards: 50/50/75 shells; weekly: 200/250/300; login: 25. Claim and Claim All only grant completed unclaimed rewards. Balance appears on Home and Missions. Currency spending is not implemented.

Progress, claim flags and balance persist together in Mantis.Missions.v1 JSON PlayerPrefs. Daily reset follows the device's local midnight; weekly reset is Monday. This is local prototype persistence, not an authoritative online economy. Pending unclaimed period rewards expire on reset; balance remains. Missions refresh while open across a date change.

Current build: Builds/MissionsDuel/ShakoMissions.exe, via HomeBuilder.Build. -missionTest validates claims, JSON persistence and period changes in an isolated profile, then runs actual combat to verify parry/win counting, duplicate score prevention and demo exclusion without saving test progress. QA/MissionsVerified holds screenshots and report.

## Crown shop

First purchasable equipment: Reef Crown, 300 shells. The crown is authored by ArtSource/A6/create_crown.py as part of a replacement cephalothorax mesh, weighted to thorax. CrownHead.blend/FBX retain the A6 bone contract. CrownBuilder creates Resources/Equipment/CrownHead.prefab with shell and gold submaterials. MantisLoadout swaps the existing head renderer through MantisModularBody.Replace, retaining six renderers, and restores the original mesh/materials/bones when removed. The gold submaterial uses an independent gold color override so shell color changes do not tint the crown.

Custom > Headgear / Shop and Collection > Replacement Parts open the crown detail page. Preview is free and does not grant ownership. Purchasing requires 300 shells and a confirmation click; the stored MissionProfile JSON contains balance, ownership and equipped state together. Already owned and insufficient-funds purchases are rejected. Equipping is separate from purchase and requires ownership. Home and player combat load the saved equipment; CPU equipment is unchanged. No currency is granted for trying the shop.

Build at that stage: HomeBuilder.Build => Builds/CrownDuel/ShakoCrown.exe. -crownTest uses isolated in-memory economy data (never saved to user preferences), tests insufficient funds, unowned equip, single purchase debit, ownership serialization, equip/remove, and equipped crown in combat. Screenshots/report under QA/CrownVerified.

## Reference-based home redesign (2026-09-25)

Current executable: Builds/HomeUIDuel/ShakoHomeUI.exe, built with HomeBuilder.Build. The approved D character and its compatible crown are retained. UI reference: ArtSource/HomeUI_Reference/reference.png.

MantisHomeDesign.cs implements the home layout: dark translucent cut-corner panels, cyan accents, orange primary combat action, vector navigation icons, wallet, audio/settings controls, daily mission progress and login reward claim. HomePlate.cs and HomeIcon.cs generate small UI meshes without raster art dependencies. The reward card uses MissionStore, including duplicate-claim protection; test rewards are isolated from user persistence. No fictional seven-win streak or unimplemented purchase route is shown. Wallet plus opens missions; settings toggle actual home music and battle camera shake.

Home automatically reflows between 1200x675 landscape and 720x1280 portrait design coordinates, fits Screen.safeArea and reframes the 3D character. Five navigation targets remain available in either orientation. Rotation rebuilds home while preserving the saved appearance. Collection, custom and missions retain their previous layouts; their full portrait redesign and mobile device testing are still pending. PlayerSettings permits portrait and both landscape orientations.

Verification: -homeUiTest checks navigation, actual login reward claims, duplicate prevention, portrait/landscape switching and safe-area bounds. Captures and report: QA/HomeDesignVerified. ReviewCapture uses an explicit render request so hidden test windows can produce usable screenshots.

## Landscape / portrait battle HUD

Reference: ArtSource/BattleUI_Reference/reference.png. BattleHud replaces the old IMGUI overlay with a safe-area-aware Canvas, actual guard bars and scores, dynamically rendered character portraits, circular movement stick, orange attack button, parry/feint icons, pause/exit confirmation and result/next-round dialog. The legacy bottom HOME button and on-screen debugging instructions are removed; keyboard demo shortcuts remain available. Shake strength remains configurable from Home settings.

BattleLayout is the shared source for visible action rectangles and pointer hit testing. Landscape uses one action row; portrait stacks parry and feint above attack. The stick keeps its owning finger while another finger chooses an action. Orientation changes reset held input. The portrait camera pulls back and centers the fight above controls. Attack timings, reach, guard economy and parry windows are unchanged.

Pause freezes the combat update, camera and CombatEffects clock; Resume and scene exit restore time and reset input. BattleHud.Verify (-battleUiTest) checks both layouts, action hit areas, multi-pointer routing and pause/resume. Captures/report: QA/BattleUIVerified. This is desktop orientation/input-route validation, not a physical mobile-device test.



### カスタム画面の共通デザイン
メインと共通の HomePlate / HomeIcon を使用し、青緑のパネルとシアンの枠、オレンジのカラー保存ボタン、5つの下部タブに統一。横画面は左プレビュー・右編集、縦画面は上プレビュー・下編集。画面回転でも編集中の部位と色を保持する。カラーは保存ボタンで確定、装備の着脱は即時保存。`-customUiTest` は所有状態を分離した検証で、縦横プレビューと装備切替・画面遷移を QA/CustomDesignVerified に記録する。

### コレクション画面の共通デザイン
メインと同じ HomePlate / HomeIcon と5タブを使用。横は左3Dプレビュー・右所持装備、縦は上プレビュー・下所持装備。所持数、装備状態、詳細とカスタムへの導線を表示する。画面回転でプレビューの向きを保持し、閲覧だけで保存済みの装備や色を変更しない。`-collectionTest` で縦横、空の所持品、所持・装備中、画面遷移を検証する。

### ミッション画面の共通デザイン
メインと同じパネル・アイコン・5つの下部タブを採用。デイリー、ウィークリー、ログインに対応し、達成済みの未受取報酬をオレンジで強調する。縦横でカード内の進捗・報酬配置を切り替え、回転中も選択タブを保持。`-missionUiTest` は検証専用プロフィールで個別受取・一括受取・ログイン・二重受取防止と画面回転を検証し、QA/MissionDesignVerified にプレビューを保存する。
`n横画面のデイリー・ウィークリーは3枚のカードを横並びに配置。各カード内にタイトル、説明、進捗バー、報酬と受取ボタンをまとめる。縦画面は3段の縦並び。

### ランキング画面（オンライン未接続）
ホームのプレイヤー欄の RANKING から開く。月間レート／月間最高連勝の2タブ、UTC暦月の期間表示、自分の順位欄、ルールと端末内CPU戦績。縦横対応。現在はCPU戦のみで認証・対人マッチング・ランキングサーバーがないため、実順位やレートは表示せず準備中と明記する。架空の順位・他プレイヤー・報酬は作らない。
CPU戦は3本先取の完了試合だけ RankingStore に勝敗を1回保存。途中退出、テスト、デモを含む試合は対象外。既存のミッション集計は変更しない。新規機能のため過去試合は復元しない。
次のオンライン実装では、サーバーで確定した試合結果によるレート・連勝、シーズン確定順位・ソフトリセット・履歴と称号／外見報酬を接続する必要がある。現在これらの集計と配布は未実装。
`-rankingTest` は隔離プロフィールを使い、画面遷移・縦横表示・月境界・CPU戦績のシリアライズを QA/RankingVerified に記録する。
ランキングの横画面は左に期間・自分の記録・ルール、右に切替タブと順位一覧をまとめた2列構成。縦画面は従来配置を維持。

### メニュー切替演出
ホーム・カスタム・コレクションのカメラ設定を SetCharacterCamera に共通化。画面移動時もモデルの向きを保持。異なるページはUIを0.18秒でクロスフェードし、同一画面の色変更や更新は即時反映。旧UIは入力を受け付けず、連続遷移時は前のフェードを破棄する。対戦へは0.28秒暗転、シーンロード後0.22秒で復帰。`-menuTransitionTest` で縦横カメラ一致、回転保持、再描画、連続遷移、Duel移動を検証。

### Xboxコントローラー
左スティック移動、X攻撃、Yフェイント、Bパリィ。メニューボタンでポーズ／再開。メニューは方向キー／左スティックで選択、Aで決定、Bでホームへ（ポーズ中は再開）。リザルトはAで次ラウンド／再戦を選択できる。既存タッチ・マウス・キーボードの戦闘操作も併用可能。`-xboxTest` は仮想Gamepadで軸・3アクション・切断後の移動解除を検証する。物理コントローラーの接続確認は別途必要。

### 貝殻の獲得リアクション
ミッション個別／一括／ログインとホームのログイン受取に共通演出を追加。保存は即時に確定し、約0.85秒で加算表示・5粒の貝殻の飛翔・カウンター金色発光・数字のカウントアップと小さな弾み・短い音を再生。画面全体は揺らさない。一括受取は合計額で1回。音はホームのミュートに従う。画面変更で表示が消えても報酬の保存は維持。`-rewardTest` で個別・重複・一括・ログイン・途中離脱を隔離プロフィールで確認。

### ショップの共通UI
共通カメラ・HomePlate・HomeIcon・5タブ・ページフェードを使用。横は左プレビュー／右商品、縦は上プレビュー／下商品。所持貝殻、価格、購入後残高、確認・キャンセル・装備操作を商品パネルに集約。画面回転で購入確認状態を維持し、新規入店では確認を解除。退出時は試着を解除して実際の装備を復元。`-shopUiTest` は隔離プロフィールで購入確認・残高不足・重複購入防止・回転・退出を確認する。

### ラウンド／試合決着
通常ラウンドはROUND WIN/LOSEと勝利数マークを短く表示。1.65秒後に自動リセットし、0.85秒のREADY/FIGHT中は操作を止める。3勝目は自動進行せず、0.8秒の吹き飛ばし余韻後に勝者の正面斜めへ寄り、既存の下から戻るパンチを勝利ポーズとして再生。ゲージや操作UIを隠し、大きなWIN/LOSE、最終スコア、約3.2秒後に再戦・ホームボタンを表示。画面揺れは追加しない。現時点のWIN文字はUI前景。専用勝利音・スキップ・試合報酬の個別表示は未追加。`-finishTest` で自動進行、3本先取、勝利カメラ、再戦リセット、敗北と縦画面を隔離プロフィールで検証。
勝敗表示は敗者基準のLOSEを廃止し、PLAYER WIN／CPU WINに統一。通常ラウンドはROUND / PLAYER WINまたはROUND / CPU WIN。

### UI余白・文字整列の統一
DrawBrandでSHAKO / PUNCHを単一のリッチテキストに統一し、位置(28,12)、文字サイズ24、補足(28,48)を共通化。装飾用の英字1字ごとの空白と重複空白を除去。見出しは太字、1行テキストは縦中央、複数行は上揃え。DrawMenuNavigationで6画面の下部タブを共通化し、外側28・間隔8、アイコン＋文字を中央配置。キャラ画面のパネル内余白24、縦画面外余白28を共通化。ホーム上部カードは間隔12に統一。下部説明文とナビ背景の重なりを解消。`-uiLayoutTest` で6画面×縦横と装備有無・設定・ルール・ログインのプレビューを確認する。

## Character name and planned iOS account integration
- Home character name is editable by tapping its name. Initial name: シャコ丸. 1–12 Unicode text elements; trim/normalize; reject control characters and rich-text markup delimiters.
- Saved locally in PlayerPrefs Mantis.CharacterName.v1. Home profile, character title, custom title and ranking self row share CharacterProfile.Name.
- iOS direction: Game Center authentication linked to Unity Authentication; game-owned character display name and leaderboard UI. Purchases via Unity IAP / Apple App Store. These online services are NOT implemented or connected by this change.
- Production integration must key accounts by authenticated player ID (never character name), sync the profile server-side and validate public names before publication. Name changes must not alter account identity, purchases or ratings.
- Current checks: Windows build; portrait/landscape UI; name dialog rendering; empty/too-long/markup rejection. iOS keyboard and device authentication remain untested.

## Initial registration and shell pack preview
- Home requires CharacterProfile.Registered (valid saved name) and opens registration if absent. No cancel on initial registration; underlying buttons disabled. Typed name survives orientation change; later rename remains optional.
- Shell shop entry: home wallet / equipment shop header. Four proposal packs: 300/JPY160, 1000/JPY480, 2200/JPY960, 6000/JPY2400. These are design prices, NOT fetched or confirmed App Store prices.
- Purchase controls are unavailable; no payments or currency grants. Before enabling: configure consumable products in App Store Connect and Unity IAP, use localized store prices, authenticate wallet, server-validate and deduplicate transaction grants, handle pending/cancel/retry and cross-device balance.
- QA/OnboardingShopVerified: isolated character and mission storage, required/empty/save/rotation checks, both shop orientations, four disabled packs, unchanged balance, navigation. No live purchases or iOS device test performed.

## English default and Japanese localization
- MantisLanguage selects Japanese for Application.systemLanguage == Japanese; all other device languages use English. No location/GPS lookup. QA overrides: -lang-en / -lang-ja.
- Shared menu and battle label creation translates static content and formats dynamic shell/ranking/reward labels. Character names remain verbatim. Text fitting and wider English navigation labels prevent overflow.
- QA/Localization/{en,ja}: 16 menu states in portrait and landscape, initial registration; checks Japanese remnants in English static UI and generated text height against actual rectangles. Window rendering checked visually. iOS fonts/keyboard/safe-area require later device testing.
- Combat floating action words removed. Stun stars use the mesh vertices weighted to the eye bones to locate the eyes (cached bone-local centroids), then follow the current pose after camera updates. Only yellow stars remain.

## Text contrast and localized title
- English header: MANTIS / PUNCH. Japanese header retains SHAKO / PUNCH. Product identifiers and saved data keys are unchanged.
- Opaque dark menu panels, darker active backgrounds, brighter secondary copy, darker orange button fills for white labels; disabled buttons keep full-opacity readable labels.
- Unbacked text over the reef receives a compact dark backing. Menu and battle text have a thin dark outline.
- Localization verification now also estimates contrast against the layered UI at the text alignment position, excluding intentionally dimmed content behind a modal. English and Japanese 16-state portrait/landscape checks pass the 4.5:1 threshold, translation and text-height checks. This is a UI color calculation plus visual preview, not iOS display certification.

## Correction: remove per-label contrast boxes
- Per-label black rectangles and the extra menu text outlines were rejected and removed.
- Character preview headings now share a soft area gradient that fades into the reef; portrait home gradient is limited to the name side so it does not cover the face. Edit name is a normal small action button.
- Footer notes use one consistent bottom band. Existing dark panels and readable secondary colors remain.
- This supersedes the compact per-label backing approach above. Visual appearance must be reviewed alongside numerical contrast checks.

- User preference: restored the original bright orange gradients on primary actions, navigation Fight, and battle Attack. The previous strict 4.5:1 palette change is superseded for these accent buttons; contrast audit may flag these intentionally restored colors.

## Redo: color-only correction over the reef
- Removed all added title gradients, per-label black boxes, footer backing bands and additional text outlines. The soft-area-gradient approach above was also rejected and is superseded.
- Character names, preview headings, rename link, descriptive copy and character-page footer notes on the light reef now use dark navy. Text on existing dark UI stays light. Restored orange button gradients remain unchanged.
- Both language/orientation screenshot sets reviewed. Translation/overflow checks found no issues. The numerical contrast report still flags the user-approved bright orange buttons; these were intentionally preserved, not darkened again.

- Final user direction: reef-overlay text is white with a glyph-shaped shadow only (offset 1.2, -1.5 UI units, dark alpha .85). This replaces the dark navy text approach. No text background boxes, bands, or area gradients. Bright orange controls remain unchanged. The flat-color contrast audit does not measure glyph shadows; visual review is required.

## Name editing entry (2026-09-26)
Character-name editing is now under Settings. Home shows the name without an edit link or tap target. Saving or cancelling an edit opened from Settings returns to Settings. Required first-launch registration remains. Windows build succeeded; English Settings preview checked in landscape and portrait with no text overflow/untranslated labels in the existing localization audit. Existing contrast warnings remain; this change does not alter visual styles. Play-energy monetization remains discussion only.

## First-run tutorial (2026-09-27)
After character registration, unfinished first-run training starts automatically once per app session. Settings offers replay; starting a normal match before completion routes to training. Four lessons: automatic guard and guard break, attack, feint bait then attack, successful parry then counterattack. Training fixes spacing, scripts the opponent, retries failed attempts, and records no round wins, missions or ranking results. Completion is saved only via Finish. English/Japanese explanations, responsive battle HUD, touch/keyboard/Xbox actions; Enter/A advances lesson prompts. Training simulation verified all four successes in both orientations and both languages. iOS device interaction remains untested. CPU personality and independent color variants are design proposals only.

## Robot armor set (2026-09-27)
Blender source, FBX, GLB and studio renders: ArtSource/RobotSet. Six independently weighted replacement modules use the D character skeleton: head, left arm, right arm, abdomen, tail and legs. Dedicated ivory/petrol/graphite/red/brass/cyan materials. Imported model has 23,035 triangles. Customize > Robot armor allows independent equip/remove and full-set toggling; selections persist and apply to home and battle via MantisLoadout. Robot head hides the crown while equipped. Prototype equipment is available without a purchase; pricing and collection listing are not added. No combat stats or hitbox changes. Verified six replacements, restoration, one-arm combination, sampled punch vertex deformation and both orientations; existing crown UI test passes. Mobile GPU performance not measured.

## Boxer equipment (2026-09-27)
Added Boxer_Set Blender/FBX/GLB and previews in ArtSource/BoxerSet. Customize > Boxer offers six independent slots and full-set equip. Shared equipment UI retains the selected set on orientation changes. Robot/boxer masks are mutually exclusive per slot, allowing mixed outfits. Head variants hide the crown. Tested six replacements, mapped bones, punch deformation, portrait layout, mixed-set precedence and restoration; robot regression passes. Imported full set: 29,871 triangles. No stats/hitbox changes or shop price introduced.

## Free equipment shop (2026-09-27)
Custom now has a prominent Shop entry and an owned-equipment entry. Shop lists Crown, Robot and Boxer with Preview, Get free, and Equip. All three are free during testing, including Crown; shell balance never decreases. Set ownership persists, existing equipped sets are migrated to owned, and acquisition is idempotent. Collection lists owned gear and links to shop. Equip applies a full set then opens individual slot controls. Preview does not save equipment; exiting restores the saved loadout. Crown removal remains available. Legacy crown-only shop/custom/collection verification entry points now run the updated free-equipment flow. New -freeEquipmentTest verifies acquisition, unchanged balance, equip, collection, orientation and preview restoration.

## CPU identities (2026-09-27)
Each new match independently samples one of seven personalities and one of eight recolored atlas materials. Identity and palette persist between rounds; rematch/Restart rolls again. Personalities: aggressive, parry, feint, retreat, balanced, guard pressure and adaptive. Weighted actions, spacing, commitment cadence, retreat speed/cooldown and parry propensity differ. Guard pressure can follow a block with another attack while protecting its own guard reserve. All styles eventually initiate. Observation uses visible action starts with reaction delay; bounded learning preserves personality differences. Adaptive begins balanced and chooses a strategy between rounds after evidence, or after at least 12 combat seconds and three observations in a prolonged round. No personality labels or color mapping. HUD CPU portrait refreshes on a new opponent. -cpuVarietyTest exercises independent draws, round persistence, styles initiating, adaptation to parries, comparative reaction rates and all eight material assets. Balance requires further player feedback.

## Unicorn equipment (2026-09-27)
Added the six-part white/gold unicorn costume with pastel mane, horn, wings, tail locks and gold hoof/club tips. Available free during testing from Customize > Shop. Four-item shop, collection and cross-set slot exclusivity support Crown, Robot, Boxer and Unicorn. Existing rig retained; no combat balance changes. Build and unicorn/free-equipment validation passed. Source and exports: ArtSource/UnicornSet.

Collection loadout fix: entering Collection now displays the saved six-part loadout, including mixed sets and crown state. Explicit Preview still works within the page; re-entry clears transient preview. Rotation preserves the current display. Regression checks compare all six equipped meshes before/after entry and rotation.

Economy v1: testing-period free acquisition replaced by priced cosmetics; existing ownership preserved. Match wins 20, losses 5, daily first win +30, login 10, daily missions 70 total, weekly missions 300 total. Prices and provisional unavailable IAP packs centralized in ShellEconomy.cs. See ECONOMY_BALANCE.md.

## Equipment navigation revision (2026-09-27)
Customization now opens with Owned gear / Colors modes. Choose one of six body slots; owned compatible parts and Standard appear as mesh thumbnail cards. Selecting a card equips only that slot and saves automatically, preserving the other slots. Reset this part and Equip full set are explicit actions. Colors remain a separate save operation for base parts.
Shop has Equipment / Shells tabs; equipment preview/purchase stays there, while owned items link to Customize without silently equipping a full set. Collection displays ownership and links owned items to Customize, unowned items to Shop. Entering Collection still preserves the current loadout. Currency and paid-purchase availability are unchanged.
Equipment flow verified in Japanese and English: mixed slot equipment, crown/reset/full-set actions, collection-to-custom keeps loadout, equipment/shell shop navigation, portrait/landscape, unchanged wallet. Economy confirmation and duplicate-purchase regression passed.

## Boxer micro bonus (2026-09-27)
Successful guarded attacks gain +0.1 guard damage per actually equipped Boxer module; six parts add +0.4 set bonus for +1.0 total. Standard guarded damage remains 100/3; full Boxer becomes 100/3+1. This is absolute guard-gauge points, not percent. Mixed sets count only Boxer parts. No bonus on parry, clash, miss or unguarded lethal hits; attack cost, range and timing unchanged. MantisLoadout tracks actual applied Boxer slots per character, avoiding player-preference leakage into CPU stats. Reset/replacement removes matching bonus bits. Custom shows current total; shop/collection Boxer preview shows the purchase-time rule.
Verification passed for all 64 slot masks: exact actual guard loss, three consecutive full-gauge blocks to break, removal/replacement, home-to-battle transfer, CPU independence, unchanged attack cost, parry, and one-hit knockout. Recovery and already-partially-drained gauges can still change threshold outcomes, as intended for a micro bonus.

## Equipment micro bonuses (2026-09-27)
Robot: attack guard-cost saving +0.1 per part, +1.0 with all six (cost 10 to 9). Incoming guarded damage is deliberately unchanged, since reducing 100/3 by even 0.1 would change three-block break to four. Unicorn: normal movement +0.1% per part, +1.0% total with six (1.65 to 1.6665 units/s); attack lunge/feint movement unchanged. Crown: +0.1 guard points recovered per second, unchanged 2-second recovery delay; lost when another head replaces it. Boxer unchanged. Per-character actual mesh loadout masks drive effects, not shared player preferences. Mixed sets receive only their equipped parts; six-part set bonuses require all six. Custom summary displays combined active bonuses, and each selected set's rule is shown in custom/shop/collection.
Micro bonus validation passed: all 64 Robot and Unicorn masks, actual attack cost/movement distance/recovery, unchanged recovery delay, mixed effects, crown overridden by head gear, complete removal, CPU isolation; equipment navigation regression and English portrait display checked.

## Rewarded ads and CPU names (2026-09-27)
Optional rewarded ads are available in the shell shop and match results: 30 shells on completion, 2 completed rewards per UTC day shared across both entry points. Google Mobile Ads Unity 11.5.0 is installed; supplied iOS IDs are configured. Development builds use official test units; tapping the Settings heading 50 times enables persistent test-only mode on this installation (each tap within 8 seconds of the previous). Desktop/Editor shows a clearly labelled simulation. No interstitials/banners. Native iOS delivery requires Mac/iPhone verification; age treatment is provisionally Child/G pending release-audience decision. See REWARDED_ADS.md.
CPU handles use 40 English and 10 Japanese fictional names, independently sampled from color/personality and unchanged between rounds. Immediate repeats and the player's name are excluded. Battle HUD shows the name with a small BOT label; enemy victory uses its name. These are local CPU opponents; online matchmaking remains unimplemented and CPU results do not affect online ratings.

## Online integration beta (2026-09-28)
Supersedes the previous offline-only status: the client now contains MPS Quick Join/Relay public matches, private friend codes, host-authoritative combat snapshots and two-party friend rematch. Public results return to a fresh search, not same-room rematch. The ranking UI reads the top ten and player entry; the paired-report Cloud Code source is included but not deployed. Bundle ID is com.kuroki.shako. IAP 5 purchasing/restoration and verified grant handling are wired, but shako_premium is intentionally unconfigured pending product definition; its purchase-validation endpoint is not yet implemented/deployed. Existing rewarded ads remain available.
Windows build and two-process localhost tests passed: handshake, names, attack state, score perspective, bilateral rematch, public-rematch restriction. Japanese/English portrait/landscape menu and ten-row ranking layout checks passed using isolated fixtures. First-to-three presentation regression passed. Backend ranking mock tests passed. Live UGS/Relay, StoreKit and iPhone ads remain unverified. See ONLINE_SETUP.md for the exact setup and remaining work; this is not a release-ready monetization/competitive backend.

## Shop layout and iOS shell packs (2026-09-28)
- Equipment and Shells share their header, tabs and wallet position.
- Shared bottom navigation has equal 12-unit top/bottom padding.
- Four consumable product IDs and amounts are documented in IOS_PRODUCTS.md. Purchases remain disabled pending App Store registration and server verification.
- QA/ShopLayout and QA/ShopLayout-JA contain English/Japanese landscape and portrait captures. SKU mapping, wallet positions and six pages of navigation padding passed verification.

