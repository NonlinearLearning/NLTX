using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

// status: proposed
// evidenceStatus: partial; Version4 type-level key is confirmed, final key mode unresolved
// crossSubsystemOwner: integration-review
public sealed class TownHousingRegistryComponent
{
  public Dictionary<TownHousingResidentKey, TilePosition> RoomsByResidentKey { get; } = new();

  public HashSet<TownHousingResidentKey> HomelessResidentKeys { get; } = new();

  public ulong Revision { get; private set; }

  public TownHousingKeyMode ResidentKeyMode { get; private set; } =
    TownHousingKeyMode.Unresolved;

  public int AssignedRoomCount => RoomsByResidentKey.Count;

  public int HomelessResidentCount => HomelessResidentKeys.Count;

  public bool IsEmpty =>
    RoomsByResidentKey.Count == 0 && HomelessResidentKeys.Count == 0;

  internal void SetResidentKeyMode(TownHousingKeyMode mode)
  {
    ResidentKeyMode = mode;
  }

  internal void AdvanceRevision()
  {
    Revision = checked(Revision + 1);
  }
}
