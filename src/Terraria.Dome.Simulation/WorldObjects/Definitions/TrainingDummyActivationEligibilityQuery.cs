namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TrainingDummyActivationEligibilityQuery
{
  private const int TilePixelSize = 16;
  private const int DummyWidthPixels = 32;
  private const int DummyHeightPixels = 48;
  private const int ActivationInflationPixels = 1600;

  public static bool IsPlayerInRange(
    TrainingDummyTileEntityState entity,
    TrainingDummyPlayerHitboxSnapshot player)
  {
    if (!player.IsActive)
    {
      return false;
    }

    int dummyX = entity.TileX * TilePixelSize;
    int dummyY = entity.TileY * TilePixelSize;
    int left = dummyX - ActivationInflationPixels;
    int top = dummyY - ActivationInflationPixels;
    int right = dummyX + DummyWidthPixels + ActivationInflationPixels;
    int bottom = dummyY + DummyHeightPixels + ActivationInflationPixels;
    return player.X < right &&
      player.X + player.Width > left &&
      player.Y < bottom &&
      player.Y + player.Height > top;
  }
}
