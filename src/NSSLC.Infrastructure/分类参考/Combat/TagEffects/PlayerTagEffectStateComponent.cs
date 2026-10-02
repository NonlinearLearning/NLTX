using System.Collections.Generic;
using Terraria.Content.StatusEffects;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Combat.TagEffects;

public sealed class PlayerTagEffectStateComponent
{
  public readonly record struct NpcTagMark(
    EntityReference TargetReference,
    int TagTicksRemaining,
    int ProcTicksRemaining);

  private readonly Dictionary<EntityReference, NpcTagMark> _marks = new();

  public PlayerTagEffectStateComponent(EntityReference ownerReference)
  {
    OwnerReference = ownerReference;
    ActiveEffectTypeId = -1;
  }

  public int ActiveEffectTypeId { get; private set; }

  public TagEffectDefinition? ActiveEffectDefinition { get; private set; }

  public IReadOnlyCollection<NpcTagMark> Marks => _marks.Values;

  public EntityReference OwnerReference { get; }

  public bool TryGetMark(EntityReference targetReference, out NpcTagMark mark)
  {
    return _marks.TryGetValue(targetReference, out mark);
  }

  internal void ClearMarks()
  {
    _marks.Clear();
  }

  internal void RemoveMark(EntityReference targetReference)
  {
    _marks.Remove(targetReference);
  }

  internal void ReplaceActiveEffect(int effectTypeId, TagEffectDefinition definition)
  {
    ActiveEffectTypeId = effectTypeId;
    ActiveEffectDefinition = definition;
    ClearMarks();
  }

  internal void SetMark(EntityReference targetReference, int tagTicks, int procTicks)
  {
    if (!targetReference.IsValid || tagTicks <= 0 && procTicks <= 0)
    {
      _marks.Remove(targetReference);
      return;
    }

    _marks[targetReference] = new NpcTagMark(
      targetReference,
      Math.Max(0, tagTicks),
      Math.Max(0, procTicks));
  }
}
