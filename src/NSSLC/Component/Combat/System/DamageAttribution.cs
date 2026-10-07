using Terraria.Relationships;

namespace Terraria.Combat;

public readonly record struct DamageAttribution(
  DamageSourceKind Kind,
  EntityReference Source);
