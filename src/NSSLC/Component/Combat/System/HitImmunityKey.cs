using Terraria.Relationships;

namespace Terraria.Combat;

public readonly record struct HitImmunityKey(
  HitImmunityScope Scope,
  EntityReference? SourceEntity,
  int? SourceDefinitionId);
