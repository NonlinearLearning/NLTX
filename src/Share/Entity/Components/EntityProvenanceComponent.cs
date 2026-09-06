using Terraria.Relationships;

namespace EntityEcs.Components;

public struct EntityProvenanceComponent
{
  public EntityProvenanceKind Kind;
  public EntityReference? SourceEntity;
  public EntityReference? ParentEntity;
  public EntityReference? StruckEntity;
  public int? SourceItemDefinitionId;
  public int? SourceProjectileDefinitionId;
  public WorldEventSourceKind? WorldEvent;

  public bool IsRootCause =>
    SourceEntity is null &&
    ParentEntity is null &&
    SourceItemDefinitionId is null &&
    SourceProjectileDefinitionId is null;
}
