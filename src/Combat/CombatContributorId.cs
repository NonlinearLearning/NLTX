namespace Terraria.Combat;

public readonly record struct CombatContributorId(
  CombatContributorKind Kind,
  string? PlayerAccountUuid);
