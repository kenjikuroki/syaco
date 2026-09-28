# D案 ベースキャラクター

全体デザインは同梱の reference_D.png。触角と腹肢は、その後ユーザーが提示した実物写真と修正指示を反映。既存ゲームのモデル・シーン・ビルドは変更していません。

## 成果物
- D_Mantis_Base.blend: 編集用。CHARACTER コレクションが本体、STUDIO はプレビュー専用。
- D_Mantis_Base.fbx: スケルトン、UV、共有マテリアル、埋め込みテクスチャ付き。
- D_Mantis_Base.glb: テクスチャを含む確認・受け渡し用。
- D_Character_Albedo.png: 1024x1024 の共通カラーアトラス。
- 01_three_quarter.png / 02_front.png / 03_side.png / 04_rear.png: 同じモデルのレンダープレビュー。

## モデル構造
6部位: 頭胸部、左捕脚、右捕脚、腹部、尾、歩脚。
1マテリアル。Shade Smooth と55度以上のシャープエッジ指定、最小限のベイク済み面取り。
各甲羅や捕脚は断面を連続させた閉じた形状で、骨にウェイト付けしています。甲羅の重なりや関節部は別の閉じたメッシュが接続する構成で、全身が一枚の連続トポロジーではありません。
三角形数などの実測値は validation.json を参照。

Blender: メートル、Z up、顔は -Y。ルートは地面付近の (0,0,0)。FBX は -Z forward / Y up の設定で出力。
43ボーン。socket_head / socket_back / socket_glove_L / socket_glove_R は非変形の装飾取り付け位置。
捕脚は merus / propodus / dactyl の3段構造で、開閉するハサミ用の対向指はありません。
腹部モジュールには5対の腹肢と簡略化したエラを含み、腹肢は10本の専用ボーンで動かせます。触角は眼柄の付け根近くの頭部前方から伸びます。
D_Idle_Inspection は目・尾・腹肢の確認用アニメーション。腹肢は位相をずらして緩やかに動きます。本番のパンチ・歩行・パリィのアニメーションは未制作です。
旧A6と同名の骨もありますが、レスト姿勢と比率が異なるため、既存の装備やアニメーションにそのまま互換とは扱わないでください。

## 確認
verify_exports.py で FBX / GLB をそれぞれ新規シーンに再インポートし、6メッシュ・1リグ・UV・マテリアル・画像参照・スケール・ウェイトと捕脚の変形を検証。
結果は export_verification.json。スマホ実機性能やUnityの戦闘アニメーションは未検証です。

再生成: Blender --background --python create_D_character.py
書き出し確認とコレクション整理: Blender --background --python verify_exports.py

## Unity用の可動モデル（2026-09-25）
- D_Mantis_Base は承認済みの確認用原本として維持。
- create_game_model.py で D_Mantis_Game.blend / .fbx を生成。42フレームの下回りパンチと上体の持ち上げ、足の接地補正をベイク。
- 原本の43ボーンに打撃位置用マーカー2本を追加（計45本）。腕の戻りは戦闘側でパンチを逆サンプリング。
- 腹肢10本はUnity側で位相をずらして動かし、ヒットストップ・スローに追従。歩脚は2関節構造で動作。
- D_CrownHead.fbx は新しい頭胸部と同じレスト姿勢の王冠交換パーツ。
- Unity素材: Assets/Mantis/Art/D。DModelBuilder.Buildで対戦モデルを差し替え、HomeBuilderでホームを再生成・ビルド。
- 実行ファイル: Builds/DCharacterDuel/ShakoD.exe。
- QA/DModelVerified: 新モデル・腹肢・王冠・パンチ・対戦への移動を検証。
- QA/DPresentationVerified: 既存戦闘演出38項目を検証、全項目成功。
- 元のシーンと王冠は Before_Unity_Integration に退避。スマホ実機の性能測定は未実施。

