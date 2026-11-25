using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Share.Movement;

/// <summary>
/// 保存实体当前活动的重力修饰器，并维护来源序号。
/// </summary>
public sealed class GravityModifierCollectionComponent
{
  private readonly List<GravityModifier> _modifiers = [];
  private readonly ReadOnlyCollection<GravityModifier> _readOnlyModifiers;
  private long _nextSequence;

  public GravityModifierCollectionComponent()
  {
    _readOnlyModifiers = _modifiers.AsReadOnly();
  }

  public IReadOnlyList<GravityModifier> Modifiers => _readOnlyModifiers;

  public GravityModifier Add(GravityModifier modifier)
  {
        modifier.Validate();
    GravityModifier sequenced = modifier with { Sequence = ++_nextSequence };
    _modifiers.Add(sequenced);
    return sequenced;
  }

  public int RemoveBySource(long sourceId)
  {
    return _modifiers.RemoveAll(modifier => modifier.SourceId == sourceId);
  }

  internal void Replace(IReadOnlyList<GravityModifier> modifiers)
  {
    _modifiers.Clear();
    _modifiers.AddRange(modifiers);
  }
}
