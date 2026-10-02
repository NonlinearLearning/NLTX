namespace Terraria.WorldSession.Queries;

public static class WeatherPresentationQuery
{
  public static bool IsHappyWindyDay(bool happyDay, float windSpeed)
  {
    return happyDay && windSpeed > 0f;
  }

  public static bool IsStorming(bool raining, float windSpeed, float stormThreshold)
  {
    if (stormThreshold < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(stormThreshold));
    }

    return raining && windSpeed >= stormThreshold;
  }
}
