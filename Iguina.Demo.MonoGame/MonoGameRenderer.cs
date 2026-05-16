using FontStashSharp;
using Iguina.Defs;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;


namespace Iguina.Demo.MonoGame
{

    /// <summary>
    /// Provide rendering for the GUI system.
    /// </summary>
    public class MonoGameRenderer : Iguina.Drivers.IRenderer
    {
        GraphicsDevice _device;
        SpriteBatch _spriteBatch;
        ContentManager _content;
        string _assetsRoot;
        Texture2D _whiteTexture;

        /// <summary>
        /// TTF/OTF font registry. Lookups in <see cref="GetFont"/> consult
        /// <see cref="_staticFonts"/> first and fall back here.
        /// </summary>
        Dictionary<string, FontSystem> _fontSystems = new();

        /// <summary>
        /// Pre-rasterized BMFont registry — entries here are returned as-is by
        /// <see cref="GetFont"/> ignoring the stylesheet's FontSize (bitmap glyphs
        /// have a baked-in size; scaling would defeat the point of using one).
        /// </summary>
        Dictionary<string, SpriteFontBase> _staticFonts = new();

        const string DefaultFontId = "default_font";

        Dictionary<string, Texture2D> _textures = new();
        Dictionary<string, Texture2D> _grayscaleClones = new();

        /// <summary>
        /// String the renderer treats as "draw this entity in grayscale" instead of
        /// loading a shader. Iguina stylesheets set <c>"EffectIdentifier": "disabled"</c>
        /// on disabled entities; we replicate the visual on the CPU so the build
        /// has no shader-compiler dependency.
        /// </summary>
        const string DisabledEffectId = "disabled";

        public float GlobalTextScale = 0.75f;

        /// <summary>
        /// Create the monogame renderer.
        /// </summary>
        /// <param name="assetsPath">Root directory to load assets from. Check out the demo project for details.</param>
        public MonoGameRenderer(ContentManager content, GraphicsDevice device, SpriteBatch spriteBatch, string assetsPath)
        {
            _content = content;
            _device = device;
            _spriteBatch = spriteBatch;
            _assetsRoot = assetsPath;

            // create white texture
            _whiteTexture = new Texture2D(_device, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });

            // Load the default font (Open Sans Regular) embedded in this assembly.
            // FontStashSharp rasterizes TTF on demand at the requested pixel size,
            // so we don't need MGCB's spritefont pipeline.
            using var s = typeof(MonoGameRenderer).Assembly.GetManifestResourceStream("default_font.ttf")
                ?? throw new InvalidOperationException("Embedded default_font.ttf missing");
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            RegisterFont(DefaultFontId, ms.ToArray());
        }

        /// <summary>
        /// Register an additional font family at runtime. Callers pass the raw
        /// TTF/OTF bytes; FontStashSharp rasterizes glyphs on demand at each
        /// requested pixel size. Pass an existing id to replace its TTF.
        /// </summary>
        public void RegisterFont(string fontId, byte[] ttf)
        {
            var system = new FontSystem();
            system.AddFont(ttf);
            _fontSystems[fontId] = system;
        }

        /// <summary>
        /// Register an additional font family by file path. Loads the bytes once
        /// and forwards to <see cref="RegisterFont(string, byte[])"/>.
        /// </summary>
        public void RegisterFont(string fontId, string ttfPath)
        {
            RegisterFont(fontId, File.ReadAllBytes(ttfPath));
        }

        /// <summary>
        /// Register a pre-rasterized BMFont under <paramref name="fontId"/>. The
        /// .fnt is read from <paramref name="fntPath"/>; referenced PNG pages are
        /// opened from the same directory and premultiplied (SpriteBatch's default
        /// AlphaBlend expects premultiplied alpha; raw PNG loads are not). Bitmap
        /// fonts ignore the stylesheet FontSize and render at their baked-in size
        /// for pixel-perfect output.
        /// </summary>
        public void RegisterBMFont(string fontId, string fntPath)
        {
            var data = File.ReadAllText(fntPath);
            var dir = Path.GetDirectoryName(Path.GetFullPath(fntPath)) ?? string.Empty;
            Func<string, TextureWithOffset> textureGetter =
                fileName => new TextureWithOffset(LoadPremultipliedTexture(Path.Combine(dir, fileName)));
            var staticFont = StaticSpriteFont.FromBMFont(data, textureGetter);
            _staticFonts[fontId] = staticFont;
        }

