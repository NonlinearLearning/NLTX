namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingRoomQualityPolicy
{
  public static HousingRoomQualityDecision Evaluate(
    int score,
    bool roomOccupied,
    bool roomEvil,
    bool roomHasStandingSpace)
  {
    if (score > 0)
    {
      return new HousingRoomQualityDecision(score, true, HousingRoomQualityFailureReason.None);
    }

    HousingRoomQualityFailureReason failureReason = HousingRoomQualityFailureReason.NoValidRoom;
    if (roomOccupied)
    {
      failureReason = HousingRoomQualityFailureReason.Occupied;
    }
    else if (roomEvil)
    {
      failureReason = HousingRoomQualityFailureReason.Evil;
    }
    else if (!roomHasStandingSpace)
    {
      failureReason = HousingRoomQualityFailureReason.NoStandingSpace;
    }

    return new HousingRoomQualityDecision(score, false, failureReason);
  }
}
