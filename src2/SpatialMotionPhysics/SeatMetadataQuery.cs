namespace Terraria.SpatialMotionPhysics;

public static class SeatMetadataQuery
{
  public static SeatMetadataValue Create(bool isAToilet)
  {
    return new SeatMetadataValue(isAToilet);
  }
}
