using System.Numerics;
using EntityEcs.Components;

namespace Terraria.LeashedEntity;

/// <summary>
/// Stores shared geometric state for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
/// <remarks>
/// <para>职责：保存拴系实体的位置、速度、朝向和尺寸。</para>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>
/// 主要源成员：position（第 10 行）； velocity（第 12 行）； direction（第 20 行）； width（第 22 行）； height（第 24 行）。
/// </para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntities.LeashedCritter。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.LeashedEntities/LeashedCritter.cs。
/// </para>
/// <para>主要源成员：_dummy（第 13 行）。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntities.LeashedKite。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.LeashedEntities/LeashedKite.cs。
/// </para>
/// <para>主要源成员：_dummy（第 13 行）。</para>
/// <para>重组说明：运动数据从原小动物的 NPC 替身和风筝的 Projectile 替身中独立表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-leashed-entity-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 182 行。</para>
/// </remarks>
public struct LeashedEntityMotionComponent
{
  /// <summary>
  /// Position candidate mapped to the existing NLTX component.
  /// </summary>
  public LocationComponent Position;

  /// <summary>
  /// Velocity candidate mapped to the existing NLTX component.
  /// </summary>
  public VelocityComponent Velocity;

  /// <summary>
  /// Horizontal direction candidate mapped to the existing NLTX component.
  /// </summary>
  public DirectionComponent Direction;

  /// <summary>
  /// Width in the shared geometry unit.
  /// </summary>
  public int Width;

  /// <summary>
  /// Height in the shared geometry unit.
  /// </summary>
  public int Height;

  /// <summary>
  /// Derived geometry; do not store or network-sync as another authority.
  /// </summary>
  public Vector2 Center => new(
    Position.X + Width / 2.0f,
    Position.Y + Height / 2.0f);

  /// <summary>
  /// Derived geometry; do not store or network-sync as another authority.
  /// </summary>
  public Vector2 Size => new(Width, Height);

  public LeashedEntityMotionComponent(
    LocationComponent position,
    VelocityComponent velocity,
    DirectionComponent direction,
    int width,
    int height)
  {
    Position = position;
    Velocity = velocity;
    Direction = direction;
    Width = width;
    Height = height;
  }
}
