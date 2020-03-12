using Microsoft.Xna.Framework;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks
{
    public class SugarcaneRenderer : FlatQuadRenderer
    {
        static SugarcaneRenderer()
        {
            RegisterRenderer(SugarcaneBlock.BlockID, new SugarcaneRenderer());
        }

        protected override Vector2 TextureMap => new Vector2(9, 4);
    }
}