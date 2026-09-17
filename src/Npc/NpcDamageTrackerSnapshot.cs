using System;
using Terraria.Combat;

namespace Terraria.Npc;

public sealed class NpcDamageTrackerSnapshot
{
  private readonly DamageCreditEntry[] _credits;

  internal NpcDamageTrackerSnapshot(
    ulong encounterId,
    NpcTypeId initialNpcType,
    EncounterDamageCreditComponent credit,
    long currentTick,
    bool isActive,
    bool isRecent,
    bool isKilled)
  {
    EncounterId = encounterId;
    InitialNpcType = initialNpcType;
    _credits = credit.Credits.ToArray();
    WorldDamage = credit.WorldDamage;
    LastContributor = credit.LastContributor;
    StartedAtTick = credit.StartedAtTick;
    LastHitAtTick = credit.LastHitAtTick;
    Duration = credit.DurationTicks;
    TimeSinceLastHit = checked(currentTick - credit.LastHitAtTick);
    IsEmpty = credit.IsEmpty;
    Lifecycle = credit.Lifecycle;
    IsActive = isActive;
    IsRecent = isRecent;
    IsKilled = isKilled;
    Revision = credit.Revision;
  }

  public ulong EncounterId { get; }

  public NpcTypeId InitialNpcType { get; }

  public ReadOnlyMemory<DamageCreditEntry> Credits =>
    new(_credits.ToArray());

  public int WorldDamage { get; }

  public CombatContributorId? LastContributor { get; }

  public long StartedAtTick { get; }

  public long LastHitAtTick { get; }

  public long Duration { get; }

  public long TimeSinceLastHit { get; }

  public bool IsEmpty { get; }

  public EncounterCreditLifecycle Lifecycle { get; }

  public bool IsActive { get; }

  public bool IsRecent { get; }

  public bool IsKilled { get; }

  public int Revision { get; }
}
