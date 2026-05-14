# CascadeImageCreator - Cascade操作マニュアル

## 概要

CascadeImageCreatorは、Cascadeが画像を間接的に生成するためのCLIツールです。  
1枚のパワーポイントスライドを編集するように、コマンドで要素を追加・編集・移動し、最終的にPNG/JPG画像として出力します。

## 前提条件

- .NET 8.0 SDK がインストールされていること
- プロジェクトのパス: `<リポジトリルート>/selfmade/CascadeImageCreator/src/CascadeImageCreator`

## 基本的なワークフロー

ユーザーから「図を作って」「画像を作って」と依頼された場合、以下の手順で画像を作成してください。

### ステップ1: スライドを作成

```bash
dotnet run --project <プロジェクトパス> -- new slide.json --width 800 --height 600
```

サイズはユーザーの用途に合わせて調整してください。

### ステップ2: 要素を追加

重なりを防ぐため、**相対配置（--below, --right-of 等）を積極的に使用**してください。

```bash
# 最初の要素は絶対座標で配置
dotnet run --project <プロジェクトパス> -- add slide.json rect --id box1 --x 300 --y 50 --width 200 --height 80 --fill "#3498db" --text "開始" --textColor "#FFFFFF" --cornerRadius 10

# 2つ目以降は相対配置を使用
dotnet run --project <プロジェクトパス> -- add slide.json rect --id box2 --below box1 --width 200 --height 80 --fill "#2ecc71" --text "処理" --textColor "#FFFFFF" --cornerRadius 10

# 要素同士を矢印で接続
dotnet run --project <プロジェクトパス> -- add slide.json arrow --from box1 --to box2 --color "#333333"
```

### ステップ3: プレビュー確認

```bash
dotnet run --project <プロジェクトパス> -- render slide.json -o preview.png
```

生成された画像を確認し、必要に応じて修正してください。

### ステップ4: 修正（必要に応じて）

```bash
# 要素を移動
dotnet run --project <プロジェクトパス> -- move slide.json box1 --x 250 --y 80

# 要素を編集
dotnet run --project <プロジェクトパス> -- edit slide.json box1 --fill "#e74c3c" --text "修正後"

# 要素を削除
dotnet run --project <プロジェクトパス> -- remove slide.json box2
```

### ステップ5: 最終出力

```bash
dotnet run --project <プロジェクトパス> -- render slide.json -o final.png
```

## コマンド一覧

### 1. new - スライド作成

```bash
dotnet run --project <プロジェクトパス> -- new <slide.json> [--width 1920] [--height 1080] [--background "#FFFFFF"]
```

### 2. add - 要素追加

```bash
# 矩形
dotnet run --project <プロジェクトパス> -- add <slide.json> rect --id <ID> --x <X> --y <Y> --width <幅> --height <高さ> [--fill <色>] [--stroke <色>] [--text <テキスト>] [--textColor <色>] [--cornerRadius <半径>]

# 円
dotnet run --project <プロジェクトパス> -- add <slide.json> circle --id <ID> --cx <中心X> --cy <中心Y> --radius <半径> [--fill <色>] [--text <テキスト>]

# 楕円
dotnet run --project <プロジェクトパス> -- add <slide.json> ellipse --id <ID> --cx <中心X> --cy <中心Y> --rx <水平半径> --ry <垂直半径> [--fill <色>]

# 直線
dotnet run --project <プロジェクトパス> -- add <slide.json> line [--x1 <X1> --y1 <Y1> --x2 <X2> --y2 <Y2>] または [--from <要素ID> --to <要素ID>] [--color <色>] [--lineStyle solid|dashed|dotted]

# 矢印
dotnet run --project <プロジェクトパス> -- add <slide.json> arrow [--from <要素ID> --to <要素ID>] [--color <色>] [--text <ラベル>]

# テキスト
dotnet run --project <プロジェクトパス> -- add <slide.json> text --id <ID> --x <X> --y <Y> --text <テキスト> [--fontSize <サイズ>] [--color <色>] [--bold] [--align left|center|right]

# 画像埋め込み
dotnet run --project <プロジェクトパス> -- add <slide.json> image --id <ID> --x <X> --y <Y> --src <画像パス> [--width <幅>] [--height <高さ>]
```

