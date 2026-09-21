using System;

namespace Terraria.WorldSession.Calendar;

public sealed class WeatherPresentationStateComponent
{
  public WeatherPresentationStateComponent(float cloudAlpha = 0.0f)
  {
    CloudAlpha = cloudAlpha;
    Validate();
  }

  public float CloudAlpha { get; internal set; }

  public void Validate()
  {
    if (!float.IsFinite(CloudAlpha) || CloudAlpha < 0.0f || CloudAlpha > 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(CloudAlpha));
    }
  }
}
