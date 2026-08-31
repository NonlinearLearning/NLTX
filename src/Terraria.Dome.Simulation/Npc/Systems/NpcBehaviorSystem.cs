using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcBehaviorResult(SimulationVector Velocity, int Facing);

public sealed class NpcBehaviorSystem
{
  private readonly NpcBehaviorRegistry _registry;

  public NpcBehaviorSystem()
  {
    _registry = new NpcBehaviorRegistry(new Dictionary<NpcBehaviorId, NpcBehaviorHandler>
    {
      [NpcBehaviorId.OrdinaryChase] = EvaluateChase,
      [NpcBehaviorId.TownHome] = EvaluateTownHome,
      [NpcBehaviorId.FloatingEye] = EvaluateFloatingEye,
      [NpcBehaviorId.Segment] = EvaluateNoOp,
      [NpcBehaviorId.TrainingDummy] = EvaluateNoOp
    });
  }

  public NpcBehaviorSystem(NpcBehaviorRegistry registry)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
  }

  public NpcBehaviorResult Evaluate(
    NpcBehaviorStateComponent state,
    NpcTargetComponent target,
    SimulationVector npcPosition,
    SimulationVector targetPosition,
    bool isDayTime)
  {
    if (!float.IsFinite(npcPosition.X) || !float.IsFinite(npcPosition.Y) ||
        !float.IsFinite(targetPosition.X) || !float.IsFinite(targetPosition.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(npcPosition));
    }

    if (!_registry.TryGet(state.BehaviorId, out NpcBehaviorHandler handler))
    {
      throw new InvalidOperationException(
        $"NPC behavior {state.BehaviorId} is not registered.");
    }

    return handler.Invoke(new NpcBehaviorInput(
      state,
      target,
      npcPosition,
      targetPosition,
      isDayTime));
  }

  private static NpcBehaviorResult EvaluateChase(NpcBehaviorInput input)
  {
    NpcChaseState chase = input.State.Chase;
    NpcTargetComponent target = input.Target;
    SimulationVector npcPosition = input.NpcPosition;
    SimulationVector targetPosition = input.TargetPosition;
    if (!target.HasTarget || !float.IsFinite(chase.Speed) || chase.Speed < 0.0f ||
        !float.IsFinite(chase.StoppingDistance) || chase.StoppingDistance < 0.0f)
    {
      return new NpcBehaviorResult(default, 0);
    }

    float horizontalDistance = targetPosition.X - npcPosition.X;
    float verticalVelocity = targetPosition.Y - npcPosition.Y;
    if (MathF.Abs(horizontalDistance) <= chase.StoppingDistance)
    {
      return new NpcBehaviorResult(new SimulationVector(0.0f, verticalVelocity), 0);
    }

    float direction = MathF.Sign(horizontalDistance);
    return new NpcBehaviorResult(new SimulationVector(direction * chase.Speed, verticalVelocity),
      direction > 0.0f ? 1 : -1);
  }

  private static NpcBehaviorResult EvaluateTownHome(NpcBehaviorInput input)
  {
    NpcTownHomeState home = input.State.TownHome;
    SimulationVector npcPosition = input.NpcPosition;
    bool isDayTime = input.IsDayTime;
    if (!isDayTime || home.IsHomeless || !IsFinite(home.HomePosition) ||
        home.ReturnTimeoutTicks < 0)
    {
      return new NpcBehaviorResult(default, 0);
    }

    float horizontalDistance = home.HomePosition.X - npcPosition.X;
    if (MathF.Abs(horizontalDistance) < 0.1f)
    {
      return new NpcBehaviorResult(default, 0);
    }

    float direction = MathF.Sign(horizontalDistance);
    return new NpcBehaviorResult(
      new SimulationVector(direction * 0.5f, 0.0f),
      direction > 0.0f ? 1 : -1);
  }

  private static NpcBehaviorResult EvaluateFloatingEye(NpcBehaviorInput input)
  {
    NpcFlyingState flying = input.State.Flying;
    NpcTargetComponent target = input.Target;
    SimulationVector npcPosition = input.NpcPosition;
    SimulationVector targetPosition = input.TargetPosition;
    if (!target.HasTarget || !IsFinite(flying.HorizontalAcceleration) ||
        !IsFinite(flying.VerticalAcceleration) || !IsFinite(flying.MaximumHorizontalSpeed) ||
        !IsFinite(flying.MaximumVerticalSpeed) || flying.HorizontalAcceleration <= 0.0f ||
        flying.VerticalAcceleration <= 0.0f || flying.MaximumHorizontalSpeed <= 0.0f ||
        flying.MaximumVerticalSpeed <= 0.0f)
    {
      return new NpcBehaviorResult(default, 0);
    }

    float horizontalDirection = MathF.Sign(targetPosition.X - npcPosition.X);
    float verticalDirection = MathF.Sign(targetPosition.Y - npcPosition.Y);
    float horizontalVelocity = Math.Clamp(
      horizontalDirection * flying.HorizontalAcceleration,
      -flying.MaximumHorizontalSpeed,
      flying.MaximumHorizontalSpeed);
    float verticalVelocity = Math.Clamp(
      verticalDirection * flying.VerticalAcceleration,
      -flying.MaximumVerticalSpeed,
      flying.MaximumVerticalSpeed);
    return new NpcBehaviorResult(
      new SimulationVector(horizontalVelocity, verticalVelocity),
      horizontalDirection > 0.0f ? 1 : horizontalDirection < 0.0f ? -1 : 0);
  }

  private static NpcBehaviorResult EvaluateNoOp(NpcBehaviorInput input)
  {
    return new NpcBehaviorResult(default, 0);
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  private static bool IsFinite(float value)
  {
    return float.IsFinite(value);
  }
}
