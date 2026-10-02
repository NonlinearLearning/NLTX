namespace Terraria.ExternalPlatformBoundaries.Workshop;

public interface IWorkshopProviderPort
{
  WorkshopOperationResult<WorkshopEntrySnapshot> Lookup(WorkshopLookupRequest request);

  WorkshopOperationResult<WorkshopEntrySnapshot> Publish(WorkshopPublishRequest request);
}
