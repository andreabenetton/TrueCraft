using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TrueCraft.Client.Input;
using TrueCraft.Client.Rendering;
using TrueCraft.API;
using System;

namespace TrueCraft.Client.Modules
{
    public class DebugInfoModule : InputModule, IGraphicalModule
    {
        public bool Chunks { get; set; }

        private TrueCraftGame Game { get; set; }
        private FontRenderer Font { get; set; }
        private SpriteBatch SpriteBatch { get; set; }
        private bool Enabled { get; set; }

        public DebugInfoModule(TrueCraftGame game, FontRenderer font)
        {
            Game = game;
            Font = font;
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
#if DEBUG
            Enabled = true;
#endif
        }

        public override bool KeyDown(GameTime gameTime, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.F3:
                    return true;
            }
            return false;
        }

        public override bool KeyUp(GameTime gameTime, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.F3:
                    Enabled = !Enabled;
                    return true;
            }
            return false;
        }
        
        public void Draw(GameTime gameTime)
        {
            if (!Enabled)
                return;

            var fps = (int)(1 / gameTime.ElapsedGameTime.TotalSeconds) + 1;

            const int xOrigin = 10;
            const int yOrigin = 5;
            const int yOffset = 25;

            SpriteBatch.Begin();
            Font.DrawText(SpriteBatch, xOrigin, yOrigin, string.Format(
                ChatFormat.Bold + "Running at {0}{1} FPS", GetFPSColor(fps), fps));

            Font.DrawText(SpriteBatch, xOrigin, yOrigin + (yOffset * 1),
                $"Standing at <{Game.Client.Position.X:N2}, {Game.Client.Position.Y:N2}, {Game.Client.Position.Z:N2}>");

            Font.DrawText(SpriteBatch, xOrigin, yOrigin + (yOffset * 2),
                ChatColor.Gray +
                $"Looking at {Game.HighlightedBlock} ({Enum.GetName(typeof(BlockFace), Game.HighlightedBlockFace)})");

            Font.DrawText(SpriteBatch, xOrigin, yOrigin + (yOffset * 3),
                ChatColor.Gray + 
                $"{Game.ChunkModule.ChunkRenderer.PendingChunks} pending chunks");

            SpriteBatch.End();
        }

        private string GetFPSColor(int fps)
        {
            if (fps <= 16)
                return ChatColor.Red;
            if (fps <= 32)
                return ChatColor.Yellow;
            return ChatColor.BrightGreen;
        }
    }
}
