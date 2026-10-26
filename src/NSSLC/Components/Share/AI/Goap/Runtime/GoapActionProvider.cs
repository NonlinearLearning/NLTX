// Project integration adapter for CrashKonijn/GOAP's GraphResolver and ActionRunner.
// It captures explicit per-agent observations; it does not replace GraphResolverJob's search.
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Ai.Goap.Core;
using Terraria.Ai.Goap.Resolver;

namespace Terraria.Ai.Goap.Runtime;

/// <summary>One per agent. Immutable definitions and a frozen resolver may be shared.</summary>
public sealed class GoapActionProvider {
  private readonly GraphResolver _resolver;
  private IGoal[] _goals;

  public float DistanceMultiplier { get; set; } = 1f;
  public JobResult? CurrentPlan { get; private set; }

  public GoapActionProvider(GraphResolver resolver, params IGoal[] goals) {
    ArgumentNullException.ThrowIfNull(resolver);
    ArgumentNullException.ThrowIfNull(goals);
    _resolver = resolver;
    _goals = (IGoal[])goals.Clone();
    ValidateGoals(_goals);
  }

  public void RequestGoals(params IGoal[] goals) {
    ArgumentNullException.ThrowIfNull(goals);
    IGoal[] requested = (IGoal[])goals.Clone();
    ValidateGoals(requested);
    _goals = requested;
    CurrentPlan = null;
    // Caller requests resolution or stops the current action according to gameplay policy.
  }

  public void ResolveAction(IAgent agent) {
    ArgumentNullException.ThrowIfNull(agent);
    IWorldData world = agent.Environment.WorldData;
    IGoal[] pending = _goals.Where(goal => !goal.Conditions.All(world.IsTrue)).ToArray();
    if (pending.Length == 0) {
      CurrentPlan = new JobResult();
      agent.StopAction(false);
      return;
    }

    IEnabledBuilder enabled = _resolver.GetEnabledBuilder();
    IExecutableBuilder executable = _resolver.GetExecutableBuilder();
    IPositionBuilder positions = _resolver.GetPositionBuilder();
    ICostBuilder costs = _resolver.GetCostBuilder();
    IConditionBuilder conditions = _resolver.GetConditionBuilder();
    Dictionary<IGoapAction, ITarget?> targets = new(ReferenceEqualityComparer.Instance);
    foreach (INode node in _resolver.GetGraph().AllNodes) {
      bool allMet = true;
      foreach (INodeCondition condition in node.Conditions) {
        bool met = world.IsTrue(condition.Condition);
        conditions.SetConditionMet(condition.Condition, met);
        allMet &= met;
      }
      if (node.Action is IGoal goal) {
        costs.SetCost(goal, goal.GetCost(agent));
      } else if (node.Action is IGoapAction action) {
        ITarget? target = agent.Environment.GetTarget(action);
        targets.Add(action, target);
        enabled.SetEnabled(action, action.IsEnabled(agent));
        executable.SetExecutable(action, action.IsExecutable(agent, allMet));
        positions.SetPosition(action, target?.Position);
        costs.SetCost(action, action.GetCost(agent, target));
      }
    }

    RunData data = new() {
      StartIndex = pending.Select(goal => _resolver.GetIndex(goal)).ToArray(),
      AgentPosition = agent.Position,
      DistanceMultiplier = DistanceMultiplier,
      IsEnabled = enabled.Build(),
      IsExecutable = executable.Build(),
      Positions = positions.Build(),
      Costs = costs.Build(),
      ConditionsMet = conditions.Build()
    };
    CurrentPlan = _resolver.StartResolve(data).Complete();
    if (CurrentPlan.Actions.FirstOrDefault() is not IGoapAction next) {
      agent.StopAction(false);
      return;
    }
    // Replan from actual observations after each action; predicted effects never edit ECS state.
    if (!ReferenceEquals(agent.ActionState.Action, next)) {
      agent.SetAction(next, targets[next]);
    }
  }

  private void ValidateGoals(IEnumerable<IGoal> goals) {
    foreach (IGoal goal in goals) {
      ArgumentNullException.ThrowIfNull(goal);
      if (_resolver.GetIndex(goal) < 0) {
        throw new ArgumentException("Every requested goal must belong to the graph.", nameof(goals));
      }
    }
  }
}
