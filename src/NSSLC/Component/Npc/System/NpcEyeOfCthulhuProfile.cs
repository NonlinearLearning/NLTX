using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// Implements the Eye of Cthulhu's opening, transformation, and early dash phases.
/// </summary>
public static class NpcEyeOfCthulhuProfile
{
  private const int DustChance = 5;
  private const int DespawnEncouragementTicks = 10;

  public static bool CanHandle(int typeId, int netId, int aiStyle)
  {
    return typeId == 4 && netId == 4 && aiStyle == 4;
  }

  public static NpcEyeOfCthulhuProfileResult Evaluate(
    in NpcEyeOfCthulhuProfileInput input)
  {
    if (!CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Eye of Cthulhu profile requires type=4, netID=4, and aiStyle=4.");
    }

    if (input.DustRoll < 0 || input.DustRoll >= DustChance)
    {
      throw new ArgumentOutOfRangeException(
        nameof(input),
        "The Eye of Cthulhu dust roll must be in [0, 5).");
    }

    bool dustRequested = input.DustRoll == 0;
    if (input.State.Ai0 == 1f || input.State.Ai0 == 2f)
    {
      return AdvanceTransformation(in input, dustRequested);
    }

    if (input.State.Ai0 != 0f)
    {
      return CreateUnsupportedResult(in input, dustRequested);
    }

    if (input.DayTime)
    {
      return CreateExitResult(
        in input,
        dustRequested,
        NpcEyeOfCthulhuExitReason.DayTime);
    }

    if (input.TargetIsDead)
    {
      return CreateExitResult(
        in input,
        dustRequested,
        NpcEyeOfCthulhuExitReason.TargetDead);
    }

    if (!input.TargetIsAvailable)
    {
      return CreateExitResult(
        in input,
        dustRequested,
        NpcEyeOfCthulhuExitReason.TargetUnavailable);
    }

    if (input.State.Ai1 < 0f || input.State.Ai1 > 2f)
    {
      return CreateUnsupportedResult(in input, dustRequested);
    }

    NpcEyeOfCthulhuProfileResult result = input.State.Ai1 switch
    {
      0f => AdvanceHover(in input, dustRequested),
      1f => BeginDash(in input, dustRequested),
      2f => AdvanceDash(in input, dustRequested),
      _ => throw new InvalidOperationException("The Eye of Cthulhu attack state is invalid."),
    };

    float phaseBoundary = input.ExpertMode ? 0.65f : 0.5f;
    if ((float)input.Life < (float)input.LifeMax * phaseBoundary)
    {
      result = result with
      {
        State = new NpcEyeOfCthulhuProfileState(1f, 0f, 0f, 0f),
        NetworkUpdateRequested = true,
      };
    }

    return result;
  }

  public static NpcEyeOfCthulhuProfileResult EvaluateWithRandom(
    in NpcEyeOfCthulhuProfileInput input,
    INpcEyeOfCthulhuRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    int dustRoll = randomPort.Next(DustChance);
    return Evaluate(input with { DustRoll = dustRoll });
  }

  public static void ApplyEffects(
    in NpcEyeOfCthulhuProfileInput input,
    in NpcEyeOfCthulhuProfileResult result,
    INpcEyeOfCthulhuProfileEffectPort effectPort)
  {
    ApplyEffects(in input, in result, effectPort, randomPort: null);
  }

  public static void ApplyEffects(
    in NpcEyeOfCthulhuProfileInput input,
    in NpcEyeOfCthulhuProfileResult result,
    INpcEyeOfCthulhuProfileEffectPort effectPort,
    INpcEyeOfCthulhuRandomPort? randomPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.DustRequested)
    {
      effectPort.SpawnDust(
        new Vector2(input.Position.X, input.Position.Y + input.Height * 0.25f),
        input.Width,
        (int)(input.Height * 0.5f),
        new Vector2(input.Velocity.X, 2f));
    }

