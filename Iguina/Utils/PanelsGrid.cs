using Iguina.Defs;
using Iguina.Entities;


namespace Iguina.Utils
{
    /// <summary>
    /// Helpers for building grid layouts from <see cref="Panel"/> entities.
    /// Mirrors GeonBit.UI's PanelsGrid utility.
    /// </summary>
    public static class PanelsGrid
    {
        /// <summary>
        /// Split <paramref name="parent"/> horizontally into <paramref name="count"/>
        /// equal-width child Panels with <c>Anchor.AutoInlineLTR</c>. Returns the
        /// columns in left-to-right order. Caller populates each column.
        /// </summary>
        /// <param name="ui">Active UI system.</param>
        /// <param name="parent">Panel to add the columns to.</param>
        /// <param name="count">Number of columns (must be >= 1).</param>
        public static Panel[] GenerateColumns(UISystem ui, Panel parent, int count)
        {
            if (count < 1) return new Panel[0];
            var widthPct = 100f / count;
            var ret = new Panel[count];
            for (var i = 0; i < count; i++)
            {
                var col = new Panel(ui, ui.DefaultStylesheets.Panels)
                {
                    Anchor = Anchor.AutoInlineLTR,
                };
                col.Size.X.SetPercents(widthPct);
                col.AutoHeight = true;
                parent.AddChild(col);
                ret[i] = col;
            }
            return ret;
        }
    }
}
