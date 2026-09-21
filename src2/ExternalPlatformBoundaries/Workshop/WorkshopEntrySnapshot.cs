namespace Terraria.ExternalPlatformBoundaries.Workshop;

public sealed class WorkshopEntrySnapshot
{
  public WorkshopEntrySnapshot(
    ulong externalWorkshopId,
    WorkshopPublicity publicity,
    IEnumerable<string> tags,
    string previewImagePath,
    int publishedVersion)
  {
    ArgumentNullException.ThrowIfNull(tags);
    ArgumentNullException.ThrowIfNull(previewImagePath);
    ExternalWorkshopId = externalWorkshopId;
    Publicity = publicity;
    Tags = Array.AsReadOnly(tags.ToArray());
    PreviewImagePath = previewImagePath;
    PublishedVersion = publishedVersion;
  }

  public ulong ExternalWorkshopId { get; }

  public WorkshopPublicity Publicity { get; }

  public IReadOnlyList<string> Tags { get; }

  public string PreviewImagePath { get; }

  public int PublishedVersion { get; }
}
