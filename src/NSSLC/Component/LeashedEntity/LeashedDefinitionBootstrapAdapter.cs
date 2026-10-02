namespace Terraria.LeashedEntity;

/// <summary>
/// Converts ordered legacy registration input into one frozen catalog commit.
/// </summary>
public sealed class LeashedDefinitionBootstrapAdapter
{
  public IReadOnlyList<LeashedDefinitionDescriptor> Bootstrap(
    LeashedDefinitionCatalog catalog,
    IEnumerable<LeashedDefinitionRegistration> orderedDefinitions)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(orderedDefinitions);

    if (catalog.IsFrozen)
    {
      throw new InvalidOperationException("The leashed definition catalog is already frozen.");
    }

    foreach (LeashedDefinitionRegistration registration in orderedDefinitions)
    {
      catalog.Register(
        registration.Key,
        registration.Kind,
        registration.ContentId);
    }

    catalog.Freeze();
    return catalog.Snapshot();
  }
}

