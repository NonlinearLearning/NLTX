using System;

namespace Terraria.Entity;

/// <summary>A newly-created value for one component slot in an entity spawn plan.</summary>
public readonly record struct EntityComponentValue {
  public Type ComponentType { get; }
  public object Value { get; }

  public EntityComponentValue(Type componentType, object value) {
    ArgumentNullException.ThrowIfNull(componentType);
    ArgumentNullException.ThrowIfNull(value);
    if (!componentType.IsInstanceOfType(value)) {
      throw new ArgumentException("The value does not match the component type.", nameof(value));
    }
    ComponentType = componentType;
    Value = value;
  }
}
