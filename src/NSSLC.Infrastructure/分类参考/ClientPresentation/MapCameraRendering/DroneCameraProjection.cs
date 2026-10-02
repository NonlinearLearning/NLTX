namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class DroneCameraProjection
{
  private uint _revision;

  public DroneCameraSnapshot Project(Guid? trackedProjectileId, int lastTrackedType)
  {
    if (lastTrackedType < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(lastTrackedType));
    }

    _revision++;
    return new DroneCameraSnapshot(
      trackedProjectileId,
      lastTrackedType,
      _revision);
  }
}
