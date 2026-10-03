namespace Terraria.WorldStorage;

public abstract class WorldLoadApiRuntimeBindings
{
  public abstract bool TryGetApi<TApi>(out TApi api)
    where TApi : class;

  public abstract bool TryGetOwnerContext<TOwnerContext>(
    string ownerId,
    out TOwnerContext ownerContext)
    where TOwnerContext : notnull;
}
