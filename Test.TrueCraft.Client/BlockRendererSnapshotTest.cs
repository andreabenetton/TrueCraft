using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Client.Rendering;
using TrueCraft.Client.Rendering.Blocks;
using TrueCraft.Core.Logic;
using Xunit;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Test.TrueCraft.Client;

/// <summary>
///     Snapshot/parity guard for the BlockRenderer output. For each registered block
///     renderer we capture a SHA-256 over the byte stream of the produced
///     <see cref="VertexPositionNormalColorTexture"/> array followed by the
///     <c>int[]</c> indices, with a fixed BlockDescriptor and offset.
///
///     Regenerating the hashes: set <c>SnapshotDump</c> to true, run the tests,
///     paste the printed dictionary back into <see cref="ExpectedHashes"/>,
///     set <c>SnapshotDump</c> back to false.
/// </summary>
public class BlockRendererSnapshotTest
{
    private const bool SnapshotDump = false;

    // Renderers that share a renderer instance across multiple block IDs are
    // included once per ID; geometry differs by metadata where applicable.
    private static readonly (string Name, byte BlockID, byte Metadata)[] Renderers =
    {
        ("Cactus", 0x51, 0),
        ("Cobweb", 0x1E, 0),
        ("CraftingTable", 0x3A, 0),
        ("Farmland (dry)", 0x3C, 0),
        ("Farmland (moist)", 0x3C, 1),
        ("Grass", 0x02, 0),
        ("Ladder (north)", 0x41, 2),
        ("Ladder (south)", 0x41, 3),
        ("Ladder (east)", 0x41, 4),
        ("Ladder (west)", 0x41, 5),
        ("Leaves (oak)", 0x12, 0),
        ("Leaves (spruce)", 0x12, 1),
        ("Leaves (birch)", 0x12, 2),
        ("Log (oak)", 0x11, 0),
        ("Log (spruce)", 0x11, 1),
        ("Log (birch)", 0x11, 2),
        ("Slab (stone)", 0x2C, 0),
        ("Slab (sandstone)", 0x2C, 1),
        ("Slab (wooden)", 0x2C, 2),
        ("Slab (cobblestone)", 0x2C, 3),
        ("DoubleSlab", 0x2B, 0),
        ("Snow", 0x4E, 0),
        ("Sugarcane", 0x53, 0),
        ("TNT", 0x2E, 0),
        ("Torch (ground)", 0x32, 5),
        ("Torch (north)", 0x32, 4),
        ("Torch (south)", 0x32, 3),
        ("Torch (east)", 0x32, 1),
        ("Torch (west)", 0x32, 2),
        ("Dandelion", 0x25, 0),
        ("Rose", 0x26, 0),
        ("TallGrass", 0x1F, 1),
        ("DeadBush", 0x20, 0),
        ("Sapling (oak)", 0x06, 0),
        ("Sapling (spruce)", 0x06, 1),
        ("Sapling (birch)", 0x06, 2),
        ("Water", 0x08, 0),
        ("StationaryWater", 0x09, 0),
        ("Wheat (stage 0)", 0x3B, 0),
        ("Wheat (stage 7)", 0x3B, 7),
        // Default renderer — pick a stone block that has no override.
        ("Default (stone)", 0x01, 0),
    };

