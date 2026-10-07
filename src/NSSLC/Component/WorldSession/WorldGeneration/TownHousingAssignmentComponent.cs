using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存住房扫描提交后的房间分配和无家居民集合。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.TownRoomManager。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/TownRoomManager.cs。</para>
/// <para>主要源成员：_roomLocationPairs（第 13 行）； _hasRoom（第 15 行）。</para>
/// <para>重组说明：扫描来源版本、GenerationId 和提交版本是住房结果交接新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 244 行。</para>
/// </remarks>
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
