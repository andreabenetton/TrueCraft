using Iguina.Defs;


namespace Iguina.Entities;

/// <summary>
/// Draws a runtime-supplied texture (registered with the active <see cref="Drivers.IRenderer"/>
/// by string id) at this entity's bounding rectangle. Unlike entities that paint via
/// stylesheet <c>FillTextureStretched</c>, an <see cref="Image"/> is intended for textures
/// whose pixel data is built or decoded at runtime (e.g. a thumbnail preview decoded from
/// a stream) rather than loaded from the assets directory.
/// </summary>
public class Image : Entity
{
    /// <summary>
    /// Texture id to render. The id is resolved by the renderer; for
    /// <c>MonoGameRenderer</c> the host must call
    /// <c>RegisterTexture(id, texture2D)</c> before the first draw.
    /// </summary>
    public string TextureId { get; set; }

    /// <summary>
    /// Source rectangle into the texture. When <c>null</c>, the full destination
    /// rectangle (i.e. the entity's bounding rect) is used as the source — the
    /// renderer interprets that as "stretch the texture to fit".
    /// </summary>
    public Rectangle? SourceRect { get; set; }

    /// <summary>
    /// Multiplicative tint applied at draw time. Defaults to white (no tint).
    /// </summary>
    public Color TintColor { get; set; } = new Color(255, 255, 255, 255);

    /// <summary>
    /// Optional drop-shadow color. When non-null the image renders a copy
    /// at <see cref="ShadowOffset"/> beneath the main draw, tinted with
    /// this color. Set to <c>null</c> (default) to disable.
    /// </summary>
    public Color? ShadowColor { get; set; }

    /// <summary>
    /// Pixel offset of the drop shadow relative to the image. Default (2, 2)
    /// places the shadow down-right. Honored only when <see cref="ShadowColor"/>
    /// is non-null.
    /// </summary>
    public Point ShadowOffset { get; set; } = new Point(2, 2);

    /// <param name="system">Parent UI system.</param>
    /// <param name="textureId">Identifier the renderer uses to look up the texture.</param>
    public Image(UISystem system, string textureId) : base(system, null)
    {
        TextureId = textureId;
        IgnoreInteractions = true;
    }

    /// <inheritdoc/>
    protected override void DrawEntityType(ref Rectangle boundingRect, ref Rectangle internalBoundingRect, DrawMethodResult parentDrawResult, DrawMethodResult? siblingDrawResult)
    {
        base.DrawEntityType(ref boundingRect, ref internalBoundingRect, parentDrawResult, siblingDrawResult);
        var src = SourceRect ?? new Rectangle(0, 0, boundingRect.Width, boundingRect.Height);
        if (ShadowColor.HasValue)
        {
            var shadowRect = new Rectangle(
                boundingRect.X + ShadowOffset.X,
                boundingRect.Y + ShadowOffset.Y,
                boundingRect.Width,
                boundingRect.Height);
            UISystem.Renderer.DrawTexture(null, TextureId, shadowRect, src, ShadowColor.Value);
        }
        UISystem.Renderer.DrawTexture(null, TextureId, boundingRect, src, TintColor);
    }
}
