using System.Numerics;

namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class MainDerivedPresentationQuery
{
  public float WindForVisuals(float currentWindSpeed)
  {
    if (float.IsNaN(currentWindSpeed) || float.IsInfinity(currentWindSpeed))
    {
      throw new ArgumentOutOfRangeException(nameof(currentWindSpeed));
    }

    return currentWindSpeed;
  }

  public int ChatLineWidthLimit(int screenWidth, float uiScale)
  {
    if (screenWidth < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(screenWidth));
    }

    if (float.IsNaN(uiScale) || float.IsInfinity(uiScale) || uiScale <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(uiScale));
    }

    return (int)(screenWidth * (1f / uiScale)) - 320;
  }

  public float BlackFadeDistance(Vector2 unscaledCameraSize)
  {
    return unscaledCameraSize.Length() / 2f + 100f;
  }
}
