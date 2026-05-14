using System;
using System.Linq;
using CascadeImageCreator.Commands;

namespace CascadeImageCreator;

public static class Program
{
    private const string Version = "1.0.0";

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0].ToLowerInvariant();

        return command switch
        {
            "new" => CommandHandler.HandleNew(args.Skip(1).ToArray()),
            "add" => CommandHandler.HandleAdd(args.Skip(1).ToArray()),
            "edit" => CommandHandler.HandleEdit(args.Skip(1).ToArray()),
            "move" => CommandHandler.HandleMove(args.Skip(1).ToArray()),
            "remove" => CommandHandler.HandleRemove(args.Skip(1).ToArray()),
            "list" => CommandHandler.HandleList(args.Skip(1).ToArray()),
            "render" => CommandHandler.HandleRender(args.Skip(1).ToArray()),
            "--help" or "-h" => PrintUsage(),
            "--version" or "-v" => PrintVersion(),
            _ => PrintUnknownCommand(command)
        };
    }

    private static int PrintUsage()
    {
        Console.WriteLine($"CascadeImageCreator v{Version}");
        Console.WriteLine("WindsurfのCascadeが画像を生成するためのスライド編集CLIツール");
        Console.WriteLine();
        Console.WriteLine("コマンド:");
        Console.WriteLine("  new    <slide.json> [オプション]           新しいスライドを作成");
        Console.WriteLine("  add    <slide.json> <要素タイプ> [オプション]  要素を追加");
        Console.WriteLine("  edit   <slide.json> <要素ID> [オプション]     要素を編集");
        Console.WriteLine("  move   <slide.json> <要素ID> [オプション]     要素を移動");
        Console.WriteLine("  remove <slide.json> <要素ID>               要素を削除");
        Console.WriteLine("  list   <slide.json>                       要素一覧を表示");
        Console.WriteLine("  render <slide.json> [-o output.png]       画像として出力");
        Console.WriteLine();
        Console.WriteLine("要素タイプ:");
        Console.WriteLine("  rect     矩形（--cornerRadius で角丸）");
        Console.WriteLine("  circle   円（--cx, --cy, --radius）");
        Console.WriteLine("  ellipse  楕円（--cx, --cy, --rx, --ry）");
        Console.WriteLine("  line     直線（--x1, --y1, --x2, --y2 または --from, --to）");
        Console.WriteLine("  arrow    矢印（直線と同じオプション）");
        Console.WriteLine("  text     テキスト（--text, --fontSize, --align）");
        Console.WriteLine("  polygon  多角形（JSON内でpoints指定）");
        Console.WriteLine("  image    画像埋め込み（--src）");
        Console.WriteLine();
        Console.WriteLine("newコマンドのオプション:");
        Console.WriteLine("  --width <幅>         スライドの幅（デフォルト: 1920）");
        Console.WriteLine("  --height <高さ>      スライドの高さ（デフォルト: 1080）");
        Console.WriteLine("  --background <色>    背景色（デフォルト: #FFFFFF）");
        Console.WriteLine();
        Console.WriteLine("共通オプション:");
        Console.WriteLine("  --id <ID>            要素のID");
        Console.WriteLine("  --fill <色>          塗りつぶし色");
        Console.WriteLine("  --stroke <色>        枠線色");
        Console.WriteLine("  --strokeWidth <幅>   枠線の太さ");
        Console.WriteLine("  --text <テキスト>    要素内のテキスト");
        Console.WriteLine("  --textColor <色>     テキスト色");
        Console.WriteLine("  --fontSize <サイズ>  フォントサイズ");
        Console.WriteLine("  --bold               太字");
        Console.WriteLine("  --opacity <0-1>      不透明度");
        Console.WriteLine("  --rotation <角度>    回転（度）");
        Console.WriteLine();
        Console.WriteLine("相対配置オプション:");
        Console.WriteLine("  --below <ID>         指定要素の下に配置");
        Console.WriteLine("  --above <ID>         指定要素の上に配置");
        Console.WriteLine("  --right-of <ID>      指定要素の右に配置");
        Console.WriteLine("  --left-of <ID>       指定要素の左に配置");
        Console.WriteLine("  --gap <間隔>         相対配置時の間隔（デフォルト: 20）");
        Console.WriteLine();
        Console.WriteLine("renderコマンドのオプション:");
        Console.WriteLine("  -o, --output <パス>  出力ファイルパス（.png, .jpg, .webp）");
        Console.WriteLine("  -q, --quality <1-100> JPEG品質（デフォルト: 90）");
        Console.WriteLine();
        Console.WriteLine("オプション:");
        Console.WriteLine("  -h, --help           ヘルプを表示");
        Console.WriteLine("  -v, --version        バージョンを表示");
        Console.WriteLine();
        Console.WriteLine("使用例:");
        Console.WriteLine("  CascadeImageCreator new slide.json --width 800 --height 600");
        Console.WriteLine("  CascadeImageCreator add slide.json rect --id box1 --x 100 --y 100 --width 200 --height 80 --fill \"#3498db\" --text \"処理\" --textColor \"#FFFFFF\"");
        Console.WriteLine("  CascadeImageCreator add slide.json rect --id box2 --below box1 --width 200 --height 80 --fill \"#2ecc71\" --text \"完了\" --textColor \"#FFFFFF\"");
        Console.WriteLine("  CascadeImageCreator add slide.json arrow --from box1 --to box2 --color \"#333333\"");
        Console.WriteLine("  CascadeImageCreator render slide.json -o output.png");
        return 0;
    }

    private static int PrintVersion()
    {
        Console.WriteLine($"CascadeImageCreator v{Version}");
        return 0;
    }

    private static int PrintUnknownCommand(string command)
    {
        Console.Error.WriteLine($"エラー: 不明なコマンド '{command}'");
        Console.Error.WriteLine("CascadeImageCreator --help でヘルプを表示できます。");
        return 1;
    }
}
