using System;

namespace Terraria.Share.Attributes;

/// <summary>
/// Owns active shared Buff application, refresh, stacking and expiry.
/// </summary>
/// <remarks>
/// The system has no world, clock or network dependency. The caller supplies the current tick
/// through an explicit tick operation and owns any periodic combat intent produced by content.
/// </remarks>
public sealed class BuffSystem
{
  private readonly BuffRegistry _definitions;

  public BuffSystem(BuffRegistry definitions)
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

    if (!_definitions.TryGet(effect, out Buff? definition))
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

  /// <summary>
  /// Applies an intent to an entity's status components. The caller must be the status-effect
  /// owner and provide the mutable components from the same entity.
  /// </summary>
  public BuffApplicationResult TryApply(
      BuffReceptionCapabilityComponent? capability,
      BuffImmunityComponent? immunity,
      BuffListComponent activeBuffs,
      AttributeSetComponent? attributes,
      in BuffApplyRequestComponent request)
  {
    return TryApply(
        capability,
        immunity,
        activeBuffs,
        attributes,
        request.Effect,
        request.SourceId,
        request.RequestedStacks,
        request.DurationTicks);
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
      Buff definition,
      ActiveBuff activeBuff)
  {
    // 期限由 BuffListComponent 单一持有并推进，过期时按来源整体撤销。
    // 因此来源描述符为永久（remainingTicks: -1），避免同一个期限被存两份。
    AttributeSourceDescriptor source = new(
        activeBuff.SourceId,
        AttributeSourceCategory.Buff,
        AttributeSourceDescriptor.PermanentTicks);

    AttributeContributionSystem.Apply(
        attributes,
        new AttributeContributionSet(source, definition.Modifiers),
        activeBuff.Stacks);
  }
}
