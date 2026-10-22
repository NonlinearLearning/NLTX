using System.Numerics;
using Terraria.WorldStorage;
using static MinimalImplementation.Server.Protocol.Version4ModulePayloads;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Dedicated-server Walker, Flyer and Jumper rules, including their derivatives.
/// Environment reads and seed creation belong to the caller; each step mutates only local state.
/// </summary>
/// <remarks>
/// Ported from D:/TRbackup/无任何删减通过编译. The designated Version4 tree contains stubbed
/// motion methods; equivalence between the two source trees has not been established.
/// </remarks>
public sealed class LeashedCritterMotionService : ILeashedEntityMotionPort
{
  private sealed class Critter
  {
    public readonly LeashedEntityRuntimeSnapshot Entity;
    public readonly ILeashedEntityEnvironment Environment;
    public readonly CritterLeashedEntityState Wire;
    public readonly LeashedRandom Random;
    public readonly Vector2 Size;
    public Vector2 Position;
    public Vector2 Velocity;
    public int Direction;
    public int SpriteDirection;
    public int TargetX;
    public int TargetY;
    public short Wait;
    public byte State;

    public Vector2 Center
    {
      get => Position + new Vector2((int)Size.X / 2, (int)Size.Y / 2);
      set => Position = value - new Vector2((int)Size.X / 2, (int)Size.Y / 2);
    }

    public Vector2 Target => TileCenter(TargetX, TargetY);

    public Critter(LeashedEntityRuntimeSnapshot entity, ILeashedEntityEnvironment environment)
    {
      Entity = entity;
      Environment = environment;
      Wire = entity.State as CritterLeashedEntityState
        ?? throw new ArgumentException("A critter requires its Full wire state.", nameof(entity));
      Size = new Vector2((int)Wire.SizeX!.Value, (int)Wire.SizeY!.Value);
      Position = CapturePosition(entity);
      Velocity = entity.Motion?.Velocity ?? Vector2.Zero;
      Direction = entity.Motion?.Direction ?? (Wire.DirectionPositive ? 1 : -1);
      SpriteDirection = entity.Motion?.SpriteDirection ?? 0;
      TargetX = entity.Anchor.X + Wire.TargetRelativeX;
      TargetY = entity.Anchor.Y + Wire.TargetRelativeY;
      Wait = Wire.WaitTime;
      State = Wire.State;
      Random = new LeashedRandom(Wire.RandomState);
    }

    public bool Solid(int x, int y)
    {
      return Environment.IsSolidTile2(x, y);
    }

    public bool Solid(Vector2 point)
    {
      return Solid((int)(point.X / 16), (int)(point.Y / 16));
    }

    public void Recall()
    {
      Center = TileCenter(Entity.Anchor.X, Entity.Anchor.Y) - new Vector2(0, 16);
      Velocity = Vector2.Zero;
    }

    public LeashedEntityMotionStep Capture(long tick)
    {
      CritterLeashedEntityState partial = Wire with
      {
        NpcType = null,
        SizeX = null,
        SizeY = null,
        PackedRelativePosition = LeashedPackedVector.Pack(
          Position - TileCenter(Entity.Anchor.X, Entity.Anchor.Y)),
        DirectionPositive = Direction > 0,
        RandomState = Random.State,
        WaitTime = Wait,
        State = State,
        TargetRelativeX = checked((sbyte)(TargetX - Entity.Anchor.X)),
        TargetRelativeY = checked((sbyte)(TargetY - Entity.Anchor.Y)),
        ButterflyVariant = null,
        OldPositionsLength = null
      };
      return new LeashedEntityMotionStep(partial,
        new LeashedEntityMotionState(Position, Velocity, SpriteDirection, tick, Direction));
    }
  }

  private sealed record FlyerRules(
    int MinimumWait, int MaximumWait, float MaximumSpeed, float Acceleration,
    int BrakeDuration, bool GroundBias);

  private sealed record JumperRules(
    int Range, int MinimumWait, int MaximumWait, float MaximumWidth, float MinimumWidth,
    float MaximumHeight, float MaximumDuration, int Cooldown, bool StandOnWater);

  private readonly Func<ILeashedEntityEnvironment> _captureEnvironment;

