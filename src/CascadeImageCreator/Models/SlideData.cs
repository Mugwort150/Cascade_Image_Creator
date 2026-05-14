using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CascadeImageCreator.Models;

public sealed class SlideData
{
    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;

    [JsonPropertyName("background")]
    public string Background { get; set; } = "#FFFFFF";

    [JsonPropertyName("elements")]
    public List<ElementData> Elements { get; set; } = new();
}

public sealed class ElementData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("width")]
    public float Width { get; set; }

    [JsonPropertyName("height")]
    public float Height { get; set; }

    [JsonPropertyName("fill")]
    public string? Fill { get; set; }

    [JsonPropertyName("stroke")]
    public string? Stroke { get; set; }

    [JsonPropertyName("strokeWidth")]
    public float StrokeWidth { get; set; } = 2f;

    [JsonPropertyName("cornerRadius")]
    public float CornerRadius { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("textColor")]
    public string TextColor { get; set; } = "#000000";

    [JsonPropertyName("fontSize")]
    public float FontSize { get; set; } = 16f;

    [JsonPropertyName("bold")]
    public bool Bold { get; set; }

    [JsonPropertyName("align")]
    public string Align { get; set; } = "center";

    [JsonPropertyName("cx")]
    public float Cx { get; set; }

    [JsonPropertyName("cy")]
    public float Cy { get; set; }

    [JsonPropertyName("radius")]
    public float Radius { get; set; }

    [JsonPropertyName("rx")]
    public float Rx { get; set; }

    [JsonPropertyName("ry")]
    public float Ry { get; set; }

    [JsonPropertyName("x1")]
    public float X1 { get; set; }

    [JsonPropertyName("y1")]
    public float Y1 { get; set; }

    [JsonPropertyName("x2")]
    public float X2 { get; set; }

    [JsonPropertyName("y2")]
    public float Y2 { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("fromId")]
    public string? FromId { get; set; }

    [JsonPropertyName("toId")]
    public string? ToId { get; set; }

    [JsonPropertyName("lineStyle")]
    public string LineStyle { get; set; } = "solid";

    [JsonPropertyName("arrowSize")]
    public float ArrowSize { get; set; } = 12f;

    [JsonPropertyName("points")]
    public List<float[]>? Points { get; set; }

    [JsonPropertyName("src")]
    public string? Src { get; set; }

    [JsonPropertyName("opacity")]
    public float Opacity { get; set; } = 1f;

    [JsonPropertyName("rotation")]
    public float Rotation { get; set; }

    [JsonPropertyName("relativeToId")]
    public string? RelativeToId { get; set; }

    [JsonPropertyName("relativePosition")]
    public string? RelativePosition { get; set; }

    [JsonPropertyName("gap")]
    public float Gap { get; set; } = 20f;
}
