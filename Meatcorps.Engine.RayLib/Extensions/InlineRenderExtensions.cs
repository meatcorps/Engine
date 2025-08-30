using System.Globalization;
using System.Numerics;
using System.Text;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.UI;
using Meatcorps.Engine.RayLib.UI.Data;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Extensions;

public static class InlineRenderExtensions
{
    // ---------- primitives ----------
    public static InlineRender AddSpacer(this InlineRender ir, int width, int height = 1, Insets? margin = null)
    {
        var id = $"spacer_{Guid.NewGuid()}";
        ir.Register(new InlineItem
        {
            Identifier = id,
            CacheSize = true,
            Margin = margin ?? Insets.Zero,
            Initialize = (_, __) => { },
            GetSize = (_, __) => new PointInt(width, height),
            Draw = (_, __, ____) => { },
            Destroy = (_, __) => { }
        });
        return ir;
    }

    public static InlineRender AddNewLine(this InlineRender ir, string? id = null)
    {
        id ??= $"icon_{Guid.NewGuid()}";
        ir.Register(new InlineItem
        {
            Identifier = id,
            CacheSize = true,
            NewLine = true,
            Initialize = (_, __) => { },
            GetSize = (_, __) => new PointInt(0, 0),
            Draw = (_, __, ____) => { },
            Destroy = (_, __) => { }
        });
        return ir;
    }

    public static InlineRender AddRectIcon(
        this InlineRender ir,
        string? id,
        int width, int height,
        Color fill,
        Color? stroke = null,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle,
        bool fixedWidth = false,
        bool fixedHeight = false)
    {
        id ??= $"icon_{Guid.NewGuid()}";
        ir.Register(new InlineItem
        {
            Identifier = id,
            HAlign = hAlign,
            VAlign = vAlign,
            FillWidth = fixedWidth,
            FillHeight = fixedHeight,
            CacheSize = true,
            Margin = margin ?? new Insets(0),
            Initialize = (_, __) => { },
            GetSize = (_, __) => new PointInt(width, height),
            Draw = (_, __, rect) =>
            {
                Raylib.DrawRectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, fill);
                if (stroke.HasValue)
                    Raylib.DrawRectangleLines((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height,
                        stroke.Value);
            },
            Destroy = (_, __) => { }
        });
        return ir;
    }

    public static InlineRender AddTexture(
        this InlineRender ir,
        string? id,
        Texture2D texture,
        PointInt? desiredSize = null,
        Rectangle? source = null,
        Color? tint = null,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle)
    {
        id ??= $"tex_{Guid.NewGuid()}";
        var src = source ?? new Rectangle(0, 0, texture.Width, texture.Height);
        var rawSize = desiredSize ?? new PointInt((int)src.Width, (int)src.Height);
        var useTint = tint ?? Color.White;

        ir.Register(new InlineItem
        {
            Identifier = id,
            HAlign = hAlign,
            VAlign = vAlign,
            CacheSize = true,
            Margin = margin ?? Insets.Zero,
            Initialize = (_, __) => { },
            GetSize = (_, __) => rawSize,
            Draw = (_, __, rect) =>
            {
                var dest = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                Raylib.DrawTexturePro(texture, src, dest, Vector2.Zero, 0f, useTint);
            },
            Destroy = (_, __) => { }
        });

        return ir;
    }

    public static InlineRender AddTexture<T>(
        this InlineRender ir,
        string? id,
        Texture2DItem<T> spriteSet,
        T spriteKey,
        PointInt? desiredSize = null,
        Color? tint = null,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle) where T : Enum
    {
        var sourceRect = spriteSet.GetSprite(spriteKey);
        desiredSize ??= new PointInt((int)sourceRect.Width, (int)sourceRect.Height);
        AddTexture(ir, id, spriteSet.Texture, desiredSize, sourceRect, tint, margin, hAlign, vAlign);
        return ir;
    }

    public static InlineRender AddSpriteAnimation<T>(
        this InlineRender ir,
        string? id,
        Texture2DItem<T> spriteSet,
        FixedTimer? timer,
        int? animationSpeed,
        T spriteKey,
        PointInt? desiredSize = null,
        Color? tint = null,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle) where T : Enum
    {
        id ??= $"ani_{Guid.NewGuid()}";
        var sourceRect = spriteSet.GetSprite(spriteKey);
        desiredSize ??= new PointInt((int)sourceRect.Width, (int)sourceRect.Height);
        var useTint = tint ?? Color.White;

        var internalTimer = timer is null;
        timer ??= new FixedTimer(animationSpeed ?? 100);

        ir.Register(new InlineItem
        {
            Identifier = id,
            HAlign = hAlign,
            VAlign = vAlign,
            CacheSize = true,
            Margin = margin ?? Insets.Zero,
            Initialize = (_, __) => { },
            GetSize = (_, __) => desiredSize.Value,
            Update = (i, item, delta) =>
            {
                if (internalTimer)
                    timer.Update(delta);
            },
            Draw = (_, __, rect) =>
            {
                var dest = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                spriteSet.DrawAnimationWithNormal(spriteKey, timer.NormalizedElapsed, dest, useTint);
            },
            Destroy = (_, __) => { }
        });

        return ir;
    }

