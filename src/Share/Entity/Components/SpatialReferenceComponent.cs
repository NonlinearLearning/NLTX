using Terraria.Relationships;

namespace EntityEcs.Components;

public struct SpatialReferenceComponent
{
  public EntityReference? ParentEntity;
  public float ParentOffsetX;
  public float ParentOffsetY;
  public bool IsParentRelative;
  public SpatialSpaceId Space;

  public EntityReference? ParentEntityId
  {
    get => ParentEntity;
    set => ParentEntity = value;
  }

  public bool IsWorldSpace => Space == SpatialSpaceId.World && !IsParentRelative;
}