### 3. edit - 要素編集

```bash
dotnet run --project <プロジェクトパス> -- edit <slide.json> <要素ID> [--fill <色>] [--text <テキスト>] [--fontSize <サイズ>] ...
```

### 4. move - 要素移動

```bash
# 絶対座標で移動
dotnet run --project <プロジェクトパス> -- move <slide.json> <要素ID> --x <X> --y <Y>

# 相対配置で移動
dotnet run --project <プロジェクトパス> -- move <slide.json> <要素ID> --below <他の要素ID> [--gap <間隔>]
```

### 5. remove - 要素削除

```bash
dotnet run --project <プロジェクトパス> -- remove <slide.json> <要素ID>
```

### 6. list - 要素一覧表示

```bash
dotnet run --project <プロジェクトパス> -- list <slide.json>
```

### 7. render - 画像出力

```bash
dotnet run --project <プロジェクトパス> -- render <slide.json> -o <output.png> [--quality <1-100>]
```

## Cascadeとしての推奨ワークフロー

### シンプルなフローチャートの場合

1. `new` でスライドを作成
2. 最初のノードを絶対座標で配置
3. 後続のノードは `--below` や `--right-of` で相対配置
4. `arrow --from --to` で接続
5. `render` で出力

### 複雑な図（パワポ風）の場合

1. `new` でスライドを作成
2. 背景となる大きな矩形（セクション）を配置
3. セクション内にノードを絶対座標で配置
4. 矢印やラインで接続
5. タイトルやラベルを `text` で追加
6. `render` でプレビュー → 修正 → 再レンダリング

### 相対配置の使い分け

| オプション | 配置位置 | 用途 |
|---|---|---|
| `--below <ID>` | 指定要素の真下 | フローチャートの縦方向 |
| `--above <ID>` | 指定要素の真上 | 逆方向フロー |
| `--right-of <ID>` | 指定要素の右隣 | 横方向フロー、並列処理 |
| `--left-of <ID>` | 指定要素の左隣 | 横方向フロー |

`--gap <間隔>` で要素間の距離を調整できます（デフォルト: 20px）。

## エラーメッセージ

| メッセージ | 原因 | 対処 |
|---|---|---|
| `エラー: ファイルが見つかりません: ...` | スライドファイルのパスが間違っている | パスを確認 |
| `エラー: JSONの解析に失敗しました: ...` | スライドファイルが破損している | `new` で再作成 |
| `エラー: 要素が見つかりません: ...` | 指定したIDの要素が存在しない | `list` で確認 |
| `エラー: ID '...' は既に使用されています。` | 重複するIDを指定 | 別のIDを使用 |
| `エラー: レンダリングエラー: ...` | 画像生成時にエラー | スライドの内容を確認 |

## 色の指定方法

HEXカラーコードで指定します：

| 色 | コード |
|---|---|
| 赤 | `#FF0000` |
| 青 | `#0000FF` |
| 緑 | `#00FF00` |
| 白 | `#FFFFFF` |
| 黒 | `#000000` |
| 灰色 | `#808080` |

よく使う色のコード例：

| 用途 | コード |
|---|---|
| 青系ヘッダー | `#3498db` |
| 緑系（成功） | `#2ecc71` |
| 赤系（エラー） | `#e74c3c` |
| オレンジ（警告） | `#f39c12` |
| 濃い灰色（テキスト） | `#333333` |
| 薄い青背景 | `#EBF5FB` |
| 薄い緑背景 | `#E8F8F5` |
| 薄い赤背景 | `#FDEDEC` |

## バージョン情報

```bash
dotnet run --project <プロジェクトパス> -- --version
```

現在のバージョン: **1.0.0**
