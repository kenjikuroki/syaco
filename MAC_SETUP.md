# Macで開発を続ける

1. Git LFSをインストールし、`git lfs install` を実行。
2. `git clone https://github.com/kenjikuroki/syaco.git`
3. `cd syaco` → `git lfs pull`
4. Unity HubでUnity 6000.3.9f1とiOS Build Supportをインストール。
5. このフォルダーをUnityプロジェクトとして開く。初回はパッケージ取得とインポートを待つ。
6. `Assets/Mantis/Generated/Home.unity` を開いてPlay。
7. iOSはBuild Profilesから切り替え、Xcodeプロジェクトへ書き出す。Xcodeで自分のTeamと署名を設定。

Bundle ID: `com.kuroki.shako`

Windowsの実行ファイル・Library・QAキャプチャはGitに含めません。モデル・音源・シーン・パッケージ設定は含めています。

Game Centerの名前取得用ネイティブ処理は追加済みですが、Xcodeビルドと実機認証は未確認です。現時点の対戦通信・ランキングはまだUGS実装であり、Game Centerへの移行が残っています。課金4商品は定義済みですが決済は無効です。詳細はONLINE_SETUP.md、IOS_PRODUCTS.md、REWARDED_ADS.mdを参照。
