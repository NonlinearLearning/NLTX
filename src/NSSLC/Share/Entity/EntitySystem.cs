using System;

namespace Terraria.Share.Entities;

/// <summary>
/// Coordinates immutable definitions with a platform ECS creation boundary.
/// </summary>
public sealed class EntitySystem<TEntity> where TEntity : notnull {
  private readonly EntityRegistry _definitions;
  private readonly IEntityDefinitionHost<TEntity> _host;

  public EntitySystem(EntityRegistry definitions,
                                IEntityDefinitionHost<TEntity> host) {
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(host);
    _definitions = definitions;
    _host = host;
  }

  /// <summary>
  /// Builds independent component values and passes the complete composition to the host.
  /// </summary>
  public TEntity Spawn(EntityId definitionId) {
    EntityDefinition definition = _definitions.Get(definitionId);
    EntityComponentSet components = definition.CreateComponents();
    return _host.Create(definition, components);
  }

  public EcsServerEntityCreationTicket SpawnDeferred(
      EntityId definitionId,
      EcsServerCommandBuffer commands) {
    ArgumentNullException.ThrowIfNull(commands);
    if (_host is not IDeferredEntityDefinitionHost<TEntity> deferredHost) {
      throw new InvalidOperationException(
          $"The entity host {_host.GetType().FullName} does not support deferred creation.");
    }

    EntityDefinition definition = _definitions.Get(definitionId);
    EntityComponentSet components = definition.CreateComponents();
    return deferredHost.CreateDeferred(definition, components, commands);
  }

  public void Despawn(TEntity entity) {
    ArgumentNullException.ThrowIfNull(entity);
    _host.Destroy(entity);
  }
}

