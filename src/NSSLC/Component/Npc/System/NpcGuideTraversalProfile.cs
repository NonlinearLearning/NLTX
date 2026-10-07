using System.Numerics;

namespace Terraria.Npc;

public enum NpcGuideDoorActionKind
{
  OpenDoor,
  OpenTallGate,
  CloseDoor,
  CloseTallGate,
}

public readonly record struct NpcGuideDoorEffectRequest(
  NpcGuideDoorActionKind Kind,
  int TileX,
  int TileY,
  int Direction);

public readonly record struct NpcGuideTraversalInput(
  int TypeId,
  int NetId,
  int AiStyle,
  NpcGuideSourceProfileState State,
  Vector2 Position,
  Vector2 Velocity,
  int Direction,
  bool ServerAuthority,
  bool ReturnPressureActive,
  bool DangerWithinBaseRange,
  bool Wet,
  bool DrownCollision,
  bool WillDrown,
  bool IsLikeTownNpc,
  int LiquidDepthTiles,
  int SolidSupportCount,
  bool AvoidFalling,
  bool KeepWalking,
  bool ThreeTilesAboveSolid,
  bool ThreeTileCollisionClear,
  bool TwoTilesAboveSolid,
  bool TwoTileCollisionClear,
  bool OneTileAboveSolid,
  bool OneTileCollisionClear,
  bool OneTileHasSlope,
  bool DoorCandidate,
  int DoorType,
  int DoorTileX,
  int DoorTileY,
  bool OpenDoorWithDirectionSucceeded,
  bool OpenDoorAgainstDirectionSucceeded,
  bool OpenTallGateSucceeded,
  bool CloseDoorPending,
  bool CloseDoorSucceeded,
  bool CloseTallGateSucceeded,
  bool OutsideClosingRange,
  bool IsGrounded)
{
  public bool TownCritter { get; init; }

  public bool OneTileLandingEligible { get; init; }
}

[Flags]
public enum NpcGuideTraversalBranch
{
  None = 0,
  CloseDoorRequested = 1 << 0,
  OpenDoorRequested = 1 << 1,
  OpenTallGateRequested = 1 << 2,
  DoorTurnedAround = 1 << 3,
  DrowningEscape = 1 << 4,
  JumpThreeTiles = 1 << 5,
  JumpTwoTiles = 1 << 6,
  JumpOneTile = 1 << 7,
  ObstructionState = 1 << 8,
  ReverseDirection = 1 << 9,
  KeepWalkingTimer = 1 << 10,
  LiquidTimerUpdated = 1 << 11,
  NetworkSynchronization = 1 << 12,
}

public readonly record struct NpcGuideTraversalResult(
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  int Direction,
  bool NetworkUpdateRequested,
  NpcGuideDoorEffectRequest? DoorEffect,
  NpcGuideDoorEffectRequest? CloseDoorEffect,
  bool CloseDoorConsumed,
  NpcGuideTraversalBranch Branches);

public interface INpcGuideTraversalRandomPort
{
  int Next(int maxExclusive);
}

public interface INpcGuideTraversalEffectPort
{
  void RequestDoor(in NpcGuideDoorEffectRequest request);

  void RequestNetworkSync();
}

/// <summary>
/// Pure Guide traversal decision for the source door, jump, obstruction, and
/// drowning branches. WorldGen, collision, and tile mutation remain ports.
/// </summary>
public static class NpcGuideTraversalProfile
{
  public static NpcGuideTraversalResult EvaluateWithRandom(
    in NpcGuideTraversalInput input,
    INpcGuideTraversalRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide traversal requires type=22, netID=22, and aiStyle=7.");
    }

    NpcGuideSourceProfileState state = input.State;
    Vector2 velocity = input.Velocity;
    int direction = input.Direction == 0 ? 1 : input.Direction;
    bool networkUpdateRequested = false;
    NpcGuideDoorEffectRequest? doorEffect = null;
    NpcGuideDoorEffectRequest? closeDoorEffect = null;
    bool closeDoorConsumed = false;
    NpcGuideTraversalBranch branches = NpcGuideTraversalBranch.None;

    if (input.State.Ai0 == 1f &&
        input.CloseDoorPending &&
        input.ServerAuthority &&
        input.OutsideClosingRange)
    {
      if (input.CloseDoorSucceeded)
      {
        closeDoorEffect = new NpcGuideDoorEffectRequest(
          NpcGuideDoorActionKind.CloseDoor,
          input.DoorTileX,
          input.DoorTileY,
          direction);
        closeDoorConsumed = true;
        branches |= NpcGuideTraversalBranch.CloseDoorRequested;
      }
      else if (input.CloseTallGateSucceeded)
      {
        closeDoorEffect = new NpcGuideDoorEffectRequest(
          NpcGuideDoorActionKind.CloseTallGate,
          input.DoorTileX,
          input.DoorTileY,
          0);
        closeDoorConsumed = true;
        branches |= NpcGuideTraversalBranch.CloseDoorRequested;
      }
    }

