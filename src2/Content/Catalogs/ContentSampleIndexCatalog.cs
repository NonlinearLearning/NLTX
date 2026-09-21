namespace Terraria.NonAuthoritative.ContentDefinitions;

public sealed record ContentSampleReference
{
  public ContentSampleReference(
    ContentDefinitionKind kind,
    ContentIdentity identity,
    string nameKey)
  {
    if (string.IsNullOrWhiteSpace(nameKey))
    {
      throw new ArgumentException("A content sample requires a stable name key.", nameof(nameKey));
    }

    Kind = kind;
    Identity = identity;
    NameKey = nameKey;
  }

  public ContentDefinitionKind Kind { get; }

  public ContentIdentity Identity { get; }

  public string NameKey { get; }
}

public readonly record struct CreativeItemOrder(int ItemType, int Group, int OrderInGroup);

public sealed class ContentSampleIndexCatalog
{
  private readonly Dictionary<(ContentDefinitionKind Kind, int LocalType), ContentSampleReference>
    _byLocalType = new();
  private readonly Dictionary<(ContentDefinitionKind Kind, int PersistentId), ContentSampleReference>
    _byPersistentId = new();
  private readonly Dictionary<(ContentDefinitionKind Kind, int NetworkId), ContentSampleReference>
    _byNetworkId = new();
  private List<ContentSampleReference> _samples = new();
  private List<CreativeItemOrder> _creativeItems = new();

  public bool IsInitialized { get; internal set; }

  public int Version { get; internal set; }

  public IReadOnlyList<ContentSampleReference> Samples => _samples;

  public IReadOnlyList<CreativeItemOrder> CreativeItems => _creativeItems;

  public bool TryGetByLocalType(
    ContentDefinitionKind kind,
    int localType,
    out ContentSampleReference sample)
  {
    return _byLocalType.TryGetValue((kind, localType), out sample!);
  }

  public bool TryGetByPersistentId(
    ContentDefinitionKind kind,
    int persistentId,
    out ContentSampleReference sample)
  {
    return _byPersistentId.TryGetValue((kind, persistentId), out sample!);
  }

  public bool TryGetByNetworkId(
    ContentDefinitionKind kind,
    int networkId,
    out ContentSampleReference sample)
  {
    return _byNetworkId.TryGetValue((kind, networkId), out sample!);
  }

  internal void Clear()
  {
    _byLocalType.Clear();
    _byPersistentId.Clear();
    _byNetworkId.Clear();
    _samples = new List<ContentSampleReference>();
    _creativeItems = new List<CreativeItemOrder>();
    IsInitialized = false;
  }

  internal void Replace(
    IReadOnlyList<ContentSampleReference> samples,
    IReadOnlyList<CreativeItemOrder> creativeItems)
  {
    Clear();
    foreach (ContentSampleReference sample in samples)
    {
      AddUnique(_byLocalType, (sample.Kind, sample.Identity.LocalType), sample);
      AddUnique(_byPersistentId, (sample.Kind, sample.Identity.PersistentId), sample);
      AddUnique(_byNetworkId, (sample.Kind, sample.Identity.NetworkId), sample);
      _samples.Add(sample);
    }

    _creativeItems = creativeItems
      .OrderBy(order => order.Group)
      .ThenBy(order => order.OrderInGroup)
      .ThenBy(order => order.ItemType)
      .ToList();
    IsInitialized = true;
    Version++;
  }

  private static void AddUnique<TKey>(
    IDictionary<TKey, ContentSampleReference> destination,
    TKey key,
    ContentSampleReference value)
    where TKey : notnull
  {
    if (!destination.TryAdd(key, value))
    {
      throw new InvalidOperationException("Content sample IDs must be unique within a content kind.");
    }
  }
}

public sealed class ContentSampleIndexSnapshot
{
  internal ContentSampleIndexSnapshot(ContentSampleIndexCatalog catalog)
  {
    Samples = catalog.Samples.ToArray();
    CreativeItems = catalog.CreativeItems.ToArray();
    Version = catalog.Version;
  }

  public int Version { get; }

  public IReadOnlyList<ContentSampleReference> Samples { get; }

  public IReadOnlyList<CreativeItemOrder> CreativeItems { get; }
}

public static class ContentSampleIndexBuildSystem
{
  public static void Rebuild(
    ContentSampleIndexCatalog catalog,
    IEnumerable<ContentSampleReference> samples,
    IEnumerable<CreativeItemOrder> creativeItems)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(samples);
    ArgumentNullException.ThrowIfNull(creativeItems);
    catalog.Replace(samples.ToArray(), creativeItems.ToArray());
  }

  public static void Clear(ContentSampleIndexCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.Clear();
  }
}

public static class ContentSampleIndexQuery
{
  public static ContentSampleIndexSnapshot Snapshot(ContentSampleIndexCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return new ContentSampleIndexSnapshot(catalog);
  }
}
