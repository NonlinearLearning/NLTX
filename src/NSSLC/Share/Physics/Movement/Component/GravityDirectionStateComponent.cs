using System;

namespace Terraria.Share.Movement;

/// <summary>
/// 保存实体在没有方向覆盖时选择的重力方向。
/// </summary>
public struct GravityDirectionStateComponent
{
  private GravityDirection _currentDirection;
  private bool _initialized;

  public GravityDirectionStateComponent()
    : this(GravityDirection.Normal)
  {
  }

  public GravityDirectionStateComponent(GravityDirection currentDirection)
  {
    if (!IsDefined(currentDirection))
    {
      throw new ArgumentOutOfRangeException(nameof(currentDirection));
    }

    _currentDirection = currentDirection;
    _initialized = true;
  }

  public GravityDirection CurrentDirection
  {
    get => _initialized ? _currentDirection : GravityDirection.Normal;
    set
    {
      if (!IsDefined(value))
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      _currentDirection = value;
      _initialized = true;
    }
  }

  private static bool IsDefined(GravityDirection direction)
  {
    return direction is GravityDirection.Normal or GravityDirection.Reversed;
  }
}
