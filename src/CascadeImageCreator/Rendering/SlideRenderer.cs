using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CascadeImageCreator.Models;
using SkiaSharp;

namespace CascadeImageCreator.Rendering;

public sealed class SlideRenderer
{
    public void Render(SlideData slide, string outputPath, int quality = 90)
    {
        using var surface = SKSurface.Create(new SKImageInfo(slide.Width, slide.Height));
        var canvas = surface.Canvas;

        canvas.Clear(ParseColor(slide.Background));

        foreach (var element in slide.Elements)
        {
            DrawElement(canvas, element, slide);
        }

        using var image = surface.Snapshot();
        var ext = Path.GetExtension(outputPath).ToLowerInvariant();

        SKEncodedImageFormat format = ext switch
        {
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".webp" => SKEncodedImageFormat.Webp,
            ".bmp" => SKEncodedImageFormat.Bmp,
            _ => SKEncodedImageFormat.Png
        };

        using var data = image.Encode(format, quality);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
    }

    private void DrawElement(SKCanvas canvas, ElementData element, SlideData slide)
    {
        canvas.Save();

        if (element.Rotation != 0)
        {
            var bounds = GetElementBounds(element, slide);
            float centerX = bounds.cx;
            float centerY = bounds.cy;
            canvas.RotateDegrees(element.Rotation, centerX, centerY);
        }

        if (element.Opacity < 1f)
        {
            canvas.SaveLayer(new SKPaint { Color = SKColors.White.WithAlpha((byte)(element.Opacity * 255)) });
        }

        switch (element.Type.ToLowerInvariant())
        {
            case "rect":
                DrawRect(canvas, element, slide);
                break;
            case "circle":
                DrawCircle(canvas, element, slide);
                break;
            case "ellipse":
                DrawEllipse(canvas, element, slide);
                break;
            case "line":
                DrawLine(canvas, element, slide);
                break;
            case "arrow":
                DrawArrow(canvas, element, slide);
                break;
            case "text":
                DrawText(canvas, element, slide);
                break;
            case "polygon":
                DrawPolygon(canvas, element);
                break;
            case "image":
                DrawImage(canvas, element);
                break;
        }

        if (element.Opacity < 1f)
        {
            canvas.Restore();
        }

        canvas.Restore();
    }

