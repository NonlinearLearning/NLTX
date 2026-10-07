using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// Pure state transition for the source type=3 / netID=3 / aiStyle=3 path.
/// Tile probing and door mutation remain owner effects; the profile only returns traversal intents.
/// </summary>
public static class NpcFighterProfile
{
  private const float BaseHorizontalSpeed = 1f;
  private const float HorizontalAcceleration = 0.07f;
  private const float GroundedDamping = 0.8f;

  public static bool CanHandle(int typeId, int netId, int aiStyle)
  {
    return typeId == 3 && netId == 3 && aiStyle == 3;
  }

  public static NpcFighterProfileResult Evaluate(
    in NpcFighterProfileInput input)
  {
    if (!CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Fighter profile requires type=3, netID=3, and aiStyle=3.");
    }

    Vector2 velocity = input.Velocity;
    NpcFighterProfileState state = input.State;
    int direction = input.Direction;
    int directionY = input.DirectionY == 0 ? 1 : input.DirectionY;
    bool despawnEncouragementRequested = false;
    bool targetClosestRequested = false;
    bool netUpdateRequested = false;
    bool jumpRequested = false;
    float jumpVelocityY = 0f;
    bool doorOpenRequested = false;
    NpcFighterSourceBranch branches = NpcFighterSourceBranch.None;

    if (input.TargetIsAvailable &&
        input.TargetHeight > 0 &&
        input.TargetCenter.Y + input.TargetHeight * 0.5f ==
          input.Position.Y + input.Height)
    {
      directionY = -1;
    }

    if (direction == 0)
    {
      direction = 1;
      netUpdateRequested = true;
      branches |= NpcFighterSourceBranch.NetworkSync;
    }

    targetClosestRequested = true;
    branches |= NpcFighterSourceBranch.TargetSelection;
    bool surfaceDaytime = input.DayTime && !input.IsBelowSurface;
    if (surfaceDaytime)
    {
      despawnEncouragementRequested = true;
      branches |= NpcFighterSourceBranch.DaytimeDespawn;
    }

    if (input.JustHit)
    {
      state = state with { Ai0 = 0f };
    }
    else if (input.IsGrounded && velocity.X == 0f && velocity.Y == 0f)
    {
      state = state with { Ai0 = state.Ai0 + 1f };
      if (state.Ai0 >= 2f)
      {
        direction *= -1;
        state = state with { Ai0 = 0f };
        netUpdateRequested = true;
        branches |= NpcFighterSourceBranch.IdleTurn |
          NpcFighterSourceBranch.NetworkSync;
      }
    }
    else
    {
      state = state with { Ai0 = 0f };
    }

    NpcFighterTraversalInput traversal = input.Traversal;
    if (input.IsGrounded)
    {
      if (traversal.DoorAhead)
      {
        state = state with { Ai2 = state.Ai2 + 1f, Ai3 = 0f };
        if (state.Ai2 >= 60f && traversal.DoorCanOpen)
        {
          state = state with { Ai2 = 0f };
          doorOpenRequested = true;
          netUpdateRequested = true;
          branches |= NpcFighterSourceBranch.DoorInteraction |
            NpcFighterSourceBranch.NetworkSync;
        }
      }
      else
      {
        state = state with { Ai2 = 0f };
      }

      if (!traversal.DoorAhead &&
          (traversal.SolidTileOneAhead ||
           traversal.SolidTileTwoAhead ||
           traversal.SolidTileThreeAhead))
      {
        jumpRequested = true;
        jumpVelocityY = traversal.SolidTileThreeAhead
          ? -8f
          : traversal.SolidTileTwoAhead
            ? -7f
            : -6f;
        branches |= NpcFighterSourceBranch.ObstacleJump;
        netUpdateRequested = true;
        branches |= NpcFighterSourceBranch.NetworkSync;
      }

      if (!jumpRequested &&
          traversal.ExpertMode &&
          traversal.TargetAbove &&
          traversal.TargetLineOfSight)
      {
        jumpRequested = true;
        jumpVelocityY = -7.9f;
        branches |= NpcFighterSourceBranch.ObstacleJump;
        netUpdateRequested = true;
        branches |= NpcFighterSourceBranch.NetworkSync;
      }
    }

    if (direction == 0)
    {
      direction = 1;
    }

    float horizontalSpeed = BaseHorizontalSpeed * (1f + (1f - input.Scale));
    horizontalSpeed = MathF.Max(0f, horizontalSpeed);
    if (velocity.X < -horizontalSpeed || velocity.X > horizontalSpeed)
    {
      if (input.IsGrounded)
      {
        velocity *= GroundedDamping;
        branches |= NpcFighterSourceBranch.HorizontalAcceleration;
      }
    }
    else if (direction == 1 && velocity.X < horizontalSpeed)
    {
      velocity.X = MathF.Min(horizontalSpeed, velocity.X + HorizontalAcceleration);
      branches |= NpcFighterSourceBranch.HorizontalAcceleration;
    }
    else if (direction == -1 && velocity.X > -horizontalSpeed)
    {
      velocity.X = MathF.Max(-horizontalSpeed, velocity.X - HorizontalAcceleration);
      branches |= NpcFighterSourceBranch.HorizontalAcceleration;
    }

    int aiAction = direction;
    return new NpcFighterProfileResult(
      velocity,
      state,
      direction,
      directionY,
      aiAction,
      despawnEncouragementRequested,
      targetClosestRequested,
      netUpdateRequested,
      branches,
      jumpRequested,
      jumpVelocityY,
      doorOpenRequested,
      traversal.DoorTileX,
      traversal.DoorTileY,
      direction);
  }

  public static void ApplyEffects(
    in NpcFighterProfileResult result,
    INpcFighterProfileEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.DespawnEncouragementRequested)
    {
      effectPort.EncourageDespawn(10);
    }

    if (result.TargetClosestRequested)
    {
      effectPort.RequestTargetReacquire();
    }

    if (result.NetUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }

    if (result.DoorOpenRequested)
    {
      effectPort.RequestOpenDoor(
        result.DoorTileX,
        result.DoorTileY,
        result.DoorDirection);
    }
  }
}
