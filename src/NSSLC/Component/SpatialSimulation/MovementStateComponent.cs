using System;
using System.Numerics;

using Terraria.Physics;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.MOVEMENT_STATE
// crossSubsystemOwner: integration-review
/// <summary>
/// Invocation-scoped input/output for movement algorithms. Runtime entities assemble this value
/// from spatial components and do not attach it as persistent component state.
/// </summary>
/// <remarks>
/// <para>职责：保存空间主体的位置、速度、加速度、重力和移动约束。</para>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：position（第 10 行）； velocity（第 12 行）。</para>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：gravDir（第 1389 行）； gravity（第 1927 行）。</para>
/// <para>重组说明：加速度、接地和移动锁定在空间模型中显式保存。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 55 行。</para>
/// </remarks>
public struct MovementStateComponent
{
  public MovementStateComponent(
    Vector2 position = default,
    Vector2 velocity = default,
    Vector2 acceleration = default,
    GravityDirection gravityDirection = GravityDirection.Down,
    float gravityScale = 0.0f,
    bool isGrounded = false,
    bool isMovementLocked = false)
  {
    EnsureFinite(position, nameof(position));
    EnsureFinite(velocity, nameof(velocity));
    EnsureFinite(acceleration, nameof(acceleration));

    if (!float.IsFinite(gravityScale))
    {
      throw new ArgumentOutOfRangeException(
        nameof(gravityScale),
        gravityScale,
        "GravityScale must be finite.");
    }

    Position = position;
    Velocity = velocity;
    Acceleration = acceleration;
    GravityDirection = gravityDirection;
    GravityScale = gravityScale;
    IsGrounded = isGrounded;
    IsMovementLocked = isMovementLocked;
  }

  public Vector2 Position;
  public Vector2 Velocity;
  public Vector2 Acceleration;
  public GravityDirection GravityDirection;
  public float GravityScale;
  public bool IsGrounded;
  public bool IsMovementLocked;

  public bool IsGravityInverted =>
    GravityDirection == GravityDirection.Up;

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "Vector components must be finite.");
    }
  }
}
