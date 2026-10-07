using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcGuideDayMovementInput(
  int TypeId,
  int NetId,
  int AiStyle,
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  int Direction,
  bool ServerAuthority,
  bool ReturnPressureActive,
  bool PlayerTalking,
  bool TownCritter,
  bool Homeless,
  bool InGoodRestingSpot,
  bool Stinky,
  bool DrownCollision,
  bool DungeonTile,
  int MyTileX,
  int MyTileY,
  int HomeFloorX,
  int HomeFloorY,
  Vector2 Position);

[Flags]
public enum NpcGuideDayMovementBranch
{
  None = 0,
  LocalTimerDecremented = 1 << 0,
  StinkyWalkEntry = 1 << 1,
  HomeFloorDamping = 1 << 2,
  HomeWalkEntry = 1 << 3,
  GoodRestingSpotExit = 1 << 4,
  DrownTimerPreserved = 1 << 5,
  DirectionalTimerAcceleration = 1 << 6,
  WalkTimerExpired = 1 << 7,
  HorizontalDamping = 1 << 8,
  HorizontalAcceleration = 1 << 9,
  ForceSittingRequested = 1 << 10,
  NetworkSynchronization = 1 << 11,
}

public readonly record struct NpcGuideDayMovementResult(
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  int Direction,
  bool NetworkUpdateRequested,
  bool ForceSittingRequested,
  NpcGuideForceSittingRequest ForceSittingRequest,
  NpcGuideDayMovementBranch Branches);

public interface INpcGuideDayMovementRandomPort
{
  int Next(int maxExclusive);
}

public interface INpcGuideDayMovementEffectPort
{
  bool TryForceSitting(in NpcGuideForceSittingRequest request);

  void RequestNetworkSync();
}

/// <summary>
/// Source-shaped multi-tick transition for Guide ordinary ai0=0/1 movement.
/// Tile stepping, doors, jumping, and collision mutation remain caller-owned.
/// </summary>
public static class NpcGuideDayMovementProfile
{
  public static NpcGuideDayMovementResult EvaluateWithRandom(
    in NpcGuideDayMovementInput input,
    INpcGuideDayMovementRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide day movement requires type=22, netID=22, and aiStyle=7.");
    }

    NpcGuideSourceProfileState state = input.State;
    Vector2 velocity = input.Velocity;
    int direction = input.Direction == 0 ? 1 : input.Direction;
    bool networkUpdateRequested = false;
    bool forceSittingRequested = false;
    NpcGuideDayMovementBranch branches = NpcGuideDayMovementBranch.None;
    NpcGuideForceSittingRequest sittingRequest = new(
      input.HomeFloorX,
      input.HomeFloorY);

    if (state.Ai0 == 0f)
    {
      if (input.Stinky)
      {
        state = state with { Ai0 = 1f };
        branches |= NpcGuideDayMovementBranch.StinkyWalkEntry;
      }

      if (state.LocalAi3 > 0f)
      {
        state = state with { LocalAi3 = state.LocalAi3 - 1f };
        branches |= NpcGuideDayMovementBranch.LocalTimerDecremented;
      }

      if (input.ReturnPressureActive &&
          !input.PlayerTalking &&
          !input.TownCritter &&
          input.ServerAuthority)
      {
        bool atHomeFloor = input.MyTileX == input.HomeFloorX &&
          input.MyTileY == input.HomeFloorY;
        if (atHomeFloor)
        {
          velocity.X = Decelerate(velocity.X, 0.1f);
          branches |= NpcGuideDayMovementBranch.HomeFloorDamping;
          if (velocity.X == 0f)
          {
            forceSittingRequested = true;
            branches |= NpcGuideDayMovementBranch.ForceSittingRequested;
          }
        }
        else
        {
          direction = input.MyTileX > input.HomeFloorX ? -1 : 1;
          state = state with
          {
            Ai0 = 1f,
            Ai1 = 200f + randomPort.Next(200),
            Ai2 = 0f,
            LocalAi3 = 0f,
          };
          networkUpdateRequested = true;
          branches |= NpcGuideDayMovementBranch.HomeWalkEntry |
            NpcGuideDayMovementBranch.NetworkSynchronization;
        }
      }
      else
      {
        velocity.X = Decelerate(velocity.X, 0.1f);
        branches |= NpcGuideDayMovementBranch.HorizontalDamping;
      }
    }

