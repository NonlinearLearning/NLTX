using System;
using System.Collections.Generic;
using Terraria.WorldCompatibility.Model;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldCompatibility.Projection;

public static class WldToCompatibilityProjection
{
  public static CompatibilityWorldSnapshot Project(LegacyWorldDocument document)
  {
    ArgumentNullException.ThrowIfNull(document);

    List<CompatibilityTile> tiles = new(document.Tiles.Count);
    List<CompatibilityUnsupportedRecord> unsupportedRecords = [];
    int height = document.Metadata.Height;
    for (int index = 0; index < document.Tiles.Count; index++)
    {
      LegacyTile source = document.Tiles[index];
      int x = height > 0 ? index / height : index;
      int y = height > 0 ? index % height : 0;
      CompatibilityTile tile = new(
        x,
        y,
        source.IsActive,
        source.TileType,
        source.WallType,
        source.LiquidAmount,
        source.LiquidKind,
        source.HasWire,
        source.HasWire2,
        source.HasWire3,
        source.HasWire4,
        source.IsHalfBrick,
        source.Slope,
        source.IsActuated,
        source.IsInactive,
        source.FrameX,
        source.FrameY,
        source.TileColor,
        source.WallColor,
        source.IsInvisibleBlock,
        source.IsInvisibleWall,
        source.IsFullbrightBlock,
        source.IsFullbrightWall);
      tiles.Add(tile);

      if (source.IsInvisibleBlock || source.IsInvisibleWall ||
          source.IsFullbrightBlock || source.IsFullbrightWall)
      {
        unsupportedRecords.Add(new CompatibilityUnsupportedRecord(
          1,
          index,
          "Tile visibility or fullbright state has no Dome representation.",
          x: x,
          y: y));
      }
    }

    List<CompatibilityChestSnapshot> chests = new(document.Chests.Count);
    foreach (LegacyChest chest in document.Chests)
    {
      chests.Add(CompatibilityChestSnapshot.From(chest));
    }

    List<CompatibilitySignSnapshot> signs = new(document.Signs.Count);
    foreach (LegacySign sign in document.Signs)
    {
      signs.Add(CompatibilitySignSnapshot.From(sign));
    }

    List<CompatibilityNpcSnapshot> npcs = new(document.Npcs.Count);
    foreach (LegacyNpc npc in document.Npcs)
    {
      npcs.Add(CompatibilityNpcSnapshot.From(npc));
    }

    List<CompatibilityTileEntitySnapshot> tileEntities =
      new(document.TileEntities.Count);
    foreach (LegacyTileEntity entity in document.TileEntities)
    {
      tileEntities.Add(CompatibilityTileEntitySnapshot.From(entity));
      if (entity.IsOpaque)
      {
        unsupportedRecords.Add(new CompatibilityUnsupportedRecord(
          5,
          entity.Id,
          "Opaque tile entity payload cannot be executed by Dome.",
          entity.Type,
          entity.X,
          entity.Y,
          entity.Payload,
          isRequired: true));
      }
    }

    foreach (LegacySectionDiagnostic diagnostic in document.Diagnostics)
    {
      unsupportedRecords.Add(new CompatibilityUnsupportedRecord(
        diagnostic.SectionIndex,
        diagnostic.Offset,
        diagnostic.Message,
        payload: diagnostic.Payload));
    }

    return new CompatibilityWorldSnapshot(
      document.Version,
      document.FormatVersion,
      CompatibilityWorldMetadata.From(document.Metadata),
      tiles,
      chests,
      signs,
      npcs,
      tileEntities,
      new CompatibilityLoadReport(unsupportedRecords));
  }
}
