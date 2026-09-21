namespace Terraria.ExternalPlatformBoundaries.Workshop;

public sealed class WorkshopOperationResult<T>
{
  private WorkshopOperationResult(WorkshopOperationStatus status, T? value)
  {
    Status = status;
    Value = value;
  }

  public WorkshopOperationStatus Status { get; }

  public T? Value { get; }

  public static WorkshopOperationResult<T> Failed(WorkshopOperationStatus status)
  {
    if (status is WorkshopOperationStatus.Succeeded or WorkshopOperationStatus.InvalidRequest)
    {
      throw new ArgumentOutOfRangeException(nameof(status));
    }

    return new WorkshopOperationResult<T>(status, default);
  }

  public static WorkshopOperationResult<T> Succeeded(T value)
  {
    return new WorkshopOperationResult<T>(WorkshopOperationStatus.Succeeded, value);
  }

  public static WorkshopOperationResult<T> InvalidRequest()
  {
    return new WorkshopOperationResult<T>(WorkshopOperationStatus.InvalidRequest, default);
  }
}
