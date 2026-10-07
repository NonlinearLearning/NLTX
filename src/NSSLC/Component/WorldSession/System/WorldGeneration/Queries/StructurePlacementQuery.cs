using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Structures;

namespace Terraria.WorldGeneration.Queries;

public enum StructurePlacementRejectionReason : byte
{
  None,
  OutsideWorldBounds,
  ProtectedOverlap,
  InvalidActiveTile,
  MissingTileSnapshot,
}

public readonly record struct StructureTileSnapshot(
  bool Exists,
  bool IsActive,
  ushort TileType);

public interface IStructureTileSnapshotReader
{
  WorldGenerationRectangle WorldBounds { get; }

  bool TryRead(TilePosition position, out StructureTileSnapshot snapshot);
}

public readonly record struct StructurePlacementResult(
  bool CanPlace,
  StructurePlacementRejectionReason RejectionReason,
  string? Detail = null);

public static class StructurePlacementQuery
{
  public static StructurePlacementResult Evaluate(
    in WorldStructureReservationRequest request,
    IReadOnlyList<WorldStructureReservationRecord> existingReservations,
    IStructureTileSnapshotReader tileReader,
    IReadOnlySet<ushort> validTileTypes)
  {
    ArgumentNullException.ThrowIfNull(existingReservations);
    ArgumentNullException.ThrowIfNull(tileReader);
    ArgumentNullException.ThrowIfNull(validTileTypes);
    request.Validate();

    WorldGenerationRectangle reservedBounds;
    try
    {
      reservedBounds = request.GetReservedBounds();
    }
    catch (OverflowException)
    {
      return Reject(
        StructurePlacementRejectionReason.OutsideWorldBounds,
        "The padded reservation bounds overflow the coordinate range.");
    }

    if (!Contains(tileReader.WorldBounds, request.Bounds) ||
      !Contains(tileReader.WorldBounds, reservedBounds))
    {
      return Reject(
        StructurePlacementRejectionReason.OutsideWorldBounds,
        "The reservation or its padding is outside the world bounds.");
    }

    for (int index = 0; index < existingReservations.Count; index++)
    {
      WorldStructureReservationRecord existing = existingReservations[index];
      if (existing.Request.GenerationId == request.GenerationId &&
        existing.Request.IsProtected &&
        existing.ReservedBounds.Intersects(reservedBounds))
      {
        return Reject(
          StructurePlacementRejectionReason.ProtectedOverlap,
          "The padded reservation overlaps a protected reservation.");
      }
    }

    for (int x = reservedBounds.X; x < reservedBounds.Right; x++)
    {
      for (int y = reservedBounds.Y; y < reservedBounds.Bottom; y++)
      {
        TilePosition position = new(x, y);
        if (!tileReader.TryRead(position, out StructureTileSnapshot tile))
        {
          return Reject(
            StructurePlacementRejectionReason.MissingTileSnapshot,
            "The tile snapshot is incomplete for the reservation bounds.");
        }

        if (tile.Exists && tile.IsActive && !validTileTypes.Contains(tile.TileType))
        {
          return Reject(
            StructurePlacementRejectionReason.InvalidActiveTile,
            "An active tile is not included in the valid placement set.");
        }
      }
    }

    return new StructurePlacementResult(
      true,
      StructurePlacementRejectionReason.None);
  }

  private static bool Contains(
    WorldGenerationRectangle outer,
    WorldGenerationRectangle inner)
  {
    return (long)inner.X >= outer.X &&
      (long)inner.Y >= outer.Y &&
      (long)inner.X + inner.Width <= (long)outer.X + outer.Width &&
      (long)inner.Y + inner.Height <= (long)outer.Y + outer.Height;
  }

  private static StructurePlacementResult Reject(
    StructurePlacementRejectionReason reason,
    string detail)
  {
    return new StructurePlacementResult(false, reason, detail);
  }
}
