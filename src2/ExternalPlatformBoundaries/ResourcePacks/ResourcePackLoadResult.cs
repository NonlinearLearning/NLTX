namespace Terraria.ExternalPlatformBoundaries.ResourcePacks;

public sealed class ResourcePackLoadResult
{
  private ResourcePackLoadResult(
    ResourcePackLoadStatus status,
    ResourcePackMetadataSnapshot? metadata,
    string? error)
  {
    Status = status;
    Metadata = metadata;
    Error = error;
  }

  public ResourcePackLoadStatus Status { get; }

  public ResourcePackMetadataSnapshot? Metadata { get; }

  public string? Error { get; }

  public static ResourcePackLoadResult Loaded(ResourcePackMetadataSnapshot metadata)
  {
    return new ResourcePackLoadResult(ResourcePackLoadStatus.Loaded, metadata, null);
  }

  public static ResourcePackLoadResult Failure(ResourcePackLoadStatus status, string error)
  {
    return new ResourcePackLoadResult(status, null, error);
  }
}
