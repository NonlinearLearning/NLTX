namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class WeatherPresentationQuery
{
  public bool IsRaining(float cloudAlpha)
  {
    return cloudAlpha > 0f;
  }

  public bool IsRainingForever(long rainTime)
  {
    return rainTime >= 5_184_000L;
  }
}
