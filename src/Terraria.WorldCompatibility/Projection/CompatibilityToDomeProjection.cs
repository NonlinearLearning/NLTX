using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.WorldCompatibility.Model;

namespace Terraria.WorldCompatibility.Projection;

public static class CompatibilityToDomeProjection
{
  public static DomeSimulationSnapshot Project(
    CompatibilityWorldSnapshot snapshot,
    WorldSeed seed,
    bool strictImport = true)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (strictImport && HasRequiredUnsupportedRecords(snapshot.LoadReport))
    {
      throw new InvalidOperationException(
        "Strict compatibility import rejected unsupported world state.");
    }

    WorldMetadata metadata = new(
      snapshot.Metadata.Name,
      seed,
      snapshot.Metadata.Width,
      snapshot.Metadata.Height,
      snapshot.Metadata.WorldId,
      snapshot.Metadata.SpawnX,
      snapshot.Metadata.SpawnY);
    WorldGrid world = new(metadata.Width, metadata.Height);
    for (int index = 0; index < snapshot.Tiles.Count; index++)
    {
      CompatibilityTile source = snapshot.Tiles[index];
      if (!world.TrySetTile(source.X, source.Y, new WorldTile(
        IsActive: source.IsActive,
        Type: source.TileType,
        LiquidAmount: source.LiquidAmount,
        LiquidType: source.LiquidKind,
        FrameX: source.FrameX,
        FrameY: source.FrameY,
        WallType: source.WallType,
        HasWire: source.HasWire,
        HasWire2: source.HasWire2,
        HasWire3: source.HasWire3,
        HasWire4: source.HasWire4,
        IsHalfBrick: source.IsHalfBrick,
        Slope: source.Slope,
        IsActuated: source.IsActuated,
        IsInactive: source.IsInactive,
        TileColor: source.TileColor,
        WallColor: source.WallColor,
        IsInvisibleBlock: source.IsInvisibleBlock,
        IsInvisibleWall: source.IsInvisibleWall,
        IsFullbrightBlock: source.IsFullbrightBlock,
        IsFullbrightWall: source.IsFullbrightWall)))
      {
        throw new InvalidOperationException("A compatibility tile was outside the Dome world.");
      }
    }

    WorldGridSnapshot worldSnapshot = world.CreateSnapshot(metadata);
    List<NpcReplicationSnapshot> npcs = new(snapshot.Npcs.Count);
    for (int index = 0; index < snapshot.Npcs.Count; index++)
    {
      CompatibilityNpcSnapshot source = snapshot.Npcs[index];
      npcs.Add(new NpcReplicationSnapshot(
        index + 1,
        source.Type,
        new SimulationVector(source.PositionX, source.PositionY),
        new SimulationVector(0.0f, 0.0f),
        100,
        true,
        1,
        GetSection(source.PositionX, source.PositionY)));
    }

    List<ChestPersistentState> chests = new(snapshot.Chests.Count);
    for (int index = 0; index < snapshot.Chests.Count; index++)
    {
      CompatibilityChestSnapshot source = snapshot.Chests[index];
      ItemStack[] slots = new ItemStack[ChestComponent.SlotCount];
      int copyCount = Math.Min(source.Items.Count, slots.Length);
      for (int slot = 0; slot < copyCount; slot++)
      {
        CompatibilityChestItem item = source.Items[slot];
        if (item.Stack > 0 && item.NetId > 0 && item.NetId <= ushort.MaxValue)
        {
          slots[slot] = new ItemStack((ushort)item.NetId, item.Stack);
        }
      }

      if (source.Items.Count > slots.Length && strictImport)
      {
        throw new InvalidOperationException("Strict compatibility import rejected oversized chest slots.");
      }

      chests.Add(new ChestPersistentState(index + 1, source.X, source.Y, slots, 1, source.Name));
    }

    List<SignPersistentState> signs = new(snapshot.Signs.Count);
    for (int index = 0; index < snapshot.Signs.Count; index++)
    {
      CompatibilitySignSnapshot source = snapshot.Signs[index];
      signs.Add(new SignPersistentState(index, source.X, source.Y, source.Text, 1));
    }

    List<TileEntityPersistentState> tileEntities = new(snapshot.TileEntities.Count);
    for (int index = 0; index < snapshot.TileEntities.Count; index++)
    {
      CompatibilityTileEntitySnapshot source = snapshot.TileEntities[index];
      tileEntities.Add(new TileEntityPersistentState(
        source.Id,
        source.Type,
        source.X,
        source.Y,
        source.Payload,
        source.IsOpaque));
    }

    List<OpaqueCompatibilityRecord> opaqueRecords = new(
      snapshot.LoadReport.UnsupportedRecords.Count);
    foreach (CompatibilityUnsupportedRecord source in snapshot.LoadReport.UnsupportedRecords)
    {
      opaqueRecords.Add(new OpaqueCompatibilityRecord(
        source.SectionIndex,
        source.Offset,
        source.Reason,
        source.Type,
        source.X,
        source.Y,
        source.Payload,
        source.IsRequired));
    }

    return new DomeSimulationSnapshot(
      worldSnapshot,
      npcs,
      [],
      0,
      chests: chests,
      signs: signs,
      tileEntities: tileEntities,
      opaqueCompatibilityRecords: opaqueRecords);
  }

  private static WorldSectionCoordinates GetSection(float x, float y)
  {
    return new WorldSectionCoordinates(
      Math.Max(0, (int)x / WorldGrid.SectionWidth),
      Math.Max(0, (int)y / WorldGrid.SectionHeight));
  }

  private static bool HasRequiredUnsupportedRecords(CompatibilityLoadReport report)
  {
    foreach (CompatibilityUnsupportedRecord record in report.UnsupportedRecords)
    {
      if (record.IsRequired)
      {
        return true;
      }
    }

    return false;
  }
}
