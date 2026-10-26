using System;
using System.Collections.Generic;
using Terraria.Ai;

namespace Terraria.Combat;

/// <summary>
/// Builds one immutable entity definition and validates its composition contract.
/// </summary>
public sealed class EntityDefinitionBuilder {
  private readonly EntityDefinitionId _id;
  private readonly List<EntityDefinition.ComponentFactory> _factories = [];
  private readonly HashSet<Type> _requiredComponents = [];
  private AiComponent? _ai;
  private AiRequirements? _aiSnapshotRequirements;

  public EntityDefinitionBuilder(EntityDefinitionId id) {
    _id = id;
  }

  public EntityDefinitionBuilder Add<TComponent>(Func<TComponent> factory)
      where TComponent : notnull {
    ArgumentNullException.ThrowIfNull(factory);
    Type type = typeof(TComponent);
    EnsureNotRegistered(type);
    _factories.Add(new EntityDefinition.ComponentFactory(type, () => factory()!));
    return this;
  }

  public EntityDefinitionBuilder AddValue<TComponent>(TComponent value) where TComponent : struct {
    return Add(() => value);
  }

  public EntityDefinitionBuilder Require<TComponent>() {
    _requiredComponents.Add(typeof(TComponent));
    return this;
  }

  public EntityDefinitionBuilder WithAi<TBehavior>() where TBehavior : IAiBehavior {
    return WithAi<TBehavior>(requirements: null);
  }

  /// <summary>
  /// Declares AI and promotes its snapshot requirements to definition-level requirements.
  /// </summary>
  public EntityDefinitionBuilder WithAi<TBehavior>(AiRequirements? requirements)
      where TBehavior : IAiBehavior {
    if (_ai.HasValue) {
      throw new InvalidOperationException(
          "An entity definition can declare only one AI component.");
    }
    _ai = AiComponent.For<TBehavior>();
    AddValue(_ai.Value);
    _aiSnapshotRequirements = requirements;
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
        _ai,
        _aiSnapshotRequirements);
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

