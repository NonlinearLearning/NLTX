namespace Terraria.SpatialMotionPhysics;

public static class SittingProjectionAdapter
{
  public interface ISeatMetadataSink
  {
    void Publish(SeatMetadataValue value);
  }

  public static void Project(
    SeatMetadataValue value,
    ISeatMetadataSink sink)
  {
    ArgumentNullException.ThrowIfNull(sink);
    sink.Publish(value);
  }
}