    private void DrawRect(SKCanvas canvas, ElementData element, SlideData slide)
    {
        var (x, y) = ResolvePosition(element, slide);
        var rect = SKRect.Create(x, y, element.Width, element.Height);

        if (element.Fill != null)
        {
            using var paint = new SKPaint
            {
                Color = ParseColor(element.Fill),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            if (element.CornerRadius > 0)
                canvas.DrawRoundRect(rect, element.CornerRadius, element.CornerRadius, paint);
            else
                canvas.DrawRect(rect, paint);
        }

        if (element.Stroke != null)
        {
            using var paint = CreateStrokePaint(element.Stroke, element.StrokeWidth, element.LineStyle);

            if (element.CornerRadius > 0)
                canvas.DrawRoundRect(rect, element.CornerRadius, element.CornerRadius, paint);
            else
                canvas.DrawRect(rect, paint);
        }

        if (!string.IsNullOrEmpty(element.Text))
        {
            DrawTextInRect(canvas, element.Text, rect, element);
        }
    }

    private void DrawCircle(SKCanvas canvas, ElementData element, SlideData slide)
    {
        float cx, cy;
        if (element.RelativeToId != null)
        {
            var (rx, ry) = ResolvePosition(element, slide);
            cx = rx + element.Radius;
            cy = ry + element.Radius;
        }
        else
        {
            cx = element.Cx;
            cy = element.Cy;
        }

        if (element.Fill != null)
        {
            using var paint = new SKPaint
            {
                Color = ParseColor(element.Fill),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawCircle(cx, cy, element.Radius, paint);
        }

        var strokeColor = element.Stroke ?? element.Color;
        if (strokeColor != null)
        {
            using var paint = CreateStrokePaint(strokeColor, element.StrokeWidth, element.LineStyle);
            canvas.DrawCircle(cx, cy, element.Radius, paint);
        }

        if (!string.IsNullOrEmpty(element.Text))
        {
            var rect = new SKRect(cx - element.Radius, cy - element.Radius,
                                  cx + element.Radius, cy + element.Radius);
            DrawTextInRect(canvas, element.Text, rect, element);
        }
    }

    private void DrawEllipse(SKCanvas canvas, ElementData element, SlideData slide)
    {
        float cx, cy;
        if (element.RelativeToId != null)
        {
            var (rx, ry) = ResolvePosition(element, slide);
            cx = rx + element.Rx;
            cy = ry + element.Ry;
        }
        else
        {
            cx = element.Cx;
            cy = element.Cy;
        }

        var rect = new SKRect(cx - element.Rx, cy - element.Ry, cx + element.Rx, cy + element.Ry);

        if (element.Fill != null)
        {
            using var paint = new SKPaint
            {
                Color = ParseColor(element.Fill),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawOval(rect, paint);
        }

        var strokeColor = element.Stroke ?? element.Color;
        if (strokeColor != null)
        {
            using var paint = CreateStrokePaint(strokeColor, element.StrokeWidth, element.LineStyle);
            canvas.DrawOval(rect, paint);
        }

        if (!string.IsNullOrEmpty(element.Text))
        {
            DrawTextInRect(canvas, element.Text, rect, element);
        }
    }

    private void DrawLine(SKCanvas canvas, ElementData element, SlideData slide)
    {
        float x1, y1, x2, y2;
        ResolveLineEndpoints(element, slide, out x1, out y1, out x2, out y2);

        var lineColor = element.Color ?? element.Stroke ?? "#000000";
        using var paint = CreateStrokePaint(lineColor, element.StrokeWidth, element.LineStyle);
        canvas.DrawLine(x1, y1, x2, y2, paint);

        if (!string.IsNullOrEmpty(element.Text))
        {
            float midX = (x1 + x2) / 2;
            float midY = (y1 + y2) / 2;
            DrawLabelAtPoint(canvas, element.Text, midX, midY - 10, element);
        }
    }

    private void DrawArrow(SKCanvas canvas, ElementData element, SlideData slide)
    {
        float x1, y1, x2, y2;
        ResolveLineEndpoints(element, slide, out x1, out y1, out x2, out y2);

        var lineColor = element.Color ?? element.Stroke ?? "#000000";
        using var paint = CreateStrokePaint(lineColor, element.StrokeWidth, element.LineStyle);
        canvas.DrawLine(x1, y1, x2, y2, paint);

        DrawArrowHead(canvas, x1, y1, x2, y2, element.ArrowSize, ParseColor(lineColor));

        if (!string.IsNullOrEmpty(element.Text))
        {
            float midX = (x1 + x2) / 2;
            float midY = (y1 + y2) / 2;
            DrawLabelAtPoint(canvas, element.Text, midX, midY - 10, element);
        }
    }

    private void DrawArrowHead(SKCanvas canvas, float x1, float y1, float x2, float y2,
                                float arrowSize, SKColor color)
    {
        float angle = MathF.Atan2(y2 - y1, x2 - x1);
        float arrowAngle = MathF.PI / 6;

        float ax1 = x2 - arrowSize * MathF.Cos(angle - arrowAngle);
        float ay1 = y2 - arrowSize * MathF.Sin(angle - arrowAngle);
        float ax2 = x2 - arrowSize * MathF.Cos(angle + arrowAngle);
        float ay2 = y2 - arrowSize * MathF.Sin(angle + arrowAngle);

        using var path = new SKPath();
        path.MoveTo(x2, y2);
        path.LineTo(ax1, ay1);
        path.LineTo(ax2, ay2);
        path.Close();

        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawPath(path, paint);
    }

    private void DrawText(SKCanvas canvas, ElementData element, SlideData slide)
    {
        var (x, y) = ResolvePosition(element, slide);
        var textColor = element.TextColor != "#000000" ? element.TextColor : (element.Color ?? "#000000");

        using var paint = new SKPaint
        {
            Color = ParseColor(textColor),
            IsAntialias = true,
            TextSize = element.FontSize,
            Typeface = element.Bold ? SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) : SKTypeface.Default
        };

        var align = element.Align.ToLowerInvariant();
        switch (align)
        {
            case "center":
                paint.TextAlign = SKTextAlign.Center;
                break;
            case "right":
                paint.TextAlign = SKTextAlign.Right;
                break;
            default:
                paint.TextAlign = SKTextAlign.Left;
                break;
        }

        canvas.DrawText(element.Text ?? string.Empty, x, y + element.FontSize, paint);
    }

    private void DrawPolygon(SKCanvas canvas, ElementData element)
    {
        if (element.Points == null || element.Points.Count < 3) return;

        using var path = new SKPath();
        path.MoveTo(element.Points[0][0], element.Points[0][1]);
        for (int i = 1; i < element.Points.Count; i++)
        {
            path.LineTo(element.Points[i][0], element.Points[i][1]);
        }
        path.Close();

        if (element.Fill != null)
        {
            using var paint = new SKPaint
            {
                Color = ParseColor(element.Fill),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawPath(path, paint);
        }

        var strokeColor = element.Stroke ?? element.Color;
        if (strokeColor != null)
        {
            using var paint = CreateStrokePaint(strokeColor, element.StrokeWidth, element.LineStyle);
            canvas.DrawPath(path, paint);
        }
    }

    private void DrawImage(SKCanvas canvas, ElementData element)
    {
        if (string.IsNullOrEmpty(element.Src) || !File.Exists(element.Src)) return;

        using var bitmap = SKBitmap.Decode(element.Src);
        if (bitmap == null) return;

        float w = element.Width > 0 ? element.Width : bitmap.Width;
        float h = element.Height > 0 ? element.Height : bitmap.Height;
        var dest = SKRect.Create(element.X, element.Y, w, h);

        canvas.DrawBitmap(bitmap, dest);
    }

    private void DrawTextInRect(SKCanvas canvas, string text, SKRect rect, ElementData element)
    {
        using var paint = new SKPaint
        {
            Color = ParseColor(element.TextColor),
            IsAntialias = true,
            TextSize = element.FontSize,
            Typeface = element.Bold ? SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) : SKTypeface.Default,
            TextAlign = SKTextAlign.Center
        };

        float textY = rect.MidY + element.FontSize / 3f;
        canvas.DrawText(text, rect.MidX, textY, paint);
    }

    private void DrawLabelAtPoint(SKCanvas canvas, string text, float x, float y, ElementData element)
    {
        using var paint = new SKPaint
        {
            Color = ParseColor(element.TextColor != "#000000" ? element.TextColor : "#333333"),
            IsAntialias = true,
            TextSize = element.FontSize > 0 ? element.FontSize * 0.75f : 12f,
            TextAlign = SKTextAlign.Center
        };

        using var bgPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 200),
            Style = SKPaintStyle.Fill
        };

        float textWidth = paint.MeasureText(text);
        var bgRect = new SKRect(x - textWidth / 2 - 4, y - paint.TextSize - 2,
                                x + textWidth / 2 + 4, y + 4);
        canvas.DrawRect(bgRect, bgPaint);
        canvas.DrawText(text, x, y, paint);
    }

