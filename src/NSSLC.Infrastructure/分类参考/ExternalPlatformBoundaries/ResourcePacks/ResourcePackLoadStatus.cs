namespace Terraria.ExternalPlatformBoundaries.ResourcePacks;

public enum ResourcePackLoadStatus
{
  Loaded,
  MissingPath,
  MissingManifest,
  InvalidManifest,
  AlreadyDisposed,
  Failed
}
