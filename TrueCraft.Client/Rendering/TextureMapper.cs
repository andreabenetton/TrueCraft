using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using TrueCraft.Core;

namespace TrueCraft.Client.Rendering;

/// <summary>
///     Provides mappings from keys to textures.
/// </summary>
public sealed class TextureMapper : IDisposable
{
    private readonly ILogger<TextureMapper> Log;

    /// <summary>
    /// </summary>
    public static readonly IDictionary<string, Texture2D> Defaults =
        new Dictionary<string, Texture2D>();

    /// <summary>
    /// </summary>
    /// <param name="graphicsDevice"></param>
    public TextureMapper(GraphicsDevice graphicsDevice, ILogger<TextureMapper> log)
    {
        Device = graphicsDevice ?? throw new ArgumentException();
        Log = log;
        Customs = new Dictionary<string, Texture2D>();
        IsDisposed = false;
    }

    /// <summary>
    /// </summary>
    private GraphicsDevice Device { get; set; }

    /// <summary>
    /// </summary>
    private IDictionary<string, Texture2D> Customs { get; set; }

    /// <summary>
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;

        foreach (var pair in Customs)
            pair.Value.Dispose();

        Customs = null;
        Device = null;
        IsDisposed = true;
    }

    /// <summary>
    /// </summary>
    /// <param name="graphicsDevice"></param>
    public static void LoadDefaults(GraphicsDevice graphicsDevice)
    {
        Defaults.Clear();

        Defaults.Add("terrain.png", LoadPng(graphicsDevice, "Content/terrain.png"));
        Defaults.Add("gui/items.png", LoadPng(graphicsDevice, "Content/items.png"));
        Defaults.Add("gui/gui.png", LoadPng(graphicsDevice, "Content/gui.png"));
        Defaults.Add("gui/icons.png", LoadPng(graphicsDevice, "Content/icons.png"));
        Defaults.Add("gui/crafting.png", LoadPng(graphicsDevice, "Content/crafting.png"));
        Defaults.Add("gui/furnace.png", LoadPng(graphicsDevice, "Content/furnace.png"));
        Defaults.Add("gui/inventory.png", LoadPng(graphicsDevice, "Content/inventory.png"));
        Defaults.Add("terrain/moon.png", LoadPng(graphicsDevice, "Content/moon.png"));
        Defaults.Add("terrain/sun.png", LoadPng(graphicsDevice, "Content/sun.png"));
    }

    private static Texture2D LoadPng(GraphicsDevice graphicsDevice, string path)
    {
        using var stream = File.OpenRead(path);
        return Texture2D.FromStream(graphicsDevice, stream);
    }

    /// <summary>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="texture"></param>
    public void AddTexture(string key, Texture2D texture)
    {
        if (string.IsNullOrEmpty(key) || texture is null)
            throw new ArgumentException();

        if (Customs.ContainsKey(key))
            Customs[key] = texture;
        else
            Customs.Add(key, texture);
    }

    /// <summary>
    /// </summary>
    /// <param name="texturePack"></param>
    public void AddTexturePack(TexturePack texturePack)
    {
        if (texturePack is null)
            return;

        // Make sure to 'silence' errors loading custom texture packs;
        // they're unimportant as we can just use default textures.
        try
        {
            using var archive = ZipFile.OpenRead(Path.Combine(Paths.TexturePacks, texturePack.Name));
            foreach (var entry in archive.Entries)
            {
                var key = entry.FullName;
                if (Path.GetExtension(key) == ".png")
                {
                    using var stream = entry.Open();
                    try
                    {
                        using var ms = new MemoryStream();
                        CopyStream(stream, ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        AddTexture(key, Texture2D.FromStream(Device, ms));
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex, "Failed to load {Key} from texture pack", key);
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    public static void CopyStream(Stream input, Stream output)
    {
        var buffer = new byte[16 * 1024];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0) output.Write(buffer, 0, read);
    }

    /// <summary>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Texture2D GetTexture(string key)
    {
        TryGetTexture(key, out var result);
        if (result is null)
            throw new InvalidOperationException();

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="texture"></param>
    /// <returns></returns>
    public bool TryGetTexture(string key, out Texture2D texture)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException();

        // -> Try to load from custom textures
        var inCustom = Customs.TryGetValue(key, out var customTexture);
        texture = inCustom ? customTexture : null;
        var hasTexture = inCustom;

        // -> Try to load from default textures
        if (!hasTexture)
        {
            var inDefault = Defaults.TryGetValue(key, out var defaultTexture);
            texture = inDefault ? defaultTexture : null;
            hasTexture = inDefault;
        }

        // -> Fail gracefully
        return hasTexture;
    }
}
