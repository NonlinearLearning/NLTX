using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

// status: proposed
// evidenceStatus: partial; Version4 type-level key is confirmed, final key mode unresolved
// crossSubsystemOwner: integration-review
public sealed class TownHousingRegistryComponent
{
  private readonly List<TownHousingResidentKey> _roomAssignmentOrder = new();
  private ulong _revision;
  private TownHousingKeyMode _residentKeyMode = TownHousingKeyMode.Unresolved;

  internal object SyncRoot { get; } = new();

  internal Dictionary<TownHousingResidentKey, TilePosition> RoomsByResidentKey { get; } = new();

  internal HashSet<TownHousingResidentKey> HomelessResidentKeys { get; } = new();

  public ulong Revision
  {
    get
    {
      lock (SyncRoot)
      {
        return _revision;
      }
    }
  }

  public TownHousingKeyMode ResidentKeyMode
  {
    get
    {
      lock (SyncRoot)
      {
        return _residentKeyMode;
      }
    }
  }

  internal List<TownHousingResidentKey> RoomAssignmentOrder => _roomAssignmentOrder;

  public int AssignedRoomCount
  {
    get
    {
      lock (SyncRoot)
      {
        return RoomsByResidentKey.Count;
      }
    }
  }

  public int HomelessResidentCount
  {
    get
    {
      lock (SyncRoot)
      {
        return HomelessResidentKeys.Count;
      }
    }
  }

  public bool IsEmpty
  {
    get
    {
      lock (SyncRoot)
      {
        return RoomsByResidentKey.Count == 0 && HomelessResidentKeys.Count == 0;
      }
    }
  }

  internal void SetResidentKeyMode(TownHousingKeyMode mode)
  {
    _residentKeyMode = mode;
  }

  internal void EnsureRevisionCanAdvance()
  {
    _ = checked(_revision + 1);
  }

  internal void AdvanceRevision()
  {
    _revision = checked(_revision + 1);
  }
}
