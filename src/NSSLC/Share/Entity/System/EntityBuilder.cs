using System;
using System.Collections.Generic;
using Terraria.Share.Ai;

namespace Terraria.Share.Entities;

/// <summary>
/// Builds one immutable entity definition and validates its composition contract.
/// </summary>
public sealed class EntityBuilder {
  private readonly EntityId _id;
  private readonly List<EntityDefinition.ComponentFactory> _factories = [];
  private readonly HashSet<Type> _requiredComponents = [];
  private AiComponent? _ai;

  public EntityBuilder(EntityId id) {
    _id = id;
  }

  public EntityBuilder Add<TComponent>(Func<TComponent> factory)
      where TComponent : notnull {
    ArgumentNullException.ThrowIfNull(factory);
    Type type = typeof(TComponent);
    EnsureNotRegistered(type);
    _factories.Add(new EntityDefinition.ComponentFactory(type, () => factory()!));
    return this;
  }

  public EntityBuilder AddValue<TComponent>(TComponent value) where TComponent : struct {
    return Add(() => value);
  }

  public EntityBuilder Require<TComponent>() {
    _requiredComponents.Add(typeof(TComponent));
    return this;
  }

  /// <summary>Declares the one AI behavior this entity definition binds.</summary>
  public EntityBuilder WithAi<TBehavior>() where TBehavior : IAiBehavior {
    if (_ai.HasValue) {
      throw new InvalidOperationException(
          "An entity definition can declare only one AI component.");
    }
    _ai = AiComponent.For<TBehavior>();
    AddValue(_ai.Value);
    return this;
  }

  public EntityDefinition Build() {
    HashSet<Type> provided = [];
    foreach (EntityDefinition.ComponentFactory factory in _factories) {
      provided.Add(factory.ComponentType);
    }
    List<Type> missing = [];
    foreach (Type required in _requiredComponents) {
      if (!provided.Contains(required)) {
        missing.Add(required);
      }
    }
    if (missing.Count != 0) {
      throw new InvalidOperationException(
          $"Entity definition {_id} is missing required components: " +
          string.Join(", ", missing));
    }
    return new EntityDefinition(
        _id,
        _factories.ToArray(),
        new List<Type>(_requiredComponents).ToArray(),
        _ai);
  }

  private void EnsureNotRegistered(Type type) {
    foreach (EntityDefinition.ComponentFactory factory in _factories) {
      if (factory.ComponentType == type) {
        throw new InvalidOperationException(
            $"Component {type.FullName} is already registered.");
      }
    }
  }
}

