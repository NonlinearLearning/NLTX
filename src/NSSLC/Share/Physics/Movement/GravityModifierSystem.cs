using System;
using System.Collections.Generic;

namespace Terraria.Share.Movement;

/// <summary>
/// 管理重力修饰器生命周期，并提交当前帧的重力投影。
/// </summary>
public static class GravityModifierSystem
{
  public static GravityStateComponent Recalculate(
    in GravityConfigurationComponent configuration,
    in GravityDirectionStateComponent directionState,
    GravityModifierCollectionComponent collection)
  {
    ArgumentNullException.ThrowIfNull(collection);
    return GravityModifierCalculator.Resolve(configuration, directionState, collection.Modifiers);
  }

  public static int Advance(GravityModifierCollectionComponent collection)
  {
    ArgumentNullException.ThrowIfNull(collection);

    List<GravityModifier> activeModifiers = [];
    int removedCount = 0;
    foreach (GravityModifier modifier in collection.Modifiers)
    {
      GravityModifier advanced = modifier.AdvanceOneFrame();
      if (!advanced.IsPermanent && advanced.RemainingFrames <= 0)
      {
        removedCount++;
        continue;
      }

      activeModifiers.Add(advanced);
    }

    collection.Replace(activeModifiers);
    return removedCount;
  }

  public static GravityModifier AddModifier(
    GravityModifierCollectionComponent collection,
    GravityModifier modifier)
  {
    ArgumentNullException.ThrowIfNull(collection);
    return collection.Add(modifier);
  }

  public static int RemoveSource(GravityModifierCollectionComponent collection, long sourceId)
  {
    ArgumentNullException.ThrowIfNull(collection);
    return collection.RemoveBySource(sourceId);
  }

  public static bool TryToggleDirection(
    in GravityStateComponent currentState,
    ref GravityDirectionStateComponent directionState)
  {
    if (!currentState.CanToggleDirection || currentState.HasDirectionOverride)
    {
      return false;
    }

    directionState.CurrentDirection = directionState.CurrentDirection == GravityDirection.Normal
      ? GravityDirection.Reversed
      : GravityDirection.Normal;
    return true;
  }
}
