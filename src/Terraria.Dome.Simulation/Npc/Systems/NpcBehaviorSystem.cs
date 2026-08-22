using System;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcBehaviorResult(SimulationVector Velocity, int Facing);

public sealed class NpcBehaviorSystem
{
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

    return state.BehaviorId switch
    {
      NpcBehaviorId.OrdinaryChase => EvaluateChase(state.Chase, target, npcPosition, targetPosition),
      NpcBehaviorId.TownHome => EvaluateTownHome(state.TownHome, npcPosition, isDayTime),
      NpcBehaviorId.Segment => new NpcBehaviorResult(default, 0),
      NpcBehaviorId.TrainingDummy => new NpcBehaviorResult(default, 0),
      _ => new NpcBehaviorResult(default, 0)
    };
  }

  private static NpcBehaviorResult EvaluateChase(
    NpcChaseState chase,
    NpcTargetComponent target,
    SimulationVector npcPosition,
    SimulationVector targetPosition)
  {
    if (!target.HasTarget)
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

  private static NpcBehaviorResult EvaluateTownHome(
    NpcTownHomeState home,
    SimulationVector npcPosition,
    bool isDayTime)
  {
    if (!isDayTime || home.IsHomeless)
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
}
