using System.Collections.Generic;

namespace NSSLC.WorldGeneration.Runtime.Components;

// status: proposed
// evidenceStatus: partial; Version4 type-level key is confirmed, final key mode unresolved
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存城镇居民住房映射、无家集合和分配顺序的运行时注册状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.TownRoomManager。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/TownRoomManager.cs。</para>
/// <para>主要源成员：_roomLocationPairs（第 13 行）； _hasRoom（第 15 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 623 行。</para>
/// <para>
/// 用途：住房管理流程维护居民键到住房位置的映射，并记录无家居民和稳定的分配顺序，查询方读取数量和版本。
/// </para>
/// <para>
/// 当前实现：公开读取使用 SyncRoot 加锁；内部集合修改和版本推进提供给外部提交流程协调。AdvanceRevision 使用 checked 递增；SetResi
/// dentKeyMode 直接保存键模式。
/// </para>
/// <para>
/// 设计状态：源码仍标记 proposed，居民键模式默认 Unresolved，最终按类型还是实例组织居民的方式应以整合后的实际规则为准。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C185，L2（通用可选玩法）。分级理由：住房扫描、分配和注册是城镇居民玩法，不是世界加载本身。
/// </para>
/// </remarks>
public sealed class GenerationHousingRegistryState
{
  private readonly List<TownHousingResidentKey> _roomAssignmentOrder = new();
  private ulong _revision;
  private TownHousingKeyMode _residentKeyMode = TownHousingKeyMode.Unresolved;

  /// <summary>
  /// 住房注册状态的同步锁，供内部提交流程协调集合和版本修改。
  /// </summary>
  internal object SyncRoot { get; } = new();

  /// <summary>
  /// 居民键到住房位置的内部可变映射。
  /// </summary>
  internal Dictionary<TownHousingResidentKey, TilePosition> RoomsByResidentKey { get; } = new();

  /// <summary>
  /// 尚未分配住房的居民键集合。
  /// </summary>
  internal HashSet<TownHousingResidentKey> HomelessResidentKeys { get; } = new();

  /// <summary>
  /// 加锁读取的住房注册版本。
  /// </summary>
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

  /// <summary>
  /// 加锁读取的居民键模式，默认 Unresolved。
  /// </summary>
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

  /// <summary>
  /// 内部维护的住房分配顺序列表。
  /// </summary>
  internal List<TownHousingResidentKey> RoomAssignmentOrder => _roomAssignmentOrder;

  /// <summary>
  /// 加锁读取的已分配住房记录数量。
  /// </summary>
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

  /// <summary>
  /// 加锁读取的无家居民数量。
  /// </summary>
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

  /// <summary>
  /// 住房映射和无家集合都为空时返回 true。
  /// </summary>
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

  /// <summary>
  /// 直接提交居民键模式，调用方负责同步和有效性。
  /// </summary>
  internal void SetResidentKeyMode(TownHousingKeyMode mode)
  {
    _residentKeyMode = mode;
  }

  /// <summary>
  /// 检查版本加一是否溢出，不改变版本。
  /// </summary>
  internal void EnsureRevisionCanAdvance()
  {
    _ = checked(_revision + 1);
  }

  /// <summary>
  /// 以 checked 运算将住房注册版本加一，调用方负责同步。
  /// </summary>
  internal void AdvanceRevision()
  {
    _revision = checked(_revision + 1);
  }
}
