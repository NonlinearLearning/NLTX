using System;

using Terraria.Relationships;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.SPATIAL_REFERENCE
// crossSubsystemOwner: integration-review
public struct SpatialReferenceComponent
{
  public SpatialReferenceComponent(
    EntityReference? parentEntity = null,
    float parentOffsetX = 0.0f,
    float parentOffsetY = 0.0f,
    bool isParentRelative = false,
    SpatialSpaceId space = SpatialSpaceId.World)
  {
    EnsureFinite(parentOffsetX, nameof(parentOffsetX));
    EnsureFinite(parentOffsetY, nameof(parentOffsetY));

    ParentEntity = parentEntity;
    ParentOffsetX = parentOffsetX;
    ParentOffsetY = parentOffsetY;
    IsParentRelative = isParentRelative;
    Space = space;
  }

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

  public bool IsWorldSpace =>
    Space == SpatialSpaceId.World && !IsParentRelative;

  private static void EnsureFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Value must be finite.");
    }
  }
}
