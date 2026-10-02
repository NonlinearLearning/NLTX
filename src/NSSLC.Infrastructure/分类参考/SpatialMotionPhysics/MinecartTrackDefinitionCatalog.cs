namespace Terraria.SpatialMotionPhysics;

public sealed class MinecartTrackDefinitionCatalog
{
  private readonly MinecartTrackType[] _trackTypes;
  private readonly bool[] _boostLeft;

  private MinecartTrackDefinitionCatalog(
    MinecartTrackType[] trackTypes,
    bool[] boostLeft)
  {
    _trackTypes = trackTypes;
    _boostLeft = boostLeft;
  }

  public float BoosterSpeed => 4f;

  public static MinecartTrackDefinitionCatalog CreateDefault()
  {
    return new MinecartTrackDefinitionCatalog(
      new[]
      {
        MinecartTrackType.Normal,
        MinecartTrackType.Pressure,
        MinecartTrackType.Booster
      },
      new[] { false, false, true });
  }

  internal MinecartTrackSample Read(int frame)
  {
    if (frame < 0 || frame >= _trackTypes.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(frame));
    }

    return new MinecartTrackSample(
      _trackTypes[frame],
      _boostLeft[frame],
      frame);
  }
}