    private (float x, float y) ResolvePosition(ElementData element, SlideData slide)
    {
        if (element.RelativeToId == null)
            return (element.X, element.Y);

        var target = slide.Elements.Find(e => e.Id == element.RelativeToId);
        if (target == null)
            return (element.X, element.Y);

        var targetBounds = GetElementBounds(target, slide);

        return (element.RelativePosition?.ToLowerInvariant()) switch
        {
            "below" => (targetBounds.x + (targetBounds.w - element.Width) / 2,
                        targetBounds.y + targetBounds.h + element.Gap),
            "above" => (targetBounds.x + (targetBounds.w - element.Width) / 2,
                        targetBounds.y - element.Height - element.Gap),
            "right" or "right-of" => (targetBounds.x + targetBounds.w + element.Gap,
                        targetBounds.y + (targetBounds.h - element.Height) / 2),
            "left" or "left-of" => (targetBounds.x - element.Width - element.Gap,
                        targetBounds.y + (targetBounds.h - element.Height) / 2),
            _ => (element.X, element.Y)
        };
    }

    private void ResolveLineEndpoints(ElementData element, SlideData slide,
                                       out float x1, out float y1, out float x2, out float y2)
    {
        if (element.FromId != null && element.ToId != null)
        {
            var fromEl = slide.Elements.Find(e => e.Id == element.FromId);
            var toEl = slide.Elements.Find(e => e.Id == element.ToId);

            if (fromEl != null && toEl != null)
            {
                var fromBounds = GetElementBounds(fromEl, slide);
                var toBounds = GetElementBounds(toEl, slide);
                ConnectBounds(fromBounds, toBounds, out x1, out y1, out x2, out y2);
                return;
            }
        }

        x1 = element.X1;
        y1 = element.Y1;
        x2 = element.X2;
        y2 = element.Y2;
    }

