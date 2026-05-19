---
id: add-hardware-profile
title: 機器情報の追加方法
---

新しいフェイシャルトラッキング機器を対応表に追加する場合は、機器を識別するenumと、対応表JSONを追加します。
対応可否は公開資料や実機検証に基づき、根拠URLを必ず残します。

## 追加するファイル

主に編集する場所は以下です。

| 種類 | パス |
| --- | --- |
| 機器ID | `Runtime/Data/FaceTrackingHardwareProfile.cs` |
| 対応表JSON | `Editor/Data/HardwareSupport/Profiles/*.json` |
| 表情名一覧 | `Runtime/Data/Expressions/UnifiedExpression.cs` |
| テスト | `Tests/Editor/HardwareSupportDataTests.cs` |

## 1. 機器IDを追加する

`FaceTrackingHardwareProfile` に新しい値を追加します。
値はビットフラグなので、既存の最大値の次の `1 << N` を使います。

```csharp
NewHardware = 1 << 18,
```

`None` は未選択を表す予約値です。機器定義には使わないでください。

## 2. 対応表JSONを追加する

`Editor/Data/HardwareSupport/Profiles/` に、enum名と対応するJSONを追加します。
`profile` は `FaceTrackingHardwareProfile` の値と完全一致させます。

```json
{
  "profile": "NewHardware",
  "displayName": "New Hardware",
  "displayOrder": 18,
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
| `profile` | 必須 | `FaceTrackingHardwareProfile` のenum名です。 |
| `displayName` | 必須 | Inspectorに表示する機器名です。 |
| `displayOrder` | 必須 | Inspectorの機器一覧で使う並び順です。既存値と重複しない値にします。 |
| `sources` | 必須 | 対応状況の根拠資料です。少なくとも1件入れます。 |
| `full` | 任意 | 機器がそのまま出力できる `UnifiedExpression` です。 |
| `converted` | 任意 | 変換、合成、左右統合、エミュレーションなどで出力できる `UnifiedExpression` です。 |
| `unknown` | 任意 | 公開資料だけでは判断できない `UnifiedExpression` です。 |

`full`、`converted`、`unknown` のいずれにも含めない表情は `非対応` として扱われます。

## 3. 表情名を確認する

JSONに書ける表情名は `UnifiedExpression` の値だけです。
存在しない名前や `None` を指定すると読み込みエラーになります。

機器側のドキュメントが独自名を使っている場合は、VRCFaceTrackingのUnified Expressionsに対応づけてから記載します。
1つの機器内で同じ表情名を重複して書く必要はありません。

## 4. テストの期待値を更新する

新しい機器を追加したら、`HardwareSupportDataTests.Profiles_LoadsJsonInDisplayOrder` の期待リストに追加します。
`displayOrder` に合わせた位置へ入れてください。

その後、UnityのEditModeテストで以下を確認します。

- JSONが読み込めること
- `profile` とenumが一致していること
- `displayOrder` が重複していないこと
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
