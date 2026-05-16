using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.API;

namespace TrueCraft.Client.Rendering;

/// <summary>
///     Represents a font renderer.
/// </summary>
public class FontRenderer
{
    /// <summary>
    /// </summary>
    /// <param name="font"></param>
    public FontRenderer(Font font)
    {
        if (font is null)
            throw new ArgumentNullException(nameof(font));

        Fonts = new[]
        {
            font
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="regular"></param>
    /// <param name="bold"></param>
    /// <param name="strikethrough"></param>
    /// <param name="underline"></param>
    /// <param name="italic"></param>
    public FontRenderer(Font regular, Font bold, Font strikethrough, Font underline, Font italic)
    {
        if (regular is null)
            throw new ArgumentNullException(nameof(regular));

        Fonts = new[]
        {
            regular,
            bold ?? regular,
            strikethrough ?? regular,
            underline ?? regular,
            italic ?? regular
        };
    }

    /// <summary>
    /// </summary>
    public Font[] Fonts { get; }

    public Point MeasureText(string text, float scale = 1.0f)
    {
        var dx = 0;
        var height = 0;
        var font = Fonts[0];
        for (var i = 0; i < text.Length; i++)
            if (text[i] == '§' && i + 1 < text.Length)
            {
                i++;
                if (IsFormatChar(text[i]))
                    font = GetFontForChar(text[i]);
            }
            else
            {
                var glyph = font.GetGlyph(text[i]);
                if (glyph is not null)
                {
                    dx += (int) (glyph.XAdvance * scale);
                    if (glyph.Height > height)
                        height = glyph.Height;
                }
            }

        return new Point(dx, height);
    }

    public void DrawText(SpriteBatch spriteBatch, int x, int y, string text, float scale = 1.0f, byte alpha = 255)
    {
        var dx = x;
        var dy = y;
        var color = Color.White;
        var font = Fonts[0];

        for (var i = 0; i < text.Length; i++)
            if (text[i] == '§' && i + 1 < text.Length)
            {
                i++;
                if (IsFormatChar(text[i]))
                    font = GetFontForChar(text[i]);
                else
                    color = GetColorForChar(text[i]);
            }
            else
            {
                var glyph = font.GetGlyph(text[i]);
                if (glyph is not null)
                {
                    var sourceRectangle = new Rectangle(glyph.X, glyph.Y, glyph.Width, glyph.Height);
                    var destRectangle = new Rectangle(
                        dx + (int) (glyph.XOffset * scale),
                        dy + (int) (glyph.YOffset * scale),
                        (int) (glyph.Width * scale),
                        (int) (glyph.Height * scale));
                    var shadowRectangle = new Rectangle(
                        dx + (int) (glyph.XOffset * scale) + 4,
                        dy + (int) (glyph.YOffset * scale) + 4,
                        (int) (glyph.Width * scale),
                        (int) (glyph.Height * scale));

                    spriteBatch.Draw(font.GetTexture(glyph.Page), shadowRectangle, sourceRectangle,
                        new Color(21, 21, 21, (int) alpha));
                    spriteBatch.Draw(font.GetTexture(glyph.Page), destRectangle, sourceRectangle,
                        new Color(color, alpha));
                    dx += (int) (glyph.XAdvance * scale);
                }
            }
    }

    private static bool IsFormatChar(char c)
    {
        switch (c)
        {
            case 'k': case 'K':
            case 'l': case 'L':
            case 'm': case 'M':
            case 'n': case 'N':
            case 'o': case 'O':
            case 'r': case 'R':
                return true;
            default:
                return false;
        }
    }

    private Font GetFontForChar(char formatChar)
    {
        // If we are a mono-font renderer, we don't actually care about formatting codes.
        if (Fonts.Length == 1)
            return Fonts[0];

        switch (formatChar)
        {
            case 'k': case 'K': // Obfuscated — not supported yet.
            case 'r': case 'R':
                return Fonts[(int) FontStyle.Regular];
            case 'l': case 'L':
                return Fonts[(int) FontStyle.Bold];
            case 'm': case 'M':
                return Fonts[(int) FontStyle.Strikethrough];
            case 'n': case 'N':
                return Fonts[(int) FontStyle.Underline];
            case 'o': case 'O':
                return Fonts[(int) FontStyle.Italic];
            default:
                return Fonts[0];
        }
    }

    // RGB values taken from http://minecraft.gamepedia.com/Formatting_codes
    private static Color GetColorForChar(char colorChar)
    {
        switch (colorChar)
        {
            case '0': return new Color(0, 0, 0);          // Black
            case '1': return new Color(0, 0, 170);        // DarkBlue
            case '2': return new Color(0, 170, 0);        // DarkGreen
            case '3': return new Color(0, 170, 170);      // DarkCyan
            case '4': return new Color(170, 0, 0);        // DarkRed
            case '5': return new Color(170, 0, 170);      // Purple
            case '6': return new Color(255, 170, 0);      // Orange
            case '7': return new Color(170, 170, 170);    // Gray
            case '8': return new Color(85, 85, 85);       // DarkGray
            case '9': return new Color(85, 85, 255);      // Blue
            case 'a': case 'A': return new Color(85, 255, 85);
            case 'b': case 'B': return new Color(85, 255, 255);
            case 'c': case 'C': return new Color(255, 85, 85);
            case 'd': case 'D': return new Color(255, 85, 255);
            case 'e': case 'E': return new Color(255, 255, 85);
            case 'f': case 'F': return new Color(255, 255, 255);
            default: return Color.White;
        }
    }
}
