using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Trusted death-time NPC, world, and closest-player facts. The caller keeps the
/// external facts current through preparation and commit; Combat writes no player vitals.
/// </summary>
public sealed record NpcCommonLootFacts(
    CombatEntityIdentity Entity,
    long SourceNpcRevision,
    float Value,
    int ExtraValue,
    bool Midas,
    bool BloodMoon,
    float ClosestPlayerLuck,
    int ClosestPlayerLife,
    int ClosestPlayerMaximumLife,
    int ClosestPlayerMana,
    int ClosestPlayerMaximumMana) {
  // All applicable money multipliers together are below 38. Use 40 for rounding
  // headroom, and stay below the largest float safely convertible to Int32.
  // This is a defensive numeric envelope, not a Version4 content/value limit.
  internal bool HasValidNumbers => float.IsFinite(Value) &&
      float.IsFinite(ClosestPlayerLuck) &&
      Math.Abs((double)Value) * 40d + Math.Abs((double)ExtraValue) <= 2147483520d;
}