    private void ConnectBounds(
        (float x, float y, float w, float h, float cx, float cy) from,
        (float x, float y, float w, float h, float cx, float cy) to,
        out float x1, out float y1, out float x2, out float y2)
    {
        float dx = to.cx - from.cx;
        float dy = to.cy - from.cy;

        if (MathF.Abs(dy) > MathF.Abs(dx))
        {
            if (dy > 0)
            {
                x1 = from.cx; y1 = from.y + from.h;
                x2 = to.cx;   y2 = to.y;
            }
            else
            {
                x1 = from.cx; y1 = from.y;
                x2 = to.cx;   y2 = to.y + to.h;
            }
        }
        else
        {
            if (dx > 0)
            {
                x1 = from.x + from.w; y1 = from.cy;
                x2 = to.x;            y2 = to.cy;
            }
            else
            {
                x1 = from.x; y1 = from.cy;
                x2 = to.x + to.w; y2 = to.cy;
            }
        }
    }

    private (float x, float y, float w, float h, float cx, float cy) GetElementBounds(
        ElementData element, SlideData slide)
    {
        switch (element.Type.ToLowerInvariant())
        {
            case "circle":
            {
                float cx = element.Cx;
                float cy = element.Cy;
                float r = element.Radius;
                return (cx - r, cy - r, r * 2, r * 2, cx, cy);
            }
            case "ellipse":
            {
                float cx = element.Cx;
                float cy = element.Cy;
                return (cx - element.Rx, cy - element.Ry,
                        element.Rx * 2, element.Ry * 2, cx, cy);
            }
            default:
            {
                var (x, y) = ResolvePosition(element, slide);
                float w = element.Width;
                float h = element.Height;
                return (x, y, w, h, x + w / 2, y + h / 2);
            }
        }
    }

    private SKPaint CreateStrokePaint(string color, float strokeWidth, string lineStyle)
    {
        var paint = new SKPaint
        {
            Color = ParseColor(color),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            StrokeCap = SKStrokeCap.Round
        };

        switch (lineStyle.ToLowerInvariant())
        {
            case "dashed":
                paint.PathEffect = SKPathEffect.CreateDash(new[] { 10f, 5f }, 0);
                break;
            case "dotted":
                paint.PathEffect = SKPathEffect.CreateDash(new[] { 3f, 3f }, 0);
                break;
        }

        return paint;
    }

    private static SKColor ParseColor(string color)
    {
        if (string.IsNullOrEmpty(color))
            return SKColors.Black;

        if (SKColor.TryParse(color, out var parsed))
            return parsed;

        return SKColors.Black;
    }
}
