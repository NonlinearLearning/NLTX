using System.Numerics;
using NLTX.PlayerInputGameplay.Runtime;

namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class CursorCoordinateQuery
{
  public Vector2 MouseScreen(MainInputFrameComponent input)
  {
    ArgumentNullException.ThrowIfNull(input);
    return new Vector2(input.MouseX, input.MouseY);
  }

  public Vector2 MouseWorld(
    MainInputFrameComponent input,
    ScreenViewportComponent viewport,
    float gravityDirection)
  {
    ArgumentNullException.ThrowIfNull(input);
    ArgumentNullException.ThrowIfNull(viewport);
    var result = MouseScreen(input) + viewport.ScreenPosition;
    if (gravityDirection == -1f)
    {
      result.Y = viewport.ScreenPosition.Y + viewport.ScreenHeight - input.MouseY;
    }

    return result;
  }
}
