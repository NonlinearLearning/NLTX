using System;
using System.Numerics;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;

namespace Terraria.Share.Physics;

/// <summary>
/// Calculates one entity's movement from detached state and a spatial frame. The system does
/// not access ECS storage and does not commit any component.
/// </summary>
public sealed class SpatialSimulationSystem
{
  public SpatialSimulationResult<TEntity> Simulate<TEntity>(
      in SpatialFrame frame,
      in SpatialEntitySnapshot<TEntity> entity,
      in MovementCapabilityComponent movement,
      in GravityCapabilityComponent gravityCapability,
      in GravityStateComponent gravity,
      in TileCollisionCapabilityComponent tileCollision,
      in LiquidInteractionCapabilityComponent liquidInteraction,
      in MovementIntent intent,
      float deltaTime)
      where TEntity : notnull
  {
    if (intent.Tick != frame.Tick)
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.Conflict,
          "The movement intent belongs to another simulation tick.");
    }

    if (!float.IsFinite(deltaTime) || deltaTime <= 0f)
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.NotReady,
          "The spatial delta time must be finite and positive.");
    }

    if (!entity.Collider.Enabled || !movement.Enabled)
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.NotReady,
          "The entity does not have an enabled movement or collider capability.");
    }

    if (gravity.Direction is not (GravityDirection.Normal or GravityDirection.Reversed))
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.NotReady,
          "The spatial gravity direction is not resolved.");
    }

    Vector2 velocity = movement.Mode == MovementMode.Flight || movement.Mode == MovementMode.Free
        ? ResolveFreeVelocity(entity.Velocity.LinearVelocity, intent, movement, deltaTime)
        : ResolveGroundVelocity(entity.Velocity.LinearVelocity, intent, movement, deltaTime);
    bool jumpConsumed = intent.JumpRequested && movement.CanJump && entity.Grounding.IsGrounded;
    if (jumpConsumed)
    {
      float jumpSpeed = intent.JumpSpeed > 0f ? intent.JumpSpeed : movement.JumpSpeed;
      velocity.Y = -jumpSpeed * (gravity.Direction == GravityDirection.Reversed ? -1f : 1f);
    }

    if (gravityCapability.Enabled && movement.Mode != MovementMode.Flight)
    {
      velocity = GravityIntegration.Apply(velocity, gravity, deltaTime);
    }

    // 子步：把同一段位移切成 N 段分别碰撞。总位移不变，只是碰撞检测更细，
    // 因此高速实体不会一帧跨过整面墙。撞到东西就停在该子步，剩余子步不再推进——
    // 否则会沿着墙继续滑，等价于把碰撞结果又抹掉。
    int substepCount = Math.Max(1, entity.Substep.Count);
    float substepDelta = deltaTime / substepCount;
    Vector2 position = entity.Location.Coordinates;
    Vector2 motionVelocity = velocity;
    bool collidedX = false;
    bool collidedY = false;
    int contactSubstep = 0;
    TileContactGeometry contact = default;
    for (int substep = 0; substep < substepCount; substep++)
    {
      Vector2 step = motionVelocity * substepDelta;
      SpatialTileMotionResult stepResult = tileCollision.Enabled
          ? frame.World.Move(position, step, entity.Collider.Size, tileCollision, gravity.Direction)
          : new SpatialTileMotionResult(position + step, step, false, false);
      if (!stepResult.IsFinite)
      {
        return SpatialSimulationResult<TEntity>.Rejected(
            entity.Entity,
            frame.Revision,
            SpatialSimulationStatus.Rejected,
            "The world returned a non-finite spatial result.");
      }

      position = stepResult.Position;
      if (tileCollision.Enabled)
      {
        motionVelocity = stepResult.Velocity / substepDelta;
      }

      collidedX |= stepResult.CollidedX;
      collidedY |= stepResult.CollidedY;
      if (stepResult.CollidedX || stepResult.CollidedY)
      {
        // 记住"第几个子步撞的"：命中顺序（先撞谁）只能靠这个序号表达。
        contactSubstep = substep;
        contact = stepResult.Contact;
        break;
      }
    }

    // 重新组装时要把接触几何一起带上，否则子步循环里查到的"撞了哪块瓦片"就丢了。
    // 位移的时间基是整帧 deltaTime 而不是 substepDelta：motionVelocity 已经是"每子步速度"
    // （按 substepDelta 归一），再乘 substepDelta 会让多子步实体的本帧位移只剩 1/substepCount，
    // 回写速度随之被均分——extraUpdates=1 的弹体每 tick 掉一半速度。单子步时两者相等，
    // 因此 NPC 与玩家路径的既有行为不变。
    SpatialTileMotionResult motion = new(
        position, motionVelocity * deltaTime, collidedX, collidedY, contact);
    if (!motion.IsFinite)
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.Rejected,
          "The world returned a non-finite spatial result.");
    }

    SpatialLiquidContact liquid = liquidInteraction.Enabled
        ? frame.World.GetLiquidContact(
            motion.Position,
            entity.Collider.Size,
            liquidInteraction)
        : new SpatialLiquidContact(LiquidContactFlags.None, 0);
    GroundingComponent grounding = ResolveGrounding(motion, gravity, velocity.Y);
    var commit = new SpatialCommit(
        new Terraria.Share.Entities.LocationComponent(motion.Position),
        new VelocityComponent(motion.Velocity / deltaTime),
        grounding,
        liquid.ToComponent(),
        motion.Contact,
        velocity,
        contactSubstep);
    return SpatialSimulationResult<TEntity>.Applied(
        entity.Entity,
        frame.Revision,
        commit,
        jumpConsumed);
  }

  private static Vector2 ResolveGroundVelocity(
      Vector2 current,
      MovementIntent intent,
      MovementCapabilityComponent movement,
      float deltaTime)
  {
    Vector2 desired = intent.HasDesiredVelocity ? intent.DesiredVelocity : current;
    if (intent.Source == MovementIntentSource.Ai)
    {
      // NPC AI owns horizontal steering. Vertical motion remains owned by gravity and the
      // collision step; forcing an AI's usual y=0 target would cancel a fall every tick.
      desired.Y = current.Y;
    }
    float maximum = movement.MaximumSpeed;
    desired.X = Math.Clamp(desired.X, -maximum, maximum);
    desired.Y = Math.Clamp(desired.Y, -maximum, maximum);
    return Terraria.Share.Physics.SimulationVector.MoveTowards(current, desired, movement.Acceleration * deltaTime);
  }

  private static Vector2 ResolveFreeVelocity(
      Vector2 current,
      MovementIntent intent,
      MovementCapabilityComponent movement,
      float deltaTime)
  {
    Vector2 desired = intent.HasDesiredVelocity ? intent.DesiredVelocity : current;
    if (movement.MaximumSpeed > 0f && desired.LengthSquared() > movement.MaximumSpeed * movement.MaximumSpeed)
    {
      desired = Vector2.Normalize(desired) * movement.MaximumSpeed;
    }

    return Terraria.Share.Physics.SimulationVector.MoveTowards(current, desired, movement.Acceleration * deltaTime);
  }

  /// <summary>
  /// 由本帧的 Y 轴碰撞结果推导着地状态。
  /// </summary>
  /// <remarks>
  /// 只有「朝重力方向运动时发生的 Y 碰撞」才算着地。仅凭 <c>CollidedY</c> 会把撞到天花板
  /// 也判成着地：向上跳跃撞到台阶上沿时垂直速度被清零、实体被当成站在地面，于是在空中悬停
  /// 并能再次起跳。墙面贴靠同样不能算着地。
  /// <para>副作用登记：纯函数，只读取入参并构造新组件，不修改任何状态。</para>
  /// </remarks>
  private static GroundingComponent ResolveGrounding(
      SpatialTileMotionResult motion,
      GravityStateComponent gravity,
      float verticalVelocity)
  {
    bool movingTowardSupport = gravity.Direction == GravityDirection.Reversed
        ? verticalVelocity <= 0f
        : verticalVelocity >= 0f;
    bool grounded = motion.CollidedY && movingTowardSupport;
    Vector2 normal = grounded
        ? gravity.Direction == GravityDirection.Reversed ? -Vector2.UnitY : Vector2.UnitY
        : Vector2.Zero;
    return new GroundingComponent(grounded, false, normal);
  }
}
