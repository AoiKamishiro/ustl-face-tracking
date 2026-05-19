# U-Stella FaceTracking

[English](README.md) | 日本語

U-Stella FaceTracking は、VRChatアバター向けのフェイシャルトラッキング設定を補助するUnityエディター拡張です。
使用するトラッキング機器、使いたい表情機能、顔メッシュのブレンドシェイプ割り当てをInspector上で設定できます。

## インストール

VCCまたはALCOMに、U-StellaのVPMリポジトリを追加してください。

```text
https://ustl-vpm.kamishiro.online/index.json
```

VCCまたはALCOMがブラウザーから起動できる環境では、以下のURLからリポジトリを追加できます。

```text
vcc://vpm/addRepo?url=https%3A%2F%2Fustl-vpm.kamishiro.online%2Findex.json
```

リポジトリ追加後、対象のUnityプロジェクトで `U-Stella FaceTracking` パッケージを追加します。

## 必要環境

- Unity 2022.3
- VRChatアバター用のUnityプロジェクト
- Modular Avatar
- フェイシャルトラッキング用のブレンドシェイプを持つ顔メッシュ

依存パッケージはVPM経由で解決されます。

## 基本的な使い方

1. Unityで対象アバターのPrefabまたはシーン上のアバターを開きます。
2. アバター直下、または設定を管理しやすい子オブジェクトを選択します。
3. Inspectorから `U-Stella/U-Stella FaceTracking` コンポーネントを追加します。
4. `Face Mesh Renderer` にフェイシャルトラッキング用ブレンドシェイプを持つ `SkinnedMeshRenderer` を指定します。
5. 使用機材、機能設定、ブレンドシェイプ割り当てを調整します。

## ドキュメント

- [ドキュメントトップ](https://ustl-face-tracking.kamishiro.online/ja/)
- [利用方法](https://ustl-face-tracking.kamishiro.online/ja/usage/)
- [機器情報の追加方法](https://ustl-face-tracking.kamishiro.online/ja/add-hardware-profile/)

## ライセンス

[Apache License 2.0](LICENSE)