    if (result.IsTransformationTick)
    {
      ApplyTransformationEffects(in input, in result, effectPort, randomPort);
      return;
    }

    if (result.ServantSummonRequested)
    {
      _ = effectPort.TrySpawnServant(
        result.ServantSummonPosition,
        result.ServantSummonVelocity);
      if (result.ServantSummonSoundRequested)
      {
        effectPort.PlaySound(3, result.ServantSummonPosition);
        for (int index = 0; index < result.ServantSummonDustCount; index++)
        {
          effectPort.SpawnDust(
            result.ServantSummonPosition,
            20,
            20,
            result.ServantSummonVelocity * 0.4f);
        }
      }
    }

    if (result.AttackIntent == NpcEyeOfCthulhuAttackIntent.Dash)
    {
      effectPort.BeginDash(result.Velocity);
    }

    if (result.ExitReason != NpcEyeOfCthulhuExitReason.None)
    {
      effectPort.EncourageDespawn(DespawnEncouragementTicks);
    }

    if (result.NetworkUpdateRequested)
    {
      effectPort.RequestNetworkUpdate();
    }
  }

  private static NpcEyeOfCthulhuProfileResult AdvanceHover(
    in NpcEyeOfCthulhuProfileInput input,
    bool dustRequested)
  {
    Vector2 npcCenter = input.Position + new Vector2(input.Width * 0.5f, input.Height * 0.5f);
    float targetX = input.TargetCenter.X - npcCenter.X;
    float targetY = input.TargetCenter.Y - 200f - npcCenter.Y;
    float distance = MathF.Sqrt(targetX * targetX + targetY * targetY);
    Vector2 velocity = input.Velocity;
    if (distance > 0f)
    {
      float speed = input.ExpertMode ? 7f : 5f;
      float acceleration = input.ExpertMode ? 0.15f : 0.04f;
      if (input.GetGoodWorld)
      {
        speed += 1f;
        acceleration += 0.05f;
      }

      float scale = speed / distance;
      velocity.X = Accelerate(velocity.X, targetX * scale, acceleration);
      velocity.Y = Accelerate(velocity.Y, targetY * scale, acceleration);
    }

    NpcEyeOfCthulhuProfileState state = input.State with { Ai2 = input.State.Ai2 + 1f };
    float hoverDuration = input.ExpertMode ? 210f : 600f;
    if (state.Ai2 >= hoverDuration)
    {
      state = state with { Ai1 = 1f, Ai2 = 0f, Ai3 = 0f };
      return new NpcEyeOfCthulhuProfileResult(
        IsSupported: true,
        state,
        velocity,
        input.DustRoll,
        dustRequested,
        NpcEyeOfCthulhuAttackIntent.None,
        NpcEyeOfCthulhuExitReason.None,
        TargetResetRequested: true,
        NetworkUpdateRequested: true);
    }

    bool targetInDashRange = distance < 500f;
    bool shouldAdvanceDashCount = targetInDashRange &&
      (input.Position.Y + input.Height < input.TargetPosition.Y || input.ExpertMode);
    if (!shouldAdvanceDashCount)
    {
      return new NpcEyeOfCthulhuProfileResult(
        IsSupported: true,
        state,
        velocity,
        input.DustRoll,
        dustRequested,
        NpcEyeOfCthulhuAttackIntent.None,
        NpcEyeOfCthulhuExitReason.None,
        TargetResetRequested: false,
        NetworkUpdateRequested: false);
    }

    float dashCount = state.Ai3 + 1f;
    float dashThreshold = 110f;
    if (input.ExpertMode)
    {
      dashThreshold *= 0.4f;
    }

    if (input.GetGoodWorld)
    {
      dashThreshold *= 0.8f;
    }

    if (dashCount < dashThreshold)
    {
      return new NpcEyeOfCthulhuProfileResult(
        IsSupported: true,
        state with { Ai3 = dashCount },
        velocity,
        input.DustRoll,
        dustRequested,
        NpcEyeOfCthulhuAttackIntent.None,
        NpcEyeOfCthulhuExitReason.None,
        TargetResetRequested: false,
        NetworkUpdateRequested: false);
    }

    Vector2 targetDelta = input.TargetCenter - npcCenter;
    float targetDistance = targetDelta.Length();
    float servantSpeed = input.ExpertMode ? 6f : 5f;
    Vector2 servantVelocity = targetDistance > 0f
      ? targetDelta * (servantSpeed / targetDistance)
      : Vector2.Zero;
    Vector2 servantPosition = npcCenter + servantVelocity * 10f;
    servantPosition = new Vector2((int)servantPosition.X, (int)servantPosition.Y);

    return new NpcEyeOfCthulhuProfileResult(
      IsSupported: true,
      state with { Ai3 = 0f },
      velocity,
      input.DustRoll,
      dustRequested,
      NpcEyeOfCthulhuAttackIntent.None,
      NpcEyeOfCthulhuExitReason.None,
      TargetResetRequested: false,
      NetworkUpdateRequested: false)
    {
      ServantSummonRequested = true,
      ServantSummonPosition = servantPosition,
      ServantSummonVelocity = servantVelocity,
      ServantSummonDustCount = 10,
      ServantSummonSoundRequested = true,
    };
  }

  private static NpcEyeOfCthulhuProfileResult AdvanceTransformation(
    in NpcEyeOfCthulhuProfileInput input,
    bool dustRequested)
  {
    NpcEyeOfCthulhuProfileState state = input.State;
    state = state.Ai0 == 1f || state.Ai3 == 1f
      ? state with { Ai2 = Math.Min(0.5f, state.Ai2 + 0.005f) }
      : state with { Ai2 = Math.Max(0f, state.Ai2 - 0.005f) };
    state = state with { Ai1 = state.Ai1 + 1f };

    int servantInterval = 20;
    if (input.GetGoodWorld && input.Life < input.LifeMax / 3)
    {
      servantInterval = 10;
    }

    bool summonServant = input.ExpertMode && state.Ai1 % servantInterval == 0f;
    bool transformationBurst = false;
    if (state.Ai1 >= 100f)
    {
      if (state.Ai3 == 1f)
      {
        state = state with { Ai3 = 0f, Ai1 = 0f };
      }
      else
      {
        state = state with { Ai0 = state.Ai0 + 1f, Ai1 = 0f };
        if (state.Ai0 == 3f)
        {
          state = state with { Ai2 = 0f };
        }
        else
        {
          transformationBurst = true;
        }
      }
    }

    Vector2 velocity = ClampSmallVelocity(input.Velocity * 0.98f);
    return new NpcEyeOfCthulhuProfileResult(
      IsSupported: true,
      state,
      velocity,
      input.DustRoll,
      dustRequested,
      NpcEyeOfCthulhuAttackIntent.None,
      NpcEyeOfCthulhuExitReason.None,
      TargetResetRequested: false,
      NetworkUpdateRequested: false)
    {
      ServantSummonRequested = summonServant,
      ServantSummonDustCount = summonServant ? 10 : 0,
      IsTransformationTick = true,
      TransformationBurstRequested = transformationBurst,
      ReflectsProjectiles = input.GetGoodWorld,
    };
  }

  private static void ApplyTransformationEffects(
    in NpcEyeOfCthulhuProfileInput input,
    in NpcEyeOfCthulhuProfileResult result,
    INpcEyeOfCthulhuProfileEffectPort effectPort,
    INpcEyeOfCthulhuRandomPort? randomPort)
  {
    if (result.ReflectsProjectiles)
    {
      effectPort.SetReflectsProjectiles(reflectsProjectiles: true);
    }

    if (result.ServantSummonRequested)
    {
      INpcEyeOfCthulhuRandomPort random = randomPort ??
        throw new ArgumentNullException(
          nameof(randomPort),
          "Transformation servant summons require the NPC random port.");
      float randomX = random.Next(-200, 200);
      float randomY = random.Next(-200, 200);
      if (input.GetGoodWorld)
      {
        randomX *= 3f;
        randomY *= 3f;
      }

      Vector2 randomDirection = new(randomX, randomY);
      float randomLength = randomDirection.Length();
      Vector2 velocity = randomLength > 0f
        ? randomDirection * (5f / randomLength)
        : Vector2.Zero;
      Vector2 center = input.Position + new Vector2(input.Width * 0.5f, input.Height * 0.5f);
      Vector2 position = center + velocity * 10f;
      position = new Vector2((int)position.X, (int)position.Y);
      _ = effectPort.TrySpawnServant(position, velocity);
      for (int index = 0; index < result.ServantSummonDustCount; index++)
      {
        effectPort.SpawnDust(position, 20, 20, velocity * 0.4f);
      }
    }

    INpcEyeOfCthulhuRandomPort? effectRandom = randomPort;
    if (result.TransformationBurstRequested)
    {
      INpcEyeOfCthulhuRandomPort random = effectRandom ??
        throw new ArgumentNullException(
          nameof(randomPort),
          "Transformation visuals require the NPC random port.");
      Vector2 integerPosition = new((int)input.Position.X, (int)input.Position.Y);
      effectPort.PlaySound(3, integerPosition);
      for (int index = 0; index < 2; index++)
      {
        SpawnTransformationGore(random, effectPort, 8, input.Position);
        SpawnTransformationGore(random, effectPort, 7, input.Position);
        SpawnTransformationGore(random, effectPort, 6, input.Position);
      }

      for (int index = 0; index < 20; index++)
      {
        Vector2 velocity = new(random.Next(-30, 31) * 0.2f, random.Next(-30, 31) * 0.2f);
        effectPort.SpawnDust(input.Position, input.Width, input.Height, velocity);
      }

      effectPort.PlaySound(15, integerPosition);
    }

    INpcEyeOfCthulhuRandomPort tickRandom = effectRandom ??
      throw new ArgumentNullException(
        nameof(randomPort),
        "Transformation dust requires the NPC random port.");
    Vector2 dustVelocity = new(
      tickRandom.Next(-30, 31) * 0.2f,
      tickRandom.Next(-30, 31) * 0.2f);
    effectPort.SpawnDust(input.Position, input.Width, input.Height, dustVelocity);
  }

  private static void SpawnTransformationGore(
    INpcEyeOfCthulhuRandomPort random,
    INpcEyeOfCthulhuProfileEffectPort effectPort,
    int goreId,
    Vector2 position)
  {
    Vector2 velocity = new(random.Next(-30, 31) * 0.2f, random.Next(-30, 31) * 0.2f);
    effectPort.SpawnGore(goreId, position, velocity);
  }

  private static NpcEyeOfCthulhuProfileResult CreateUnsupportedResult(
    in NpcEyeOfCthulhuProfileInput input,
    bool dustRequested)
  {
    return new NpcEyeOfCthulhuProfileResult(
      IsSupported: false,
      input.State,
      input.Velocity,
      input.DustRoll,
      dustRequested,
      NpcEyeOfCthulhuAttackIntent.None,
      NpcEyeOfCthulhuExitReason.None,
      TargetResetRequested: false,
      NetworkUpdateRequested: false);
  }

  private static NpcEyeOfCthulhuProfileResult BeginDash(
    in NpcEyeOfCthulhuProfileInput input,
    bool dustRequested)
  {
    Vector2 npcCenter = input.Position + new Vector2(input.Width * 0.5f, input.Height * 0.5f);
    Vector2 delta = input.TargetCenter - npcCenter;
    float distance = delta.Length();
    if (distance <= 0f)
    {
      return new NpcEyeOfCthulhuProfileResult(
        IsSupported: false,
        input.State,
        input.Velocity,
        input.DustRoll,
        dustRequested,
        NpcEyeOfCthulhuAttackIntent.None,
        NpcEyeOfCthulhuExitReason.None,
        TargetResetRequested: false,
        NetworkUpdateRequested: false);
    }

    float speed = input.ExpertMode ? 7f : 6f;
    if (input.GetGoodWorld)
    {
      speed += 1f;
    }

    Vector2 velocity = delta * (speed / distance);
    return new NpcEyeOfCthulhuProfileResult(
      IsSupported: true,
      input.State with { Ai1 = 2f },
      velocity,
      input.DustRoll,
      dustRequested,
      NpcEyeOfCthulhuAttackIntent.Dash,
      NpcEyeOfCthulhuExitReason.None,
      TargetResetRequested: false,
      NetworkUpdateRequested: true);
  }

  private static NpcEyeOfCthulhuProfileResult AdvanceDash(
    in NpcEyeOfCthulhuProfileInput input,
    bool dustRequested)
  {
    NpcEyeOfCthulhuProfileState state = input.State with { Ai2 = input.State.Ai2 + 1f };
    Vector2 velocity = input.Velocity;
    if (state.Ai2 >= 40f)
    {
      velocity *= 0.98f;
      if (input.ExpertMode)
      {
        velocity *= 0.985f;
      }

      if (input.GetGoodWorld)
      {
        velocity *= 0.99f;
      }

      velocity = ClampSmallVelocity(velocity);
    }

    float dashDuration = input.ExpertMode ? 100f : 150f;
    if (input.GetGoodWorld)
    {
      dashDuration -= 15f;
    }

    bool targetResetRequested = false;
    if (state.Ai2 >= dashDuration)
    {
      state = state with { Ai2 = 0f, Ai3 = state.Ai3 + 1f };
      targetResetRequested = true;
      if (state.Ai3 >= 3f)
      {
        state = state with { Ai1 = 0f, Ai3 = 0f };
      }
      else
      {
        state = state with { Ai1 = 1f };
      }
    }

    return new NpcEyeOfCthulhuProfileResult(
      IsSupported: true,
      state,
      velocity,
      input.DustRoll,
      dustRequested,
      NpcEyeOfCthulhuAttackIntent.None,
      NpcEyeOfCthulhuExitReason.None,
      targetResetRequested,
      NetworkUpdateRequested: false);
  }

  private static NpcEyeOfCthulhuProfileResult CreateExitResult(
    in NpcEyeOfCthulhuProfileInput input,
    bool dustRequested,
    NpcEyeOfCthulhuExitReason exitReason)
  {
    return new NpcEyeOfCthulhuProfileResult(
      IsSupported: true,
      input.State,
      input.Velocity + new Vector2(0f, -0.04f),
      input.DustRoll,
      dustRequested,
      NpcEyeOfCthulhuAttackIntent.None,
      exitReason,
      TargetResetRequested: false,
      NetworkUpdateRequested: false);
  }

  private static float Accelerate(float velocity, float targetVelocity, float acceleration)
  {
    if (velocity < targetVelocity)
    {
      velocity += acceleration;
      if (velocity < 0f && targetVelocity > 0f)
      {
        velocity += acceleration;
      }
    }
    else if (velocity > targetVelocity)
    {
      velocity -= acceleration;
      if (velocity > 0f && targetVelocity < 0f)
      {
        velocity -= acceleration;
      }
    }

    return velocity;
  }

  private static Vector2 ClampSmallVelocity(Vector2 velocity)
  {
    if (velocity.X > -0.1f && velocity.X < 0.1f)
    {
      velocity.X = 0f;
    }

    if (velocity.Y > -0.1f && velocity.Y < 0.1f)
    {
      velocity.Y = 0f;
    }

    return velocity;
  }
}
