using System.Numerics;

namespace Terraria.Input.LockOn;

public sealed class LockOnCursorProjectionAdapter
{
  private Vector2 _position;

  public bool IsActive { get; private set; }

  public Vector2 Position => IsActive ? _position : Vector2.Zero;

  public bool SetDown()
  {
    if (!IsActive)
    {
      return false;
    }

    IsActive = false;
    _position = Vector2.Zero;
    return true;
  }

  public bool SetUp(Vector2 position)
  {
    if (IsActive)
    {
      return false;
    }

    IsActive = true;
    _position = position;
    return true;
  }
}