    public static InlineRender AddLabel(
        this InlineRender ir,
        Font font,
        string? id,
        string text,
        float fontSize,
        Color color,
        float spacing = 1f,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle,
        bool cacheSize = false,
        bool fixedWidth = false,
        bool fixedHeight = false)
    {
        id ??= $"lbl_{Guid.NewGuid()}";

        ir.Register(new InlineItem
        {
            Identifier = id,
            HAlign = hAlign,
            VAlign = vAlign,
            FillWidth = fixedWidth,
            FillHeight = fixedHeight,
            CacheSize = cacheSize, // false => remeasure each frame (text can change)
            Margin = margin ?? Insets.Zero,
            Data = new LabelData(text, fontSize, color, spacing),
            Initialize = (_, __) => { },
            GetSize = (_, item) =>
            {
                var d = (LabelData)item.Data!;
                var m = Raylib.MeasureTextEx(font, d.Text, d.FontSize, d.Spacing);
                return new PointInt((int)m.X, (int)m.Y);
            },
            Draw = (_, item, rect) =>
            {
                var d = (LabelData)item.Data!;
                Raylib.DrawTextEx(font, d.Text, new Vector2(rect.X, rect.Y), d.FontSize, d.Spacing, d.Color);
            },
            Destroy = (_, __) => { }
        });

        return ir;
    }
    
    public static InlineRender AddSmoothValue(
        this InlineRender ir,
        Font font,
        string? id,
        SmoothValue value,
        float fontSize,
        Color color,
        string floatParseSettings = "F0",
        float spacing = 1f,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle)
    {
        id ??= $"lbl_{Guid.NewGuid()}";

        ir.Register(new InlineItem
        {
            Identifier = id,
            HAlign = hAlign,
            VAlign = vAlign,
            CacheSize = false,
            Margin = margin ?? Insets.Zero,
            Data = new LabelData<SmoothValue>(value, floatParseSettings, fontSize, color, spacing),
            Initialize = (_, item) => {  },
            GetSize = (_, item) =>
            {
                var d = (LabelData<SmoothValue>)item.Data!;
                var m = Raylib.MeasureTextEx(font, d.Data.DisplayValue.ToString(d.Text), d.FontSize, d.Spacing);
                return new PointInt((int)m.X, (int)m.Y);
            },
            Update = (_, item, delta) =>
            {
                var d = (LabelData<SmoothValue>)item.Data!;
                d.Data.Update(delta);
            },
            Draw = (_, item, rect) =>
            {
                var d = (LabelData<SmoothValue>)item.Data!;
                Raylib.DrawTextEx(font, d.Data.DisplayValue.ToString(d.Text), new Vector2(rect.X, rect.Y), d.FontSize, d.Spacing, d.Color);
            },
            Destroy = (_, __) => { }
        });

        return ir;
    }

    // ---------- text streams ----------
    // Per-WORD: better perf, fewer items; relies on renderer wrap.
    public static InlineRender AddWordStream(
        this InlineRender ir,
        Font font,
        string idPrefix,
        string text,
        float fontSize,
        Color color,
        float spacing = 1f,
        Insets? marginPerWord = null)
    {
        var margin = marginPerWord ?? Insets.Zero;
        var tokens = TokenizeWordsAndNewlines(text);
        var measureCache = new Dictionary<string, PointInt>(64);

        foreach (var token in tokens)
        {
            // Handle explicit newline
            if (token == "\n")
            {
                var nlId = $"{idPrefix}_nl_{Guid.NewGuid():N}";
                ir.Register(new InlineItem
                {
                    Identifier = nlId,
                    CacheSize = true,
                    NewLine = true, 
                    Margin = Insets.Zero,
                    Initialize = (_, __) => { },
                    GetSize = (_, __) => new PointInt(0, 0),
                    Draw = (_, __, ____) => { },
                    Destroy = (_, __) => { }
                });
                continue;
            }

            var isSpace = token == " ";

            // Cache measurement per token
            PointInt Measure(string s)
            {
                if (measureCache.TryGetValue(s, out var sz))
                    return sz;

                if (s == " ")
                {
                    var mA = Raylib.MeasureTextEx(font, "A", fontSize, spacing);
                    var mSp = Raylib.MeasureTextEx(font, " ", fontSize, spacing);
                    sz = new PointInt((int)mSp.X, (int)mA.Y);
                }
                else
                {
                    var m = Raylib.MeasureTextEx(font, s, fontSize, spacing);
                    sz = new PointInt((int)m.X, (int)m.Y);
                }

                measureCache[s] = sz;
                return sz;
            }

            var tokenId = $"{idPrefix}_{Guid.NewGuid():N}";
            var tokenCopy = token; // capture for closure

            ir.Register(new InlineItem
            {
                Identifier = tokenId,
                CacheSize = true,
                Margin = margin,
                Initialize = (_, __) => { },
                GetSize = (_, __) => Measure(tokenCopy),
                Draw = (_, __, rect) =>
                {
                    if (!isSpace)
                        Raylib.DrawTextEx(font, tokenCopy, new Vector2(rect.X, rect.Y), fontSize, spacing, color);
                },
                Destroy = (_, __) => { }
            });
        }

        return ir;
    }

