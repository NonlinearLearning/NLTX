namespace Terraria.Entity;

/// <summary>Optional host boundary for tick-deferred Arch entity creation.</summary>
public interface IDeferredEntityDefinitionHost<TEntity> where TEntity : notnull
{
  EcsServerEntityCreationTicket CreateDeferred(
      EntityDefinition definition,
      EntityComponentSet components,
      EcsServerCommandBuffer commands);
}