        Texture2D LoadPremultipliedTexture(string path)
        {
            using var stream = File.OpenRead(path);
            var tex = Texture2D.FromStream(_device, stream);
            var pixels = new Color[tex.Width * tex.Height];
            tex.GetData(pixels);
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                if (p.A == 255) continue;
                pixels[i] = new Color(
                    (byte)(p.R * p.A / 255),
                    (byte)(p.G * p.A / 255),
                    (byte)(p.B * p.A / 255),
                    p.A);
            }
            tex.SetData(pixels);
            return tex;
        }

        /// <summary>
        /// Resolve a font id to a <see cref="SpriteFontBase"/>. Static (BMFont)
        /// entries win over dynamic (TTF); unknown ids fall back to the embedded
        /// default font.
        /// </summary>
        SpriteFontBase GetFont(string? fontName, int fontSize)
        {
            var key = fontName ?? DefaultFontId;
            if (_staticFonts.TryGetValue(key, out var staticFont))
                return staticFont;
            if (!_fontSystems.TryGetValue(key, out var system))
                system = _fontSystems[DefaultFontId];
            return system.GetFont(fontSize * GlobalTextScale);
        }

        /// <summary>
        /// Load / get texture.
        /// </summary>
        Texture2D GetTexture(string textureId)
        {
            if (_textures.TryGetValue(textureId, out var texture))
            {
                return texture;
            }

            var path = System.IO.Path.Combine(_assetsRoot, textureId);
            var ret = Texture2D.FromFile(_device, path);
            _textures[textureId] = ret;
            return ret;
        }

        /// <summary>
        /// Register a runtime-loaded <see cref="Texture2D"/> under a texture id so
        /// subsequent <c>DrawTexture(null, id, ...)</c> calls find it without going
        /// to disk. The caller retains ownership; replacing an existing id does
        /// not dispose the previous texture.
        /// </summary>
        public void RegisterTexture(string id, Texture2D texture)
        {
            _textures[id] = texture;
        }

        /// <summary>
        /// Remove a texture id from the cache. Does not dispose the underlying
        /// <see cref="Texture2D"/> — that remains the caller's responsibility.
        /// </summary>
        public void UnregisterTexture(string id)
        {
            _textures.Remove(id);
        }

        /// <summary>
        /// Return the (Width, Height) of a registered or disk-loaded texture in pixels.
        /// </summary>
        public Point GetTextureSize(string id)
        {
            var t = GetTexture(id);
            return new Point(t.Width, t.Height);
        }

        /// <summary>
        /// Load / get effect from id. The <see cref="DisabledEffectId"/> sentinel
        /// resolves to no shader — disabled visuals are applied on the CPU via
        /// <see cref="GetGrayscaleClone"/> and <see cref="Desaturate"/>.
        /// </summary>
        Effect? GetEffect(string? effectId)
        {
            if (effectId is null || effectId == DisabledEffectId) { return null; }
            return _content.Load<Effect>(effectId);
        }

        /// <summary>
        /// Lazily build and cache a grayscale clone of a texture using the
        /// Rec.601 luma formula. Used to render entities in the disabled state
        /// without a GPU shader.
        /// </summary>
        Texture2D GetGrayscaleClone(string textureId)
        {
            if (_grayscaleClones.TryGetValue(textureId, out var cached)) return cached;
            var source = GetTexture(textureId);
            var pixels = new Microsoft.Xna.Framework.Color[source.Width * source.Height];
            source.GetData(pixels);
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                var luma = (byte)((p.R * 299 + p.G * 587 + p.B * 114) / 1000);
                pixels[i] = new Microsoft.Xna.Framework.Color(luma, luma, luma, p.A);
            }
            var gray = new Texture2D(_device, source.Width, source.Height);
            gray.SetData(pixels);
            _grayscaleClones[textureId] = gray;
            return gray;
        }

        /// <summary>
        /// Desaturate a color via Rec.601 luma — used to render text in the
        /// disabled state without a GPU shader. (SpriteBatch multiplies the
        /// tint across all glyph pixels, so a grayscale tint is equivalent
        /// to per-pixel desaturation when the source glyph atlas is white.)
        /// </summary>
        static Color Desaturate(Color c)
        {
            var luma = (byte)((c.R * 299 + c.G * 587 + c.B * 114) / 1000);
            return new Color(luma, luma, luma, c.A);
        }

        /// <summary>
        /// Set active effect id.
        /// </summary>
        void SetEffect(string? effectId)
        {
            if (_currEffectId != effectId)
            {
                _spriteBatch.End();
                _currEffectId = effectId;
                BeginBatch();
            }
        }
        string? _currEffectId;

        /// <summary>
        /// Convert iguina color to mg color.
        /// </summary>
        Microsoft.Xna.Framework.Color ToMgColor(Color color)
        {
            var colorMg = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
            if (color.A < 255)
            {
                float factor = (float)color.A / 255f;
                colorMg.R = (byte)((float)color.R * factor);
                colorMg.G = (byte)((float)color.G * factor);
                colorMg.B = (byte)((float)color.B * factor);
            }
            return colorMg;
        }

        /// <summary>
        /// Called at the beginning of every frame.
        /// </summary>
        public void StartFrame()
        {
            _currEffectId = null;
            _currScissorRegion = null;
            BeginBatch();
        }

        /// <summary>
        /// Called at the end of every frame.
        /// </summary>
        public void EndFrame()
        {
            _spriteBatch.End();
        }

        /// <inheritdoc/>
        public Rectangle GetScreenBounds()
        {
            int screenWidth = _device.Viewport.Width;
            int screenHeight = _device.Viewport.Height;
            return new Rectangle(0, 0, screenWidth, screenHeight);
        }

        /// <inheritdoc/>
        public void DrawTexture(string? effectIdentifier, string textureId, Rectangle destRect, Rectangle sourceRect, Color color)
        {
            // "disabled" effect short-circuits to a cached grayscale clone + null shader.
            var disabled = effectIdentifier == DisabledEffectId;
            SetEffect(disabled ? null : effectIdentifier);
            var texture = disabled ? GetGrayscaleClone(textureId) : GetTexture(textureId);
            var colorMg = ToMgColor(color);
            _spriteBatch.Draw(texture,
                new Microsoft.Xna.Framework.Rectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height),
                new Microsoft.Xna.Framework.Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height),
                colorMg);
        }

        /// <inheritdoc/>
        public Point MeasureText(string text, string? fontId, int fontSize, float spacing)
        {
            var font = GetFont(fontId, fontSize);
            var size = font.MeasureString(text, characterSpacing: spacing - 1f);
            return new Point((int)Math.Ceiling(size.X), (int)Math.Ceiling(size.Y));
        }

        /// <inheritdoc/>
        public int GetTextLineHeight(string? fontId, int fontSize)
        {
            return GetFont(fontId, fontSize).LineHeight;
        }

        /// <inheritdoc/>

        public void DrawText(string? effectIdentifier, string text, string? fontId, int fontSize, Point position, Color fillColor, Color outlineColor, int outlineWidth, float spacing)
        {
            // "disabled" effect short-circuits to desaturated fill/outline colors + null shader.
            if (effectIdentifier == DisabledEffectId)
            {
                SetEffect(null);
                fillColor = Desaturate(fillColor);
                outlineColor = Desaturate(outlineColor);
            }
            else
            {
                SetEffect(effectIdentifier);
            }

            var font = GetFont(fontId, fontSize);
            var characterSpacing = spacing - 1f;

            // draw outline via 8-direction offset rendering. Naive but matches the
            // SpriteFont-era output exactly; FontStashSharp has no built-in stroked
            // text mode.
            if ((outlineColor.A > 0) && (outlineWidth > 0))
            {
                // outline fades faster than fill, replicating the prior behavior
                if (outlineColor.A < 255)
                {
                    float alphaFactor = (float)(outlineColor.A / 255f);
                    outlineColor.A = (byte)((float)fillColor.A * Math.Pow(alphaFactor, 7));
                }

                var outline = ToMgColor(outlineColor);
                for (int dx = -outlineWidth; dx <= outlineWidth; dx += outlineWidth)
                {
                    for (int dy = -outlineWidth; dy <= outlineWidth; dy += outlineWidth)
                    {
                        if (dx == 0 && dy == 0) continue;
                        font.DrawText(_spriteBatch, text,
                            new Microsoft.Xna.Framework.Vector2(position.X + dx, position.Y + dy),
                            outline,
                            characterSpacing: characterSpacing);
                    }
                }
            }

            // draw fill
            font.DrawText(_spriteBatch, text,
                new Microsoft.Xna.Framework.Vector2(position.X, position.Y),
                ToMgColor(fillColor),
                characterSpacing: characterSpacing);
        }

        /// <inheritdoc/>
        public void DrawRectangle(Rectangle rectangle, Color color)
        {
            SetEffect(null);

            var texture = _whiteTexture;
            var colorMg = ToMgColor(color);
            _spriteBatch.Draw(texture,
                new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height),
                null,
                colorMg);
        }

        /// <inheritdoc/>
        public void SetScissorRegion(Rectangle region)
        {
            _currScissorRegion = region;
            _currEffectId = null;
            _spriteBatch.End();
            BeginBatch();
        }

        /// <inheritdoc/>
        public Rectangle? GetScissorRegion()
        {
            return _currScissorRegion;
        }

        // current scissor region
        Rectangle? _currScissorRegion = null;

        /// <summary>
        /// Begin a new rendering batch.
        /// </summary>
        void BeginBatch()
        {
            var effect = GetEffect(_currEffectId);
            if (_currScissorRegion is not null)
            {
                _device.ScissorRectangle = new Microsoft.Xna.Framework.Rectangle(_currScissorRegion.Value.X, _currScissorRegion.Value.Y, _currScissorRegion.Value.Width, _currScissorRegion.Value.Height);
            }
            var raster = new RasterizerState();
            raster.CullMode = _device.RasterizerState.CullMode;
            raster.DepthBias = _device.RasterizerState.DepthBias;
            raster.FillMode = _device.RasterizerState.FillMode;
            raster.MultiSampleAntiAlias = _device.RasterizerState.MultiSampleAntiAlias;
            raster.SlopeScaleDepthBias = _device.RasterizerState.SlopeScaleDepthBias;
            raster.ScissorTestEnable = _currScissorRegion.HasValue;
            _device.RasterizerState = raster;
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: effect, rasterizerState: raster);
        }

        /// <inheritdoc/>
        public void ClearScissorRegion()
        {
            _currScissorRegion = null;
            _currEffectId = null;
            _spriteBatch.End();
            BeginBatch();
        }

        /// <inheritdoc/>
        public Color GetPixelFromTexture(string textureId, Point sourcePosition)
        {
            var texture = GetTexture(textureId);
            var pixelData = new Microsoft.Xna.Framework.Color[1];
            if (sourcePosition.X < 0) sourcePosition.X = 0;
            if (sourcePosition.Y < 0) sourcePosition.Y = 0;
            if (sourcePosition.X >= texture.Width) sourcePosition.X = texture.Width - 1;
            if (sourcePosition.Y >= texture.Height) sourcePosition.Y = texture.Height - 1;
            texture.GetData(0, new Microsoft.Xna.Framework.Rectangle(sourcePosition.X, sourcePosition.Y, 1, 1), pixelData, 0, 1);
            var pixelColor = pixelData[0];
            return new Color(pixelColor.R, pixelColor.G, pixelColor.B, pixelColor.A);
        }

        /// <inheritdoc/>
        public Point? FindPixelOffsetInTexture(string textureId, Rectangle sourceRect, Color color, bool returnNearestColor)
        {
            var texture = GetTexture(textureId);
            var pixelData = new Microsoft.Xna.Framework.Color[sourceRect.Width * sourceRect.Height];
            texture.GetData(0, new Microsoft.Xna.Framework.Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height), pixelData, 0, pixelData.Length);
            Point? ret = null;
            float nearestDistance = 255f * 255f * 255f * 255f;
            for (int x = 0; x < sourceRect.Width; x++)
            {
                for (int y = 0; y < sourceRect.Height; y++)
                {
                    var curr = pixelData[x + y * sourceRect.Width];
                    if (curr.R == color.R && curr.G == color.G && curr.B == color.B && curr.A == color.A)
                    {
                        return new Point(x, y);
                    }
                    else if (returnNearestColor)
                    {
                        float distance = Vector4.Distance(new Vector4(curr.R, curr.G, curr.B, curr.A), new Vector4(color.R, color.G, color.B, color.A));
                        if (distance <  nearestDistance)
                        {
                            nearestDistance = distance;
                            ret = new Point(x, y);
                        }
                    }
                }
            }
            return ret;
        }

    }
}
