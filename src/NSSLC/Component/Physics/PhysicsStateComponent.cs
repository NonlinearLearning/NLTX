using System.Numerics;

namespace Terraria.Physics;

/// <summary>
/// 保存加速度、重力缩放、接地和移动锁定状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Player 的重力、移动约束和接地求解流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>重组说明：重力缩放、接地和锁定标记是通用物理模型新增的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 410 行。</para>
/// </remarks>
public struct PhysicsStateComponent
{
  public Vector2 Acceleration;
  public GravityDirection GravityDirection;
  public float GravityScale;
  public bool IsGrounded;
  public bool IsMovementLocked;

  public bool IsGravityInverted => GravityDirection == GravityDirection.Up;
}
