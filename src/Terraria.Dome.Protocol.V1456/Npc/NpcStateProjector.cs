using System;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Snapshots;

namespace Terraria.Dome.Protocol.V1456.Npc;

public readonly record struct NpcProjectionResult(
  bool IsSupported,
  NpcSyncPacket Packet,
  string UnsupportedReason)
{
  public static NpcProjectionResult Unsupported(string reason)
  {
    return new(false, default, reason);
  }
}

public sealed class NpcStateProjector
{
  public NpcProjectionResult Project(NpcStateSnapshot state)
  {
    NpcReplicationSnapshot replication = state.Replication;
    if (replication.ReplicationId is <= 0 or > short.MaxValue)
    {
      return NpcProjectionResult.Unsupported("NPC replication identity does not fit SyncNPC.");
    }

    if (state.NetId is <= 0 or > short.MaxValue)
    {
      return NpcProjectionResult.Unsupported("NPC NetId does not fit SyncNPC.");
    }

    if (state.TargetStableId < 0 || state.TargetStableId > ushort.MaxValue)
    {
      return NpcProjectionResult.Unsupported("NPC target identity does not fit SyncNPC.");
    }

    if (!IsFinite(replication.Position) || !IsFinite(replication.Velocity))
    {
      return NpcProjectionResult.Unsupported("NPC transform contains a non-finite value.");
    }

    if (state.Spawn.ReleaseOwner < 0 || state.Spawn.ReleaseOwner > byte.MaxValue)
    {
      return NpcProjectionResult.Unsupported("NPC release owner does not fit SyncNPC.");
    }

    if (!TryProjectBehavior(state, out float? ai0, out float? ai1, out float? ai2,
        out float? ai3, out string unsupportedReason))
    {
      return NpcProjectionResult.Unsupported(unsupportedReason);
    }

    int lifeMaximum = state.MaximumHealth;
    int life = state.Lifecycle.IsActive && replication.IsActive ? replication.Health : 0;
    if (lifeMaximum <= 0 || life < 0 || life > lifeMaximum)
    {
      return NpcProjectionResult.Unsupported("NPC life state is outside the SyncNPC range.");
    }

    float difficulty = state.Spawn.DifficultyScale > 0.0f
      ? state.Spawn.DifficultyScale
      : 1.0f;
    byte releaseOwner = state.Spawn.ReleaseOwner is >= 0 and <= byte.MaxValue
      ? (byte)state.Spawn.ReleaseOwner
      : (byte)0;
    return new NpcProjectionResult(
      true,
      new NpcSyncPacket(
        (short)replication.ReplicationId,
        replication.Position,
        replication.Velocity,
        state.HasTarget ? (ushort)state.TargetStableId : (ushort)0,
        state.Facing > 0,
        false,
        ai0,
        ai1,
        ai2,
        ai3,
        state.Facing > 0,
        (short)state.NetId,
        1,
        state.Spawn.SpawnedFromStatue,
        difficulty,
        false,
        0.0f,
        life,
        lifeMaximum,
        releaseOwner,
        false,
        replication.Revision),
      string.Empty);
  }

  private static bool TryProjectBehavior(
    NpcStateSnapshot state,
    out float? ai0,
    out float? ai1,
    out float? ai2,
    out float? ai3,
    out string unsupportedReason)
  {
    ai0 = null;
    ai1 = null;
    ai2 = null;
    ai3 = null;
    unsupportedReason = string.Empty;
    switch (state.Behavior.BehaviorId)
    {
      case NpcBehaviorId.OrdinaryChase:
        ai0 = Present(state.Behavior.Chase.Speed);
        ai1 = Present(state.Behavior.Chase.StoppingDistance);
        return true;
      case NpcBehaviorId.TownHome:
        ai0 = Present(state.Behavior.TownHome.HomePosition.X);
        ai1 = Present(state.Behavior.TownHome.HomePosition.Y);
        ai2 = state.Behavior.TownHome.IsHomeless ? 1.0f : null;
        ai3 = Present(state.Behavior.TownHome.ReturnTimeoutTicks);
        return true;
      case NpcBehaviorId.Segment:
        if (!state.HasSegment)
        {
          unsupportedReason = "Segment behavior is missing segment relationship state.";
          return false;
        }

        ai0 = Present(state.Segment.SegmentIndex);
        ai1 = Present(state.Segment.Root.Value);
        ai2 = Present(state.Segment.Parent.Value);
        ai3 = Present(state.Segment.Child.Value);
        return true;
      case NpcBehaviorId.FloatingEye:
        NpcFlyingState flying = state.Behavior.Flying;
        if (!IsPositiveFinite(flying.HorizontalAcceleration) ||
            !IsPositiveFinite(flying.VerticalAcceleration) ||
            !IsPositiveFinite(flying.MaximumHorizontalSpeed) ||
            !IsPositiveFinite(flying.MaximumVerticalSpeed))
        {
          unsupportedReason = "FloatingEye behavior contains an invalid movement parameter.";
          return false;
        }

        ai0 = Present(flying.HorizontalAcceleration);
        ai1 = Present(flying.VerticalAcceleration);
        ai2 = Present(flying.MaximumHorizontalSpeed);
        ai3 = Present(flying.MaximumVerticalSpeed);
        return true;
      default:
        unsupportedReason = $"NPC behavior {state.Behavior.BehaviorId} has no SyncNPC projection.";
        return false;
    }
  }

  private static float? Present(float value)
  {
    return value == 0.0f ? null : value;
  }

  private static float? Present(int value)
  {
    return value == 0 ? null : value;
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  private static bool IsPositiveFinite(float value)
  {
    return float.IsFinite(value) && value > 0.0f;
  }
}
