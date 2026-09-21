namespace NLTX.ClientPresentation.MapCameraRendering;

public static class MapClockQuery
{
  public static MapClockSnapshot Read(IMapClockSource source)
  {
    ArgumentNullException.ThrowIfNull(source);
    return new MapClockSnapshot(source.NowMilliseconds);
  }
}