    bool recognizedDoor = input.DoorType == 10 || input.DoorType == 388;
    bool openRollPasses = recognizedDoor && input.DoorCandidate &&
      (randomPort.Next(10) == 0 || input.ReturnPressureActive);
    if (input.IsGrounded && velocity.Y == 0f &&
        recognizedDoor && openRollPasses)
    {
      if (input.ServerAuthority)
      {
        if (input.OpenDoorWithDirectionSucceeded)
        {
          doorEffect = new NpcGuideDoorEffectRequest(
            NpcGuideDoorActionKind.OpenDoor,
            input.DoorTileX,
            input.DoorTileY,
            direction);
          state = state with { Ai1 = state.Ai1 + 80f };
          networkUpdateRequested = true;
          branches |= NpcGuideTraversalBranch.OpenDoorRequested |
            NpcGuideTraversalBranch.NetworkSynchronization;
        }
        else if (input.OpenDoorAgainstDirectionSucceeded)
        {
          doorEffect = new NpcGuideDoorEffectRequest(
            NpcGuideDoorActionKind.OpenDoor,
            input.DoorTileX,
            input.DoorTileY,
            -direction);
          state = state with { Ai1 = state.Ai1 + 80f };
          networkUpdateRequested = true;
          branches |= NpcGuideTraversalBranch.OpenDoorRequested |
            NpcGuideTraversalBranch.NetworkSynchronization;
        }
        else if (input.OpenTallGateSucceeded)
        {
          doorEffect = new NpcGuideDoorEffectRequest(
            NpcGuideDoorActionKind.OpenTallGate,
            input.DoorTileX,
            input.DoorTileY,
            0);
          state = state with { Ai1 = state.Ai1 + 80f };
          networkUpdateRequested = true;
          branches |= NpcGuideTraversalBranch.OpenTallGateRequested |
            NpcGuideTraversalBranch.NetworkSynchronization;
        }
        else
        {
          direction *= -1;
          networkUpdateRequested = true;
          branches |= NpcGuideTraversalBranch.DoorTurnedAround |
            NpcGuideTraversalBranch.NetworkSynchronization;
        }
      }

      return Finish(
        state,
        velocity,
        direction,
        networkUpdateRequested,
        doorEffect,
        closeDoorEffect,
        closeDoorConsumed,
        branches);
    }

    bool drowningEscape = false;
    if (input.IsGrounded &&
        velocity.Y == 0f &&
        input.Wet && input.IsLikeTownNpc && input.WillDrown &&
        state.LocalAi3 <= 0f)
    {
      float jumpSpeed = MathF.Sqrt(
        (input.LiquidDepthTiles * 16f + 16f) * 2f * 0.3f);
      velocity.Y = -MathF.Min(26f, jumpSpeed);
      state = state with { LocalAi3 = input.Position.X };
      drowningEscape = true;
      branches |= NpcGuideTraversalBranch.DrowningEscape;
    }
    bool traversalJumpStarted = drowningEscape;

    bool avoidFallingRecovery = false;
    if (input.IsGrounded &&
        velocity.Y == 0f &&
        input.AvoidFalling &&
        !drowningEscape &&
        input.SolidSupportCount <= 2)
    {
      if (velocity.X != 0f)
      {
        networkUpdateRequested = true;
        branches |= NpcGuideTraversalBranch.NetworkSynchronization;
      }

      state = state with
      {
        Ai0 = 0f,
        Ai1 = 50f + randomPort.Next(50),
        Ai2 = 0f,
        LocalAi3 = 40f,
      };
      avoidFallingRecovery = true;
    }

    if (input.IsGrounded && velocity.Y == 0f)
    {
      if (input.Position.X == state.LocalAi3 && !drowningEscape)
      {
        direction *= -1;
        networkUpdateRequested = true;
        state = state with { LocalAi3 = 180f };
        branches |= NpcGuideTraversalBranch.ReverseDirection |
          NpcGuideTraversalBranch.NetworkSynchronization;
      }

      if (input.DrownCollision && !drowningEscape)
      {
        state = state with { LocalAi3 = MathF.Min(state.LocalAi3, 180f) };
        if (state.LocalAi3 > 0f)
        {
          state = state with { LocalAi3 = state.LocalAi3 - 1f };
        }

        branches |= NpcGuideTraversalBranch.LiquidTimerUpdated;
      }
      else if (!drowningEscape)
      {
        state = state with { LocalAi3 = -1f };
      }
    }

