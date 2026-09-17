using System;
using System.Collections.Generic;
using Terraria.Combat;

namespace Terraria.Npc;

public sealed class NpcDamageTrackingSystem
{
  private const int ExtraRecentTrackerExpiryTime = 54000;
  private const int MaxRecentTrackers = 3;

  private readonly List<NpcDamageEncounterTracker> _activeTrackers = new();
  private readonly List<NpcDamageEncounterTracker> _recentFinishedTrackers = new();
  private readonly Func<NpcTypeId, INpcDamageTrackingStrategy?> _strategyFactory;
  private ulong _nextEncounterId;
  private long _currentTick;
  private bool _hasCurrentTick;

  public NpcDamageTrackingSystem(
    Func<NpcTypeId, INpcDamageTrackingStrategy?> strategyFactory)
  {
    ArgumentNullException.ThrowIfNull(strategyFactory);
    _strategyFactory = strategyFactory;
  }

  public int ActiveTrackerCount => _activeTrackers.Count;

  public int RecentTrackerCount => _recentFinishedTrackers.Count;

  public void AdvanceTo(
    long tick,
    IReadOnlySet<NpcTypeId> activeNpcTypes)
  {
    ArgumentNullException.ThrowIfNull(activeNpcTypes);
    SetCurrentTick(tick);

    for (int index = _activeTrackers.Count - 1; index >= 0; index--)
    {
      NpcDamageEncounterTracker tracker = _activeTrackers[index];
      if (!tracker.Strategy.IsStillActive(activeNpcTypes))
      {
        _activeTrackers.RemoveAt(index);
        CloseAndRetain(tracker);
      }
    }

    ExpireRecentTrackers();
  }

  public bool TryRecordAppliedDamage(
    NpcTypeId npcType,
    CombatContributorId contributor,
    int appliedAmount,
    long tick)
  {
    EnsureCurrentTick(tick);
    if (!npcType.IsValid)
    {
      throw new ArgumentException("NPC type must be valid.", nameof(npcType));
    }

    if (appliedAmount <= 0)
    {
      return false;
    }

    NpcDamageEncounterTracker? tracker = FindActiveTracker(npcType);
    if (tracker is null)
    {
      INpcDamageTrackingStrategy? strategy = _strategyFactory(npcType);
      if (strategy is null)
      {
        return false;
      }

      ulong encounterId = GetNextEncounterId();
      tracker = CreateTracker(npcType, strategy, encounterId);
      bool recorded = tracker.TryRecordDamage(contributor, appliedAmount, tick);
      if (!recorded)
      {
        return false;
      }

      _nextEncounterId = encounterId;
      _activeTrackers.Add(tracker);
      return true;
    }

    return tracker.TryRecordDamage(contributor, appliedAmount, tick);
  }

  public NpcDamageEncounterTracker StartTracking(
    NpcTypeId initialNpcType,
    INpcDamageTrackingStrategy strategy)
  {
    EnsureCurrentTickInitialized();
    if (!initialNpcType.IsValid)
    {
      throw new ArgumentException(
        "Initial NPC type must be valid.",
        nameof(initialNpcType));
    }

    ArgumentNullException.ThrowIfNull(strategy);
    if (!strategy.Includes(initialNpcType))
    {
      throw new ArgumentException(
        "The strategy must include the initial NPC type.",
        nameof(strategy));
    }

    ulong encounterId = AllocateEncounterId();
    NpcDamageEncounterTracker tracker = CreateTracker(
      initialNpcType,
      strategy,
      encounterId);
    _activeTrackers.Add(tracker);
    return tracker;
  }

  public bool MarkKilled(NpcTypeId npcType, long tick)
  {
    EnsureCurrentTick(tick);
    if (!npcType.IsValid)
    {
      throw new ArgumentException("NPC type must be valid.", nameof(npcType));
    }

    NpcDamageEncounterTracker? tracker = FindActiveTracker(npcType);
    if (tracker is null)
    {
      return false;
    }

    tracker.MarkKilled(npcType);
    return true;
  }

