using System.Collections.Generic;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TrueCraft.Launcher.Panels
{
    public class BackgroundManagingPanel : Panel
    {
        private static readonly List<BackgroundManagingPanel> PanelList = new();

        protected Texture2D BckTexture2D;
        private LauncherGame _game;
        private int _textureWidth;
        private int _textureHeight;
        private string _backgroundTexturePath = @"textures\gui\options_background";
        private int _ratio = 4;

        public BackgroundManagingPanel(LauncherGame game)
        {
            _game = game;
            PanelList.Add(this);
        }

        ~BackgroundManagingPanel()
        {
            PanelList.Remove(this);
        }

        protected string BackgroundTexturePath
        {
            get => _backgroundTexturePath;
            set => _backgroundTexturePath = value;
        }

        public int Ratio
        {
            get => _ratio;
            set => _ratio = value;
        }

        protected virtual void LoadBackgroundTexture()
        {
            BckTexture2D = _game.Content.Load<Texture2D>(_backgroundTexturePath);
            _textureWidth = BckTexture2D.Width * _ratio;
            _textureHeight = BckTexture2D.Height * _ratio;
        }

        protected virtual void DrawBackground()
        {
            _game.Sprites.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            for (int i = 0; i < _game.ScreenWidth; i += _textureWidth)
            {
                for (int j = 0; j < _game.ScreenHeight; j += _textureHeight)
                {
                    _game.Sprites.Draw(BckTexture2D, new Rectangle(i, j, _textureWidth, _textureHeight), Color.White);
                }
            }
            _game.Sprites.End();
        }

        public static void LoadPanelsBackgroundTexture()
        {
            foreach (BackgroundManagingPanel backgroundManagingPanel in PanelList)
            {
                backgroundManagingPanel.LoadBackgroundTexture();
            }
        }

        public static void DrawPanelsBackground()
        {
            foreach (BackgroundManagingPanel backgroundManagingPanel in PanelList)
            {
                if (backgroundManagingPanel.IsVisible())
                {
                    backgroundManagingPanel.DrawBackground();
                }
            }
        }
    }
}