    private static IEnumerable<string> TokenizeWordsAndNewlines(string text)
    {
        var cur = "";
        foreach (var ch in text)
        {
            if (ch == '\n')
            {
                if (cur.Length > 0)
                {
                    yield return cur;
                    cur = "";
                }

                yield return "\n";
            }
            else if (char.IsWhiteSpace(ch) && ch != '\n')
            {
                if (cur.Length > 0)
                {
                    yield return cur;
                    cur = "";
                }

                yield return " ";
            }
            else
            {
                cur += ch;
            }
        }

        if (cur.Length > 0)
            yield return cur;
    }

    // Per-CHAR: exact wrapping/kerning-ish look; more items (use for stylized effects).
    public static InlineRender AddCharStream(
        this InlineRender ir,
        Font font,
        string idPrefix,
        string text,
        float fontSize,
        Color color,
        float spacing = 1f,
        Insets? marginPerChar = null)
    {
        var margin = marginPerChar ?? Insets.Zero;

        foreach (var ch in text)
        {
            var tokenId = $"{idPrefix}_{Guid.NewGuid():N}";
            var s = ch.ToString();
            var isEnter = ch == '\n';
            ir.Register(new InlineItem
            {
                Identifier = tokenId,
                CacheSize = true,
                NewLine = isEnter,
                Margin = margin,
                Initialize = (_, __) => { },
                GetSize = (_, __) =>
                {
                    if (isEnter)
                        return new PointInt(0, 0);
                    var mA = Raylib.MeasureTextEx(font, "A", fontSize, spacing);
                    var m = Raylib.MeasureTextEx(font, s, fontSize, spacing);
                    // ensure height is line-height for all glyphs
                    return new PointInt((int)m.X, (int)mA.Y);
                },
                Draw = (_, __, rect) =>
                {
                    // skip drawing “true spaces” to avoid overdraw, but preserve measured width
                    if (ch != ' ')
                        Raylib.DrawTextEx(font, s, new Vector2(rect.X, rect.Y), fontSize, spacing, color);
                },
                Destroy = (_, __) => { }
            });
        }

        return ir;
    }


    public static InlineRender AddIconFont(
        this InlineRender ir,
        Font font,
        string? id,
        string iconGlyph,
        float fontSize,
        Color color,
        float spacing = 1f,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle)
    {
        if (string.IsNullOrEmpty(iconGlyph))
            throw new ArgumentException("Icon glyph cannot be null or empty.", nameof(iconGlyph));

        id ??= $"icon_{Guid.NewGuid():N}";
        var useMargin = margin ?? Insets.Zero;

        ir.Register(new InlineItem
        {
            Identifier = id,
            CacheSize = true,
            HAlign = hAlign,
            VAlign = vAlign,
            Margin = useMargin,
            Initialize = (_, __) => { },
            GetSize = (_, __) =>
            {
                var m = Raylib.MeasureTextEx(font, iconGlyph, fontSize, spacing);
                return new PointInt((int)m.X, (int)m.Y);
            },
            Draw = (_, __, rect) =>
            {
                Raylib.DrawTextEx(font, iconGlyph, new Vector2(rect.X, rect.Y), fontSize, spacing, color);
            },
            Destroy = (_, __) => { }
        });

        return ir;
    }

    // --- Char overload ---
    public static InlineRender AddIconFont(
        this InlineRender ir,
        Font font,
        string? id,
        char iconChar,
        float fontSize,
        Color color,
        float spacing = 1f,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle)
    {
        return ir.AddIconFont(font, id, iconChar.ToString(), fontSize, color, spacing, margin, hAlign, vAlign);
    }

    // --- Unicode codepoint overload ---
    public static InlineRender AddIconFont(
        this InlineRender ir,
        Font font,
        string? id,
        int codepoint,
        float fontSize,
        Color color,
        float spacing = 1f,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle)
    {
        // Convert int → string via Rune
        var rune = new Rune(codepoint);
        var glyph = rune.ToString();
        return ir.AddIconFont(font, id, glyph, fontSize, color, spacing, margin, hAlign, vAlign);
    }

    public class LabelData<T> : LabelData
    {
        public T Data { get; init; }

        public LabelData(T data, string text, float fontSize, Color color, float spacing): base(text, fontSize, color, spacing)
        {
            Data = data;
        }
    }
    
    public class LabelData
    {
        public LabelData(string text, float fontSize, Color color, float spacing)
        {
            Text = text;
            FontSize = fontSize;
            Color = color;
            Spacing = spacing;
        }
        
        public string Text { get; set; }
        public float FontSize { get; set; }
        public Color Color { get; set; }
        public float Spacing { get; set; }
    }
}