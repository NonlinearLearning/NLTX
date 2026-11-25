using System;

namespace Terraria.Share.Movement;

/// <summary>
/// 保存实体的基础重力配置。它不是当前帧的派生结果。
/// </summary>
public struct GravityConfigurationComponent
{
  private readonly float _baseGravityMagnitude;
  private readonly float _baseMaxFallSpeed;
  private readonly GravityDirection _defaultDirection;
  private readonly bool _initialized;

  public GravityConfigurationComponent()
    : this(0.4f, 10f, GravityDirection.Normal)
  {
  }

  public GravityConfigurationComponent(
    float baseGravityMagnitude = 0.4f,
    float baseMaxFallSpeed = 10f,
    GravityDirection defaultDirection = GravityDirection.Normal)
  {
    if (!float.IsFinite(baseGravityMagnitude) || baseGravityMagnitude < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(baseGravityMagnitude));
    }

    if (!float.IsFinite(baseMaxFallSpeed) || baseMaxFallSpeed < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(baseMaxFallSpeed));
    }

    _baseGravityMagnitude = baseGravityMagnitude;
    _baseMaxFallSpeed = baseMaxFallSpeed;
    if (!IsDefined(defaultDirection))
    {
      throw new ArgumentOutOfRangeException(nameof(defaultDirection));
    }

    _defaultDirection = defaultDirection;
    _initialized = true;
  }

  public float BaseGravityMagnitude => _initialized ? _baseGravityMagnitude : 0.4f;

  public float BaseMaxFallSpeed => _initialized ? _baseMaxFallSpeed : 10f;

  public GravityDirection DefaultDirection => _initialized ? _defaultDirection : GravityDirection.Normal;

  private static bool IsDefined(GravityDirection direction)
  {
    return direction is GravityDirection.Normal or GravityDirection.Reversed;
  }
}
