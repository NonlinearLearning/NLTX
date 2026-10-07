using System;
using Terraria.Combat;

namespace Terraria.Npc;

public sealed class NpcDamageEncounterTracker
{
  private EncounterDamageCreditComponent _credit;

  internal NpcDamageEncounterTracker(
    ulong encounterId,
    NpcTypeId initialNpcType,
    NpcTypeId trackerNpcType,
    INpcDamageTrackingStrategy strategy,
    long startedAtTick)
  {
    if (encounterId == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(encounterId));
    }

    if (!initialNpcType.IsValid)
    {
      throw new ArgumentException(
        "Initial NPC type must be valid.",
        nameof(initialNpcType));
    }

    if (!trackerNpcType.IsValid)
    {
      throw new ArgumentException(
        "Tracker NPC type must be valid.",
        nameof(trackerNpcType));
    }

    ArgumentNullException.ThrowIfNull(strategy);
    if (!strategy.Includes(trackerNpcType))
    {
      throw new ArgumentException(
        "The strategy must include the tracker NPC type.",
        nameof(strategy));
    }

    if (startedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(startedAtTick));
    }

    EncounterId = encounterId;
    InitialNpcType = initialNpcType;
    TrackerNpcType = trackerNpcType;
    Strategy = strategy;
    _credit = new EncounterDamageCreditComponent(
      encounterId,
      startedAtTick: startedAtTick,
      lastHitAtTick: startedAtTick);
  }

  public ulong EncounterId { get; }

  public NpcTypeId InitialNpcType { get; }

  public NpcTypeId TrackerNpcType { get; }

  internal INpcDamageTrackingStrategy Strategy { get; }

  public bool IsEmpty => _credit.IsEmpty;

  public EncounterCreditLifecycle Lifecycle => _credit.Lifecycle;

  internal bool TryRecordDamage(
    CombatContributorId contributor,
    int appliedAmount,
    long tick)
  {
    return _credit.TryAddCredit(contributor, appliedAmount, tick);
  }

  internal void Close()
  {
    _credit.Close();
  }

  internal void Expire()
  {
    _credit.Expire();
  }

  internal void MarkKilled(NpcTypeId npcType)
  {
    Strategy.OnNpcKilled(npcType);
  }

  internal long LastHitAtTick => _credit.LastHitAtTick;

  internal NpcDamageTrackerSnapshot CreateSnapshot(
    long currentTick,
    bool isActive,
    bool isRecent)
  {
    if (currentTick < _credit.LastHitAtTick)
    {
      throw new ArgumentOutOfRangeException(nameof(currentTick));
    }

    return new NpcDamageTrackerSnapshot(
      EncounterId,
      InitialNpcType,
      TrackerNpcType,
      _credit,
      currentTick,
      isActive,
      isRecent,
      Strategy.IsKilled);
  }
}
