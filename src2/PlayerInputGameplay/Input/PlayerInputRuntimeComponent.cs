using System.Numerics;

namespace NLTX.PlayerInputGameplay.Input;

public sealed class PlayerInputRuntimeComponent
{
  public bool LockGamepadTileUseButton { get; private set; }

  public int OriginalScreenWidth { get; private set; }

  public int OriginalScreenHeight { get; private set; }

  public Vector2 OriginalScreenSize => new(OriginalScreenWidth, OriginalScreenHeight);

  public void SetOriginalScreenSize(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(width <= 0 ? nameof(width) : nameof(height));
    }

    OriginalScreenWidth = width;
    OriginalScreenHeight = height;
  }

  public void SetLockGamepadTileUseButton(bool locked)
  {
    LockGamepadTileUseButton = locked;
  }
}