    if (input.IsGrounded &&
        velocity.Y == 0f &&
        ((velocity.X < 0f && direction == -1) ||
         (velocity.X > 0f && direction == 1)))
    {
      bool obstruction = false;
      bool reverse = false;
      if (input.ThreeTilesAboveSolid)
      {
        if (input.ThreeTileCollisionClear)
        {
          velocity.Y = -6f;
          traversalJumpStarted = true;
          networkUpdateRequested = true;
          branches |= NpcGuideTraversalBranch.JumpThreeTiles |
            NpcGuideTraversalBranch.NetworkSynchronization;
        }
        else if (input.DangerWithinBaseRange)
        {
          obstruction = true;
        }
        else if (!input.WillDrown)
        {
          reverse = true;
        }
      }
      else if (input.TwoTilesAboveSolid)
      {
        if (input.TwoTileCollisionClear)
        {
          velocity.Y = -5f;
          traversalJumpStarted = true;
          networkUpdateRequested = true;
          branches |= NpcGuideTraversalBranch.JumpTwoTiles |
            NpcGuideTraversalBranch.NetworkSynchronization;
        }
        else if (input.DangerWithinBaseRange)
        {
          obstruction = true;
        }
        else
        {
          reverse = true;
        }
      }
      else if (input.OneTileAboveSolid &&
               input.OneTileLandingEligible &&
               !input.OneTileHasSlope)
      {
        if (input.OneTileCollisionClear)
        {
          velocity.Y = -4.4f;
          traversalJumpStarted = true;
          networkUpdateRequested = true;
          branches |= NpcGuideTraversalBranch.JumpOneTile |
            NpcGuideTraversalBranch.NetworkSynchronization;
        }
        else if (input.DangerWithinBaseRange)
        {
          obstruction = true;
        }
        else
        {
          reverse = true;
        }
      }
      else if (input.AvoidFalling && !drowningEscape &&
               !avoidFallingRecovery)
      {
        obstruction = input.DangerWithinBaseRange;
        reverse = !input.WillDrown && !obstruction;
      }

      if (obstruction)
      {
        state = state with { Ai0 = 8f, Ai1 = 240f };
        velocity.X = 0f;
        networkUpdateRequested = true;
        branches |= NpcGuideTraversalBranch.ObstructionState |
          NpcGuideTraversalBranch.NetworkSynchronization;
      }
      else if (reverse)
      {
        direction *= -1;
        velocity.X *= -1f;
        networkUpdateRequested = true;
        branches |= NpcGuideTraversalBranch.ReverseDirection |
          NpcGuideTraversalBranch.NetworkSynchronization;
      }

      if (input.KeepWalking && !obstruction && !avoidFallingRecovery)
      {
        state = state with { Ai1 = 90f };
        networkUpdateRequested = true;
        branches |= NpcGuideTraversalBranch.KeepWalkingTimer |
          NpcGuideTraversalBranch.NetworkSynchronization;
      }
    }

    if (velocity.Y < 0f && input.Wet)
    {
      velocity.Y *= 1.2f;
    }

    if (velocity.Y < 0f && input.IsLikeTownNpc && input.TownCritter)
    {
      velocity.Y *= 1.2f;
    }

    if (traversalJumpStarted)
    {
      state = state with { LocalAi3 = input.Position.X };
    }

    return new NpcGuideTraversalResult(
      state,
      velocity,
      direction,
      networkUpdateRequested,
      doorEffect,
      closeDoorEffect,
      closeDoorConsumed,
      branches);
  }

  public static void ApplyEffects(
    in NpcGuideTraversalResult result,
    INpcGuideTraversalEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.CloseDoorEffect is NpcGuideDoorEffectRequest closeDoorEffect)
    {
      effectPort.RequestDoor(in closeDoorEffect);
    }

    if (result.DoorEffect is NpcGuideDoorEffectRequest doorEffect)
    {
      effectPort.RequestDoor(in doorEffect);
    }

    if (result.NetworkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }
  }

  private static NpcGuideTraversalResult Finish(
    NpcGuideSourceProfileState state,
    Vector2 velocity,
    int direction,
    bool networkUpdateRequested,
    NpcGuideDoorEffectRequest? doorEffect,
    NpcGuideDoorEffectRequest? closeDoorEffect,
    bool closeDoorConsumed,
    NpcGuideTraversalBranch branches)
  {
    return new NpcGuideTraversalResult(
      state,
      velocity,
      direction,
      networkUpdateRequested,
      doorEffect,
      closeDoorEffect,
      closeDoorConsumed,
      branches);
  }
}
