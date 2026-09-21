namespace Terraria.ExternalPlatformBoundaries.Workshop;

public sealed class WorkshopBoundaryAdapter
{
  private readonly IWorkshopProviderPort _provider;

  public WorkshopBoundaryAdapter(IWorkshopProviderPort provider)
  {
    _provider = provider ?? throw new ArgumentNullException(nameof(provider));
  }

  public WorkshopOperationResult<WorkshopEntrySnapshot> Lookup(WorkshopLookupRequest request)
  {
    if (request.ExternalWorkshopId == 0)
    {
      return WorkshopOperationResult<WorkshopEntrySnapshot>.InvalidRequest();
    }

    try
    {
      return _provider.Lookup(request);
    }
    catch
    {
      return WorkshopOperationResult<WorkshopEntrySnapshot>.Failed(
        WorkshopOperationStatus.Failed);
    }
  }

  public WorkshopOperationResult<WorkshopEntrySnapshot> Publish(WorkshopPublishRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (string.IsNullOrWhiteSpace(request.PreviewImagePath) ||
      request.Tags.Any(tag => string.IsNullOrWhiteSpace(tag.InternalNameForApis)))
    {
      return WorkshopOperationResult<WorkshopEntrySnapshot>.InvalidRequest();
    }

    var copy = new WorkshopPublishRequest(
      request.Tags.ToArray(),
      request.Publicity,
      request.PreviewImagePath);
    try
    {
      return _provider.Publish(copy);
    }
    catch
    {
      return WorkshopOperationResult<WorkshopEntrySnapshot>.Failed(
        WorkshopOperationStatus.Failed);
    }
  }
}
