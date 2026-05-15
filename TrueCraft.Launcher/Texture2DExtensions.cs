using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeonBit.UI.Extensions
{
    /// <summary>
    ///     Runtime <see cref="Texture2D"/> helpers used by the launcher's themed widgets
    ///     (e.g. <c>MenuButton</c> slicing a 3-frame sprite atlas) and the
    ///     texture-pack thumbnail preview. Operates on existing in-memory textures —
    ///     no MonoGame Content Pipeline involvement.
    /// </summary>
    public static class Texture2DExtensions
    {
        /// <summary>
        ///     Returns a new <see cref="Texture2D"/> containing the pixels under
        ///     <paramref name="sourceRectangle"/> of <paramref name="originalTexture"/>.
        ///     The source texture is unchanged.
        /// </summary>
        public static Texture2D Crop(this Texture2D originalTexture, Rectangle sourceRectangle)
        {
            if (originalTexture == null)
                return null;

            var cropTexture = new Texture2D(originalTexture.GraphicsDevice,
                sourceRectangle.Width, sourceRectangle.Height);
            var data = new Color[sourceRectangle.Width * sourceRectangle.Height];
            originalTexture.GetData(0, sourceRectangle, data, 0, data.Length);
            cropTexture.SetData(data);
            return cropTexture;
        }

        /// <summary>
        ///     Returns a new <see cref="Texture2D"/> the same size as <paramref name="area"/>,
        ///     copying pixels from <paramref name="source"/> at the given rectangle. Used by
        ///     the launcher's UI when an entity's bitmap region needs to be extracted at a
        ///     specific size.
        /// </summary>
        public static Texture2D HorizontalResize(this Texture2D source, Rectangle area)
        {
            if (source == null)
                return null;

            var cropped = new Texture2D(source.GraphicsDevice, area.Width, area.Height);
            var data = new Color[source.Width * source.Height];
            var cropData = new Color[cropped.Width * cropped.Height];

            source.GetData(data);

            var index = 0;
            for (var y = area.Y; y < area.Y + area.Height; y++)
            for (var x = area.X; x < area.X + area.Width; x++)
                cropData[index++] = data[x + y * source.Width];

            cropped.SetData(cropData);
            return cropped;
        }
    }
}
