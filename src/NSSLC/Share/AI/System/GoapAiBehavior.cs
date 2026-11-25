using System;
using Terraria.Share.Ai.Goap.Core;
using Terraria.Share.Ai.Goap.Resolver;
using Terraria.Share.Ai.Goap.Runtime;

namespace Terraria.Share.Ai;

/// <summary>
/// GOAP adapter; each instance owns its environment, action runtime and resolver.
/// </summary>
public abstract class GoapAiBehavior<TEnvironment, TIntent> : AiBehavior<TIntent>
    where TEnvironment : GoapAiEnvironment<TIntent>
    where TIntent : unmanaged {
  private readonly GraphResolver _resolver;
  private readonly GoapAgent _agent;

  protected readonly TEnvironment _environment;

  protected GoapAiBehavior(AiRequirements requirements, TEnvironment environment,
                           IGoapAction[] actions, params IGoal[] goals) : base(requirements) {
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentNullException.ThrowIfNull(actions);
    ArgumentNullException.ThrowIfNull(goals);
    _environment = environment;
    IConnectable[] definitions = new IConnectable[actions.Length + goals.Length];
    for (int i = 0; i < actions.Length; i++) {
      ArgumentNullException.ThrowIfNull(actions[i]);
      actions[i].Created();
      definitions[i] = actions[i];
    }
    for (int i = 0; i < goals.Length; i++) {
      ArgumentNullException.ThrowIfNull(goals[i]);
      definitions[actions.Length + i] = goals[i];
    }
    _resolver = new GraphResolver(definitions, new KeyResolver());
    GoapActionProvider provider = new(_resolver, goals);
    _agent = new GoapAgent(environment, provider);
  }

  protected sealed override void CalculateCore(AiSnapshot snapshot, float deltaTime,
                                               AiOutput<TIntent> output) {
    _environment.Begin(output);
    try {
      _environment.Refresh(snapshot, deltaTime);
      // Request each frame; GoapAgent still observes the current action's MayResolve policy.
      _agent.ResolveAction();
      _agent.Run(deltaTime);
    } finally {
      _environment.End();
    }
  }

  protected sealed override void StopCore(AiOutput<TIntent> output) {
    _environment.Begin(output);
    try {
      _agent.StopAction(false);
    } finally {
      _environment.End();
    }
  }

  protected sealed override void DisposeCore(AiOutput<TIntent> output) {
    _environment.Begin(output);
    try {
      _agent.Dispose();
    } finally {
      _resolver.Dispose();
      _environment.End();
    }
  }
}
