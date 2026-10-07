using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Structures;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Read-only projection suitable for persistence or diagnostics.
/// It never writes back to the reservation owner.
/// </summary>
public sealed class WorldStructureReservationSnapshotProjection
{
  public readonly record struct Entry(
    string ReservationId,
    WorldGenerationRectangle RequestedBounds,
    WorldGenerationRectangle ReservedBounds,
    int Padding,
    bool IsProtected);

  private WorldStructureReservationSnapshotProjection(
    long generationId,
    IReadOnlyList<Entry> reservations)
  {
    GenerationId = generationId;
    Reservations = reservations;
  }

  public long GenerationId { get; }

  public IReadOnlyList<Entry> Reservations { get; }

  public int Count => Reservations.Count;

  public static WorldStructureReservationSnapshotProjection From(
    WorldStructureReservationSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);

    Entry[] entries = new Entry[snapshot.Reservations.Count];
    for (int index = 0; index < entries.Length; index++)
    {
      WorldStructureReservationRecord record = snapshot.Reservations[index];
      entries[index] = new Entry(
        record.Request.ReservationId,
        record.Request.Bounds,
        record.ReservedBounds,
        record.Request.Padding,
        record.Request.IsProtected);
    }

    return new WorldStructureReservationSnapshotProjection(
      snapshot.GenerationId,
      Array.AsReadOnly(entries));
  }
}
