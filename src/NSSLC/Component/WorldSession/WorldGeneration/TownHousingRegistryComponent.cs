using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

// status: proposed
// evidenceStatus: partial; Version4 type-level key is confirmed, final key mode unresolved
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存居民与房间的索引及房间分配顺序。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.TownRoomManager。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/TownRoomManager.cs。</para>
/// <para>主要源成员：_roomLocationPairs（第 13 行）； _hasRoom（第 15 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 623 行。</para>
/// </remarks>
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
