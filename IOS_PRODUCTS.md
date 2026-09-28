# App Store Connect 商品登録一覧

アプリのBundle ID: `com.kuroki.shako`

| 参照名 | 商品ID | 種類 | 付与する貝殻 | 仮の日本価格 |
|---|---|---|---:|---:|
| Shells 800 | `shako_shells_800` | 消耗型 / Consumable | 800 | ¥160 |
| Shells 2800 | `shako_shells_2800` | 消耗型 / Consumable | 2,800 | ¥480 |
| Shells 6000 | `shako_shells_6000` | 消耗型 / Consumable | 6,000 | ¥960 |
| Shells 16000 | `shako_shells_16000` | 消耗型 / Consumable | 16,000 | ¥2,400 |

コード側の定義は `Assets/Mantis/Resources/AppleProduct.json`。`shako_premium` は今回の4パックには使用しません。

この一覧は商品IDの定義です。App Store Connectへの商品登録・価格設定・審査提出はまだ行っていません。各IDをそのままApp内課金の消耗型商品として登録してください。上記価格は既存の仮価格であり、本番画面ではAppleから取得する地域別価格を表示します。

4商品の取得、商品ごとの購入と付与量照合にクライアントを変更済みです。決済検証サーバーとApp Store設定が完了するまで `paymentsEnabled` は false に保ちます。未完了のサーバー詳細は ONLINE_SETUP.md を参照してください。消耗型の使用済み貝殻を購入復元で再付与することはありません。
