// Adapted from CrashKonijn/GOAP GoalBase; see LICENSE.txt and SOURCES.md.
using System;
using Terraria.Share.Ai.Goap.Core;

namespace Terraria.Share.Ai.Goap.Runtime;

public abstract class GoalBase : IGoal {
  public Guid Guid { get; } = Guid.NewGuid();
  public IEffect[] Effects { get; } = [];
  public ICondition[] Conditions { get; }
  public float BaseCost { get; }

  protected GoalBase(float baseCost, params ICondition[] conditions) {
    if (!float.IsFinite(baseCost) || baseCost < 0f) {
      throw new ArgumentOutOfRangeException(nameof(baseCost));
    }
    BaseCost = baseCost;
    Conditions = (ICondition[])conditions.Clone();
  }

  public virtual float GetCost(IAgent agent) {
    return BaseCost;
  }
}
