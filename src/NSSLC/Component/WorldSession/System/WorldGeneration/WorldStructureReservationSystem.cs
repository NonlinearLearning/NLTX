using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Structures;

namespace Terraria.WorldGeneration.Systems;

public sealed class WorldStructureReservationSystem
{
  private readonly object _sync = new();
  private readonly Dictionary<long, List<WorldStructureReservationRecord>>
    _reservationsByGeneration = [];

  public ReservationResult Reserve(
    in WorldStructureReservationRequest request,
    IStructureTileSnapshotReader tileReader,
    IReadOnlySet<ushort> validTileTypes)
  {
    ArgumentNullException.ThrowIfNull(tileReader);
    ArgumentNullException.ThrowIfNull(validTileTypes);
    request.Validate();

    lock (_sync)
    {
      List<WorldStructureReservationRecord> reservations =
        GetOrCreateReservations(request.GenerationId);
      for (int index = 0; index < reservations.Count; index++)
      {
        if (string.Equals(
          reservations[index].Request.ReservationId,
          request.ReservationId,
          StringComparison.Ordinal))
        {
          return new ReservationResult(
            false,
            request.ReservationId,
            "The reservation identity already exists in this generation.");
        }
      }

      StructurePlacementResult placement = StructurePlacementQuery.Evaluate(
        request,
        reservations,
        tileReader,
        validTileTypes);
      if (!placement.CanPlace)
      {
        return new ReservationResult(
          false,
          request.ReservationId,
          placement.Detail ?? placement.RejectionReason.ToString());
      }

      reservations.Add(
        new WorldStructureReservationRecord(
          request,
          request.GetReservedBounds()));
      return new ReservationResult(true, request.ReservationId);
    }
  }

  public StructurePlacementResult CanPlace(
    in WorldStructureReservationRequest request,
    IStructureTileSnapshotReader tileReader,
    IReadOnlySet<ushort> validTileTypes)
  {
    ArgumentNullException.ThrowIfNull(tileReader);
    ArgumentNullException.ThrowIfNull(validTileTypes);
    request.Validate();

    lock (_sync)
    {
      List<WorldStructureReservationRecord> reservations =
        _reservationsByGeneration.TryGetValue(
          request.GenerationId,
          out List<WorldStructureReservationRecord>? existing)
          ? existing
          : [];
      return StructurePlacementQuery.Evaluate(
        request,
        reservations,
        tileReader,
        validTileTypes);
    }
  }

  public WorldStructureReservationSnapshot CreateSnapshot(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    lock (_sync)
    {
      if (!_reservationsByGeneration.TryGetValue(
        generationId,
        out List<WorldStructureReservationRecord>? reservations))
      {
        return new WorldStructureReservationSnapshot(
          generationId,
          Array.Empty<WorldStructureReservationRecord>());
      }

      return new WorldStructureReservationSnapshot(
        generationId,
        new List<WorldStructureReservationRecord>(reservations).AsReadOnly());
    }
  }

  public int Reset(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    lock (_sync)
    {
      if (!_reservationsByGeneration.TryGetValue(
        generationId,
        out List<WorldStructureReservationRecord>? reservations))
      {
        return 0;
      }

      int releasedProtectedCount = 0;
      for (int index = 0; index < reservations.Count; index++)
      {
        WorldStructureReservationRecord reservation = reservations[index];
        if (!reservation.Request.IsProtected)
        {
          continue;
        }

        // StructureMap.Reset clears the protected index but retains the
        // ordinary structure entry. Preserve that lifetime split in the
        // generation-scoped snapshot instead of deleting the record.
        WorldStructureReservationRequest ordinaryRequest =
          reservation.Request with { IsProtected = false };
        reservations[index] = new WorldStructureReservationRecord(
          ordinaryRequest,
          reservation.ReservedBounds);
        releasedProtectedCount++;
      }

      return releasedProtectedCount;
    }
  }

  /// <summary>
  /// Drops every reservation owned by one generation for unload or abort.
  /// </summary>
  /// <remarks>
  /// This operation is deliberately separate from <see cref="Reset"/>:
  /// reset mirrors StructureMap's protected-index reset, while discard is a
  /// lifecycle boundary that must be requested by the generation owner.
  /// </remarks>
  public int DiscardGeneration(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    lock (_sync)
    {
      if (!_reservationsByGeneration.Remove(
        generationId,
        out List<WorldStructureReservationRecord>? reservations))
      {
        return 0;
      }

      return reservations.Count;
    }
  }

  private List<WorldStructureReservationRecord> GetOrCreateReservations(
    long generationId)
  {
    if (!_reservationsByGeneration.TryGetValue(
      generationId,
      out List<WorldStructureReservationRecord>? reservations))
    {
      reservations = [];
      _reservationsByGeneration.Add(generationId, reservations);
    }

    return reservations;
  }
}
