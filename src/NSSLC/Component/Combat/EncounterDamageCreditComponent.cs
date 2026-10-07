using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>
/// 保存一次战斗的贡献账户、世界伤害、最后贡献者及统计生命周期。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.NPCDamageTracker。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/NPCDamageTracker.cs。</para>
/// <para>
/// 主要源成员：_list（第 58 行）； _worldCredit（第 60 行）； _lastAttacker（第 62 行）； _ticks（第 64 行）；
/// _lastHitTime（第 66 行）。
/// </para>
/// <para>重组说明：贡献账户 UUID、EncounterId、Revision 和显式生命周期是拆分时新增的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-06-version4-combat-and-status-component-design-report.md。</para>
/// <para>依据位置：第 277 行。</para>
/// </remarks>
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
    new((_credits ?? Array.Empty<DamageCreditEntry>()).ToArray());

  public bool IsEmpty => (_credits is null or { Length: 0 }) &&
    WorldDamage == 0;

  public long DurationTicks => Math.Max(0, LastHitAtTick - StartedAtTick);

  public bool TryAddCredit(
    CombatContributorId contributor,
    int appliedDamage,
    long hitTick)
  {
    ValidateContributor(contributor);
    if (appliedDamage <= 0)
    {
      return false;
    }

    if (Lifecycle != EncounterCreditLifecycle.Active)
    {
      throw new InvalidOperationException(
        "Damage credit can only be added to an active encounter.");
    }

    if (hitTick < LastHitAtTick)
    {
      throw new ArgumentOutOfRangeException(
        nameof(hitTick),
        "Damage credit ticks must be monotonic.");
    }

    DamageCreditEntry[] nextCredits = (_credits ?? Array.Empty<DamageCreditEntry>()).ToArray();
    int existingIndex = FindContributor(nextCredits, contributor);
    if (existingIndex >= 0)
    {
      nextCredits[existingIndex].AppliedDamage = checked(
        nextCredits[existingIndex].AppliedDamage + appliedDamage);
    }
    else
    {
      Array.Resize(ref nextCredits, nextCredits.Length + 1);
      nextCredits[^1] = new DamageCreditEntry(contributor, appliedDamage);
    }

    int nextWorldDamage = WorldDamage;
    if (contributor.Kind == CombatContributorKind.World)
    {
      nextWorldDamage = checked(WorldDamage + appliedDamage);
    }

    int nextRevision = checked(Revision + 1);
    _credits = nextCredits;
    WorldDamage = nextWorldDamage;
    LastContributor = contributor;
    LastHitAtTick = hitTick;
    Revision = nextRevision;
    return true;
  }

  public void Close()
  {
    if (Lifecycle != EncounterCreditLifecycle.Active)
    {
      throw new InvalidOperationException(
        "Only an active encounter can be closed.");
    }

    int nextRevision = checked(Revision + 1);
    Lifecycle = EncounterCreditLifecycle.Closed;
    Revision = nextRevision;
  }

  public void Expire()
  {
    if (Lifecycle != EncounterCreditLifecycle.Closed)
    {
      throw new InvalidOperationException(
        "Only a closed encounter can expire.");
    }

    int nextRevision = checked(Revision + 1);
    Lifecycle = EncounterCreditLifecycle.Expired;
    Revision = nextRevision;
  }

  private static int FindContributor(
    IReadOnlyList<DamageCreditEntry> credits,
    CombatContributorId contributor)
  {
    for (int index = 0; index < credits.Count; index++)
    {
      if (credits[index].Contributor == contributor)
      {
        return index;
      }
    }

    return -1;
  }

  private static void ValidateContributor(CombatContributorId contributor)
  {
    if (contributor.Kind == CombatContributorKind.Player &&
        string.IsNullOrWhiteSpace(contributor.PlayerAccountUuid))
    {
      throw new ArgumentException(
        "A player contributor requires account provenance.",
        nameof(contributor));
    }

    if (contributor.Kind == CombatContributorKind.World &&
        contributor.PlayerAccountUuid is not null)
    {
      throw new ArgumentException(
        "World credit cannot carry player account provenance.",
        nameof(contributor));
    }

    if (contributor.Kind != CombatContributorKind.Player &&
        contributor.Kind != CombatContributorKind.World)
    {
      throw new ArgumentOutOfRangeException(
        nameof(contributor),
        "Unknown contributor kinds cannot receive damage credit.");
    }
  }
}
