# U-Stella FaceTracking docs

このディレクトリは Docusaurus で構築する静的ドキュメントサイトです。

## Development

Node.js 18以上とnpm 11.10以上を使用してください。
このプロジェクトのnpm installは `.npmrc` により Takumi Guard 経由で行われ、公開から3日以内のパッケージは解決対象から除外されます。

```bash
npm install
npm run start
```

## Build

```bash
npm run build
```

`build/` に静的ファイル一式が生成されます。

公開URLが決まっている環境では、ビルド時に以下を指定してください。

```bash
DOCS_URL=https://example.com DOCS_BASE_URL=/ npm run build
```

サブディレクトリへ配置する場合は、`DOCS_BASE_URL=/path/` のように前後のスラッシュを含めます。
