using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Attributes;

/// <summary>
/// 声明实体拥有属性能力，并保存该实体的基础值和活动属性修饰器。
/// </summary>
public sealed class AttributeSetComponent
{
  private readonly List<AttributeBaseValue> _baseValues = [];
  private readonly List<AttributeModifier> _modifiers = [];
  private readonly ReadOnlyCollection<AttributeBaseValue> _readOnlyBaseValues;
  private readonly ReadOnlyCollection<AttributeModifier> _readOnlyModifiers;
  private long _nextSequence;

  public AttributeSetComponent()
  {
    _readOnlyBaseValues = _baseValues.AsReadOnly();
    _readOnlyModifiers = _modifiers.AsReadOnly();
  }

  /// <summary>
  /// Creates an independent runtime copy of this attribute set.
  /// </summary>
  public AttributeSetComponent Clone()
  {
    AttributeSetComponent clone = new();
    clone._baseValues.AddRange(_baseValues);
    clone._modifiers.AddRange(_modifiers);
    clone._nextSequence = _nextSequence;
    return clone;
  }

  /// <summary>
  /// 返回基础值的只读视图。实体必须先拥有本组件，才有属性基础值。
  /// </summary>
  public IReadOnlyList<AttributeBaseValue> BaseValues => _readOnlyBaseValues;

  /// <summary>
  /// 返回活动修饰器的只读视图。实际存储为可变 List，以支持来源增删和过期清理。
  /// </summary>
  public IReadOnlyList<AttributeModifier> Modifiers => _readOnlyModifiers;

  public void SetBaseValue(AttributeBaseValue baseValue)
  {
    int index = _baseValues.FindIndex(item => item.Attribute == baseValue.Attribute);
    if (index >= 0)
    {
      _baseValues[index] = baseValue;
      return;
    }

    _baseValues.Add(baseValue);
  }

  public bool TryGetBaseValue(AttributeKind attribute, out float value)
  {
    foreach (AttributeBaseValue baseValue in _baseValues)
    {
      if (baseValue.Attribute == attribute)
      {
        value = baseValue.Value;
        return true;
      }
    }

    value = default;
    return false;
  }

  public AttributeModifier AddModifier(AttributeModifier modifier)
  {
    modifier.Validate();
    if (_nextSequence == long.MaxValue)
    {
      throw new InvalidOperationException("The attribute modifier sequence is exhausted.");
    }

    AttributeModifier sequenced = modifier with { Sequence = ++_nextSequence };
    _modifiers.Add(sequenced);
    return sequenced;
  }

  public int RemoveBySource(long sourceId)
  {
    if (sourceId == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sourceId));
    }

    return _modifiers.RemoveAll(modifier => modifier.SourceId == sourceId);
  }

  public int RemoveBySourceCategory(AttributeSourceCategory sourceCategory)
  {
    if (!Enum.IsDefined(sourceCategory))
    {
      throw new ArgumentOutOfRangeException(
        nameof(sourceCategory),
        sourceCategory,
        "Unknown attribute source category.");
    }

    return _modifiers.RemoveAll(modifier => modifier.SourceCategory == sourceCategory);
  }

  internal void ReplaceModifiers(IReadOnlyList<AttributeModifier> modifiers)
  {
    ArgumentNullException.ThrowIfNull(modifiers);
    _modifiers.Clear();
    _modifiers.AddRange(modifiers);
  }
}
