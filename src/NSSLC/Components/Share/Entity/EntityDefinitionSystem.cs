using System;

namespace Terraria.Entity;

/// <summary>
/// Coordinates immutable definitions with a platform ECS creation boundary.
/// </summary>
public sealed class EntityDefinitionSystem<TEntity> where TEntity : notnull {
  private readonly EntityDefinitionRegistry _definitions;
  private readonly IEntityDefinitionHost<TEntity> _host;

  public EntityDefinitionSystem(EntityDefinitionRegistry definitions,
                                IEntityDefinitionHost<TEntity> host) {
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(host);
    _definitions = definitions;
    _host = host;
  }

  /// <summary>
  /// Builds independent component values and passes the complete composition to the host.
  /// </summary>
  public TEntity Spawn(EntityDefinitionId definitionId) {
    EntityDefinition definition = _definitions.Get(definitionId);
    EntityComponentSet components = definition.CreateComponents();
    return _host.Create(definition, components);
  }

  public void Despawn(TEntity entity) {
    ArgumentNullException.ThrowIfNull(entity);
    _host.Destroy(entity);
  }
}

