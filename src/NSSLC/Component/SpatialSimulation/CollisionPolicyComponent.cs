using Terraria.Physics;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.COLLISION_POLICY
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存方块、实体、液体、平台、门和斜坡的碰撞策略。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：noTileCollide（第 6369 行）。</para>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：tileCollide（第 194 行）； ignoreWater（第 202 行）。</para>
/// <para>拆分来源：Terraria.Item。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Item.cs。</para>
/// <para>主要源成员：noWet（第 258 行）。</para>
/// <para>重组说明：平台、门和斜坡选项由原碰撞调用参数及规则重组。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-spatial-simulation-component-design.md。</para>
/// <para>依据位置：第 351 行。</para>
/// </remarks>
public struct CollisionPolicyComponent
{
  public CollisionPolicyComponent(
    bool collidesWithTiles,
    bool collidesWithEntities,
    bool ignoresLiquids,
    bool canFallThroughPlatforms = false,
    bool isFallingThroughPlatforms = false,
    bool ignoresDoors = false,
    bool ignoresAetheriumPlatforms = false,
    bool allowsHoikTraversal = true,
    SlopeCollisionMode slopeMode = SlopeCollisionMode.Default)
  {
    CollidesWithTiles = collidesWithTiles;
    CollidesWithEntities = collidesWithEntities;
    IgnoresLiquids = ignoresLiquids;
    CanFallThroughPlatforms = canFallThroughPlatforms;
    IsFallingThroughPlatforms = isFallingThroughPlatforms;
    IgnoresDoors = ignoresDoors;
    IgnoresAetheriumPlatforms = ignoresAetheriumPlatforms;
    AllowsHoikTraversal = allowsHoikTraversal;
    SlopeMode = slopeMode;
  }

  public bool CollidesWithTiles;
  public bool CollidesWithEntities;
  public bool IgnoresLiquids;
  public bool CanFallThroughPlatforms;
  public bool IsFallingThroughPlatforms;
  public bool IgnoresDoors;
  public bool IgnoresAetheriumPlatforms;
  public bool AllowsHoikTraversal;
  public SlopeCollisionMode SlopeMode;

  public bool CanApplyFallThrough =>
    CanFallThroughPlatforms && IsFallingThroughPlatforms;
}
