using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TrueCraft.API;
using TrueCraft.Client.Input;
using TrueCraft.Client.Rendering;

namespace TrueCraft.Client.Modules
{
    public class DebugInfoModule : InputModule, IGraphicalModule
    {
        public DebugInfoModule(TrueCraftGame game, FontRenderer font)
        {
            Game = game;
            Font = font;
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
#if DEBUG
            Enabled = true;
#endif
        }

        public bool Chunks { get; set; }

        private TrueCraftGame Game { get; }
        private FontRenderer Font { get; }
        private SpriteBatch SpriteBatch { get; }
        private bool Enabled { get; set; }

        private int _lastFps = int.MinValue;
        private string _fpsLine;
        private double _lastPosX = double.NaN, _lastPosY = double.NaN, _lastPosZ = double.NaN;
        private string _positionLine;
        private object _lastHighlightedBlock;
        private BlockFace _lastHighlightedFace = (BlockFace) (-1);
        private string _highlightLine;
        private int _lastPendingChunks = int.MinValue;
        private string _pendingChunksLine;

        public void Draw(GameTime gameTime)
        {
            if (!Enabled)
                return;

            var fps = (int) (1 / gameTime.ElapsedGameTime.TotalSeconds) + 1;
            if (fps != _lastFps)
            {
                _fpsLine = ChatFormat.Bold + "Running at " + GetFPSColor(fps) + fps + " FPS";
                _lastFps = fps;
            }

            var pos = Game.Client.Position;
            if (pos.X != _lastPosX || pos.Y != _lastPosY || pos.Z != _lastPosZ)
            {
                _positionLine = $"Standing at <{pos.X:N2}, {pos.Y:N2}, {pos.Z:N2}>";
                _lastPosX = pos.X;
                _lastPosY = pos.Y;
                _lastPosZ = pos.Z;
            }

            var highlightedBlock = (object) Game.HighlightedBlock;
            if (!Equals(highlightedBlock, _lastHighlightedBlock) ||
                Game.HighlightedBlockFace != _lastHighlightedFace)
            {
                _highlightLine = ChatColor.Gray + "Looking at " + Game.HighlightedBlock +
                                 " (" + Enum.GetName(typeof(BlockFace), Game.HighlightedBlockFace) + ")";
                _lastHighlightedBlock = highlightedBlock;
                _lastHighlightedFace = Game.HighlightedBlockFace;
            }

            var pending = Game.ChunkModule.ChunkRenderer.PendingChunks;
            if (pending != _lastPendingChunks)
            {
                _pendingChunksLine = ChatColor.Gray + pending + " pending chunks";
                _lastPendingChunks = pending;
            }

            const int xOrigin = 10;
            const int yOrigin = 5;
            const int yOffset = 25;

            SpriteBatch.Begin();
            Font.DrawText(SpriteBatch, xOrigin, yOrigin, _fpsLine);
            Font.DrawText(SpriteBatch, xOrigin, yOrigin + yOffset * 1, _positionLine);
            Font.DrawText(SpriteBatch, xOrigin, yOrigin + yOffset * 2, _highlightLine);
            Font.DrawText(SpriteBatch, xOrigin, yOrigin + yOffset * 3, _pendingChunksLine);
            SpriteBatch.End();
        }

        public override bool KeyDown(GameTime gameTime, KeyboardKeyEventArgs e)
        {
            if (InputBindings.Matches(InputAction.ToggleDebugOverlay, e.Key))
                return true;
            return false;
        }

        public override bool KeyUp(GameTime gameTime, KeyboardKeyEventArgs e)
        {
            if (InputBindings.Matches(InputAction.ToggleDebugOverlay, e.Key))
            {
                Enabled = !Enabled;
                return true;
            }
            return false;
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