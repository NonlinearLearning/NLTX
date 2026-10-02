namespace Terraria.ExternalPlatformBoundaries.Workshop;

public sealed class WorkshopPublishRequest
{
  public WorkshopPublishRequest(
    IEnumerable<WorkshopTagValue> tags,
    WorkshopPublicity publicity,
    string previewImagePath)
  {
    ArgumentNullException.ThrowIfNull(tags);
    ArgumentNullException.ThrowIfNull(previewImagePath);
    Tags = Array.AsReadOnly(tags.ToArray());
    Publicity = publicity;
    PreviewImagePath = previewImagePath;
  }

  public IReadOnlyList<WorkshopTagValue> Tags { get; }

  public WorkshopPublicity Publicity { get; }

  public string PreviewImagePath { get; }
}
