---
id: add-hardware-profile
title: 機器情報の追加方法
---

新しいフェイシャルトラッキング機器を対応表に追加する場合は、対応表JSONを追加します。
対応可否は公開資料や実機検証に基づき、根拠URLを必ず残します。

## 追加するファイル

主に編集する場所は以下です。

| 種類 | パス |
| --- | --- |
| 対応表JSON | `Editor/Data/HardwareSupport/Profiles/*.json` |
| 表情名一覧（新しい表情キーを追加する場合のみ） | `Runtime/Data/Expressions/UnifiedExpression.cs` |

## 1. 対応表JSONを追加する

`Editor/Data/HardwareSupport/Profiles/` にJSONを追加します。
`profile` は機器を識別する一意なキーです。
`id` はInspectorでの並び順と保存時のビット位置（`1 << id`）の両方に使うため、既存値と重複しない値にします。

```json
{
  "profile": "NewHardware",
  "displayName": "New Hardware",
  "id": 18,
  "sources": [
    {
      "title": "New Hardware tracking specification",
      "url": "https://example.com/spec",
      "note": "UnifiedExpressionへの対応を確認した資料。"
    }
  ],
  "full": [
    "EyeLookOutRight",
    "EyeLookInRight",
    "EyeLookUpRight",
    "EyeLookDownRight"
  ],
  "converted": [
    "EyeClosedRight"
  ],
  "unknown": [
    "TongueOut"
  ]
}
```

各フィールドの意味は以下です。

| フィールド | 必須 | 説明 |
| --- | --- | --- |
| `profile` | 必須 | 機器を識別する一意なキーです。 |
| `displayName` | 必須 | Inspectorに表示する機器名です。 |
| `id` | 必須 | ビット位置とInspectorでの並び順です。0から30の範囲で、既存値と重複しない値にします。 |
| `sources` | 必須 | 対応状況の根拠資料です。少なくとも1件入れます。 |
| `full` | 任意 | 機器がそのまま出力できる `UnifiedExpression` です。 |
| `converted` | 任意 | 変換、合成、左右統合、エミュレーションなどで出力できる `UnifiedExpression` です。 |
| `unknown` | 任意 | 公開資料だけでは判断できない `UnifiedExpression` です。 |

`full`、`converted`、`unknown` のいずれにも含めない表情は `非対応` として扱われます。

既存の `id` を変えると、その機器の保存済みビットフラグも変わります。既存IDは固定値として扱ってください。

## 2. 表情名を確認する

JSONに書ける表情名は `UnifiedExpression` の値だけです。
存在しない名前や `None` を指定すると読み込みエラーになります。

機器側のドキュメントが独自名を使っている場合は、VRCFaceTrackingのUnified Expressionsに対応づけてから記載します。
1つの機器内で同じ表情名を重複して書く必要はありません。

## 3. テストを実行する

UnityのEditModeテストで以下を確認します。

- JSONが読み込めること
- `profile` が重複していないこと
- `id` が重複していないこと
- JSON内の `UnifiedExpression` 名がすべて存在すること

## 判定基準

対応状態を選ぶときは、以下の基準にそろえます。

| 状態 | 判断基準 |
| --- | --- |
| `full` | 機器またはVRCFTモジュールが対象の表情値を直接出力します。 |
| `converted` | 別信号からの推定、左右共有、合成値、モジュール内変換などを含みます。 |
| `unknown` | 資料上の記載が不足しており、対応可否を確定できません。 |
| 非対応 | 対応なし、または対応が確認できないため出力対象にしません。 |

判断に迷う場合は `unknown` に置き、`sources.note` に未確定理由を残します。
