using System;
using System.IO;
using System.IO.Compression;

namespace TrueCraft.Core;

/// <summary>
///     Represents a Minecraft 1.7.3 texture pack (.zip archive).
/// </summary>
public class TexturePack
{
    // Lazily constructed so a missing Content asset doesn't tank class
    // loading (and every static field that touches TexturePack with it).
    private static readonly Lazy<TexturePack> _unknown = new(() => Load("?"));
    private static readonly Lazy<TexturePack> _default = new(() => Load("Default"));

    public static TexturePack Unknown => _unknown.Value;
    public static TexturePack Default => _default.Value;

    // Unknown and Default share the same on-disk default assets — they
    // existed as separate names historically but only one set of asset
    // files (default-pack.png / default-pack.txt) is shipped.
    private static TexturePack Load(string name)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return new TexturePack(
            name,
            File.OpenRead(Path.Combine(baseDir, "Content/default-pack.png")),
            File.ReadAllText(Path.Combine(baseDir, "Content/default-pack.txt")));
    }

    public TexturePack(string name, Stream image, string description)
    {
        Name = name;
        Image = image;
        Description = description;
    }

    public string Name { get; }

    public Stream Image { get; }

    public string Description { get; }

    public static TexturePack FromArchive(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException();

        var description = Unknown.Description;
        var image = Unknown.Image;
        try
        {
            using var archive = ZipFile.OpenRead(path);
            foreach (var entry in archive.Entries)
                if (entry.FullName == "pack.txt")
                {
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream);
                    description = reader.ReadToEnd().TrimEnd('\n', '\r', ' ');
                }
                else if (entry.FullName == "pack.png")
                {
                    using var stream = entry.Open();
                    var ms = new MemoryStream((int) entry.Length);
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    image = ms;
                }
        }
        catch
        {
            return null;
        }

        var name = new FileInfo(path).Name;
        return new TexturePack(name, image, description);
    }
}
