namespace Terraria.ExternalPlatformBoundaries.ResourcePacks;

public sealed class ResourcePackMetadataSnapshot
{
  public ResourcePackMetadataSnapshot(
    string fullPath,
    string fileName,
    bool isCompressed,
    ResourcePackBranding branding,
    string name,
    string author,
    int version,
    bool hasIcon)
  {
    FullPath = fullPath;
    FileName = fileName;
    IsCompressed = isCompressed;
    Branding = branding;
    Name = name;
    Author = author;
    Version = version;
    HasIcon = hasIcon;
  }

  public string FullPath { get; }

  public string FileName { get; }

  public bool IsCompressed { get; }

  public ResourcePackBranding Branding { get; }

  public string Name { get; }

  public string Author { get; }

  public int Version { get; }

  public bool HasIcon { get; }
}
