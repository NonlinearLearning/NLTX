using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

public sealed class TownHousingAssignmentComponent
{
  public TownHousingAssignmentComponent(
    long generationId,
    IReadOnlyDictionary<PersistentEntityId, TilePosition>? assignedRooms = null,
    IReadOnlySet<PersistentEntityId>? homelessResidents = null,
    ulong revision = 0,
    ulong? sourceScanRevision = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    Dictionary<PersistentEntityId, TilePosition> roomCopy =
      assignedRooms is null
        ? []
        : new Dictionary<PersistentEntityId, TilePosition>(assignedRooms.Count);
    if (assignedRooms is not null)
    {
      foreach (KeyValuePair<PersistentEntityId, TilePosition> entry in assignedRooms)
      {
        if (!entry.Key.IsValid)
        {
          throw new ArgumentException(
            "Assigned residents must have a persistent identity.",
            nameof(assignedRooms));
        }

        roomCopy.Add(entry.Key, entry.Value);
      }
    }

    HashSet<PersistentEntityId> homelessCopy = homelessResidents is null
      ? []
      : new HashSet<PersistentEntityId>(homelessResidents);
    foreach (PersistentEntityId resident in homelessCopy)
    {
      if (!resident.IsValid)
      {
        throw new ArgumentException(
          "Homeless residents must have a persistent identity.",
          nameof(homelessResidents));
      }

      if (roomCopy.ContainsKey(resident))
      {
        throw new ArgumentException(
          "A resident cannot be assigned and homeless at the same time.",
          nameof(homelessResidents));
      }
    }

    GenerationId = generationId;
    AssignedRooms = new ReadOnlyDictionary<PersistentEntityId, TilePosition>(roomCopy);
    HomelessResidents = homelessCopy.ToFrozenSet();
    Revision = revision;
    SourceScanRevision = sourceScanRevision;
  }

  public long GenerationId { get; }

  public IReadOnlyDictionary<PersistentEntityId, TilePosition> AssignedRooms { get; }

  public IReadOnlySet<PersistentEntityId> HomelessResidents { get; }

  public ulong Revision { get; }

  public ulong? SourceScanRevision { get; }

  public int AssignedRoomCount => AssignedRooms.Count;

  public int HomelessResidentCount => HomelessResidents.Count;
}
