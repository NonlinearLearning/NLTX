using System.Numerics;
using Terraria.WorldStorage;
using static MinimalImplementation.Server.Protocol.Version4ModulePayloads;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Additional world facts required by the Version4 kite visual simulation.</summary>
public interface ILeashedKiteEnvironment : ILeashedEntityEnvironment
{
  float WindTarget { get; }

  float CloudAlpha { get; }

  Vector2 GetProjectileSize(int projectileType);

  bool IsWindArea(Vector2 position, Vector2 size);

  bool IsSolidArea(Vector2 position, Vector2 size);
}

/// <summary>
/// Server-side kite motion and Full/Partial projection. Wind and collision facts are captured by
/// an explicit environment port; this service does not read global time or visual state.
/// </summary>
public sealed class LeashedKiteMotionService : ILeashedEntityMotionPort
{
  private const float TickSeconds = 1f / 60f;
  private const float DefaultKiteDistance = 250f;
  private const float NoWindKiteDistance = 48f;

  private readonly Func<ILeashedKiteEnvironment> _captureEnvironment;

  public LeashedKiteMotionService(Func<ILeashedKiteEnvironment> captureEnvironment)
  {
    _captureEnvironment = captureEnvironment
      ?? throw new ArgumentNullException(nameof(captureEnvironment));
  }

  public bool TryAdvance(
    LeashedEntityRuntimeSnapshot entity,
    long tick,
    out LeashedEntityMotionStep? step)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentOutOfRangeException.ThrowIfNegative(tick);
    if (entity.Type != 1 || entity.State is not KiteLeashedEntityState kite ||
        kite.ProjectileType is not int projectileType)
    {
      throw new ArgumentException("Kite motion requires a type-1 Full state.", nameof(entity));
    }
    if (entity.Motion is not null && tick <= entity.Motion.LastTick)
    {
      throw new InvalidOperationException("A leashed tick must advance monotonically.");
    }

    ILeashedKiteEnvironment environment = _captureEnvironment.Invoke();
    if (environment.WorldScope != entity.Handle.WorldScope ||
        environment.Width <= 0 || environment.Height <= 0)
    {
      throw new InvalidOperationException("The kite environment belongs to a different world.");
    }
    Vector2 size = environment.GetProjectileSize(projectileType);
    if (!float.IsFinite(size.X) || !float.IsFinite(size.Y) || size.X < 1 || size.Y < 1)
    {
      throw new InvalidOperationException("The kite projectile size is invalid.");
    }

    Vector2 position = new(kite.PositionX, kite.PositionY);
    Vector2 velocity = LeashedPackedVector.Unpack(kite.PackedVelocity);
    float windTarget = float.IsFinite(environment.WindTarget) ? environment.WindTarget : 0;
    float windCurrent = float.IsFinite(kite.WindTarget)
      ? Lerp(kite.WindTarget, windTarget, 0.05f)
      : windTarget;
    bool inWind = environment.IsWindArea(position, size);
    if (!inWind)
    {
      windCurrent = 0;
      windTarget = 0;
    }

    float timeCounter = kite.TimeCounter + TickSeconds;
    float distance = inWind ? DefaultKiteDistance : NoWindKiteDistance;
    Vector2 anchor = new(entity.Anchor.X * 16f + 8f, entity.Anchor.Y * 16f + 8f);
    Vector2 windVector = new(windCurrent, windCurrent >= 0 ? -2f : 2f);
    if (windVector.LengthSquared() > 0)
    {
      windVector = Vector2.Normalize(windVector) * distance;
    }
    Vector2 target = anchor + windVector;
    Vector2 delta = target - position;
    if (delta.LengthSquared() > 0)
    {
      Vector2 acceleration = Vector2.Normalize(delta) * 0.12f;
      velocity += acceleration;
    }
    if (velocity.Length() > 4f)
    {
      velocity = Vector2.Normalize(velocity) * 4f;
    }
    Vector2 next = position + velocity;
    if (environment.IsSolidArea(next, size))
    {
      next = MoveTowards(next, anchor, 2f);
      velocity *= 0.5f;
    }
    if (!float.IsFinite(next.X) || !float.IsFinite(next.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y))
    {
      throw new InvalidOperationException("The kite motion left the finite world range.");
    }

    byte rotation = ToRotationByte(velocity);
    KiteLeashedEntityState partial = kite with
    {
      ProjectileType = null,
      PositionX = next.X,
      PositionY = next.Y,
      PackedVelocity = LeashedPackedVector.Pack(velocity),
      Rotation = rotation,
      WindTarget = windTarget,
      CloudAlpha = float.IsFinite(environment.CloudAlpha) ? environment.CloudAlpha : 0,
      TimeCounter = timeCounter
    };
    if (!LeashedPackedVector.IsFinite(partial.PackedVelocity))
    {
      throw new InvalidOperationException("The kite velocity cannot be represented by the wire.");
    }
    step = new LeashedEntityMotionStep(
      partial,
      new LeashedEntityMotionState(next, velocity, Math.Sign(velocity.X), tick));
    return true;
  }

  public static KiteLeashedEntityState CreateSpawnState(
    int projectileType,
    TileCoordinate anchor,
    ILeashedKiteEnvironment environment)
  {
    ArgumentNullException.ThrowIfNull(environment);
    if (projectileType < 0 || anchor.X < 0 || anchor.Y < 0 ||
        anchor.X >= environment.Width || anchor.Y >= environment.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(projectileType));
    }
    Vector2 position = new(anchor.X * 16f + 8f, anchor.Y * 16f + 8f);
    Vector2 velocity = new(0, -5f);
    var state = new KiteLeashedEntityState(
      projectileType,
      position.X,
      position.Y,
      LeashedPackedVector.Pack(velocity),
      0,
      float.IsFinite(environment.WindTarget) ? environment.WindTarget : 0,
      float.IsFinite(environment.CloudAlpha) ? environment.CloudAlpha : 0,
      0);
    if (!LeashedPackedVector.IsFinite(state.PackedVelocity))
    {
      throw new ArgumentException("The kite spawn velocity is not representable.", nameof(environment));
    }
    return state;
  }

  private static byte ToRotationByte(Vector2 velocity)
  {
    if (velocity.LengthSquared() <= 0)
    {
      return 0;
    }
    double turns = Math.Atan2(velocity.Y, velocity.X) / (Math.PI * 2d);
    turns -= Math.Floor(turns);
    return (byte)Math.Clamp((int)Math.Round(turns * 256d), 0, byte.MaxValue);
  }

  private static float Lerp(float value, float target, float amount)
  {
    return value + (target - value) * amount;
  }

  private static Vector2 MoveTowards(Vector2 value, Vector2 target, float distance)
  {
    Vector2 delta = target - value;
    float length = delta.Length();
    return length <= distance || length == 0 ? target : value + delta * (distance / length);
  }
}
