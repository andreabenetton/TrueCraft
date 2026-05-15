using System.Collections.Generic;
using GeonBit.UI.Entities;
using GeonBit.UI.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TrueCraft.Launcher.Entities
{
    public class MenuButton : Button
    {
        private static readonly List<MenuButton> ButtonsList = new List<MenuButton>();

        private readonly string _buttonTexturePath = @"textures\gui\widgets";
        private static LauncherGame _game;

        public MenuButton(string text,
            Anchor anchor = Anchor.Auto,
            Vector2? size = null,
            Vector2? offset = null) : base(text, ButtonSkin.Default, anchor, size, offset)
        {
            ButtonsList.Add(this);
        }

        ~MenuButton()
        {
            ButtonsList.Remove(this);
        }

        public static void Initialize(LauncherGame game)
        {
            _game = game;
        }

        protected virtual void LoadTexture()
        {
            Texture2D tocropTexture = _game.Content.Load<Texture2D>(_buttonTexturePath);
            var defaultRectangle = new Rectangle(0, 66, 200, 20);
            var mouseHoverRectangle = new Rectangle(0, 86, 200, 20);
            var mouseDownRectangle = new Rectangle(0, 46, 200, 20);
            SetCustomSkin(tocropTexture.Crop(defaultRectangle), tocropTexture.Crop(mouseHoverRectangle),
                tocropTexture.Crop(mouseDownRectangle), null);
        }

        public static void LoadButtonsTexture()
        {
            foreach (MenuButton menuButton in ButtonsList)
            {
                menuButton.LoadTexture();
            }
        }
    }
}
