using Iguina.Defs;


namespace Iguina.Entities
{
    /// <summary>
    /// Solid-color filled rectangle with an optional outline. Renders no texture
    /// and ignores the stylesheet system entirely — purely procedural color fill
    /// via <see cref="Drivers.IRenderer.DrawRectangle"/>. Use for dividers,
    /// backgrounds, debug markers, or anywhere a plain block of color is needed
    /// without the overhead of a Panel stylesheet.
    /// </summary>
    public class ColoredRectangle : Entity
    {
        /// <summary>Fill color (no fill if <see cref="Color.A"/> is 0).</summary>
        public Color FillColor { get; set; } = new Color(255, 255, 255, 255);

        /// <summary>
        /// Outline color drawn just inside the bounding rect. <c>null</c> disables
        /// the outline.
        /// </summary>
        public Color? OutlineColor { get; set; }

        /// <summary>Outline thickness in pixels. Only used when <see cref="OutlineColor"/> is set.</summary>
        public int OutlineWidth { get; set; } = 1;

        public ColoredRectangle(UISystem system) : base(system, null)
        {
            IgnoreInteractions = true;
        }

        public ColoredRectangle(UISystem system, Color fillColor) : this(system)
        {
            FillColor = fillColor;
        }

        /// <inheritdoc/>
        protected override void DrawEntityType(ref Rectangle boundingRect, ref Rectangle internalBoundingRect, DrawMethodResult parentDrawResult, DrawMethodResult? siblingDrawResult)
        {
            base.DrawEntityType(ref boundingRect, ref internalBoundingRect, parentDrawResult, siblingDrawResult);
            if (FillColor.A > 0)
            {
                UISystem.Renderer.DrawRectangle(boundingRect, FillColor);
            }
            if (OutlineColor.HasValue && OutlineWidth > 0)
            {
                var c = OutlineColor.Value;
                var t = OutlineWidth;
                // top / bottom / left / right rectangles forming the frame
                UISystem.Renderer.DrawRectangle(new Rectangle(boundingRect.X, boundingRect.Y, boundingRect.Width, t), c);
                UISystem.Renderer.DrawRectangle(new Rectangle(boundingRect.X, boundingRect.Y + boundingRect.Height - t, boundingRect.Width, t), c);
                UISystem.Renderer.DrawRectangle(new Rectangle(boundingRect.X, boundingRect.Y, t, boundingRect.Height), c);
                UISystem.Renderer.DrawRectangle(new Rectangle(boundingRect.X + boundingRect.Width - t, boundingRect.Y, t, boundingRect.Height), c);
            }
        }
    }
}
