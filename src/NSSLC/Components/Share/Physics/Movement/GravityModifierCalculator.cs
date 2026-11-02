using System;
using System.Collections.Generic;

namespace Terraria.Movement;

/// <summary>
/// 负责无副作用地计算当前帧的重力状态。
/// </summary>
public static class GravityModifierCalculator
{
  public static GravityStateComponent Resolve(
    in GravityConfigurationComponent configuration,
    in GravityDirectionStateComponent directionState,
    IReadOnlyList<GravityModifier> modifiers)
  {
    ArgumentNullException.ThrowIfNull(modifiers);

    float magnitudeAdditive = 0f;
    float magnitudeMultiplier = 1f;
    float maxFallSpeedAdditive = 0f;
    float maxFallSpeedMultiplier = 1f;
    bool canToggleDirection = false;
    GravityModifier? magnitudeOverride = null;
    GravityModifier? maxFallSpeedOverride = null;
    GravityModifier? directionOverride = null;
    long iterationSequence = 0;

    foreach (GravityModifier modifier in modifiers)
    {
            modifier.Validate();
      iterationSequence++;
      GravityModifier orderedModifier = modifier.Sequence == 0
        ? modifier with { Sequence = iterationSequence }
        : modifier;
      switch (orderedModifier.Kind)
      {
        case GravityModifierKind.MagnitudeAdditive:
          magnitudeAdditive += orderedModifier.Value;
          break;
        case GravityModifierKind.MagnitudeMultiplier:
          magnitudeMultiplier *= orderedModifier.Value;
          break;
        case GravityModifierKind.MagnitudeOverride:
          magnitudeOverride = SelectOverride(magnitudeOverride, orderedModifier);
          break;
        case GravityModifierKind.MaxFallSpeedAdditive:
          maxFallSpeedAdditive += orderedModifier.Value;
          break;
        case GravityModifierKind.MaxFallSpeedMultiplier:
          maxFallSpeedMultiplier *= orderedModifier.Value;
          break;
        case GravityModifierKind.MaxFallSpeedOverride:
          maxFallSpeedOverride = SelectOverride(maxFallSpeedOverride, orderedModifier);
          break;
        case GravityModifierKind.DirectionOverride:
          directionOverride = SelectOverride(directionOverride, orderedModifier);
          break;
        case GravityModifierKind.ToggleDirectionCapability:
          canToggleDirection |= orderedModifier.Value > 0f;
          break;
        default:
          throw new ArgumentOutOfRangeException(
            nameof(orderedModifier.Kind),
            orderedModifier.Kind,
            "Unknown gravity modifier kind.");
      }
    }

    float gravityMagnitude = magnitudeOverride.HasValue
      ? ResolveNonNegativeOverride(magnitudeOverride.Value)
      : MathF.Max(0f, (configuration.BaseGravityMagnitude + magnitudeAdditive) * magnitudeMultiplier);
    float maxFallSpeed = maxFallSpeedOverride.HasValue
      ? ResolveNonNegativeOverride(maxFallSpeedOverride.Value)
      : MathF.Max(0f, (configuration.BaseMaxFallSpeed + maxFallSpeedAdditive) * maxFallSpeedMultiplier);

    GravityDirection direction = directionOverride?.Direction
      ?? ResolveDirection(directionState.CurrentDirection, configuration.DefaultDirection);
    return new GravityStateComponent(
      gravityMagnitude,
      maxFallSpeed,
      direction,
      canToggleDirection,
      directionOverride.HasValue);
  }

  private static GravityModifier SelectOverride(GravityModifier? current, GravityModifier candidate)
  {
    if (!current.HasValue ||
      candidate.Priority > current.Value.Priority ||
      (candidate.Priority == current.Value.Priority && candidate.Sequence > current.Value.Sequence))
    {
      return candidate;
    }

    return current.Value;
  }

  private static float ResolveNonNegativeOverride(GravityModifier modifier)
  {
    return MathF.Max(0f, modifier.Value);
  }

  private static GravityDirection ResolveDirection(
    GravityDirection direction,
    GravityDirection fallback)
  {
    if (direction is GravityDirection.Normal or GravityDirection.Reversed)
    {
      return direction;
    }

    return fallback is GravityDirection.Normal or GravityDirection.Reversed
      ? fallback
      : GravityDirection.Normal;
  }
}
