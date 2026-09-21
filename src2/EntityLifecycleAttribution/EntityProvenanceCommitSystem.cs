namespace Terraria.EntityLifecycleAttribution;

public sealed class EntityProvenanceCommitSystem
{
  public bool Commit(EntityProvenanceComponent provenance, EntitySpawnSourceContext context)
  {
    ArgumentNullException.ThrowIfNull(provenance);

    if (!context.IsKnown)
    {
      return false;
    }

    return provenance.TryCommit(context);
  }
}
