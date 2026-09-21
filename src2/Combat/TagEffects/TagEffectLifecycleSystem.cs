using Terraria.Combat.TagEffects.Commands;
using Terraria.Content.StatusEffects;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Combat.TagEffects;

public sealed class TagEffectLifecycleSystem
{
  private readonly TagEffectBehaviorPort _behavior;

  public TagEffectLifecycleSystem()
    : this(new TagEffectBehaviorPort())
  {
  }

  public TagEffectLifecycleSystem(TagEffectBehaviorPort behavior)
  {
    _behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
  }

  public bool Apply(
    PlayerTagEffectStateComponent state,
    ApplyTagToNpcCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    TagEffectDefinition? definition = state.ActiveEffectDefinition;
    if (definition is null || !command.TargetReference.IsValid || definition.TagDurationTicks <= 0)
    {
      return false;
    }

    int procTicks = state.TryGetMark(
      command.TargetReference,
      out PlayerTagEffectStateComponent.NpcTagMark previous)
      ? previous.ProcTicksRemaining
      : 0;
    state.SetMark(command.TargetReference, definition.TagDurationTicks, procTicks);
    _behavior.OnTagAppliedToNpc(definition, state.OwnerReference, command.TargetReference);
    return true;
  }

  public bool Apply(
    PlayerTagEffectStateComponent state,
    ClearTagProcCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.TryGetMark(
      command.TargetReference,
      out PlayerTagEffectStateComponent.NpcTagMark mark))
    {
      return false;
    }

    state.SetMark(command.TargetReference, mark.TagTicksRemaining, 0);
    return true;
  }

  public bool Apply(
    PlayerTagEffectStateComponent state,
    EnableTagProcCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    TagEffectDefinition? definition = state.ActiveEffectDefinition;
    if (definition is null || !TagEffectHitEligibilityQuery.IsTagged(
      state,
      command.TargetReference))
    {
      return false;
    }

    int procTicks = Math.Max(1, definition.TagDurationTicks);
    state.TryGetMark(
      command.TargetReference,
      out PlayerTagEffectStateComponent.NpcTagMark mark);
    state.SetMark(command.TargetReference, mark.TagTicksRemaining, procTicks);
    return true;
  }

  public bool Apply(
    PlayerTagEffectStateComponent state,
    ResetNpcTagStateCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.TryGetMark(command.TargetReference, out _))
    {
      return false;
    }

    state.RemoveMark(command.TargetReference);
    return true;
  }

  public bool Apply(
    PlayerTagEffectStateComponent state,
    SetActiveTagEffectCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (command.Definition is null || command.EffectTypeId < 0 ||
      command.Definition.EffectTypeId != command.EffectTypeId)
    {
      return false;
    }
    if (state.ActiveEffectTypeId == command.EffectTypeId &&
      ReferenceEquals(state.ActiveEffectDefinition, command.Definition))
    {
      return false;
    }

    TagEffectDefinition? previous = state.ActiveEffectDefinition;
    if (previous is not null)
    {
      _behavior.OnRemovedFromPlayer(previous, state.OwnerReference);
    }

    state.ReplaceActiveEffect(command.EffectTypeId, command.Definition);
    _behavior.OnSetToPlayer(command.Definition, state.OwnerReference);
    return true;
  }

  public int Advance(PlayerTagEffectStateComponent state, int ticks)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    int removedCount = 0;
    for (int tick = 0; tick < ticks; tick++)
    {
      foreach (PlayerTagEffectStateComponent.NpcTagMark mark in state.Marks.ToArray())
      {
        int tagTicks = Math.Max(0, mark.TagTicksRemaining - 1);
        int procTicks = Math.Max(0, mark.ProcTicksRemaining - 1);
        if (tagTicks == 0 && procTicks == 0)
        {
          state.RemoveMark(mark.TargetReference);
          removedCount++;
        }
        else
        {
          state.SetMark(mark.TargetReference, tagTicks, procTicks);
        }
      }
    }

    return removedCount;
  }
}
