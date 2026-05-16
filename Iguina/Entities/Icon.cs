using Iguina.Defs;


namespace Iguina.Entities;

/// <summary>
/// Renders a single icon-style sub-region of a texture at its native size
/// (multiplied by <see cref="IconTexture.TextureScale"/>). Unlike <see cref="Image"/>,
/// which stretches to fill its bounding rect, <see cref="Icon"/> auto-sizes
/// to its source rectangle so callers don't have to compute pixel sizes by hand.
/// </summary>
public class Icon : Entity
{
    /// <summary>
    /// Icon definition: texture id, source rect within the texture, scale,
    /// and optional centering / offset.
    /// </summary>
    public IconTexture IconTexture { get; set; }

    /// <param name="system">Parent UI system.</param>
    /// <param name="iconTexture">Icon definition.</param>
    public Icon(UISystem system, IconTexture iconTexture) : base(system, null)
    {
        IconTexture = iconTexture;
        IgnoreInteractions = true;
    }

    /// <param name="system">Parent UI system.</param>
    /// <param name="textureId">Texture identifier (resolved by the active <see cref="Drivers.IRenderer"/>).</param>
    /// <param name="sourceRect">Region of the texture to draw.</param>
    public Icon(UISystem system, string textureId, Rectangle sourceRect)
        : this(system, new IconTexture { TextureId = textureId, SourceRect = sourceRect })
    {
    }

    /// <inheritdoc/>
    protected override MeasureVector GetDefaultEntityTypeSize()
    {
        var ret = new MeasureVector();
        var w = (int)(IconTexture.SourceRect.Width * IconTexture.TextureScale);
        var h = (int)(IconTexture.SourceRect.Height * IconTexture.TextureScale);
        ret.X.SetPixels(w);
        ret.Y.SetPixels(h);
        return ret;
    }

    /// <inheritdoc/>
    protected override void DrawEntityType(ref Rectangle boundingRect, ref Rectangle internalBoundingRect, DrawMethodResult parentDrawResult, DrawMethodResult? siblingDrawResult)
    {
        base.DrawEntityType(ref boundingRect, ref internalBoundingRect, parentDrawResult, siblingDrawResult);
        var dest = new Rectangle(
            boundingRect.X + IconTexture.Offset.X,
            boundingRect.Y + IconTexture.Offset.Y,
            boundingRect.Width,
            boundingRect.Height);
        UISystem.Renderer.DrawTexture(null, IconTexture.TextureId, dest, IconTexture.SourceRect, new Color(255, 255, 255, 255));
    }
}
