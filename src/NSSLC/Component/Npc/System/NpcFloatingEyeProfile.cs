using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// Pure state transition for the source type=2 / netID=2 / aiStyle=2 Demon Eye path.
/// Discouragement, target lookup, dust, and other effects remain ordered owner effects.
/// </summary>
public static class NpcFloatingEyeProfile
{
  private const float DefaultHorizontalSpeed = 4f;
  private const float DefaultVerticalSpeed = 1.5f;
  private const int DustChance = 40;

  public static bool CanHandle(int typeId, int netId, int aiStyle)
  {
    return typeId == 2 && netId == 2 && aiStyle == 2;
  }

  public static NpcFloatingEyeProfileResult Evaluate(
    in NpcFloatingEyeProfileInput input)
  {
    return EvaluateCore(in input, randomPort: null);
  }

  private static NpcFloatingEyeProfileResult EvaluateCore(
    in NpcFloatingEyeProfileInput input,
    INpcFloatingEyeRandomPort? randomPort)
  {
    if (!CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Floating Eye profile requires type=2, netID=2, and aiStyle=2.");
    }

    Vector2 velocity = input.Velocity;
    int direction = input.Direction;
    int directionY = input.DirectionY;
    bool despawnEncouragementRequested = false;
    bool targetClosestRequested = false;
    bool wetTargetClosestRequested = false;
    NpcFloatingEyeSourceBranch branches = NpcFloatingEyeSourceBranch.None;

    if (!input.NoTileCollide)
    {
      if (input.CollideX)
      {
        velocity.X = input.OldVelocity.X * -0.5f;
        if (direction == -1 && velocity.X > 0f && velocity.X < 2f)
        {
          velocity.X = 2f;
        }

        if (direction == 1 && velocity.X < 0f && velocity.X > -2f)
        {
          velocity.X = -2f;
        }

        branches |= NpcFloatingEyeSourceBranch.CollisionBounce;
      }

      if (input.CollideY)
      {
        velocity.Y = input.OldVelocity.Y * -0.5f;
        if (velocity.Y > 0f && velocity.Y < 1f)
        {
          velocity.Y = 1f;
        }

        if (velocity.Y < 0f && velocity.Y > -1f)
        {
          velocity.Y = -1f;
        }

        branches |= NpcFloatingEyeSourceBranch.CollisionBounce;
      }
    }

    bool discouraged = !input.ZoneGraveyard && input.DayTime &&
      input.Position.Y <= input.WorldSurfacePixels;
    if (discouraged)
    {
      despawnEncouragementRequested = true;
      directionY = -1;
      direction = velocity.Y > 0f ? 1 : -1;
      if (velocity.X > 0f)
      {
        direction = 1;
      }

      branches |= NpcFloatingEyeSourceBranch.Discouraged;
    }
    else
    {
      targetClosestRequested = true;
      branches |= NpcFloatingEyeSourceBranch.TargetSelection;
    }

    float horizontalSpeed = DefaultHorizontalSpeed * (1f + (1f - input.Scale));
    float verticalSpeed = DefaultVerticalSpeed * (1f + (1f - input.Scale));
    if (direction == -1 && velocity.X > -horizontalSpeed)
    {
      velocity.X -= 0.1f;
      if (velocity.X > horizontalSpeed)
      {
        velocity.X -= 0.1f;
      }
      else if (velocity.X > 0f)
      {
        velocity.X += 0.05f;
      }

      velocity.X = MathF.Max(velocity.X, -horizontalSpeed);
      branches |= NpcFloatingEyeSourceBranch.GenericAcceleration;
    }
    else if (direction == 1 && velocity.X < horizontalSpeed)
    {
      velocity.X += 0.1f;
      if (velocity.X < -horizontalSpeed)
      {
        velocity.X += 0.1f;
      }
      else if (velocity.X < 0f)
      {
        velocity.X -= 0.05f;
      }

      velocity.X = MathF.Min(velocity.X, horizontalSpeed);
      branches |= NpcFloatingEyeSourceBranch.GenericAcceleration;
    }

    if (directionY == -1 && velocity.Y > -verticalSpeed)
    {
      velocity.Y -= 0.04f;
      if (velocity.Y > verticalSpeed)
      {
        velocity.Y -= 0.05f;
      }
      else if (velocity.Y > 0f)
      {
        velocity.Y += 0.03f;
      }

      velocity.Y = MathF.Max(velocity.Y, -verticalSpeed);
      branches |= NpcFloatingEyeSourceBranch.GenericAcceleration;
    }
    else if (directionY == 1 && velocity.Y < verticalSpeed)
    {
      velocity.Y += 0.04f;
      if (velocity.Y < -verticalSpeed)
      {
        velocity.Y += 0.05f;
      }
      else if (velocity.Y < 0f)
      {
        velocity.Y -= 0.03f;
      }

      velocity.Y = MathF.Min(velocity.Y, verticalSpeed);
      branches |= NpcFloatingEyeSourceBranch.GenericAcceleration;
    }

    int? dustRoll = input.DustRoll;
    if (!dustRoll.HasValue && randomPort is not null)
    {
      dustRoll = randomPort.Next(DustChance);
    }

    bool dustRequested = dustRoll == 0;
    if (dustRequested)
    {
      branches |= NpcFloatingEyeSourceBranch.Dust;
    }

    if (input.Wet)
    {
      if (velocity.Y > 0f)
      {
        velocity.Y *= 0.95f;
      }

      velocity.Y = MathF.Max(velocity.Y - 0.5f, -4f);
      wetTargetClosestRequested = true;
      branches |= NpcFloatingEyeSourceBranch.WetMovement |
        NpcFloatingEyeSourceBranch.WetTargetSelection;
    }

    return new NpcFloatingEyeProfileResult(
      velocity,
      direction,
      directionY,
      NoGravity: true,
      NoTileCollide: input.NoTileCollide,
      despawnEncouragementRequested,
      targetClosestRequested,
      wetTargetClosestRequested,
      dustRequested,
      branches);
  }

  public static void ApplyEffects(
    in NpcFloatingEyeProfileInput input,
    in NpcFloatingEyeProfileResult result,
    INpcFloatingEyeProfileEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.DespawnEncouragementRequested)
    {
      effectPort.EncourageDespawn(10);
    }
    else if (result.TargetClosestRequested)
    {
      effectPort.RequestTargetReacquire();
    }

    if (result.DustRequested)
    {
      effectPort.SpawnDust(
        new Vector2(input.Position.X, input.Position.Y + input.Height * 0.25f),
        input.Width,
        (int)(input.Height * 0.5f),
        new Vector2(result.Velocity.X, 2f));
    }

    if (result.WetTargetClosestRequested)
    {
      effectPort.RequestTargetReacquire();
    }
  }

  public static NpcFloatingEyeProfileResult EvaluateWithRandom(
    in NpcFloatingEyeProfileInput input,
    INpcFloatingEyeRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    NpcFloatingEyeProfileInput randomizedInput = input with { DustRoll = null };
    return EvaluateCore(in randomizedInput, randomPort);
  }
}
