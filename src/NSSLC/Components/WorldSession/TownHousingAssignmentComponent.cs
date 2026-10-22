using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存按居民持久身份组织的住房分配只读结果。
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
/// <para>
/// 用途：住房分配完成后提交居民到房间的位置映射及无家居民集合，读取方按持久身份查询居住结果。
/// </para>
/// <para>
/// 当前实现：构造时复制输入，要求身份有效，并拒绝同一居民同时出现在已分配与无家集合。Revision 和 SourceScanRevision 分别记录分配版本与来源扫描
/// 版本；实际入住和持久化由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C184，L2（通用可选玩法）。分级理由：住房扫描、分配和注册是城镇居民玩法，不是世界加载本身。
/// </para>
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

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 居民持久身份到住房位置的只读映射，构造时复制输入。
  /// </summary>
  public IReadOnlyDictionary<PersistentEntityId, TilePosition> AssignedRooms { get; }

  /// <summary>
  /// 无家居民持久身份的冻结集合。
  /// </summary>
  public IReadOnlySet<PersistentEntityId> HomelessResidents { get; }

  /// <summary>
  /// 此住房分配结果的版本。
  /// </summary>
  public ulong Revision { get; }

  /// <summary>
  /// 产生此分配结果的扫描版本，可为空。
  /// </summary>
  public ulong? SourceScanRevision { get; }

  /// <summary>
  /// 已分配住房的居民记录数。
  /// </summary>
  public int AssignedRoomCount => AssignedRooms.Count;

  /// <summary>
  /// 无家居民记录数。
  /// </summary>
  public int HomelessResidentCount => HomelessResidents.Count;
}
