using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TileEntityTrainingDummyValidityQuery
{
  private const ushort TrainingDummyTileType = 378;
  private const short TrainingDummyFrameWidth = 36;

  public static bool IsValid(WorldTile tile)
  {
    return tile.IsActive &&
      tile.Type == TrainingDummyTileType &&
      tile.FrameY == 0 &&
      tile.FrameX % TrainingDummyFrameWidth == 0;
  }
}
