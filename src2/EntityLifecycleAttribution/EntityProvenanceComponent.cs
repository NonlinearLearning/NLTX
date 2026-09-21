namespace Terraria.EntityLifecycleAttribution;

public sealed class EntityProvenanceComponent
{
  public bool IsCommitted { get; private set; }

  public EntitySpawnSourceContext Context { get; private set; }

  internal bool TryCommit(EntitySpawnSourceContext context)
  {
    if (IsCommitted)
    {
      return false;
    }

    Context = context;
    IsCommitted = true;
    return true;
  }
}
