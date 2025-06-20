# Survey for VRChat

このリポジトリは、VRChat内で使用するアンケート機能「Survey for VRChat」を提供します。この機能を利用することで、イベントに参加したユーザーの声をリアルタイムに収集できます。

## 概要
「Survey for VRChat」は、その場でのアンケートを可能にするため、イベントの熱が冷めないうちに、参加者からフィードバックを得られます。また、ウェブとの連携により、質問の作成や結果確認もスムーズに行えます。
- 直感的なUIパネル
- 4種類の質問形式（選択式／段階評価／星評価／自由記述）
- 回答を即座に反映・グラフ化
- 一斉表示型／常設型の2モード対応

## インストール方法

このパッケージはVCC（VRChat Creator Companion）を通じて簡単にインストールできます。以下の手順に従ってください：

1. VCCを開きます。
2. 「Add Repository」をクリックし、以下のURLを入力してください: `https://wondernote.github.io/SurveyForVRChat/index.json`
3. 「Add」ボタンをクリックして、リポジトリをVCCに追加します。
4. パッケージリストから「Survey for VRChat」を見つけ、インストールを進めてください。

## 使い方
1. `Runtime` フォルダ内の `Survey.prefab` または `SurveySwitch.prefab` をシーンに配置します。
   - `Survey.prefab` は常設型、`SurveySwitch.prefab` は一斉表示型です。
2. 配置したプレハブを選択し、インスペクターで「アンケートコード」を入力してください。
   
詳しくは[公式ドキュメント](https://wondernote.net/survey)をご覧ください。

## 依存関係と互換性
- Unityバージョン: >= 2022.3
- VRChat SDK: com.vrchat.worlds >= 3.7.6, com.vrchat.base >= 3.7.6

## 連絡先
サポートが必要な場合、またはフィードバックをお持ちの場合は、[contact@wondernote.net](mailto:contact@wondernote.net)までご連絡ください。

## ライセンス
このプロジェクトは「WonderNote Survey Custom License」の下で公開されています。詳細は[こちら](https://github.com/wondernote/SurveyForVRChat/blob/main/LICENSE.txt)を参照してください。