# VRCFaceTracking 5系 OSC送信仕様 調査メモ

- 更新日: 2026-08-24
- 対象: VRCFaceTracking `master`（AssemblyVersion `5.4.5.0`）
- 目的: U-Stella FaceTrackingが生成するVRChatアバター用パラメーターとAnimatorを、VRCFaceTracking（以下VRCFT）5系の現行OSC送信実装に照合する
- 対象外: トラッキング機器からVRCFTへデータを入力するModule SDK

> GitHub Releases上の最新版 `5.2.3.0` は非サポートと明記されている。そのため、本メモではSteamで配布される現行5系に近い `master` の実装を基準とする。

## 結論

現行の生成方式はVRCFT 5系の送信仕様と整合している。

- `USTL/v2/...` プレフィックスは、VRCFTの末尾一致によるパラメーター検出に対応する。
- FloatとBinaryの名前、型、値域、Binaryの復号方法は現行実装と一致する。
- `EyeLid*` は `0 = 閉じ`、`0.75 = 通常開眼`、`1 = 見開き` として扱う。
- `TongueArchY` は正がCurl Up、負がBend Down、`TongueShape` は正がFlat、負がSquishである。
- `VRChat Native` 選択時にアバター用の視線・まぶたパラメーターを生成しない方式は、VRCFTのネイティブ視線OSCへのフォールバック条件に合う。
- 同期コストは、最終的に生成される一意なExpression Parameterと型から計算する。

## 仕様判断の優先順位

