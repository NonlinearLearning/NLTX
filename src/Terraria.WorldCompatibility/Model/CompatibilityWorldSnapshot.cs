using System;
using System.Collections.Generic;
using Terraria.WorldFile.V319.Format;

namespace Terraria.WorldCompatibility.Model;

public sealed class CompatibilityWorldSnapshot
{
  public CompatibilityWorldSnapshot(
    int version,
    WldFormatVersion formatVersion,
    CompatibilityWorldMetadata metadata,
    IReadOnlyList<CompatibilityTile> tiles,
    IReadOnlyList<CompatibilityChestSnapshot> chests,
    IReadOnlyList<CompatibilitySignSnapshot> signs,
    IReadOnlyList<CompatibilityNpcSnapshot> npcs,
    IReadOnlyList<CompatibilityTileEntitySnapshot> tileEntities,
    CompatibilityLoadReport loadReport)
  {
    Version = version;
    FormatVersion = formatVersion;
    Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    Tiles = new CompatibilityReadOnlyList<CompatibilityTile>(tiles ??
      throw new ArgumentNullException(nameof(tiles)));
    Chests = new CompatibilityReadOnlyList<CompatibilityChestSnapshot>(chests ??
      throw new ArgumentNullException(nameof(chests)));
    Signs = new CompatibilityReadOnlyList<CompatibilitySignSnapshot>(signs ??
      throw new ArgumentNullException(nameof(signs)));
    Npcs = new CompatibilityReadOnlyList<CompatibilityNpcSnapshot>(npcs ??
      throw new ArgumentNullException(nameof(npcs)));
    TileEntities = new CompatibilityReadOnlyList<CompatibilityTileEntitySnapshot>(tileEntities ??
      throw new ArgumentNullException(nameof(tileEntities)));
    LoadReport = loadReport ?? throw new ArgumentNullException(nameof(loadReport));
  }

  public IReadOnlyList<CompatibilityChestSnapshot> Chests { get; }

  public WldFormatVersion FormatVersion { get; }

  public CompatibilityLoadReport LoadReport { get; }

  public CompatibilityWorldMetadata Metadata { get; }

  public IReadOnlyList<CompatibilityNpcSnapshot> Npcs { get; }

  public IReadOnlyList<CompatibilitySignSnapshot> Signs { get; }

  public IReadOnlyList<CompatibilityTileEntitySnapshot> TileEntities { get; }

  public IReadOnlyList<CompatibilityTile> Tiles { get; }

  public int Version { get; }
}
