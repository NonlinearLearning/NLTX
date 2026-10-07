using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Terraria.DeathPenaltyAndRevenge;

/// <summary>
/// 保存世界内金币复仇标记的索引和集合。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.CoinLossRevengeSystem。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/CoinLossRevengeSystem.cs。</para>
/// <para>主要源成员：_markers（第 295 行）。</para>
/// <para>重组说明：Revision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 215 行。</para>
/// </remarks>
public sealed class RevengeMarkerRegistryComponent
{
  private readonly Dictionary<RevengeMarkerId, RevengeMarkerSnapshot?> _markers;
  private IReadOnlySet<RevengeMarkerId> _markerIdsView =
    new HashSet<RevengeMarkerId>().ToFrozenSet();
  private IReadOnlyList<RevengeMarkerSnapshot> _markersView =
    Array.Empty<RevengeMarkerSnapshot>();

  public RevengeMarkerRegistryComponent()
    : this(Array.Empty<RevengeMarkerId>())
  {
  }

  public RevengeMarkerRegistryComponent(IEnumerable<RevengeMarkerId> markerIds)
  {
    ArgumentNullException.ThrowIfNull(markerIds);

    _markers = new Dictionary<RevengeMarkerId, RevengeMarkerSnapshot?>();
    foreach (RevengeMarkerId markerId in markerIds)
    {
      if (!markerId.IsAssigned)
      {
        throw new ArgumentException(
          "A marker registry can contain only assigned marker IDs.",
          nameof(markerIds));
      }

      if (!_markers.TryAdd(markerId, null))
      {
        throw new ArgumentException(
          "A marker registry cannot contain duplicate marker IDs.",
          nameof(markerIds));
      }
    }

    RefreshViews();
  }

  public IReadOnlySet<RevengeMarkerId> MarkerIds => _markerIdsView;

  public IReadOnlyList<RevengeMarkerSnapshot> Markers => _markersView;

  public uint Revision { get; private set; }

  internal bool TryGet(
    RevengeMarkerId markerId,
    out RevengeMarkerSnapshot? snapshot)
  {
    return _markers.TryGetValue(markerId, out snapshot) && snapshot is not null;
  }

  internal bool TryRegister(
    RevengeMarkerSnapshot snapshot,
    out RevengeMarkerSnapshot committed)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    committed = snapshot;
    if (!snapshot.MarkerId.IsAssigned || _markers.ContainsKey(snapshot.MarkerId))
    {
      return false;
    }

    committed = snapshot.WithRevision(NextRevision());
    _markers.Add(committed.MarkerId, committed);
    RefreshViews();
    return true;
  }

  internal bool TryRegister(RevengeMarkerId markerId)
  {
    if (!markerId.IsAssigned)
    {
      return false;
    }

    if (!_markers.TryAdd(markerId, null))
    {
      return false;
    }

    NextRevision();
    RefreshMarkerIdView();
    return true;
  }

  internal bool TryReplace(
    RevengeMarkerSnapshot snapshot,
    uint expectedMarkerRevision,
    out RevengeMarkerSnapshot committed)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    committed = snapshot;
    if (!_markers.TryGetValue(snapshot.MarkerId, out RevengeMarkerSnapshot? current) ||
      current is null)
    {
      return false;
    }

    if (current.Revision != expectedMarkerRevision)
    {
      committed = current;
      return false;
    }

    committed = snapshot.WithRevision(NextRevision());
    _markers[snapshot.MarkerId] = committed;
    RefreshViews();
    return true;
  }

  internal bool TryUnregister(
    RevengeMarkerId markerId,
    uint expectedMarkerRevision,
    out RevengeMarkerSnapshot? removed)
  {
    removed = null;
    if (!_markers.TryGetValue(markerId, out RevengeMarkerSnapshot? current))
    {
      return false;
    }

    if (current is not null && current.Revision != expectedMarkerRevision)
    {
      removed = current;
      return false;
    }

    if (!_markers.Remove(markerId))
    {
      return false;
    }

    removed = current;
    NextRevision();
    RefreshMarkerIdView();
    return true;
  }

  internal void Clear()
  {
    if (_markers.Count == 0)
    {
      Revision = 0;
      RefreshViews();
      return;
    }

    _markers.Clear();
    Revision = 0;
    RefreshMarkerIdView();
  }

  private uint NextRevision()
  {
    if (Revision == uint.MaxValue)
    {
      throw new InvalidOperationException(
        "The revenge marker registry revision is exhausted.");
    }

    Revision++;
    return Revision;
  }

  private void RefreshMarkerIdView()
  {
    RefreshViews();
  }

  private void RefreshViews()
  {
    List<RevengeMarkerId> markerIds = new(_markers.Keys);
    _markerIdsView = markerIds.ToFrozenSet();

    List<RevengeMarkerSnapshot> snapshots = new();
    foreach (RevengeMarkerSnapshot? marker in _markers.Values)
    {
      if (marker is not null)
      {
        snapshots.Add(marker);
      }
    }

    _markersView = new ReadOnlyCollection<RevengeMarkerSnapshot>(snapshots);
  }
}
