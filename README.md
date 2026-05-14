# CascadeImageCreator

WindsurfのCascadeが間接的に画像を生成するためのスライド編集CLIツールです。  
1枚のパワーポイントスライドを編集するように、要素を追加・編集・移動しながら画像を作成できます。

## 動作環境

- .NET 8.0
- Windows / macOS

## ビルド

```bash
cd src/CascadeImageCreator
dotnet build -c Release
```

## 基本的な使い方

### 1. スライドを新規作成

```bash
CascadeImageCreator new slide.json --width 800 --height 600
```

### 2. 要素を追加

```bash
# 矩形を追加
CascadeImageCreator add slide.json rect --id box1 --x 100 --y 100 --width 200 --height 80 --fill "#3498db" --text "要件定義" --textColor "#FFFFFF" --cornerRadius 10

# box1の下に別の矩形を追加（相対配置）
CascadeImageCreator add slide.json rect --id box2 --below box1 --width 200 --height 80 --fill "#2ecc71" --text "設計" --textColor "#FFFFFF" --cornerRadius 10

# 矢印で接続
CascadeImageCreator add slide.json arrow --from box1 --to box2 --color "#333333"

# テキストを追加
CascadeImageCreator add slide.json text --id title --x 400 --y 50 --text "業務フロー" --fontSize 32 --bold --color "#333333"

# 円を追加
CascadeImageCreator add slide.json circle --id c1 --cx 500 --cy 300 --radius 40 --fill "#e74c3c" --text "開始" --textColor "#FFFFFF"
```

### 3. 要素を編集・移動

```bash
# テキスト変更
CascadeImageCreator edit slide.json box1 --text "新しいテキスト" --fill "#e74c3c"

# 座標で移動
CascadeImageCreator move slide.json box1 --x 200 --y 150

# 相対位置で移動
CascadeImageCreator move slide.json box2 --below box1 --gap 30
```

### 4. 要素を削除

```bash
CascadeImageCreator remove slide.json box2
```

### 5. 要素一覧を確認

```bash
CascadeImageCreator list slide.json
```

### 6. 画像として出力

```bash
# PNG出力
CascadeImageCreator render slide.json -o output.png

# JPG出力（品質指定）
CascadeImageCreator render slide.json -o output.jpg --quality 90
```

## 要素タイプ

| タイプ | 説明 | 主なオプション |
|---|---|---|
| `rect` | 矩形 | `--x`, `--y`, `--width`, `--height`, `--fill`, `--cornerRadius` |
| `circle` | 円 | `--cx`, `--cy`, `--radius`, `--fill` |
| `ellipse` | 楕円 | `--cx`, `--cy`, `--rx`, `--ry`, `--fill` |
| `line` | 直線 | `--x1`, `--y1`, `--x2`, `--y2` または `--from`, `--to` |
| `arrow` | 矢印 | `--from`, `--to` または `--x1`, `--y1`, `--x2`, `--y2` |
| `text` | テキスト | `--x`, `--y`, `--text`, `--fontSize`, `--align`, `--bold` |
| `polygon` | 多角形 | JSON内で `points` を指定 |
| `image` | 画像埋め込み | `--x`, `--y`, `--width`, `--height`, `--src` |

## 相対配置

要素を他の要素に対して相対的に配置できます。座標を直接指定する必要がないため、重なりを防げます。

```bash
# box1の下に配置（間隔20px）
CascadeImageCreator add slide.json rect --id box2 --below box1 --width 200 --height 80

# box1の右に配置（間隔30px）
CascadeImageCreator add slide.json rect --id box3 --right-of box1 --width 200 --height 80 --gap 30

# box1の上に配置
CascadeImageCreator add slide.json rect --id box4 --above box1 --width 200 --height 80

# box1の左に配置
CascadeImageCreator add slide.json rect --id box5 --left-of box1 --width 200 --height 80
```

## スタイルオプション

### 共通オプション

| オプション | 説明 | デフォルト |
|---|---|---|
| `--id <ID>` | 要素のID | 自動生成 |
| `--fill <色>` | 塗りつぶし色 | なし |
| `--stroke <色>` | 枠線色 | なし |
| `--strokeWidth <幅>` | 枠線の太さ | 2 |
| `--text <テキスト>` | 要素内テキスト | なし |
| `--textColor <色>` | テキスト色 | #000000 |
| `--fontSize <サイズ>` | フォントサイズ | 16 |
| `--bold` | 太字 | false |
| `--opacity <0-1>` | 不透明度 | 1.0 |
| `--rotation <角度>` | 回転（度） | 0 |
| `--lineStyle <スタイル>` | 線のスタイル（solid/dashed/dotted） | solid |

## 出力形式

| 拡張子 | 形式 |
|---|---|
| `.png` | PNG（デフォルト、透過対応） |
| `.jpg` / `.jpeg` | JPEG（`--quality` で品質指定） |
| `.webp` | WebP |
| `.bmp` | BMP |

## 依存ライブラリ

- [SkiaSharp](https://github.com/mono/SkiaSharp) (MIT License) - クロスプラットフォーム2Dグラフィックスライブラリ
- SkiaSharp が利用する Skia などの native 依存は、上流リポジトリのライセンス表記に従います。

## 公開配布時の注意

このリポジトリを OSS として再配布する場合は、少なくとも以下を同梱してください。

- このリポジトリ自体の `LICENSE`
- `THIRD_PARTY_NOTICES.md`
- 依存ライブラリのライセンス表記や著作権表記
