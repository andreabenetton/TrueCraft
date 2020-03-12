using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TrueCraft.Client.Rendering
{
    /// <summary>
    ///     Represents a font.
    /// </summary>
    public class Font
    {
        private FontFile _definition;
        private Dictionary<char, FontChar> _glyphs;
        private Texture2D[] _textures;

        /// <summary>
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="contentManager"></param>
        /// <param name="name"></param>
        /// <param name="style"></param>
        public Font(GraphicsDevice graphicsDevice, ContentManager contentManager, string name,
            FontStyle style = FontStyle.Regular)
        {
            Name = name;
            Style = style;

            LoadContent(graphicsDevice, contentManager);
            GenerateGlyphs();
        }

        /// <summary>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        public FontStyle Style { get; }

        /// <summary>
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public Texture2D GetTexture(int page = 0)
        {
            return _textures[page];
        }

        /// <summary>
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public FontChar GetGlyph(char ch)
        {
            _glyphs.TryGetValue(ch, out var glyph);
            return glyph;
        }

        /// <summary>
        /// </summary>
        /// <param name="contentManager"></param>
        private void LoadContent(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            var definitionPath = $"{Name}_{Style}.fnt";
            using (var contents = File.OpenRead(Path.Combine(contentManager.RootDirectory, definitionPath)))
            {
                _definition = FontLoader.Load(contents);
            }

            if (_textures != null)
                foreach (var texture in _textures)
                    texture.Dispose();

            // We need to support multiple texture pages for more than plain ASCII text.
            _textures = new Texture2D[_definition.Pages.Count];
            for (var i = 0; i < _definition.Pages.Count; i++)
            {
                var texturePath = $"{Name}_{Style}_{i}.png";
                //_textures[i] = contentManager.Load<Texture2D>(texturePath);

                var fileStream = new FileStream(Path.Combine(contentManager.RootDirectory, texturePath), FileMode.Open);
                _textures[i] = Texture2D.FromStream(graphicsDevice, fileStream);
                fileStream.Dispose();
            }
        }

        /// <summary>
        /// </summary>
        private void GenerateGlyphs()
        {
            _glyphs = new Dictionary<char, FontChar>();
            foreach (var glyph in _definition.Chars)
            {
                var c = (char) glyph.ID;
                _glyphs.Add(c, glyph);
            }
        }
    }
}