1. [VRCFTの送信実装](https://github.com/benaclejames/VRCFaceTracking): 実際に送られる名前、型、値、送信条件
2. [VRCFT公式Docs](https://docs.vrcft.io/): 公開されたアバター作者向け仕様
3. [VRChat公式Docs](https://docs.vrchat.com/docs/osc-overview): OSC受信、OSCQuery、Expression Parametersの仕様

VRCFT公式Docsは、最適化用パラメーターの一部を公開表へ掲載していないと明記している。このため、対応可否の最終判断にはソース上のパラメーター定義を使う。

## 1. データフローとOSCトランスポート

```text
Tracking Module
      |
      v
UnifiedTrackingData -> Calibration / Filter / Corrector
      |
      v
装着中アバターに存在し、OSC型も一致するパラメーターを有効化
      |
      v
OSC Messageをキューへ追加
      |
      v
IPv4 UDP -> VRChat受信ポート -> ローカルアバターAnimator
                               |
                               +-> SyncedなExpression Parameterを他ユーザーへ同期
```

| 項目 | 現行仕様 |
| --- | --- |
| VRCFTの既定送信先 | `127.0.0.1:9000` |
| VRCFTの既定受信ポート | `9001` |
| VRChat側の既定 | 受信 `9000`、送信 `9001` |
| 更新ループ | 約10 ms周期 |
| パケット化 | 複数のOSC MessageをOSC Bundleへまとめ、送信バッファに収まる単位で送信 |
| 信頼性 | UDPのため到達、再送、順序を保証しない |

VRCFTからローカルVRChatへのOSC更新と、VRChatから他ユーザーへの同期は別の処理である。リモート表示の更新頻度、量子化、帯域はVRChatの同期仕様に従う。

## 2. アバターと送信対象の検出

VRChatはアバター切り替え時に、アバターIDをString引数として `/avatar/change` へ送る。VRCFTはこれを受けてアバターのOSC定義を読み直し、送信対象を再構築する。

現行VRCFTはOSCQueryからVRChatの `/avatar` ノードを取得する。静的OSC Config JSONも互換経路として使われる。

```text
%USERPROFILE%\AppData\LocalLow\VRChat\VRChat\OSC\{userId}\Avatars\{avatarId}.json
```

通常は、アバターのOSC入力定義が次をすべて満たす場合に送信対象となる。

- OSC input addressの末尾がVRCFTのパラメーター名と一致する。
- 大文字・小文字が一致する。
- OSC入力型がVRCFT側の候補型と一致する。
- 一致後は、OSCQueryまたはJSONに記載された実際の `input.address` へ送る。

例えば、生成するFloatパラメーターは次のように解決される。

```text
Avatar parameter: USTL/v2/JawOpen
OSC address:      /avatar/parameters/USTL/v2/JawOpen
OSC argument:     Float
```

VRCFTは `v2/JawOpen` より前のカテゴリを許容するため、`USTL/v2/JawOpen` は検出対象になる。Floatとして送信させるパラメーターをVRChat側でIntやBoolとして定義した場合は、同名でもFloat候補に一致しない。

アバターパラメーターの追加、削除、改名、型変更を伴う再アップロード後は、VRChatのAction Menuで `Options > OSC > Reset OSC Config` を実行する。古いConfigが残るとVRCFTが現行パラメーターを検出できない。

`Build & Test` ではPublished Avatar用のOSC Config JSONが生成されない。通常の関連性検出まで含めた確認にはPublished Avatarを使う。`Force Relevancy` はFloat送信の診断には使えるが、Binaryのbit構成を検出する用途には適さない。

## 3. Floatパラメーター

アバター側に同名のFloat Expression Parameterがある場合、VRCFTはOSC Floatを1個送る。

| 値の種類 | 範囲 | 意味 |
| --- | --- | --- |
| 符号なし表情 | `0..1` | `0`が無表情、`1`が最大 |
| 符号付き統合軸 | `-1..1` | 正負に異なる表情または方向を割り当てる |
| `EyeLid*` | `0..1` | `0`が閉じ、`0.75`が通常開眼、`1`が見開き |
| Tracking Active | Bool | Moduleの有効状態。通常の `v2/` Float群とは別 |

VRCFTがローカルへ送るOSC Floatの精度と、Synced FloatがVRChatのExpression Parameters上で消費する8 bitは別である。

VRCFT公式Docsが現行のアバター実装形式として公開しているのはFloatとBinaryである。ソースには同名のBool候補もあるが、U-Stella FaceTrackingの生成形式には使用しない。

## 4. Binaryパラメーター

Binary形式では、1つのFloat値を複数のBool Expression Parameterへ分解する。4 bitの符号付き `USTL/v2/JawX` は次の5個を使う。

| パラメーター | 用途 |
| --- | --- |
| `USTL/v2/JawX1` | magnitude bit 0 |
| `USTL/v2/JawX2` | magnitude bit 1 |
| `USTL/v2/JawX4` | magnitude bit 2 |
| `USTL/v2/JawX8` | magnitude bit 3 |
| `USTL/v2/JawXNegative` | `true`なら負 |

生成時は次の条件を守る。

- bit名の数値サフィックスは `1, 2, 4, 8, ...` と連続する2の累乗にする。
- bitと `Negative` はBoolで定義する。
- signed値だけ `Negative` を追加する。
- 同じベース名のLocal Only FloatとSynced Binaryは併設できる。VRCFTは一致する両形式へ送信する。

### 現行実装の量子化

入力 `x`、magnitude bit数 `N` に対し、送信値は次の処理に相当する。

```text
steps = 2^N
max   = steps - 1

if Negativeが存在せず、x < 0:
    q = 0
else if abs(x) > 0.99999:
    q = max
else:
    q = floor(abs(x) * steps)

negative = Negativeが存在し、かつx < 0
```

Animator側はbitの合計 `q` を次の式でFloatへ戻す。

```text
decoded = q / (2^N - 1)
if negative:
    decoded = -decoded
```

たとえば2 bitでは、入力が `0.25`、`0.5`、`0.75` に達するたびに次の段階へ進み、復号値は `0`、`1/3`、`2/3`、`1` となる。`abs(x) > 0.99999` は最大値へ飽和する。

VRCFTは検出したbitの個数から量子化段数を決めるため、`1` から始まる連続したbitを生成する必要がある。bit数が少ないと `EyeLid*` の中立値 `0.75` を正確に表せない点にも注意する。

## 5. 符号付きパラメーターの向き

| パラメーター群 | 正 | 負 |
| --- | --- | --- |
| `EyeLeftX`, `EyeRightX`, `EyeX` | 右 | 左 |
| `EyeLeftY`, `EyeRightY`, `EyeY` | 上 | 下 |
| `BrowExpression*` | 眉上げ | 眉下げ |
| `CheekPuffSuck*` | 頬を膨らませる | 頬を吸う |
| `JawX` | 右 | 左 |
| `JawZ` | 前 | 後 |
| `MouthUpperX`, `MouthLowerX`, `MouthX` | 右 | 左 |
| `SmileFrown*` | Smile | Frown |
| `SmileSad*` | Smile | Sad |
| `TongueX` | 右 | 左 |
| `TongueY` | 上 | 下 |
| `TongueArchY` | Curl Up | Bend Down |
| `TongueShape` | Flat | Squish |

`TongueArchY` と `TongueShape` は、公式パラメーター表の説明と現行送信実装で正負が逆である。実際の互換性を優先し、U-Stella FaceTrackingは次の送信実装に合わせる。

```text
TongueArchY = TongueCurlUp - TongueBendDown
TongueShape = TongueFlat   - TongueSquish
```

公式表では `NasalConstrictRight` と `NasalConstrictLeft` の説明も左右が入れ替わっている。現行送信実装はパラメーター名と同じ側のUnified Expressionを送るため、U-Stella FaceTrackingもパラメーター名を基準にする。

## 6. VRChatネイティブ視線OSC

対応するアバター用パラメーターがない場合、VRCFTはVRChatのネイティブ視線OSCへフォールバックする。

| OSC address | 引数 | VRCFT送信値 |
| --- | --- | --- |
| `/tracking/eye/LeftRightPitchYaw` | Float 4個 | 左Pitch、左Yaw、右Pitch、右Yaw（度） |
| `/tracking/eye/EyesClosedAmount` | Float 1個 | 両目共通の閉じ量 `0..1` |

VRCFT用の視線パラメーターが見つかると `LeftRightPitchYaw` を止め、まぶたパラメーターが見つかると `EyesClosedAmount` を止める。判定は独立しており、視線とまぶたの片方だけをNativeにする構成も可能である。

VRChat NativeはExpression Parameterを経由しないため、アバターの同期コストを消費しない。U-Stella FaceTrackingが `VRChat Native` 選択時に該当パラメーターを生成しない設計は、このフォールバック条件に適合する。

VRChatは視線とまぶたについて、それぞれ10秒間データを受信しないと自動視線・自動まばたきへ戻す。

## 7. VRChat同期コスト

同期コストは、最終的に生成される一意なVRCFTパラメーターごとに数える。

| Sync Mode | 1 VRCFTパラメーター当たりの同期コスト |
| --- | --- |
| `None` | 0 bit。パラメーターを生成しない |
| `LocalOnly` | 0 bit。Floatは生成するが同期しない |
| `Float8` | 8 bit |
| `BinaryNBit`、UnsignedまたはEyeLid | N bit |
| `BinaryNBit`、Signed | N + 1 bit（`Negative`を含む） |

1つのVRCFTパラメーターが複数のUnified Expressionを駆動しても、コストは駆動先の数では増えない。複数Featureが同じVRCFTパラメーターを共有する場合も、最終Sync Modeへマージした後に1回だけ数える。

## 参考資料

- [VRCFaceTracking Releases](https://github.com/benaclejames/VRCFaceTracking/releases)
- [VRCFaceTracking Parameters](https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/parameters)
- [VRCFaceTracking Binary Parameters](https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/parameters/types/binary)
- [VRCFaceTracking source](https://github.com/benaclejames/VRCFaceTracking)
- [現行 `BinaryBaseParameter.cs`](https://github.com/benaclejames/VRCFaceTracking/blob/master/VRCFaceTracking.Core/OSC/DataTypes/BinaryBaseParameter.cs)
- [現行 `UnifiedExpressionsParameters.cs`](https://github.com/benaclejames/VRCFaceTracking/blob/master/VRCFaceTracking.Core/Params/Expressions/UnifiedExpressionsParameters.cs)
- [VRChat OSC Overview](https://docs.vrchat.com/docs/osc-overview)
- [VRChat OSC Avatar Parameters](https://docs.vrchat.com/docs/osc-avatar-parameters)
- [VRChat OSCQuery](https://docs.vrchat.com/docs/oscquery)
- [VRChat OSC Eye Tracking](https://docs.vrchat.com/docs/osc-eye-tracking)
- [VRChat Animator Parameters](https://creators.vrchat.com/avatars/animator-parameters/)
