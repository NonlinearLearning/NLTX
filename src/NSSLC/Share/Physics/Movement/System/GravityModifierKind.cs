namespace Terraria.Movement;

/// <summary>
/// 表示重力修饰器作用的通道。
/// </summary>
public enum GravityModifierKind : byte
{
  MagnitudeAdditive,
  MagnitudeMultiplier,
  MagnitudeOverride,
  MaxFallSpeedAdditive,
  MaxFallSpeedMultiplier,
  MaxFallSpeedOverride,
  DirectionOverride,
  ToggleDirectionCapability
}
