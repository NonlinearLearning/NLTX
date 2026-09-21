using System;

namespace Terraria.Dome.Simulation.WorldModel;

public static class WorldCoordinateRules
{
  public static float LeftWorld(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return 0f;
  }

  public static float RightWorld(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return metadata.Width * 16f;
  }

  public static float TopWorld(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return 0f;
  }

  public static float BottomWorld(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return metadata.Height * 16f;
  }
}
