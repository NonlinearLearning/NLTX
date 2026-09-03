using System;
using System.Collections.Generic;
using Terraria.WorldFile.V319.Format;

namespace Terraria.WorldFile.V319.Model;

public sealed class LegacyWorldDocument
{
  public LegacyWorldDocument(
    int version,
    WldFormatVersion formatVersion,
    LegacyWorldMetadata metadata,
    IReadOnlyList<LegacyTile> tiles,
    IReadOnlyList<LegacyChest> chests,
    IReadOnlyList<LegacySign> signs,
    IReadOnlyList<LegacyNpc> npcs,
    IReadOnlyList<LegacyTileEntity> tileEntities,
    IReadOnlyList<LegacySectionDiagnostic> diagnostics)
  {
    Version = version;
    FormatVersion = formatVersion;
    Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    Tiles = Copy(tiles, nameof(tiles));
    Chests = Copy(chests, nameof(chests));
    Signs = Copy(signs, nameof(signs));
    Npcs = Copy(npcs, nameof(npcs));
    TileEntities = Copy(tileEntities, nameof(tileEntities));
    Diagnostics = Copy(diagnostics, nameof(diagnostics));
  }

  public IReadOnlyList<LegacyChest> Chests { get; }

  public IReadOnlyList<LegacySectionDiagnostic> Diagnostics { get; }

  public WldFormatVersion FormatVersion { get; }

  public LegacyWorldMetadata Metadata { get; }

  public IReadOnlyList<LegacyNpc> Npcs { get; }

  public IReadOnlyList<LegacySign> Signs { get; }

  public IReadOnlyList<LegacyTileEntity> TileEntities { get; }

  public IReadOnlyList<LegacyTile> Tiles { get; }

  public int Version { get; }

  private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> values, string parameterName)
  {
    if (values is null)
    {
      throw new ArgumentNullException(parameterName);
    }

    return new LegacyReadOnlyList<T>(values);
  }
}
