// Adapted from CrashKonijn/GOAP GoapActionBase; see LICENSE.txt and SOURCES.md.
using System;
using System.Linq;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Runtime;

public abstract class GoapActionBase<TActionData, TActionProperties>
    : AgentActionBase<TActionData, TActionProperties>, IGoapAction
    where TActionData : IActionData, new()
    where TActionProperties : class, IActionProperties, new() {
  public ActionConfig Config { get; }
  public Guid Guid { get; } = Guid.NewGuid();
  public IEffect[] Effects => Config.Effects;
  public ICondition[] Conditions => Config.Conditions;
  public override TActionProperties Properties => (TActionProperties)Config.Properties;

  protected GoapActionBase(ActionConfig config) {
    ArgumentNullException.ThrowIfNull(config);
    if (config.Properties is not TActionProperties) {
      throw new ArgumentException("Action properties have the wrong type.", nameof(config));
    }
    if (!float.IsFinite(config.BaseCost) || config.BaseCost < 0f ||
        !float.IsFinite(config.StoppingDistance) || config.StoppingDistance < 0f) {
      throw new ArgumentException("Costs and distances must be finite and nonnegative.",
                                  nameof(config));
    }
    Config = config;
  }

  public virtual float GetCost(IAgent agent, ITarget? target) {
    return Config.BaseCost;
  }

  public ActionMoveMode GetMoveMode(IAgent agent) {
    return Config.MoveMode;
  }

  public virtual float GetStoppingDistance() {
    return Config.StoppingDistance;
  }

  public bool IsInRange(IAgent agent, float distance, IActionData data) {
    return IsInRange(agent, distance, (TActionData)data);
  }

  public virtual bool IsInRange(IAgent agent, float distance, TActionData data) {
    return distance <= GetStoppingDistance();
  }

  public bool IsValid(IAgent agent, IActionData data) {
    if (Config.ValidateConditions && !Conditions.All(agent.Environment.WorldData.IsTrue)) {
      return false;
    }
    if (Config.ValidateTarget && Config.RequiresTarget &&
        (data.Target == null || !data.Target.IsValid())) {
      return false;
    }
    return IsValid(agent, (TActionData)data);
  }

  public virtual bool IsValid(IAgent agent, TActionData data) {
    return true;
  }

  public virtual bool IsExecutable(IAgent agent, bool conditionsMet) {
    return conditionsMet && (!Config.RequiresTarget ||
                            agent.Environment.GetTarget(this)?.IsValid() == true);
  }

  public virtual bool IsEnabled(IAgent agent) {
    return !agent.IsDisabled(this);
  }
}
