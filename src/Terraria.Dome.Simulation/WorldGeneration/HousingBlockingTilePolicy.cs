namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingBlockingTilePolicy
{
  public static HousingBlockingTileDecision Evaluate(
    ushort tileType,
    bool isSolid,
    short frameX,
    short frameY)
  {
    if (isSolid)
    {
      return new HousingBlockingTileDecision(HousingBlockingTileReason.BlockingWall, true);
    }

    bool isOpenGate = tileType == 389 ||
      (tileType == 11 &&
       (frameX == 0 || frameX == 54 || frameX == 72 || frameX == 126)) ||
      tileType == 386 && ((frameX < 36 && frameY == 18) ||
                          (frameX >= 36 && frameY == 0));
    return isOpenGate
      ? new HousingBlockingTileDecision(HousingBlockingTileReason.BlockingOpenGate, true)
      : new HousingBlockingTileDecision(HousingBlockingTileReason.None, false);
  }
}
