using System;
using Terraria.Share.Ai.Htn;
using Terraria.Share.Ai.Htn.Contexts;

namespace Terraria.Share.Ai;

/// <summary>
/// FluidHTN adapter implementing the common snapshot, output and lifecycle contract.
/// </summary>
public abstract class HtnAiBehavior<TContext, TIntent> : AiBehavior<TIntent>
    where TContext : BaseContext
    where TIntent : unmanaged {
  private readonly HtnAgent<TContext> _agent;

  protected TContext _context => _agent.Context;

  protected HtnAiBehavior(AiRequirements requirements, Domain<TContext> domain, TContext context)
      : base(requirements) {
    ArgumentNullException.ThrowIfNull(domain);
    ArgumentNullException.ThrowIfNull(context);
    _agent = new HtnAgent<TContext>(domain, context);
  }

  protected abstract void ClearIntents();
  protected abstract void ReadSnapshot(AiSnapshot snapshot, float deltaTime);
  protected abstract void WriteIntents(AiOutput<TIntent> output);

  protected sealed override void CalculateCore(AiSnapshot snapshot, float deltaTime,
                                               AiOutput<TIntent> output) {
    ClearIntents();
    ReadSnapshot(snapshot, deltaTime);
    _agent.Tick();
    WriteIntents(output);
  }

  protected sealed override void StopCore(AiOutput<TIntent> output) {
    ClearIntents();
    _agent.Stop();
    WriteIntents(output);
  }

  protected sealed override void DisposeCore(AiOutput<TIntent> output) {
    _agent.Dispose();
  }
}
