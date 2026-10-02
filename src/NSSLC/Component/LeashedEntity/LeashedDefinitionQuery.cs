namespace Terraria.LeashedEntity;

/// <summary>
/// Read-only access to the committed definition catalog.
/// </summary>
public sealed class LeashedDefinitionQuery
{
  private readonly LeashedDefinitionCatalog _catalog;

  public LeashedDefinitionQuery(LeashedDefinitionCatalog catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public int Count => _catalog.Count;

  public bool TryGetById(int definitionId, out LeashedDefinitionDescriptor descriptor)
  {
    return _catalog.TryGet(definitionId, out descriptor!);
  }

  public bool TryGetByKey(string key, out LeashedDefinitionDescriptor descriptor)
  {
    return _catalog.TryGetByKey(key, out descriptor!);
  }

  public IReadOnlyList<LeashedDefinitionDescriptor> Snapshot()
  {
    return _catalog.Snapshot();
  }
}

