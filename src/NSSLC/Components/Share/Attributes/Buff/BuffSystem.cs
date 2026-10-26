using System;
using Terraria.Attributes;
using Terraria.Combat;

namespace Terraria.StatusEffects;

/// <summary>
/// Owns active shared Buff application, refresh, stacking and expiry.
/// </summary>
/// <remarks>
/// The system has no world, clock or network dependency. The caller supplies the current tick
/// through an explicit tick operation and owns any periodic combat intent produced by content.
/// </remarks>
public sealed class BuffSystem
{
  private readonly BuffDefinitionRegistry _definitions;

  public BuffSystem(BuffDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitions = definitions;
  }

  public BuffApplicationResult TryApply(
      BuffReceptionCapabilityComponent? capability,
      BuffImmunityComponent? immunity,
      BuffListComponent activeBuffs,
      AttributeSetComponent? attributes,
      BuffEffectId effect,
      long sourceId,
      int requestedStacks = 1,
      int? durationTicks = null)
  {
    ArgumentNullException.ThrowIfNull(activeBuffs);
    if (requestedStacks <= 0)
    {
      return BuffApplicationResult.Rejected(BuffAdmissionResult.InvalidRequest, sourceId);
    }

    if (!_definitions.TryGet(effect, out BuffDefinition? definition))
    {
      return BuffApplicationResult.Rejected(BuffAdmissionResult.UnknownDefinition, sourceId);
    }

    BuffAdmissionResult admission = BuffAdmissionSystem.Check(
        capability,
        immunity,
        definition,
        sourceId);
    if (admission != BuffAdmissionResult.Accepted)
    {
      return BuffApplicationResult.Rejected(admission, sourceId);
    }

    int duration = durationTicks ?? definition.DurationTicks;
    if (duration == 0 || duration < -1)
    {
      return BuffApplicationResult.Rejected(BuffAdmissionResult.InvalidRequest, sourceId);
    }

    bool refreshed = activeBuffs.TryGet(effect, sourceId, out ActiveBuff previous);
    int stacks = refreshed
        ? Math.Min(definition.MaximumStacks, previous.Stacks + requestedStacks)
        : Math.Min(definition.MaximumStacks, requestedStacks);

    if (refreshed && attributes is not null)
    {
      AttributeSystem.RemoveSource(attributes, sourceId);
    }

    ActiveBuff activeBuff = new ActiveBuff(
        effect,
        definition.ImmunityKey,
        sourceId,
        duration,
        stacks);
    activeBuffs.AddOrReplace(activeBuff);
    if (attributes is not null)
    {
      AddAttributeModifiers(attributes, definition, activeBuff);
    }

    return new BuffApplicationResult(
        BuffAdmissionResult.Accepted,
        !refreshed,
        refreshed,
        stacks,
        sourceId);
  }

  public int Advance(
      BuffListComponent activeBuffs,
      AttributeSetComponent? attributes = null)
  {
    ArgumentNullException.ThrowIfNull(activeBuffs);
    IReadOnlyList<ActiveBuff> expired = activeBuffs.AdvanceOneTick();
    if (attributes is not null)
    {
      foreach (ActiveBuff activeBuff in expired)
      {
        AttributeSystem.RemoveSource(attributes, activeBuff.SourceId);
      }
    }

    return expired.Count;
  }

  private static void AddAttributeModifiers(
      AttributeSetComponent attributes,
      BuffDefinition definition,
      ActiveBuff activeBuff)
  {
    foreach (BuffAttributeModifierDefinition modifier in definition.Modifiers)
    {
      attributes.AddModifier(new AttributeModifier(
          modifier.Attribute,
          AttributeSourceCategory.Buff,
          modifier.Operation,
          modifier.Value * activeBuff.Stacks,
          activeBuff.SourceId,
          modifier.Priority,
          activeBuff.RemainingTicks));
    }
  }
}
