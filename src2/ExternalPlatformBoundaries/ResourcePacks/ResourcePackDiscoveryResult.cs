namespace Terraria.ExternalPlatformBoundaries.ResourcePacks;

public sealed class ResourcePackDiscoveryResult
{
  public ResourcePackDiscoveryResult(
    IReadOnlyList<ResourcePackMetadataSnapshot> packs,
    IReadOnlyList<string> invalidPaths)
  {
    Packs = packs;
    InvalidPaths = invalidPaths;
  }

  public IReadOnlyList<ResourcePackMetadataSnapshot> Packs { get; }

  public IReadOnlyList<string> InvalidPaths { get; }
}
