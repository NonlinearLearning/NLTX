using System;

namespace Terraria.Combat;

public readonly record struct CombatContributorId(
  CombatContributorKind Kind,
  string? PlayerAccountUuid)
{
  public bool IsValid =>
    (Kind == CombatContributorKind.Player &&
      !string.IsNullOrWhiteSpace(PlayerAccountUuid)) ||
    (Kind == CombatContributorKind.World &&
      PlayerAccountUuid is null);

  public static CombatContributorId FromLegacyOwner(
    int legacyOwner,
    string? playerAccountUuid = null)
  {
    if (legacyOwner < 0 || legacyOwner >= 255)
    {
      if (playerAccountUuid is not null)
      {
        throw new ArgumentException(
          "World damage cannot carry player account provenance.",
          nameof(playerAccountUuid));
      }

      return new CombatContributorId(CombatContributorKind.World, null);
    }

    if (string.IsNullOrWhiteSpace(playerAccountUuid))
    {
      throw new ArgumentException(
        "A legacy player owner requires account provenance.",
        nameof(playerAccountUuid));
    }

    return new CombatContributorId(
      CombatContributorKind.Player,
      playerAccountUuid);
  }

  public bool MatchesLegacyOwner(int legacyOwner)
  {
    if (legacyOwner < 0 || legacyOwner >= 255)
    {
      return Kind == CombatContributorKind.World &&
        PlayerAccountUuid is null;
    }

    return Kind == CombatContributorKind.Player &&
      !string.IsNullOrWhiteSpace(PlayerAccountUuid);
  }
}
