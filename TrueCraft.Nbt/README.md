# TrueCraft.Nbt

A complete, spec-compliant Java-edition Minecraft NBT (Named Binary Tag) library
for .NET 9. Ported from [fNbt](https://github.com/fragmer/fNbt) with significant
additions: TAG_Long_Array (MC 1.12+), Java Modified UTF-8 string encoding, SNBT
parser and writer, nested-depth and allocation guards, and the nameless-root
network NBT framing introduced in Java protocol 1.20.2.

## Coverage at a glance

| Variant                                              | Status |
|------------------------------------------------------|--------|
| Disk format: GZip / ZLib / uncompressed              | ✅     |
| All 13 tag types (id 0..12 including TAG_Long_Array) | ✅     |
| Java Modified UTF-8 strings                          | ✅     |
| Standard UTF-8 (for network NBT)                     | ✅     |
| Big-endian (Java) and little-endian (e.g. Bedrock)   | ✅     |
| Nameless-root network NBT (Java 1.20.2+)             | ✅     |
| Forward-only `NbtReader` / `NbtWriter`               | ✅     |
| SNBT parse / write (`Snbt.SnbtParser` / `SnbtWriter`)| ✅     |
| Nesting-depth cap (default 512, configurable)        | ✅     |
| Allocation cap on array tags (default 16 M elements) | ✅     |
| NaN bit-pattern preservation                         | ✅     |
| Bedrock varint NBT                                   | ❌ out of scope |

## Quickstart

```csharp
using TrueCraft.Nbt;
using TrueCraft.Nbt.Tags;

// Load an existing NBT file (compression auto-detected).
var file = new NbtFile();
file.LoadFromFile("level.dat");

// Walk the tree.
var name = file.RootTag["Data"]["LevelName"].StringValue;
var seed = file.RootTag["Data"]["WorldGenSettings"]["seed"].LongValue;

// Build a tag tree and save it.
var root = new NbtCompound("Data") {
    new NbtString("LevelName", "My World"),
    new NbtInt("SpawnX", 0),
    new NbtInt("SpawnY", 64),
    new NbtInt("SpawnZ", 0),
    new NbtList("FavoriteBlocks", NbtTagType.String) {
        new NbtString("minecraft:stone"),
        new NbtString("minecraft:dirt"),
    },
};
new NbtFile(root).SaveToFile("out.dat", NbtCompression.GZip);
```

## SNBT (Stringified NBT)

Mojang's text format used in `/data` commands and datapacks.

```csharp
using TrueCraft.Nbt.Snbt;

// Parse SNBT into an NbtTag tree:
var tag = SnbtParser.Parse("{name:\"Steve\", level:50, inventory:[I; 1, 2, 3]}");

// Render any NbtTag tree back to SNBT:
var snbt = SnbtWriter.ToSnbt(tag);            // compact
var pretty = SnbtWriter.ToSnbt(tag, pretty: true);  // indented
```

### SNBT cheat sheet

| Construct        | Example                            |
|------------------|------------------------------------|
| Byte             | `42b` (also `42B`, `true`, `false`)|
| Short            | `42s`                              |
| Int              | `42`                               |
| Long             | `42L`                              |
| Float            | `1.5f`                             |
| Double           | `1.5d` or `1.5`                    |
| String           | `"text"` or `'text'`               |
| Compound         | `{key: value, ...}`                |
| List             | `[v, v, ...]` (homogeneous)        |
| Byte array       | `[B; 1b, 2b, 3b]`                  |
| Int array        | `[I; 1, 2, 3]`                     |
| Long array       | `[L; 1L, 2L, 3L]`                  |
| Quoted key       | `{"with spaces": 1}`               |

Trailing commas in compounds and lists are accepted. List element type is
inferred from the first element; mixing types is a parse error.

## API reference

### `NbtFile`

The high-level entry point. Reads and writes whole NBT files.

```csharp
var file = new NbtFile();

// Inputs (sync + async variants of each):
file.LoadFromFile(path, NbtCompression.AutoDetect);
file.LoadFromStream(stream, NbtCompression.AutoDetect);
file.LoadFromBuffer(bytes, offset, length, NbtCompression.AutoDetect);

// Outputs:
file.SaveToFile(path, NbtCompression.GZip);
file.SaveToStream(stream, NbtCompression.None);
var bytes = file.SaveToBuffer(NbtCompression.ZLib);
```

Configurable properties:

| Property             | Default | Meaning                                                                                                   |
|----------------------|---------|-----------------------------------------------------------------------------------------------------------|
| `BigEndian`          | `true`  | Wire endianness. `true` for vanilla Java disk NBT; `false` for the Bedrock byte order.                    |
| `UseStandardUtf8`    | `false` | Use standard UTF-8 instead of Java Modified UTF-8. Pair with `RootHasName = false` for network NBT.       |
| `RootHasName`        | `true`  | `true` for disk format; `false` for the nameless-root network NBT (Java 1.20.2+).                         |
| `MaxDepth`           | `512`   | Cap on compound/list nesting depth during load. Set to 0 to disable.                                      |
| `MaxArrayElements`   | `16M`   | Cap on declared length of any array tag or list during load. Set to 0 to disable.                         |

### Tag types

All inherit from `NbtTag` and live in `TrueCraft.Nbt.Tags`:

| Class          | Id  | Spec name        |
|----------------|-----|------------------|
| `NbtByte`      | 1   | TAG_Byte         |
| `NbtShort`     | 2   | TAG_Short        |
| `NbtInt`       | 3   | TAG_Int          |
| `NbtLong`      | 4   | TAG_Long         |
| `NbtFloat`     | 5   | TAG_Float        |
| `NbtDouble`    | 6   | TAG_Double       |
| `NbtByteArray` | 7   | TAG_Byte_Array   |
| `NbtString`    | 8   | TAG_String       |
| `NbtList`      | 9   | TAG_List         |
| `NbtCompound`  | 10  | TAG_Compound     |
| `NbtIntArray`  | 11  | TAG_Int_Array    |
| `NbtLongArray` | 12  | TAG_Long_Array (MC 1.12+) |

`NbtTag` exposes convenience accessors `ByteValue`, `IntValue`, `LongValue`,
`StringValue`, `IntArrayValue`, `LongArrayValue`, etc., that throw
`InvalidCastException` if the underlying tag type is wrong.

### Streaming: `NbtReader` and `NbtWriter`

For loading or writing huge files without building the whole tree in memory.

```csharp
using var stream = File.OpenRead("region.nbt");
var reader = new NbtReader(stream);
while (reader.ReadToFollowing("LevelName")) {
    Console.WriteLine(reader.ReadValue());
}
```

`NbtReader` supports `ReadToFollowing`, `ReadToDescendant`, `ReadAsTag`,
`Skip` and `ReadValue` over the on-disk format. It does not load the tree.

`NbtWriter` mirrors the API for forward-only writes; useful for streaming
out large generated data.

## Edge cases worth knowing

- **Modified UTF-8.** Java's `DataInputStream.readUTF` uses MUTF-8: U+0000 is
  encoded as `0xC0 0x80`, and supplementary characters (most emoji) are encoded
  as two CESU-8 surrogate halves (six bytes) rather than as a single 4-byte
  UTF-8 sequence. TrueCraft.Nbt does this by default. A standard 4-byte UTF-8
  sequence in NBT bytes is rejected as malformed — Mojang's parser does the
  same. Flip `UseStandardUtf8 = true` for the Java 1.20.2+ network framing.

- **NaN preservation.** Float and double NaN bit patterns are written and read
  verbatim — `BitConverter.SingleToInt32Bits(read) == orig` even for signaling
  NaN payloads like `0x7FA00000`.

- **Empty list type.** Vanilla writes an empty `TAG_List` as element-type
  `TAG_End` followed by length 0. TrueCraft.Nbt matches this bit for bit when
  writing, and reads it back as `NbtList { ListType = End, Count = 0 }`.

- **Depth bombs.** Malicious files can declare hundreds of millions of nested
  compounds. The default `MaxDepth = 512` matches Mojang's cap; deeper input
  raises `NbtFormatException` long before stack overflow.

- **Array bombs.** Array length prefixes are signed `int32` on the wire, so a
  malicious file can claim 16 GiB of `TAG_Long_Array` content.
  `MaxArrayElements` (default 16 M) rejects such input as
  `NbtFormatException` rather than `OutOfMemoryException`.

## Migrating from fNbt

| fNbt                  | TrueCraft.Nbt                     |
|-----------------------|-----------------------------------|
| `fNbt.NbtFile`        | `TrueCraft.Nbt.NbtFile`           |
| `fNbt.Tags.NbtCompound` etc. | `TrueCraft.Nbt.Tags.NbtCompound` etc. |
| `fNbt.NbtCompression` | `TrueCraft.Nbt.NbtCompression`    |
| (none)                | `TrueCraft.Nbt.Tags.NbtLongArray` |
| (none)                | `TrueCraft.Nbt.Snbt.SnbtParser`   |
| (none)                | `TrueCraft.Nbt.Snbt.SnbtWriter`   |
| `fNbt.Serialization.NbtConvert` | removed — see git history if you need the reflection-based serialiser |

## Spec sources

The implementation tracks:

- [minecraft.wiki/w/NBT_format](https://minecraft.wiki/w/NBT_format) (the
  canonical living spec).
- [minecraft.wiki/w/NBT_format/SNBT](https://minecraft.wiki/w/NBT_format/SNBT)
  for stringified NBT.
- Notch's 2011 spec for the original 0..10 tag IDs.
- Java's
  [`DataInput`](https://docs.oracle.com/en/java/javase/17/docs/api/java.base/java/io/DataInput.html#modified-utf-8)
  Modified UTF-8 contract.

## Tests

The companion `Test.TrueCraft.Nbt/` project runs 217 xUnit cases covering every
tag type, all compression modes, both endiannesses, Modified UTF-8 bit-level
encoding, depth and allocation guards, SNBT round-trip, NaN preservation, the
empty-list TAG_End shape, and malformed-input rejection. Run with:

```bash
dotnet test Test.TrueCraft.Nbt
```

## License

BSD-3-Clause, inherited from upstream fNbt by Matvei Stefarov. See the project
root `LICENSE`.
