using Terraria.Combat;

namespace Terraria.Npc;

public readonly record struct NpcDamageRequest(
  CombatContributorId Contributor,
  int Damage,
  long Tick,
  int Defense = 0,
  bool Critical = false,
  float TakenDamageMultiplier = 1.0f,
  bool Immortal = false,
  bool IsBoss = false,
  bool BypassTrackerCredit = false,
  bool RedHatSkeletronAdjustmentEnabled = false,
  int? LegacyOwner = null,
  bool IsHostileDamage = false,
  bool IsTrapDamage = false)
{
  public bool IsForcedWorldDamage =>
    LegacyOwner == 255 &&
    Damage >= 9999 &&
    Contributor.Kind == CombatContributorKind.World;

  public static NpcDamageRequest FromLegacyOwner(
    int damage,
    long tick,
    int legacyOwner,
    string? playerAccountUuid = null,
    int defense = 0,
    bool critical = false,
    float takenDamageMultiplier = 1.0f,
    bool immortal = false,
    bool isBoss = false,
    bool redHatSkeletronAdjustmentEnabled = false,
    bool isHostileDamage = false,
    bool isTrapDamage = false)
  {
    CombatContributorId contributor = CombatContributorId.FromLegacyOwner(
      legacyOwner,
      playerAccountUuid);
    return new NpcDamageRequest(
      contributor,
      damage,
      tick,
      defense,
      critical,
      takenDamageMultiplier,
      immortal,
      isBoss,
      BypassTrackerCredit: damage >= 9999 && legacyOwner == 255,
      RedHatSkeletronAdjustmentEnabled: redHatSkeletronAdjustmentEnabled,
      LegacyOwner: legacyOwner,
      IsHostileDamage: isHostileDamage,
      IsTrapDamage: isTrapDamage);
  }
}
