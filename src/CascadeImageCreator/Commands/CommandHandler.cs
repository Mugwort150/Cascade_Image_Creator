using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CascadeImageCreator.Models;
using CascadeImageCreator.Rendering;

namespace CascadeImageCreator.Commands;

public static class CommandHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static int HandleNew(string[] args)
    {
        if (args.Length == 0)
        {
            WriteError("スライドファイルのパスを指定してください。");
            return 1;
        }

        var filePath = args[0];
        int width = 1920;
        int height = 1080;
        string background = "#FFFFFF";

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--width" when i + 1 < args.Length:
                    if (int.TryParse(args[++i], out var w)) width = w;
                    break;
                case "--height" when i + 1 < args.Length:
                    if (int.TryParse(args[++i], out var h)) height = h;
                    break;
                case "--background" or "--bg" when i + 1 < args.Length:
                    background = args[++i];
                    break;
            }
        }

        var slide = new SlideData
        {
            Width = width,
            Height = height,
            Background = background
        };

        SaveSlide(filePath, slide);
        Console.WriteLine($"スライドを作成しました: {filePath}");
        Console.WriteLine($"  サイズ: {width}x{height}");
        Console.WriteLine($"  背景色: {background}");
        return 0;
    }

    public static int HandleAdd(string[] args)
    {
        if (args.Length < 2)
        {
            WriteError("使い方: CascadeImageCreator add <slide.json> <要素タイプ> [オプション]");
            return 1;
        }

        var filePath = args[0];
        var slide = LoadSlide(filePath);
        if (slide == null) return 1;

        var elementType = args[1].ToLowerInvariant();
        var element = new ElementData { Type = elementType };

        var remaining = args.Skip(2).ToArray();

        if (!ParseElementOptions(element, remaining, elementType))
            return 1;

        if (string.IsNullOrEmpty(element.Id))
        {
            element.Id = $"{elementType}_{slide.Elements.Count + 1}";
        }

        if (slide.Elements.Any(e => e.Id == element.Id))
        {
            WriteError($"ID '{element.Id}' は既に使用されています。別のIDを指定してください。");
            return 1;
        }

        slide.Elements.Add(element);
        SaveSlide(filePath, slide);

        Console.WriteLine($"要素を追加しました: {element.Id} ({elementType})");
        return 0;
    }

    public static int HandleEdit(string[] args)
    {
        if (args.Length < 2)
        {
            WriteError("使い方: CascadeImageCreator edit <slide.json> <要素ID> [オプション]");
            return 1;
        }

        var filePath = args[0];
        var slide = LoadSlide(filePath);
        if (slide == null) return 1;

        var elementId = args[1];
        var element = slide.Elements.Find(e => e.Id == elementId);
        if (element == null)
        {
            WriteError($"要素が見つかりません: {elementId}");
            return 1;
        }

        var remaining = args.Skip(2).ToArray();
        ApplyEditOptions(element, remaining);

        SaveSlide(filePath, slide);
        Console.WriteLine($"要素を編集しました: {elementId}");
        return 0;
    }

    public static int HandleMove(string[] args)
    {
        if (args.Length < 2)
        {
            WriteError("使い方: CascadeImageCreator move <slide.json> <要素ID> --x <X> --y <Y>");
            return 1;
        }

        var filePath = args[0];
        var slide = LoadSlide(filePath);
        if (slide == null) return 1;

        var elementId = args[1];
        var element = slide.Elements.Find(e => e.Id == elementId);
        if (element == null)
        {
            WriteError($"要素が見つかりません: {elementId}");
            return 1;
        }

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--x" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var x)) element.X = x;
                    break;
                case "--y" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var y)) element.Y = y;
                    break;
                case "--cx" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var cx)) element.Cx = cx;
                    break;
                case "--cy" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var cy)) element.Cy = cy;
                    break;
                case "--below" when i + 1 < args.Length:
                    element.RelativeToId = args[++i];
                    element.RelativePosition = "below";
                    break;
                case "--above" when i + 1 < args.Length:
                    element.RelativeToId = args[++i];
                    element.RelativePosition = "above";
                    break;
                case "--right-of" when i + 1 < args.Length:
                    element.RelativeToId = args[++i];
                    element.RelativePosition = "right";
                    break;
                case "--left-of" when i + 1 < args.Length:
                    element.RelativeToId = args[++i];
                    element.RelativePosition = "left";
                    break;
            }
        }

        SaveSlide(filePath, slide);
        Console.WriteLine($"要素を移動しました: {elementId}");
        return 0;
    }

    public static int HandleRemove(string[] args)
    {
        if (args.Length < 2)
        {
            WriteError("使い方: CascadeImageCreator remove <slide.json> <要素ID>");
            return 1;
        }

        var filePath = args[0];
        var slide = LoadSlide(filePath);
        if (slide == null) return 1;

        var elementId = args[1];
        var removed = slide.Elements.RemoveAll(e => e.Id == elementId);
        if (removed == 0)
        {
            WriteError($"要素が見つかりません: {elementId}");
            return 1;
        }

        SaveSlide(filePath, slide);
        Console.WriteLine($"要素を削除しました: {elementId}");
        return 0;
    }

    public static int HandleList(string[] args)
    {
        if (args.Length == 0)
        {
            WriteError("スライドファイルのパスを指定してください。");
            return 1;
        }

        var filePath = args[0];
        var slide = LoadSlide(filePath);
        if (slide == null) return 1;

        Console.WriteLine($"スライド: {filePath}");
        Console.WriteLine($"サイズ: {slide.Width}x{slide.Height}");
        Console.WriteLine($"背景色: {slide.Background}");
        Console.WriteLine($"要素数: {slide.Elements.Count}");

        if (slide.Elements.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("要素一覧:");
            foreach (var element in slide.Elements)
            {
                string posInfo = element.Type.ToLowerInvariant() switch
                {
                    "circle" => $"center=({element.Cx},{element.Cy}) r={element.Radius}",
                    "ellipse" => $"center=({element.Cx},{element.Cy}) rx={element.Rx} ry={element.Ry}",
                    "line" or "arrow" => element.FromId != null
                        ? $"from={element.FromId} to={element.ToId}"
                        : $"({element.X1},{element.Y1})->({element.X2},{element.Y2})",
                    _ => element.RelativeToId != null
                        ? $"{element.RelativePosition} {element.RelativeToId} (gap={element.Gap})"
                        : $"pos=({element.X},{element.Y}) size={element.Width}x{element.Height}"
                };

                string textInfo = !string.IsNullOrEmpty(element.Text) ? $" text=\"{element.Text}\"" : "";
                Console.WriteLine($"  [{element.Id}] {element.Type} {posInfo}{textInfo}");
            }
        }

        return 0;
    }

    public static int HandleRender(string[] args)
    {
        if (args.Length == 0)
        {
            WriteError("スライドファイルのパスを指定してください。");
            return 1;
        }

        var filePath = args[0];
        var slide = LoadSlide(filePath);
        if (slide == null) return 1;

        string? outputPath = null;
        int quality = 90;

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--output" or "-o" when i + 1 < args.Length:
                    outputPath = args[++i];
                    break;
                case "--quality" or "-q" when i + 1 < args.Length:
                    if (int.TryParse(args[++i], out var q))
                        quality = Math.Clamp(q, 1, 100);
                    break;
            }
        }

        if (outputPath == null)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".";
            var name = Path.GetFileNameWithoutExtension(filePath);
            outputPath = Path.Combine(dir, name + ".png");
        }

        var outputDir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (outputDir != null)
        {
            Directory.CreateDirectory(outputDir);
        }

        try
        {
            var renderer = new SlideRenderer();
            renderer.Render(slide, outputPath, quality);

            var fileInfo = new FileInfo(outputPath);
            Console.WriteLine($"画像を出力しました: {outputPath}");
            Console.WriteLine($"  サイズ: {slide.Width}x{slide.Height}");
            Console.WriteLine($"  ファイルサイズ: {FormatFileSize(fileInfo.Length)}");
            Console.WriteLine($"  要素数: {slide.Elements.Count}");
            return 0;
        }
        catch (Exception ex)
        {
            WriteError($"レンダリングエラー: {ex.Message}");
            return 1;
        }
    }

    private static bool ParseElementOptions(ElementData element, string[] args, string elementType)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--id" when i + 1 < args.Length:
                    element.Id = args[++i];
                    break;
                case "--x" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var x)) element.X = x;
                    break;
                case "--y" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var y)) element.Y = y;
                    break;
                case "--width" or "-w" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var w)) element.Width = w;
                    break;
                case "--height" or "-h" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var h)) element.Height = h;
                    break;
                case "--fill" when i + 1 < args.Length:
                    element.Fill = args[++i];
                    break;
                case "--stroke" when i + 1 < args.Length:
                    element.Stroke = args[++i];
                    break;
                case "--strokeWidth" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var sw)) element.StrokeWidth = sw;
                    break;
                case "--cornerRadius" or "--cr" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var cr)) element.CornerRadius = cr;
                    break;
                case "--text" when i + 1 < args.Length:
                    element.Text = args[++i];
                    break;
                case "--textColor" when i + 1 < args.Length:
                    element.TextColor = args[++i];
                    break;
                case "--fontSize" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var fs)) element.FontSize = fs;
                    break;
                case "--bold":
                    element.Bold = true;
                    break;
                case "--align" when i + 1 < args.Length:
                    element.Align = args[++i];
                    break;
                case "--color" when i + 1 < args.Length:
                    element.Color = args[++i];
                    break;
                case "--cx" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var cx)) element.Cx = cx;
                    break;
                case "--cy" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var cy)) element.Cy = cy;
                    break;
                case "--radius" or "-r" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var r)) element.Radius = r;
                    break;
                case "--rx" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var rx)) element.Rx = rx;
                    break;
                case "--ry" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var ry)) element.Ry = ry;
                    break;
                case "--x1" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var x1)) element.X1 = x1;
                    break;
                case "--y1" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var y1)) element.Y1 = y1;
                    break;
                case "--x2" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var x2)) element.X2 = x2;
                    break;
                case "--y2" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var y2)) element.Y2 = y2;
                    break;
                case "--from" when i + 1 < args.Length:
                    element.FromId = args[++i];
                    break;
                case "--to" when i + 1 < args.Length:
                    element.ToId = args[++i];
                    break;
                case "--lineStyle" when i + 1 < args.Length:
                    element.LineStyle = args[++i];
                    break;
                case "--arrowSize" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var aSize)) element.ArrowSize = aSize;
                    break;
                case "--src" when i + 1 < args.Length:
                    element.Src = args[++i];
                    break;
                case "--opacity" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var op)) element.Opacity = Math.Clamp(op, 0f, 1f);
                    break;
                case "--rotation" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var rot)) element.Rotation = rot;
                    break;
                case "--below" when i + 1 < args.Length:
                    element.RelativeToId = args[++i];
                    element.RelativePosition = "below";
                    break;
                case "--above" when i + 1 < args.Length:
                    element.RelativeToId = args[++i];
                    element.RelativePosition = "above";
                    break;
                case "--right-of" when i + 1 < args.Length:
                    element.RelativeToId = args[++i];
                    element.RelativePosition = "right";
                    break;
                case "--left-of" when i + 1 < args.Length:
                    element.RelativeToId = args[++i];
                    element.RelativePosition = "left";
                    break;
                case "--gap" when i + 1 < args.Length:
                    if (float.TryParse(args[++i], out var gap)) element.Gap = gap;
                    break;
            }
        }

        return true;
    }

    private static void ApplyEditOptions(ElementData element, string[] args)
    {
        ParseElementOptions(element, args, element.Type);
    }

    private static SlideData? LoadSlide(string filePath)
    {
        if (!File.Exists(filePath))
        {
            WriteError($"ファイルが見つかりません: {filePath}");
            return null;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var slide = JsonSerializer.Deserialize<SlideData>(json, JsonOptions);
            if (slide == null)
            {
                WriteError("スライドデータの読み込みに失敗しました。");
                return null;
            }
            return slide;
        }
        catch (JsonException ex)
        {
            WriteError($"JSONの解析に失敗しました: {ex.Message}");
            return null;
        }
    }

    private static void SaveSlide(string filePath, SlideData slide)
    {
        var json = JsonSerializer.Serialize(slide, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private static void WriteError(string message)
    {
        Console.Error.WriteLine($"エラー: {message}");
    }
}