    private static readonly IReadOnlyDictionary<string, string> ExpectedHashes = new Dictionary<string, string>
    {
        // Baked from a pre-refactor run (commit 0c1ae3d). To regenerate, flip
        // SnapshotDump to true, paste this block from the failure output, flip
        // back. Hashes are over the raw byte stream of vertices then indices.
        // Cactus / Wheat hashes changed in the per-block API refactor: the legacy
        // path allocated 5*6 / 4*2*6 vertex slots but only wrote 5*4 / 4*2*4, leaving
        // trailing zero-vertices in the GPU buffer that no index referenced. The new
        // path emits exactly the live vertex count.
        {"Cactus", "F80FE848971F245CAFB631D6EC30C1BE310DD0AA26D81FA75602E2D2EB95E659"},
        {"Cobweb", "2D3EAFFA12A95ECE4C41053DF7E74C4541A433613D5E5D300AF07CDE23BA4123"},
        {"CraftingTable", "A335DD4A86E0A3D6F93FC0587686556472BF570636FE272957CE0ACAC761C420"},
        {"Farmland (dry)", "AD559956ACA51B1809C3DB44F0827E0D0331B9D635C2F59AD393AC8C1332EB46"},
        {"Farmland (moist)", "AD559956ACA51B1809C3DB44F0827E0D0331B9D635C2F59AD393AC8C1332EB46"},
        {"Grass", "397FE229AADAF15AD183395E8D51069A2799B2619FDF5592B82BA67C6856F8AE"},
        {"Ladder (north)", "D699A9D7BDFEB30B6FCFCAE2AD563F2B3937F83DD9CD66B1C35A2F706D13031A"},
        {"Ladder (south)", "DDC652D1A6E7D57E6A8422A186C11590E07A093C7B9F1221721A6A33208B09E5"},
        {"Ladder (east)", "2CD4D5FE8E0C9FA21280A0B8FD31DCF064D1A669434EA2BE38B96954C1BF0832"},
        {"Ladder (west)", "BDE1CABD33343EEAD5C1EF1951F759CF487C50B392B22F9AC92454C648402267"},
        {"Leaves (oak)", "006C3F289F3D6DD98ED838D38D3C5F7391961D3FF2AAE18B54A1BE9276676752"},
        {"Leaves (spruce)", "8C62001BEA256AA02B0F476B186FDF1E0154E1B6AAC22DCA29ABDF3A0F9D9EAD"},
        {"Leaves (birch)", "006C3F289F3D6DD98ED838D38D3C5F7391961D3FF2AAE18B54A1BE9276676752"},
        {"Log (oak)", "99EAC80681A81F2A64F5344A26828D90D917E3A0D1E09A5E445C714286563184"},
        {"Log (spruce)", "48BEB36D11620DC6E84F6D07BACACC7FA9A0061C3109BC0F90DE3493EA78E2D8"},
        {"Log (birch)", "1BBC31C60B07B2FC22DF81A1E8A660A0C90B0F9211951BEC5B24F59B6DA8CD32"},
        {"Slab (stone)", "ADB2DFBC29697FCA26AD93B9D5FA819DABDA0FCF46161B09286A7CA1DD7051A8"},
        {"Slab (sandstone)", "F9957326A109CDF3253DEE8368D65C0F08C17DD5D8095B742774C859C6F66607"},
        {"Slab (wooden)", "9009A2C5E7B4899BA35A53F7B99810A86638BC04E670C178FDB4AF52D043B9A9"},
        {"Slab (cobblestone)", "09E128B80469AC03DCE48F77A2106FBEAF2EBA75271264F336311666B52963EA"},
        {"DoubleSlab", "137ADC952594BF9E98A6BF8FD7D6E99ED122C16A66A5DB47041C12AABD7158FC"},
        {"Snow", "FBB4AE208F83CD4864F16521A32ECC61726C15617F4BA530E24DB5B116CF2BD9"},
        {"Sugarcane", "74D0544C4E8613B343135156D7FBF4A041A678990F7E54CC0D5942575798514C"},
        {"TNT", "DC58FF9CD2F3AB4CEF4D73FDBDC087FC4A80B49072B3054120D0B87D7272EFFF"},
        {"Torch (ground)", "D1ED097B23940F729B79B71E1F423586671196DDAFF263A938C1CA346B815F8B"},
        {"Torch (north)", "2D90414AD63B2440809AB4E41B7D6C7C1B50638379B6D2DDE1FC92EF74E51B57"},
        {"Torch (south)", "3FA02215897B070AC97E9D3BE60435552520DEFA8646D09F45195B44E9E9E130"},
        {"Torch (east)", "12ED1D53327CA6D00DAF5FC3765E4D54AC6220B36DE796D5B9D8FC1640348C69"},
        {"Torch (west)", "8495C8992FE7A1F90495338E323FC6F81FCDD374C00B9E232A18180D7E870C4A"},
        {"Dandelion", "D25AA67B21CF549A33F4568290E86A2AD367993F73841684ABECC311A127E609"},
        {"Rose", "A9751FC516C842101CBD7A6872DF63E8B8A9A3342D5B6C9813503A7AA0DFA0ED"},
        {"TallGrass", "35C683B98FE8C2510A3EFB1481B7E6997B8E38EB779F1C6830B14C5BC3E72F19"},
        {"DeadBush", "090F0457BE5E762DC43FC79D56B3F74F7B063BC2B7714828A808D5CF5248CF6C"},
        {"Sapling (oak)", "F52B50C00A3503564ADE3E205E54262DBFCC011ADC6332629981E818E553EBB2"},
        {"Sapling (spruce)", "6A5777A64394FD968835A8D3049457C1C6A1F442D61B4668A9368E0A65D4B4F3"},
        {"Sapling (birch)", "B35553BCDF52CAC26F41857027FB0EA80A50C05EBE3FA1ED129345D9A54BB13C"},
        {"Water", "0FDE0C756F7233AC2FE512134013D1F72B13DFABD53040D68E61C38C03CD3F44"},
        {"StationaryWater", "0FDE0C756F7233AC2FE512134013D1F72B13DFABD53040D68E61C38C03CD3F44"},
        {"Wheat (stage 0)", "11CEC24E7AA503259D223CC7CA157D2120EEC2026412EB707546939D7740921F"},
        {"Wheat (stage 7)", "F8E475581E91AF57E561F19EC359791E8AAA09F34640B1F6FD657B57C85B6D96"},
        {"Default (stone)", "FAAFF20702FFB6918284A18456F37183110BD0FD70A6EC5662C2B422CBE56C9A"},
    };

