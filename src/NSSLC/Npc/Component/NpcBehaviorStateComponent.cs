using System;

namespace Terraria.Npc;

/// <summary>Stores only observable NPC behavior progress between simulation ticks.</summary>
/// <remarks>
/// Planners, contexts, worlds, Arch handles and service references belong to AiBinding and are not
/// persisted in this component. Domain-specific target, combat, spawn and home state stays separate.
/// </remarks>
public readonly record struct NpcBehaviorStateComponent
{
  public NpcBehaviorStateComponent(
      NpcBehaviorPhase phase = NpcBehaviorPhase.Idle,
      long stateEnteredTick = 0,
      long lastDecisionTick = 0,
      int decisionCooldown = 0,
      int actionPhase = 0,
      float stateSlot0 = 0f,
      float stateSlot1 = 0f)
  {
    if (!Enum.IsDefined(phase))
    {
      throw new ArgumentOutOfRangeException(nameof(phase));
    }

    if (stateEnteredTick < 0 || lastDecisionTick < 0 || decisionCooldown < 0 || actionPhase < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stateEnteredTick));
    }

    if (!float.IsFinite(stateSlot0) || !float.IsFinite(stateSlot1))
    {
      throw new ArgumentOutOfRangeException(nameof(stateSlot0));
    }

    Phase = phase;
    StateEnteredTick = stateEnteredTick;
    LastDecisionTick = lastDecisionTick;
    DecisionCooldown = decisionCooldown;
    ActionPhase = actionPhase;
    StateSlot0 = stateSlot0;
    StateSlot1 = stateSlot1;
  }

  public NpcBehaviorPhase Phase { get; }

  public long StateEnteredTick { get; }

  public long LastDecisionTick { get; }

  public int DecisionCooldown { get; }

  public int ActionPhase { get; }

  public float StateSlot0 { get; }

  public float StateSlot1 { get; }

  public NpcBehaviorStateComponent WithDecision(
      NpcBehaviorPhase phase,
      long tick,
      int decisionCooldown,
      int actionPhase,
      float stateSlot0 = 0f,
      float stateSlot1 = 0f)
  {
    if (tick < LastDecisionTick)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    long enteredTick = phase == Phase ? StateEnteredTick : tick;
    return new NpcBehaviorStateComponent(
        phase,
        enteredTick,
        tick,
        decisionCooldown,
        actionPhase,
        stateSlot0,
        stateSlot1);
  }
}