    if (input.State.Ai0 == 1f)
    {
      if (input.ServerAuthority &&
          input.ReturnPressureActive &&
          input.InGoodRestingSpot &&
          !input.TownCritter)
      {
        state = state with
        {
          Ai0 = 0f,
          Ai1 = 200f + randomPort.Next(200),
          LocalAi3 = 60f,
        };
        networkUpdateRequested = true;
        branches |= NpcGuideDayMovementBranch.GoodRestingSpotExit |
          NpcGuideDayMovementBranch.NetworkSynchronization;
      }
      else
      {
        bool drownTimerPreserved = input.DrownCollision;
        if (drownTimerPreserved)
        {
          branches |= NpcGuideDayMovementBranch.DrownTimerPreserved;
        }
        else
        {
          if (input.ServerAuthority &&
              !input.Homeless &&
              !input.DungeonTile &&
              (input.MyTileX < input.HomeFloorX - 35 ||
               input.MyTileX > input.HomeFloorX + 35) &&
              ((input.Position.X < input.HomeFloorX * 16f && direction == -1) ||
               (input.Position.X > input.HomeFloorX * 16f && direction == 1)))
          {
            state = state with { Ai1 = state.Ai1 - 5f };
            branches |= NpcGuideDayMovementBranch.DirectionalTimerAcceleration;
          }

          state = state with { Ai1 = state.Ai1 - 1f };
          if (state.Ai1 <= 0f)
          {
            state = state with
            {
              Ai0 = 0f,
              Ai1 = 300f + randomPort.Next(300),
              Ai2 = 0f,
              LocalAi3 = 60f,
            };
            networkUpdateRequested = true;
            branches |= NpcGuideDayMovementBranch.WalkTimerExpired |
              NpcGuideDayMovementBranch.NetworkSynchronization;
          }
        }

        }

        velocity = AdvanceGuideHorizontalVelocity(velocity, direction, ref branches);
      }

    return new NpcGuideDayMovementResult(
      state,
      velocity,
      direction,
      networkUpdateRequested,
      forceSittingRequested,
      sittingRequest,
      branches);
  }

  public static void ApplyEffects(
    in NpcGuideDayMovementResult result,
    INpcGuideDayMovementEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.ForceSittingRequested)
    {
      NpcGuideForceSittingRequest sittingRequest = result.ForceSittingRequest;
      _ = effectPort.TryForceSitting(in sittingRequest);
    }

    if (result.NetworkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }
  }

  private static float Decelerate(float velocity, float amount)
  {
    if (velocity > amount)
    {
      return velocity - amount;
    }

    if (velocity < -amount)
    {
      return velocity + amount;
    }

    return 0f;
  }

  private static Vector2 AdvanceGuideHorizontalVelocity(
    Vector2 velocity,
    int direction,
    ref NpcGuideDayMovementBranch branches)
  {
    const float maxSpeed = 1f;
    const float acceleration = 0.07f;
    if (velocity.X < -maxSpeed || velocity.X > maxSpeed)
    {
      if (velocity.Y == 0f)
      {
        velocity *= 0.8f;
        branches |= NpcGuideDayMovementBranch.HorizontalDamping;
      }

      return velocity;
    }

    if (direction == 1 && velocity.X < maxSpeed)
    {
      velocity.X = MathF.Min(maxSpeed, velocity.X + acceleration);
      branches |= NpcGuideDayMovementBranch.HorizontalAcceleration;
    }
    else if (direction == -1 && velocity.X > -maxSpeed)
    {
      velocity.X = MathF.Max(-maxSpeed, velocity.X - acceleration);
      branches |= NpcGuideDayMovementBranch.HorizontalAcceleration;
    }

    return velocity;
  }
}