    private static readonly Lazy<BlockRepository> Repository = new Lazy<BlockRepository>(() =>
    {
        // Touch a BlockRenderer subclass so the BlockRenderer static ctor (which uses
        // reflection over AppDomain assemblies) fires while we still have those assemblies
        // referenced.
        _ = typeof(CactusRenderer);
        var repo = new BlockRepository();
        repo.DiscoverBlockProviders();
        return repo;
    });

    [Fact]
    public void AllRenderersProduceStableSnapshots()
    {
        var actual = new Dictionary<string, string>();
        foreach (var (name, blockID, metadata) in Renderers)
        {
            var provider = Repository.Value.GetBlockProvider(blockID);
            Assert.NotNull(provider);

            var descriptor = new BlockDescriptor
            {
                ID = blockID,
                Metadata = metadata,
                // BlockLight / SkyLight default to 0; Chunk null -> GetLight returns 15.
                Coordinates = new Coordinates3D(0, 64, 0),
            };

            var verts = BlockRenderer.RenderBlock(
                provider, descriptor, VisibleFaces.All,
                new Vector3(0, 0, 0), 0, out var idx);

            actual[name] = Hash(verts, idx);
        }

#pragma warning disable CS0162 // SnapshotDump is a compile-time const flipped manually to regenerate the embedded hash table.
        if (SnapshotDump)
        {
            var sb = new StringBuilder();
            foreach (var (name, _, _) in Renderers)
                sb.AppendLine($"            {{\"{name}\", \"{actual[name]}\"}},");
            Assert.Fail("Snapshot dump:\n" + sb);
        }
#pragma warning restore CS0162

        foreach (var (name, _, _) in Renderers)
        {
            Assert.True(ExpectedHashes.TryGetValue(name, out var expected),
                $"No expected hash for '{name}'");
            Assert.True(expected != "PLACEHOLDER",
                $"Expected hash for '{name}' has not been baked in yet — run with SnapshotDump=true to capture, then bake.");
            Assert.Equal(expected, actual[name]);
        }
    }

    private static string Hash(VertexPositionNormalColorTexture[] verts, int[] indices)
    {
        using var sha = SHA256.Create();
        using var ms = new MemoryStream();
        ms.Write(MemoryMarshal.AsBytes(verts.AsSpan()));
        ms.Write(MemoryMarshal.AsBytes(indices.AsSpan()));
        ms.Position = 0;
        return Convert.ToHexString(sha.ComputeHash(ms));
    }
}
