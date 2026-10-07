using System;

using Terraria.Relationships;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.SPATIAL_REFERENCE
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存实体所属空间和相对父实体的坐标关系。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Entity.position 的空间定位模型重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>重组说明：父实体引用、相对偏移和空间类别是拆分时新增的坐标关系。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-spatial-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 266 行。</para>
/// </remarks>
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
