using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Trusted facts consumed by the ordinary NPC death slime-rain rule. The caller captures
/// these from the same death generation and world window; Combat does not read global state.
/// </summary>
public sealed record NpcSlimeRainDeathFacts(
    CombatEntityIdentity Entity,
    long SourceNpcRevision,
    short NpcType,
    bool SlimeRain,
    bool SlimeRainNpc,
    bool KingSlimePresent,
    bool DownedSlimeKing,
    int KillCountBefore,
    byte ClosestPlayerSlot);

/// <summary>Server-side spawn intent produced when the slime-rain threshold is reached.</summary>
public sealed record NpcSlimeRainSpawnIntent(
    short NpcType,
    byte TargetPlayerSlot);

/// <summary>
/// Pure result for one DoDeathEvents_AdvanceSlimeRain evaluation. Applying the counter and
/// spawn intent remains an owner operation outside this rules file.
/// </summary>
public sealed record NpcSlimeRainDeathResult(
    CombatEntityIdentity Entity,
    long SourceNpcRevision,
    int KillCountAfter,
    bool ThresholdReached,
    NpcSlimeRainSpawnIntent? Spawn,
    ProtocolErrorCode? ErrorCode,
    string? Detail)
{
  public bool IsAccepted => ErrorCode is null;
}

/// <summary>Source-ordered ordinary slime-rain death arithmetic for migrated NPC types.</summary>
public static class NpcSlimeRainDeathRules
{
  private const short KingSlimeType = 50;
  private const int NormalThreshold = 150;

  public static NpcSlimeRainDeathResult Evaluate(NpcSlimeRainDeathFacts facts)
  {
    ArgumentNullException.ThrowIfNull(facts);
    if (!facts.Entity.IsValid || facts.SourceNpcRevision <= 0)
    {
      return Rejected(
          facts,
          ProtocolErrorCode.InvalidLength,
          "Slime-rain death facts require a valid NPC generation and revision.");
    }
    if (facts.NpcType is not (1 or 2 or 3))
    {
      return Rejected(
          facts,
          ProtocolErrorCode.NotReady,
          "Slime-rain death rules are limited to migrated ordinary NPC types.");
    }
    if (!facts.SlimeRain || !facts.SlimeRainNpc || facts.KingSlimePresent)
    {
      return Accepted(facts, facts.KillCountBefore, false, null);
    }
    if (facts.KillCountBefore == int.MaxValue)
    {
      return Rejected(
          facts,
          ProtocolErrorCode.NotReady,
          "The slime-rain kill counter cannot advance without overflowing.");
    }

    int threshold = facts.DownedSlimeKing ? NormalThreshold / 2 : NormalThreshold;
    int killCountAfter = facts.KillCountBefore + 1;
    if (killCountAfter < threshold)
    {
      return Accepted(facts, killCountAfter, false, null);
    }

    return Accepted(
        facts,
        -threshold / 2,
        true,
        new NpcSlimeRainSpawnIntent(KingSlimeType, facts.ClosestPlayerSlot));
  }

  private static NpcSlimeRainDeathResult Accepted(
      NpcSlimeRainDeathFacts facts,
      int killCountAfter,
      bool thresholdReached,
      NpcSlimeRainSpawnIntent? spawn)
  {
    return new NpcSlimeRainDeathResult(
        facts.Entity,
        facts.SourceNpcRevision,
        killCountAfter,
        thresholdReached,
        spawn,
        null,
        null);
  }

  private static NpcSlimeRainDeathResult Rejected(
      NpcSlimeRainDeathFacts facts,
      ProtocolErrorCode errorCode,
      string detail)
  {
    return new NpcSlimeRainDeathResult(
        facts.Entity,
        facts.SourceNpcRevision,
        facts.KillCountBefore,
        false,
        null,
        errorCode,
        detail);
  }
}
