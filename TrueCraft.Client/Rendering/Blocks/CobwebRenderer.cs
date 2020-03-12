using Microsoft.Xna.Framework;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Client.Rendering.Blocks
{
    public class CobwebRenderer : FlatQuadRenderer
    {
        static CobwebRenderer()
        {
            RegisterRenderer(CobwebBlock.BlockID, new CobwebRenderer());
        }

        protected override Vector2 TextureMap => new Vector2(11, 0);
    }
}