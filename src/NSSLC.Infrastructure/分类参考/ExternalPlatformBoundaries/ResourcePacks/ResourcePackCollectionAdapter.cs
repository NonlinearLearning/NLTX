namespace Terraria.ExternalPlatformBoundaries.ResourcePacks;

public sealed class ResourcePackCollectionAdapter : IDisposable
{
  private readonly List<ResourcePackMetadataSnapshot> _resourcePacks = new();
  private bool _disposed;

  public ResourcePackDiscoveryResult Discover(IEnumerable<ResourcePackCandidate> candidates)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(ResourcePackCollectionAdapter));
    }

    _resourcePacks.Clear();
    var invalidPaths = new List<string>();
    var seenFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (ResourcePackCandidate candidate in candidates
      .OrderBy(candidate => System.IO.Path.GetFileName(candidate.Path), StringComparer.OrdinalIgnoreCase)
      .ThenBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase))
    {
      using var adapter = new ResourcePackBoundaryAdapter(candidate.Path, candidate.Branding);
      ResourcePackLoadResult result = adapter.Load();
      if (result.Status != ResourcePackLoadStatus.Loaded || result.Metadata is null)
      {
        invalidPaths.Add(candidate.Path);
        continue;
      }

      if (seenFileNames.Add(result.Metadata.FileName))
      {
        _resourcePacks.Add(result.Metadata);
      }
    }

    _resourcePacks.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(
      left.FileName,
      right.FileName));
    return new ResourcePackDiscoveryResult(
      Array.AsReadOnly(_resourcePacks.ToArray()),
      Array.AsReadOnly(invalidPaths.ToArray()));
  }

  public IReadOnlyList<ResourcePackMetadataSnapshot> Snapshot()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(ResourcePackCollectionAdapter));
    }

    return Array.AsReadOnly(_resourcePacks.ToArray());
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _resourcePacks.Clear();
    GC.SuppressFinalize(this);
  }
}
