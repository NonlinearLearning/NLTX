using System.Numerics;

namespace Terraria.Npc;

public sealed class NpcDeathLifecycleSystem
{
  public NpcDeathLifecycleResult Reconcile(
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle)
  {
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);

    if (lifecycle.Stage == NpcLifecycleStage.Despawned)
    {
      return new NpcDeathLifecycleResult(
        IsTerminal: true,
        Transitioned: false,
        AlreadyTerminal: true);
    }

    if (!health.IsDead || !lifecycle.IsActive)
    {
      return new NpcDeathLifecycleResult(
        IsTerminal: false,
        Transitioned: false,
        AlreadyTerminal: false);
    }

    bool transitioned = lifecycle.TryCommitDespawn();
    return new NpcDeathLifecycleResult(
      IsTerminal: true,
      Transitioned: transitioned,
      AlreadyTerminal: !transitioned);
  }

  public NpcDeathLifecycleResult Reconcile(
    NpcTypeId npcType,
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    bool isLifeOwner,
    Vector2 center,
    NpcAiStateComponent aiState,
    bool isGoodWorld = false,
    float? bottomY = null,
    NpcDeathClothierSkeletronContext? clothierSkeletronContext = null,
    NpcInstanceId sourceNpcInstanceId = default,
    int netMode = 0)
  {
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);

    if (lifecycle.Stage == NpcLifecycleStage.Despawned)
    {
      return Reconcile(health, lifecycle);
    }

    var input = new NpcDeathPhaseInput(
      npcType,
      lifecycle.IsActive,
      isLifeOwner,
      health.CurrentLife,
      aiState,
      center,
      isGoodWorld,
      bottomY,
      clothierSkeletronContext,
      sourceNpcInstanceId,
      netMode);
    NpcDeathPhaseDecision decision = NpcDeathPhaseQuery.Evaluate(in input);
    if (!decision.SuppressTerminalDeath)
    {
      if (decision.AnnouncementIntent.HasValue ||
          decision.LadyBugKilledIntent.HasValue ||
          decision.SkeletronSpawnIntent.HasValue ||
          decision.WorldEffectIntent.HasValue ||
          decision.MotherSlimeDeathSplitIntent.HasValue)
      {
        return new NpcDeathLifecycleResult(
          IsTerminal: false,
          Transitioned: false,
          AlreadyTerminal: false)
        {
          PhaseDecision = decision,
          TerminalCommitPending = true,
        };
      }

      return Reconcile(health, lifecycle) with
      {
        PhaseDecision = decision,
      };
    }

    return new NpcDeathLifecycleResult(
      IsTerminal: false,
      Transitioned: false,
      AlreadyTerminal: false)
    {
      PhaseDecision = decision,
    };
  }

  public NpcDeathLifecycleResult CommitAfterPreTerminalEffects(
    NpcHealthComponent health,
    NpcLifecycleComponent lifecycle,
    NpcDeathLifecycleResult pendingResult)
  {
    ArgumentNullException.ThrowIfNull(health);
    ArgumentNullException.ThrowIfNull(lifecycle);

    if (!pendingResult.TerminalCommitPending)
    {
      return new NpcDeathLifecycleResult(
        IsTerminal: false,
        Transitioned: false,
        AlreadyTerminal: false)
      {
        PhaseDecision = pendingResult.PhaseDecision,
      };
    }

    return Reconcile(health, lifecycle) with
    {
      PhaseDecision = pendingResult.PhaseDecision,
    };
  }
}
