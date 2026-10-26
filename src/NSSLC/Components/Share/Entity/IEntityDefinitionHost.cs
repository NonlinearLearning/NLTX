namespace Terraria.Combat;

/// <summary>
/// Platform adapter that atomically creates or destroys an ECS entity from a definition.
/// </summary>
public interface IEntityDefinitionHost<TEntity> where TEntity : notnull {
  /// <summary>
  /// Creates an entity atomically. If this method throws, no live entity may remain registered.
  /// </summary>
  TEntity Create(EntityDefinition definition, EntityComponentSet components);

  /// <summary>Removes the entity and all host-owned indexes for its current identity.</summary>
  void Destroy(TEntity entity);
}
