using Terraria.Relationships;

namespace Terraria.Combat;

public readonly record struct DeathCause(
  DamageSourceKind Kind,
  EntityReference Source);
