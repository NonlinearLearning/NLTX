using System.Numerics;

namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class ScreenViewportComponent
{
  public Vector2 ScreenPosition { get; private set; }

  public int ScreenWidth { get; private set; } = 1152;

  public int ScreenHeight { get; private set; } = 864;

  public void Set(Vector2 screenPosition, int screenWidth, int screenHeight)
  {
    if (screenWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(screenWidth));
    }

    if (screenHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(screenHeight));
    }

    ScreenPosition = screenPosition;
    ScreenWidth = screenWidth;
    ScreenHeight = screenHeight;
  }
}
