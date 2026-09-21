namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapInvalidationQueueSystem
{
  public bool Enqueue(
    MapInvalidationQueueComponent component,
    MapInvalidationCommand command)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.Enqueue(command);
  }

  public bool TryDequeue(
    MapInvalidationQueueComponent component,
    out MapInvalidationCommand command)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryDequeue(out command);
  }
}
