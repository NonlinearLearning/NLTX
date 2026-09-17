using System;
using Terraria.Combat;

namespace Terraria.Npc;

public sealed class NpcDamageEncounterTracker
{
  private EncounterDamageCreditComponent _credit;

  internal NpcDamageEncounterTracker(
    ulong encounterId,
    NpcTypeId initialNpcType,
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

    ArgumentNullException.ThrowIfNull(strategy);
    if (!strategy.Includes(initialNpcType))
    {
      throw new ArgumentException(
        "The strategy must include the initial NPC type.",
        nameof(strategy));
    }

    if (startedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(startedAtTick));
    }

    EncounterId = encounterId;
    InitialNpcType = initialNpcType;
    Strategy = strategy;
    _credit = new EncounterDamageCreditComponent(
      encounterId,
      startedAtTick: startedAtTick,
      lastHitAtTick: startedAtTick);
  }

  public ulong EncounterId { get; }

  public NpcTypeId InitialNpcType { get; }

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
      _credit,
      currentTick,
      isActive,
      isRecent,
      Strategy.IsKilled);
  }
}
