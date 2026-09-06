using System;
using System.Collections.Generic;

namespace Terraria.Combat;

public struct EncounterDamageCreditComponent
{
  public EncounterDamageCreditComponent(
    ulong encounterId,
    IReadOnlyList<DamageCreditEntry>? credits = null,
    int worldDamage = 0,
    CombatContributorId? lastContributor = null,
    long startedAtTick = 0,
    long lastHitAtTick = 0,
    EncounterCreditLifecycle lifecycle = EncounterCreditLifecycle.Active,
    int revision = 0)
  {
    EncounterId = encounterId;
    _credits = credits is null
      ? null
      : new List<DamageCreditEntry>(credits).ToArray();
    WorldDamage = worldDamage;
    LastContributor = lastContributor;
    StartedAtTick = startedAtTick;
    LastHitAtTick = lastHitAtTick;
    Lifecycle = lifecycle;
    Revision = revision;
  }

  public ulong EncounterId;
  private DamageCreditEntry[]? _credits;
  public int WorldDamage;
  public CombatContributorId? LastContributor;
  public long StartedAtTick;
  public long LastHitAtTick;
  public EncounterCreditLifecycle Lifecycle;
  public int Revision;

  public ReadOnlyMemory<DamageCreditEntry> Credits =>
    new(_credits ?? Array.Empty<DamageCreditEntry>());

  public bool IsEmpty => (_credits is null or { Length: 0 }) &&
    WorldDamage == 0;

  public long DurationTicks => Math.Max(0, LastHitAtTick - StartedAtTick);
}
