# Legacy World-Format Source Manifest

## Purpose

The files in this directory are byte-for-byte legacy source evidence. They are
excluded from compilation and are not a runtime dependency of the net10 parser.
The independent parser must reproduce only the documented binary-format behavior.

## Recorded Sources

| File | Absolute origin | SHA-256 | Original namespace and type | Selected format evidence |
| --- | --- | --- | --- | --- |
| `WorldFile.cs` | `D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFile.cs` | `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289` | `Terraria.IO.WorldFile` | `LoadWorld`, `LoadWorld_Version2`, `LoadFileFormatHeader`, `LoadHeader`, `LoadWorldTiles`, `LoadChests`, `LoadSigns`, `LoadNPCs`, `LoadFooter`, `LoadTileEntities`, `LoadWorld_Version1_Old_BeforeRelease88` |
| `Tile.cs` | `D:\TRbackup\无任何删减通过编译\Terraria\Tile.cs` | `65FF4F662640D1BBD2FD994631E3B71CA5C15D1AF9785EC3E6402F4B04F86595` | `Terraria.Tile` | Legacy tile field representation used to identify serialized tile state and RLE effects. |
| `Sign.cs` | `D:\TRbackup\无任何删减通过编译\Terraria\Sign.cs` | `AA8B1D36046BF06E1D0ACCF21B75867A591BFE2D02AF3CEA9C58425796A1947A` | `Terraria.Sign` | Legacy sign coordinates and text representation used by the sign-section reader. |

## Runtime Boundary

`Terraria.WorldFile.V319.csproj` removes `LegacyReference\**\*.cs` from `Compile`
and adds it as `None`. No project may reference a legacy Terraria assembly, this
source evidence, or a compiled legacy type at runtime. The evidence is retained
solely for implementation review and for recorded differential-oracle work.

## Differential Oracle

`Test/Terraria.WorldFile.V319.Verification/Oracle/RecordedLegacyOracleSnapshots.json`
is a normalized fixture-oracle artifact tied to the `WorldFile.cs` hash above. It
is copied only by the verification project; the parser project has no build or runtime
dependency on the artifact or the verification project.
