using System.Collections.Generic;
using System.Numerics;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.Physics;

/// <summary>
/// 保存一次碰撞求解的接触对象、阻挡轴、法线和登阶结果。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Collision 的碰撞与登阶求解流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Collision.cs。</para>
/// <para>重组说明：接触集合、阻挡轴、法线和求解时刻是拆分后的结果模型。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 36 行。</para>
/// </remarks>
public struct CollisionResultComponent
{
  public List<EntityReference>? TouchedEntities;
  public List<TileCoordinate>? TouchedTiles;

  public CollisionResultComponent()
    : this(collidedOnX: false, collidedOnY: false)
  {
  }

  public CollisionResultComponent(bool collidedOnX, bool collidedOnY)
  {
    CollidedOnX = collidedOnX;
    CollidedOnY = collidedOnY;
    BlockedAxes = (collidedOnX ? CollisionAxisMask.Horizontal : CollisionAxisMask.None) |
      (collidedOnY ? CollisionAxisMask.Vertical : CollisionAxisMask.None);
  }

  public Vector2 BlockingNormal;
  public CollisionAxisMask BlockedAxes;
  public bool CollidedOnX;
  public bool CollidedOnY;
  public bool DidStepUp;
  public long? ResolvedAtTick;
  public bool HasEntityContact => TouchedEntities is { Count: > 0 };
  public bool HasTileContact => TouchedTiles is { Count: > 0 };
}