  public LeashedCritterMotionService(Func<ILeashedEntityEnvironment> captureEnvironment)
  {
    _captureEnvironment = captureEnvironment
      ?? throw new ArgumentNullException(nameof(captureEnvironment));
  }

  /// <summary>
  /// Returns the same top-left position used by TryAdvance, without reading the world or RNG.
  /// Retained authoritative motion takes precedence over the quantized wire fallback.
  /// </summary>
  public static Vector2 CapturePosition(LeashedEntityRuntimeSnapshot entity)
  {
    ArgumentNullException.ThrowIfNull(entity);
    if (entity.Type is < 2 or > 19 || entity.State is not CritterLeashedEntityState critter)
    {
      throw new ArgumentException("Position capture requires a critter runtime snapshot.",
        nameof(entity));
    }
    Vector2 position = entity.Motion?.Position ??
      TileCenter(entity.Anchor.X, entity.Anchor.Y) +
      LeashedPackedVector.Unpack(critter.PackedRelativePosition);
    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
    {
      throw new ArgumentException("The critter position is not finite.", nameof(entity));
    }
    return position;
  }

  public bool TryAdvance(
    LeashedEntityRuntimeSnapshot entity,
    long tick,
    out LeashedEntityMotionStep? step)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentOutOfRangeException.ThrowIfNegative(tick);
    if (entity.Type is < 2 or > 19)
    {
      throw new NotSupportedException("The critter motion service does not run kite prototypes.");
    }
    if (entity.Motion is not null && tick <= entity.Motion.LastTick)
    {
      throw new InvalidOperationException("A leashed tick must advance monotonically.");
    }
    ILeashedEntityEnvironment environment = _captureEnvironment.Invoke();
    if (environment.WorldScope != entity.Handle.WorldScope ||
        environment.Width <= 0 || environment.Height <= 0)
    {
      throw new InvalidOperationException("Leashed environment belongs to a different world.");
    }
    var critter = new Critter(entity, environment);
    if (entity.Type <= 5)
    {
      AdvanceWalker(critter);
    }
    else if (entity.Type <= 17)
    {
      AdvanceFlyer(critter, GetFlyerRules(entity.Type));
    }
    else
    {
      AdvanceJumper(critter, GetJumperRules(entity.Type));
    }
    step = critter.Capture(tick);
    return true;
  }

  /// <summary>Creates spawn state from authoritative NPC defaults and an explicit host seed.</summary>
  public static CritterLeashedEntityState CreateSpawnState(
    byte type, int npcType, float width, float height, TileCoordinate anchor,
    uint seed, ILeashedEntityEnvironment environment,
    byte? butterflyVariant = null, byte? oldPositionsLength = null)
  {
    return (CritterLeashedEntityState)CreateSpawn(type, npcType, width, height, anchor,
      seed, environment, 0, butterflyVariant, oldPositionsLength).FullState;
  }

  /// <summary>Retains the exact spawn position and Flyer hover velocity for the first tick.</summary>
  public static LeashedEntitySpawnState CreateSpawn(
    byte type, int npcType, float width, float height, TileCoordinate anchor,
    uint seed, ILeashedEntityEnvironment environment, long tick,
    byte? butterflyVariant = null, byte? oldPositionsLength = null)
  {
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentOutOfRangeException.ThrowIfNegative(tick);
    if (type is < 2 or > 19 || npcType < 0 ||
        !float.IsFinite(width) || !float.IsFinite(height) || width < 1 || height < 1 ||
        width >= int.MaxValue || height >= int.MaxValue ||
        anchor.X < 0 || anchor.Y < 0 || anchor.X >= environment.Width ||
        anchor.Y >= environment.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(type), "Invalid critter spawn defaults.");
    }
    if ((type == 7) != (butterflyVariant is not null) ||
        (type == 11) != (oldPositionsLength is not null))
    {
      throw new ArgumentException(
        "Butterfly requires its variant and ShimmerFly requires its old-position length.");
    }
    width = (int)width;
    height = (int)height;
    var random = new LeashedRandom(seed);
    Vector2 center = TileCenter(anchor.X, anchor.Y);
    Vector2 position = center - new Vector2((int)width / 2, (int)height / 2);
    Vector2 velocity = Vector2.Zero;
    int targetX = anchor.X;
    int targetY = anchor.Y;
    if (type is >= 6 and <= 17)
    {
      if (!environment.IsSolidTile2(anchor.X, anchor.Y + 1))
      {
        velocity.Y = 0.0001f;
      }
      PickFlyerTarget(random, anchor, ref targetX, ref targetY, GetFlyerRules(type));
    }
    else if (type >= 18)
    {
      PickJumperTarget(random, anchor, environment, ref targetX, ref targetY,
        GetJumperRules(type));
    }
    var full = new CritterLeashedEntityState(npcType, width, height,
      LeashedPackedVector.Pack(position - center),
      true, random.State, 0, 0,
      checked((sbyte)(targetX - anchor.X)), checked((sbyte)(targetY - anchor.Y)),
      butterflyVariant, oldPositionsLength);
    if (!LeashedPackedVector.IsFinite(full.PackedRelativePosition))
    {
      throw new ArgumentOutOfRangeException(nameof(width),
        "The critter spawn offset cannot be represented by the wire projection.");
    }
    return new LeashedEntitySpawnState(full,
      new LeashedEntityMotionState(position, velocity, 1, tick, 1));
  }

  /// <summary>
  /// Applies Spawn(false): reset center, target and RNG while retaining wait/state/direction
  /// and velocity. The caller supplies the next authoritative host seed explicitly.
  /// </summary>
  public static LeashedEntitySpawnState CreateRespawn(
    LeashedEntityRuntimeSnapshot entity,
    uint seed,
    ILeashedEntityEnvironment environment,
    long tick)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentOutOfRangeException.ThrowIfNegative(tick);
    if (entity.Type is < 2 or > 19 || entity.Motion is null ||
        entity.Handle.WorldScope != environment.WorldScope || tick < entity.Motion.LastTick ||
        environment.Width <= 0 || environment.Height <= 0 ||
        entity.Anchor.X < 0 || entity.Anchor.Y < 0 ||
        entity.Anchor.X >= environment.Width || entity.Anchor.Y >= environment.Height)
    {
      throw new ArgumentException("Critter respawn requires its authoritative motion and world.",
        nameof(entity));
    }
    var critter = new Critter(entity, environment);
    Vector2 center = TileCenter(entity.Anchor.X, entity.Anchor.Y);
    critter.Center = center;
    critter.TargetX = entity.Anchor.X;
    critter.TargetY = entity.Anchor.Y;
    var random = new LeashedRandom(seed);
    if (entity.Type is >= 6 and <= 17)
    {
      if (!environment.IsSolidTile2(entity.Anchor.X, entity.Anchor.Y + 1))
      {
        critter.Velocity.Y = 0.0001f;
      }
      PickFlyerTarget(random, entity.Anchor, ref critter.TargetX, ref critter.TargetY,
        GetFlyerRules(entity.Type));
    }
    else if (entity.Type >= 18)
    {
      PickJumperTarget(random, entity.Anchor, environment,
        ref critter.TargetX, ref critter.TargetY, GetJumperRules(entity.Type));
    }
    CritterLeashedEntityState full = critter.Wire with
    {
      PackedRelativePosition = LeashedPackedVector.Pack(critter.Position - center),
      DirectionPositive = critter.Direction > 0,
      RandomState = random.State,
      TargetRelativeX = checked((sbyte)(critter.TargetX - entity.Anchor.X)),
      TargetRelativeY = checked((sbyte)(critter.TargetY - entity.Anchor.Y))
    };
    return new LeashedEntitySpawnState(full,
      new LeashedEntityMotionState(critter.Position, critter.Velocity,
        critter.SpriteDirection, tick, critter.Direction));
  }

  private static void AdvanceWalker(Critter critter)
  {
    const int range = 3;
    int tileX = (int)(critter.Center.X / 16);
    int tileY = (int)(critter.Center.Y / 16);
    if (critter.Solid(tileX, tileY + 1))
    {
      critter.Velocity.Y = 0;
      if (critter.State is 3 or 4)
      {
        critter.Center = critter.Target;
      }
      if (critter.State == 3)
      {
        critter.State = 0;
        critter.Wait = 0;
      }
    }
    else
    {
      critter.Velocity.Y = Math.Min(critter.Velocity.Y + 0.3f, 10);
      critter.TargetX = tileX;
      critter.TargetY = Math.Min(tileY + 1, critter.Environment.Height - 1);
      if (critter.State != 4)
      {
        if (critter.TargetY - critter.Entity.Anchor.Y > range)
        {
          critter.State = 4;
          critter.Wait = 20;
        }
        else
        {
          critter.State = 3;
        }
      }
    }
    critter.Wait--;
    if (critter.Wait <= 0)
    {
      if (critter.State == 4)
      {
        critter.Recall();
      }
      critter.Wait = (short)critter.Random.Next(60, 61);
      critter.State = (byte)critter.Random.Next(0, 2);
    }
    if (critter.State is not (3 or 4))
    {
      float pace = critter.Entity.Type switch { 3 or 4 => 0.4f, 5 => 1.5f, _ => 0.8f };
      critter.Velocity.X = pace * critter.Direction;
      if (critter.State != 0 && Vector2.Distance(critter.Center, critter.Target) < 1)
      {
        if (critter.State == 1)
        {
          critter.Direction = critter.Random.Next(0, 2) * 2 - 1;
          critter.State = 2;
        }
        if (!AdvanceWalkerTarget(critter, range))
        {
          critter.Wait = 30;
          critter.State = 0;
        }
      }
    }
    int deltaY = critter.TargetY - tileY;
    if (Math.Abs(critter.TargetX - tileX) == 1 && Math.Abs(deltaY) == 1)
    {
      critter.Velocity.Y = deltaY * 2;
    }
    critter.Center = MoveTowards(critter.Center, critter.Target, critter.Velocity.Length());
    if (critter.Center == critter.Target && critter.State == 0)
    {
      critter.Velocity = Vector2.Zero;
    }
  }

  private static bool AdvanceWalkerTarget(Critter critter, int range)
  {
    if (Math.Abs(critter.TargetX - critter.Entity.Anchor.X) >= range)
    {
      critter.Direction = Math.Sign(critter.Entity.Anchor.X - critter.TargetX);
    }
    if (critter.TargetX + critter.Direction < 0 ||
        critter.TargetX + critter.Direction >= critter.Environment.Width ||
        critter.TargetY < 0 || critter.TargetY >= critter.Environment.Height)
    {
      critter.Direction *= -1;
    }
    critter.SpriteDirection = critter.Direction;
    int x = critter.TargetX + critter.Direction;
    int y = critter.TargetY;
    bool aboveFree = !critter.Solid(x, y - 1);
    bool free = !critter.Solid(x, y);
    bool belowFree = !critter.Solid(x, y + 1);
    if (aboveFree && !free)
    {
      critter.TargetY--;
    }
    else if (free && belowFree && !critter.Environment.HasLiquid(x, y + 1) &&
        critter.Solid(x, y + 2))
    {
      critter.TargetY++;
    }
    else if (!(free && !belowFree))
    {
      return false;
    }
    critter.TargetX = x;
    return true;
  }

  private static FlyerRules GetFlyerRules(byte type)
  {
    return type switch
    {
      12 => new(10, 300, 2.5f, 0.4f, 10, false),
      14 or 15 => new(120, 420, 1.2f, 0.1f, 10, type == 15),
      16 => new(120, 300, 0.5f, 0.015f, 10, false),
      17 => new(30, 90, 1.1f, 0.05f, 30, false),
      _ => new(60, 300, 1, 0.2f, 10, type == 13)
    };
  }

  private static void AdvanceFlyer(Critter critter, FlyerRules rules)
  {
    critter.Wait--;
    if (critter.Wait <= 0)
    {
      critter.Wait = (short)critter.Random.Next(rules.MinimumWait, rules.MaximumWait + 1);
      PickFlyerTarget(critter.Random, critter.Entity.Anchor,
        ref critter.TargetX, ref critter.TargetY, rules);
    }
    Vector2 delta = critter.Target - critter.Center;
    float distance = delta.Length();
    Vector2 toward = distance == 0 ? Vector2.Zero : delta / distance;
    critter.Velocity += toward * rules.Acceleration;
    float speed = critter.Velocity.Length();
    float maximum = rules.MaximumSpeed * Math.Max(
      Math.Min(1, distance / (rules.BrakeDuration * rules.MaximumSpeed)), 0.25f);
    if (speed > maximum)
    {
      critter.Velocity *= maximum / speed;
    }
    bool arrived = distance < rules.MaximumSpeed;
    if (arrived || critter.Solid(critter.Center + critter.Size * 0.5f * toward + critter.Velocity))
    {
      if (arrived)
      {
        critter.Center = critter.Target;
      }
      critter.Velocity = new Vector2(0,
        critter.Solid((int)(critter.Center.X / 16), (int)(critter.Center.Y / 16) + 1)
          ? 0 : 0.0001f);
    }
    else
    {
      critter.Center += critter.Velocity;
      if (critter.Velocity.Y == 0 &&
          !critter.Solid((int)(critter.Center.X / 16), (int)(critter.Center.Y / 16) + 1))
      {
        critter.Velocity.Y = 0.0001f;
      }
    }
    int direction = Math.Sign(critter.Velocity.X);
    if (direction != 0 && direction != critter.Direction)
    {
      critter.Direction = direction;
      critter.SpriteDirection = -direction;
    }
    if (critter.Wire.NpcType is 356 or 444 or 653 or 661)
    {
      critter.SpriteDirection = critter.Direction;
    }
  }

  private static void PickFlyerTarget(
    LeashedRandom random, TileCoordinate anchor, ref int x, ref int y, FlyerRules rules)
  {
    bool stayOnGround = rules.GroundBias && anchor.Y == y && random.Next(0, 4) != 0;
    x = anchor.X + random.Next(-7, 8);
    y = anchor.Y + random.Next(-7, 1);
    if (stayOnGround)
    {
      y = anchor.Y;
    }
  }

  private static JumperRules GetJumperRules(byte type)
  {
    return type == 19
      ? new(5, 60, 120, 32, 8, 0, 14, 15, true)
      : new(12, 180, 300, 112, 48, 64, 30, 60, false);
  }

  private static void AdvanceJumper(Critter critter, JumperRules rules)
  {
    critter.Wait--;
    if (critter.Wait <= 0)
    {
      if (critter.State == 0 && !TryStartJump(critter, rules))
      {
        ResetJumpTarget(critter, rules);
      }
      else if (critter.State == 1)
      {
        critter.Recall();
        ResetJumpTarget(critter, rules);
        critter.State = 0;
      }
    }
    MoveJumper(critter, rules, out bool hit);
    if (hit && critter.State != 1)
    {
      ResetJumpTarget(critter, rules);
    }
    if (Vector2.Distance(critter.Target, critter.Center) < 8)
    {
      critter.Center = critter.Target;
      critter.Velocity = Vector2.Zero;
      ResetJumpTarget(critter, rules);
    }
    critter.SpriteDirection = critter.Direction;
  }

  private static void ResetJumpTarget(Critter critter, JumperRules rules)
  {
    PickJumperTarget(critter.Random, critter.Entity.Anchor, critter.Environment,
      ref critter.TargetX, ref critter.TargetY, rules);
    critter.Wait = (short)critter.Random.Next(rules.MinimumWait, rules.MaximumWait + 1);
  }

  private static bool TryStartJump(Critter critter, JumperRules rules)
  {
    Vector2 delta = critter.Target - critter.Center;
    if (-delta.Y > rules.MaximumHeight)
    {
      return false;
    }
    float width = Math.Min(Math.Abs(delta.X), rules.MaximumWidth);
    if (width <= rules.MinimumWidth)
    {
      return false;
    }
    critter.Direction = Math.Sign(delta.X);
    float proportion = width / rules.MaximumWidth;
    float duration = rules.MaximumDuration * proportion;
    critter.Velocity.X = width / duration * critter.Direction;
    critter.Velocity.Y = delta.Y * proportion / duration - 0.5f * 0.3f * duration;
    if (critter.Velocity.Y >= 0)
    {
      return false;
    }
    critter.Wait = (short)(duration + rules.Cooldown);
    return true;
  }

  private static void MoveJumper(Critter critter, JumperRules rules, out bool hit)
  {
    int tileX = (int)(critter.Center.X / 16);
    int tileY = (int)(critter.Center.Y / 16);
    int directionX = Math.Sign((int)critter.Velocity.X);
    int directionY = Math.Sign((int)critter.Velocity.Y);
    if (directionX != 0)
    {
      critter.Direction = directionX;
    }
    Vector2 next = critter.Center +
      new Vector2(directionX, directionY) * critter.Size * 0.5f + critter.Velocity;
    hit = critter.Solid(next);
    if (!hit)
    {
      if (critter.Velocity.Y >= 0 && next.Y % 16 >= 8 &&
          (critter.Solid(tileX, tileY + 1) ||
          (rules.StandOnWater && critter.Environment.HasLiquid(tileX, tileY + 1, 0))))
      {
        critter.Center = TileCenter(tileX, tileY);
        critter.Velocity = Vector2.Zero;
        return;
      }
      critter.Center += critter.Velocity;
      critter.Velocity.Y = Math.Min(critter.Velocity.Y + 0.3f, 10);
      if (critter.State != 1 && tileY - critter.Entity.Anchor.Y > rules.Range)
      {
        critter.State = 1;
        critter.Wait = 20;
      }
      return;
    }
    bool verticalHit = directionY != 0 && critter.Solid(tileX, tileY + directionY);
    bool horizontalHit = directionX != 0 && critter.Solid(tileX + directionX, tileY);
    if (verticalHit)
    {
      critter.Velocity.Y = 0;
    }
    if (horizontalHit)
    {
      critter.Velocity.X = 0;
    }
    if (!verticalHit && !horizontalHit)
    {
      critter.Velocity = Vector2.Zero;
    }
  }

  private static void PickJumperTarget(
    LeashedRandom random, TileCoordinate anchor, ILeashedEntityEnvironment environment,
    ref int x, ref int y, JumperRules rules)
  {
    int maximumTiles = (int)(rules.MaximumWidth / 16);
    int minimumTiles = (int)(rules.MinimumWidth / 16);
    int left = x - (anchor.X - rules.Range);
    int right = anchor.X + rules.Range - x;
    bool canLeft = left >= minimumTiles;
    bool canRight = right >= minimumTiles;
    if (!canLeft && !canRight)
    {
      return;
    }
    int direction = canLeft && canRight ? random.Next(0, 2) * 2 - 1 : canLeft ? -1 : 1;
    int available = direction < 1 ? left : right;
    int jumps = random.Next(1, available / maximumTiles + 1);
    int remainder = available % maximumTiles;
    if (remainder < minimumTiles)
    {
      remainder = 0;
    }
    int startX = x + (jumps * maximumTiles + remainder) * direction;
    int towardAnchor = Math.Sign(anchor.X - startX);
    if (towardAnchor == 0)
    {
      return;
    }
    for (int candidateX = startX; candidateX != anchor.X; candidateX += towardAnchor)
    {
      int candidateY = anchor.Y;
      if (environment.IsSolidTile2(candidateX, candidateY))
      {
        for (int step = 0; step < rules.MaximumHeight / 16; step++)
        {
          candidateY--;
          if (!environment.IsSolidTile2(candidateX, candidateY))
          {
            x = candidateX;
            y = candidateY;
            return;
          }
        }
        continue;
      }
      for (int step = 0; step < rules.Range; step++)
      {
        candidateY++;
        if (environment.IsSolidTile2(candidateX, candidateY) ||
            (rules.StandOnWater && environment.HasLiquid(candidateX, candidateY, 0)))
        {
          x = candidateX;
          y = candidateY - 1;
          return;
        }
      }
    }
  }

  private static Vector2 TileCenter(int x, int y)
  {
    return new Vector2(x * 16f + 8, y * 16f + 8);
  }

  private static Vector2 MoveTowards(Vector2 value, Vector2 target, float distance)
  {
    Vector2 delta = target - value;
    float length = delta.Length();
    return length <= distance || length == 0 ? target : value + delta * (distance / length);
  }
}