  public bool StopTracking(ulong encounterId, long tick)
  {
    EnsureCurrentTick(tick);
    for (int index = 0; index < _activeTrackers.Count; index++)
    {
      NpcDamageEncounterTracker tracker = _activeTrackers[index];
      if (tracker.EncounterId != encounterId)
      {
        continue;
      }

      _activeTrackers.RemoveAt(index);
      CloseAndRetain(tracker);
      return true;
    }

    return false;
  }

  public NpcDamageTrackerSnapshot[] GetActiveSnapshots()
  {
    EnsureCurrentTickInitialized();
    var snapshots = new NpcDamageTrackerSnapshot[_activeTrackers.Count];
    for (int index = 0; index < _activeTrackers.Count; index++)
    {
      snapshots[index] = _activeTrackers[index].CreateSnapshot(
        _currentTick,
        isActive: true,
        isRecent: false);
    }

    return snapshots;
  }

  public NpcDamageTrackerSnapshot[] GetRecentSnapshots()
  {
    EnsureCurrentTickInitialized();
    var snapshots = new NpcDamageTrackerSnapshot[_recentFinishedTrackers.Count];
    for (int index = 0; index < _recentFinishedTrackers.Count; index++)
    {
      snapshots[index] = _recentFinishedTrackers[index].CreateSnapshot(
        _currentTick,
        isActive: false,
        isRecent: true);
    }

    return snapshots;
  }

  public void Reset()
  {
    _activeTrackers.Clear();
    _recentFinishedTrackers.Clear();
    _nextEncounterId = 0;
    _currentTick = 0;
    _hasCurrentTick = false;
  }

  private NpcDamageEncounterTracker? FindActiveTracker(NpcTypeId npcType)
  {
    for (int index = 0; index < _activeTrackers.Count; index++)
    {
      if (_activeTrackers[index].Strategy.Includes(npcType))
      {
        return _activeTrackers[index];
      }
    }

    return null;
  }

  private NpcDamageEncounterTracker CreateTracker(
    NpcTypeId initialNpcType,
    INpcDamageTrackingStrategy strategy,
    ulong encounterId)
  {
    return new NpcDamageEncounterTracker(
      encounterId,
      initialNpcType,
      strategy,
      _currentTick);
  }

  private void CloseAndRetain(NpcDamageEncounterTracker tracker)
  {
    tracker.Close();
    if (tracker.IsEmpty)
    {
      return;
    }

    _recentFinishedTrackers.Add(tracker);
    if (_recentFinishedTrackers.Count > MaxRecentTrackers)
    {
      NpcDamageEncounterTracker expired = _recentFinishedTrackers[0];
      _recentFinishedTrackers.RemoveAt(0);
      expired.Expire();
    }
  }

  private void ExpireRecentTrackers()
  {
    while (_recentFinishedTrackers.Count > 1 &&
           _currentTick - _recentFinishedTrackers[0].LastHitAtTick >
             ExtraRecentTrackerExpiryTime)
    {
      NpcDamageEncounterTracker expired = _recentFinishedTrackers[0];
      _recentFinishedTrackers.RemoveAt(0);
      expired.Expire();
    }
  }

  private ulong AllocateEncounterId()
  {
    if (_nextEncounterId == ulong.MaxValue)
    {
      throw new OverflowException("Encounter identity allocation overflowed.");
    }

    return ++_nextEncounterId;
  }

  private ulong GetNextEncounterId()
  {
    if (_nextEncounterId == ulong.MaxValue)
    {
      throw new OverflowException("Encounter identity allocation overflowed.");
    }

    return _nextEncounterId + 1;
  }

  private void SetCurrentTick(long tick)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (_hasCurrentTick && tick < _currentTick)
    {
      throw new ArgumentOutOfRangeException(
        nameof(tick),
        "The damage tracking clock cannot move backwards.");
    }

    _currentTick = tick;
    _hasCurrentTick = true;
  }

  private void EnsureCurrentTick(long tick)
  {
    EnsureCurrentTickInitialized();
    if (tick != _currentTick)
    {
      throw new InvalidOperationException(
        "Damage commits must use the current tracking tick.");
    }
  }

  private void EnsureCurrentTickInitialized()
  {
    if (!_hasCurrentTick)
    {
      throw new InvalidOperationException(
        "AdvanceTo must establish a tracking tick before state access.");
    }
  }
}
