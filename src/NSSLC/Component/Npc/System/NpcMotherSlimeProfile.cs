using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// Deterministic Mother Slime state slice from the type=16 AI_001_Slimes path.
/// </summary>
public static class NpcMotherSlimeProfile
{
  private const int MotherSlimeTypeId = 16;
  private const int MotherSlimeNetId = 16;
  private const int SlimeAiStyle = 1;
  private const float InitialGroundedCounter = -100f;
  private const float JumpPhaseBoundary = -1000f;

  public static bool SupportsContainedItemGeneration => false;

  public static bool CanHandle(int typeId, int netId, int aiStyle)
  {
    return typeId == MotherSlimeTypeId &&
      netId == MotherSlimeNetId &&
      aiStyle == SlimeAiStyle;
  }

  public static NpcMotherSlimeProfileResult Evaluate(
    in NpcMotherSlimeProfileInput input)
  {
    if (!CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new ArgumentException(
        "The Mother Slime profile requires type=16, netID=16, and aiStyle=1.",
        nameof(input));
    }

    Vector2 position = input.Position;
    Vector2 velocity = input.Velocity;
    NpcMotherSlimeProfileState state = input.State;
    int direction = input.Direction;
    int aiAction = 0;
    bool netUpdateRequested = false;
    bool targetClosestRequested = false;
    NpcMotherSlimeSourceBranch branches = NpcMotherSlimeSourceBranch.None;

    if (state.Ai0 == -999f)
    {
      branches |= NpcMotherSlimeSourceBranch.FrozenSentinel;
      return new NpcMotherSlimeProfileResult(
        position,
        velocity,
        state,
        direction,
        aiAction,
        ContainedItemSelectionRejected: true,
        netUpdateRequested,
        targetClosestRequested,
        IsFrozen: true,
        branches);
    }

    if (state.Ai2 > 1f)
    {
      state = state with { Ai2 = state.Ai2 - 1f };
      branches |= NpcMotherSlimeSourceBranch.Ai2Cooldown;
    }

    bool activeJumpCadence = !input.DayTime ||
      input.IsDamaged ||
      input.IsBelowSurface ||
      input.SlimeRain;

    if (input.Wet)
    {
      branches |= NpcMotherSlimeSourceBranch.WetMovement;
      if (input.CollideY)
      {
        velocity.Y = -2f;
      }

      if (velocity.Y < 0f && state.Ai3 == position.X)
      {
        direction *= -1;
        state = state with { Ai2 = 200f };
        branches |= NpcMotherSlimeSourceBranch.DirectionTurn;
      }

      if (velocity.Y > 0f)
      {
        state = state with { Ai3 = position.X };
      }

      if (velocity.Y > 2f)
      {
        velocity.Y *= 0.9f;
      }

      velocity.Y = Math.Max(-4f, velocity.Y - 0.5f);
      if (state.Ai2 == 1f && activeJumpCadence)
      {
        targetClosestRequested = true;
        branches |= NpcMotherSlimeSourceBranch.TargetReacquire;
      }
    }

    if (state.Ai2 == 0f)
    {
      state = state with { Ai0 = InitialGroundedCounter, Ai2 = 1f };
      targetClosestRequested = true;
      branches |= NpcMotherSlimeSourceBranch.TargetInitialization |
        NpcMotherSlimeSourceBranch.TargetReacquire;
    }

    if (velocity.Y == 0f)
    {
      if (input.CollideY &&
          input.OldVelocity.Y != 0f &&
          input.SolidCollision)
      {
        position.X -= velocity.X + direction;
      }

      if (state.Ai3 == position.X)
      {
        direction *= -1;
        state = state with { Ai2 = 200f };
        branches |= NpcMotherSlimeSourceBranch.DirectionTurn;
      }

      state = state with { Ai3 = 0f };
      velocity.X *= 0.8f;
      if (velocity.X > -0.1f && velocity.X < 0.1f)
      {
        velocity.X = 0f;
      }

      state = state with
      {
        Ai0 = state.Ai0 + (activeJumpCadence ? 2f : 1f),
      };
      branches |= NpcMotherSlimeSourceBranch.GroundedCounter;

      int jumpPhase = ResolveJumpPhase(state.Ai0);
      if (jumpPhase > 0)
      {
        netUpdateRequested = true;
        branches |= NpcMotherSlimeSourceBranch.JumpImpulse |
          NpcMotherSlimeSourceBranch.NetworkSync;
        if (activeJumpCadence && state.Ai2 == 1f)
        {
          targetClosestRequested = true;
          branches |= NpcMotherSlimeSourceBranch.TargetReacquire;
        }

        if (jumpPhase == 3)
        {
          velocity.Y = -8f;
          velocity.X += 3f * direction;
          state = state with { Ai0 = -200f, Ai3 = position.X };
        }
        else
        {
          velocity.Y = -6f;
          velocity.X += 2f * direction;
          state = state with
          {
            Ai0 = jumpPhase == 1
              ? JumpPhaseBoundary - 120f
              : JumpPhaseBoundary * 2f - 120f,
          };
        }
      }
      else if (state.Ai0 >= -30f)
      {
        aiAction = 1;
      }
    }
    else if (input.TargetSlot < 255 &&
             ((direction == 1 && velocity.X < 3f) ||
              (direction == -1 && velocity.X > -3f)))
    {
      branches |= NpcMotherSlimeSourceBranch.AirborneMovement;
      if (input.CollideX && MathF.Abs(velocity.X) == 0.2f)
      {
        position.X -= 1.4f * direction;
      }

      if ((direction == -1 && velocity.X < 0.01f) ||
          (direction == 1 && velocity.X > -0.01f))
      {
        velocity.X += 0.2f * direction;
      }
      else
      {
        velocity.X *= 0.93f;
      }
    }

    return new NpcMotherSlimeProfileResult(
      position,
      velocity,
      state,
      direction,
      aiAction,
      ContainedItemSelectionRejected: true,
      netUpdateRequested,
      targetClosestRequested,
      IsFrozen: false,
      branches);
  }

  public static void ApplyEffects(
    in NpcMotherSlimeProfileResult result,
    INpcMotherSlimeProfileEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.NetUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }

    if (result.TargetClosestRequested)
    {
      effectPort.RequestTargetReacquire();
    }
  }

  private static int ResolveJumpPhase(float ai0)
  {
    if (ai0 >= 0f)
    {
      return 1;
    }

    if (ai0 >= -1000f && ai0 <= -500f)
    {
      return 2;
    }

    if (ai0 >= -2000f && ai0 <= -1500f)
    {
      return 3;
    }

    return 0;
  }
}
