namespace Terraria.WorldSession.Components;

public sealed class WorldWeatherState
{
  private const int EndlessRainThreshold = 5_184_000;

  public bool IsRaining;
  public int RainTime;
  public float MaximumRainStrength;
  public float WindSpeedTarget;
  public float WindSpeedCurrent;
  public int WeatherCounter;
  public int WindCounter;
  public int ExtremeWindCounter;
  public float PreviousMaximumRainStrength;

  public bool IsRainingForever => RainTime >= EndlessRainThreshold;
}
