using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// Pure state transition for the source type=1 / netID=1 Blue Slime path.
/// Item selection, networking, and target lookup remain ordered owner effects.
/// </summary>
public static class NpcBlueSlimeProfile
{
  private const float InitialGroundedCounter = -100f;

  public static bool CanHandle(int typeId, int netId, int aiStyle)
  {
    return typeId == 1 && netId == 1 && aiStyle == 1;
  }

  public static bool ShouldGenerateContainedItem(
    in NpcBlueSlimeProfileInput input)
  {
    return input.CanContainItems &&
      input.State.Ai1 == 0f &&
      !input.IsClient &&
      input.Value > 0f;
  }

  public static NpcBlueSlimeProfileInput WithContainedItemSelection(
    in NpcBlueSlimeProfileInput input,
    in NpcBlueSlimeTypeOneSelectionResult selection)
  {
    if (!selection.Attempted)
    {
      return input;
    }

    return input with
    {
      State = input.State with { Ai1 = selection.ItemState },
    };
  }

  public static NpcBlueSlimeProfileResult Evaluate(
    in NpcBlueSlimeProfileInput input)
  {
    NpcBlueSlimeProfileState state = input.State;
    Vector2 velocity = input.Velocity;
    int defense = input.BaseDefense;
    int direction = input.Direction;
    int aiAction = 0;
    bool netUpdateRequested = false;
    bool targetClosestRequested = false;
    bool containedItemGenerationRequested = false;
    NpcBlueSlimeSourceBranch branches = NpcBlueSlimeSourceBranch.None;

    bool activeJumpCadence = !input.DayTime ||
      input.IsDamaged ||
      input.IsBelowSurface ||
      input.SlimeRain;

    if (ShouldGenerateContainedItem(in input))
    {
      containedItemGenerationRequested = true;
      branches |= NpcBlueSlimeSourceBranch.ContainedItemGeneration;
    }

    if (state.Ai1 == 2f && velocity.Y == 0f)
    {
      state = state with { Ai0 = state.Ai0 + 9f };
      branches |= NpcBlueSlimeSourceBranch.DirtSlimeGroundCounter;
    }

    if (state.Ai1 == 9f)
    {
      defense = input.BaseDefense + 16;
      branches |= NpcBlueSlimeSourceBranch.WoodSlimeDefense;
    }

    if (state.Ai1 == 3f && velocity.Y > 0f)
    {
      velocity.Y += input.Gravity * 2f;
      branches |= NpcBlueSlimeSourceBranch.StoneSlimeGravity;
    }
    else if (state.Ai1 == 751f && velocity.Y != 0f)
    {
      velocity.Y -= input.Gravity * 0.6f;
      branches |= NpcBlueSlimeSourceBranch.CloudSlimeGravity;
    }

    if (direction == 0)
    {
      direction = 1;
      netUpdateRequested = true;
      branches |= NpcBlueSlimeSourceBranch.DirectionInitialization |
        NpcBlueSlimeSourceBranch.NetworkSync;
    }

    if (state.Ai0 == -999f)
    {
      branches |= NpcBlueSlimeSourceBranch.FrozenSentinel;
      return new NpcBlueSlimeProfileResult(
        velocity,
        defense,
        state,
        direction,
        aiAction,
        netUpdateRequested,
        targetClosestRequested,
        containedItemGenerationRequested,
        branches);
    }

    if (state.Ai2 > 1f)
    {
      state = state with { Ai2 = state.Ai2 - 1f };
      branches |= NpcBlueSlimeSourceBranch.Ai2Cooldown;
    }

    if (input.Wet)
    {
      branches |= NpcBlueSlimeSourceBranch.WetMovement;
      if (input.CollideY)
      {
        velocity.Y = -2f;
      }

      if (velocity.Y < 0f && state.Ai3 == input.Position.X)
      {
        direction *= -1;
        state = state with { Ai2 = 200f };
      }

      if (velocity.Y > 0f)
      {
        state = state with { Ai3 = input.Position.X };
      }

      if (velocity.Y > 2f)
      {
        velocity.Y *= 0.9f;
      }

      velocity.Y = Math.Max(-4f, velocity.Y - 0.5f);
      if (state.Ai2 == 1f && activeJumpCadence)
      {
        targetClosestRequested = true;
        branches |= NpcBlueSlimeSourceBranch.TargetReacquire;
      }
    }

    if (state.Ai2 == 0f)
    {
      state = state with { Ai0 = InitialGroundedCounter, Ai2 = 1f };
      targetClosestRequested = true;
      branches |= NpcBlueSlimeSourceBranch.TargetInitialization;
    }

    if (velocity.Y == 0f)
    {
      if (input.CollideY &&
          input.OldVelocity.Y != 0f &&
          state.Ai3 == input.Position.X)
      {
        direction *= -1;
        state = state with { Ai2 = 200f };
      }

      state = state with { Ai3 = 0f };
      if (state.Ai1 == 3609f)
      {
        velocity.X += direction < 0 ? -0.1f : 0.1f;
        velocity.X = Math.Clamp(velocity.X, -2.5f, 2.5f);
        branches |= NpcBlueSlimeSourceBranch.StoredAi1GroundAcceleration;
      }
      else
      {
        velocity.X *= 0.8f;
        if (velocity.X > -0.1f && velocity.X < 0.1f)
        {
          velocity.X = 0f;
        }
      }

      state = state with
      {
        Ai0 = state.Ai0 + (activeJumpCadence ? 2f : 1f),
      };
      branches |= NpcBlueSlimeSourceBranch.GroundedCounter;

      int jumpPhase = ResolveJumpPhase(state.Ai0);
      if (jumpPhase > 0)
      {
        netUpdateRequested = true;
        branches |= NpcBlueSlimeSourceBranch.JumpImpulse |
          NpcBlueSlimeSourceBranch.NetworkSync;
        if (activeJumpCadence && state.Ai2 == 1f)
        {
          targetClosestRequested = true;
          branches |= NpcBlueSlimeSourceBranch.TargetReacquire;
        }

        if (jumpPhase == 3)
        {
          velocity.Y = -8f;
          velocity.X += 3f * direction;
          state = state with { Ai0 = -200f, Ai3 = input.Position.X };
        }
        else
        {
          velocity.Y = -6f;
          velocity.X += 2f * direction;
          state = state with
          {
            Ai0 = jumpPhase == 1 ? -1120f : -2120f,
          };
        }
      }
      else if (state.Ai0 >= -30f)
      {
        aiAction = 1;
      }
    }
    else if (input.TargetSlot >= 0 && input.TargetSlot < 255 &&
             ((direction == 1 && velocity.X < 3f) ||
              (direction == -1 && velocity.X > -3f)))
    {
      branches |= NpcBlueSlimeSourceBranch.AirborneAcceleration;
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

    return new NpcBlueSlimeProfileResult(
      velocity,
      defense,
      state,
      direction,
      aiAction,
      netUpdateRequested,
      targetClosestRequested,
      containedItemGenerationRequested,
      branches);
  }

  public static void ApplyEffects(
    in NpcBlueSlimeProfileResult result,
    bool isBallooned,
    INpcBlueSlimeProfileEffectPort effectPort,
    NpcBlueSlimeTypeOneSelectionResult? preselectedItem = null)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    NpcBlueSlimeTypeOneSelectionResult itemSelection = preselectedItem ?? default;
    if (result.ContainedItemGenerationRequested && !preselectedItem.HasValue)
    {
      itemSelection = effectPort.GenerateContainedItem(isBallooned);
    }

    bool networkOccursBeforeTarget =
      itemSelection.NetUpdateRequested ||
      (result.Branches & (NpcBlueSlimeSourceBranch.DirectionInitialization |
        NpcBlueSlimeSourceBranch.JumpImpulse)) != 0;
    bool networkUpdateRequested = result.NetUpdateRequested || itemSelection.NetUpdateRequested;
    if (networkOccursBeforeTarget && networkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }

    if (result.TargetClosestRequested)
    {
      effectPort.RequestTargetReacquire();
    }

    if (!networkOccursBeforeTarget && networkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
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
