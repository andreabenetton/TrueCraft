using Iguina.Defs;

namespace Iguina.Entities;

/// <summary>
/// Vertical spacer sized in text-line-height units. Use when the surrounding
/// content is text and you want the gap to scale with the line height of the
/// default paragraph stylesheet — unlike <see cref="RowsSpacer"/>, whose
/// height comes from a global system constant.
/// </summary>
public class LineSpace : Entity
{
    /// <param name="system">Parent UI system.</param>
    /// <param name="lines">Number of text lines worth of vertical space.</param>
    public LineSpace(UISystem system, int lines = 1) : base(system, null)
    {
        IgnoreInteractions = true;
        var lineHeight = system.Renderer.GetTextLineHeight(null, system.SystemStyleSheet.RowSpaceHeight);
        Size.Y.SetPixels(lineHeight * lines);
    }

    /// <inheritdoc/>
    protected override Anchor GetDefaultEntityTypeAnchor()
    {
        return Anchor.AutoLTR;
    }

    /// <inheritdoc/>
    protected override MeasureVector GetDefaultEntityTypeSize()
    {
        var ret = new MeasureVector();
        ret.X.SetPercents(100f);
        ret.Y.SetPixels(8);
        return ret;
    }
}
