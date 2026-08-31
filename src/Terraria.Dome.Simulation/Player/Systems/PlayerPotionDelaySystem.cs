using System;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.StatusEffects.Components;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerPotionDelaySystem
{
  public bool CanApplyPotionUse(
    PlayerPotionStateComponent state,
    BuffCollectionComponent buffs,
    PlayerHandle source,
    int reservedBuffSlots = 0)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    if (!source.IsValid || state.PotionDelayTicks < 0 || state.PotionDelayTicks > 0 ||
        reservedBuffSlots < 0)
    {
      return false;
    }

    for (int index = 0; index < buffs.Count; index++)
    {
      if (buffs.Entries[index].Type == PlayerPotionStateComponent.PotionSicknessBuffType)
      {
        return false;
      }
    }

    return buffs.Count <= buffs.MaximumCount - reservedBuffSlots - 1;
  }

  public bool IsBlocked(
    ItemDefinition definition,
    int potionDelayTicks)
  {
    if (potionDelayTicks < 0)
    {
      return true;
    }

    return definition.Use?.Potion == true && potionDelayTicks > 0;
  }

  public bool TryApplyPotionUse(
    ref PlayerPotionStateComponent state,
    BuffCollectionComponent buffs,
    PlayerHandle source,
    int reservedBuffSlots = 0)
  {
    if (!CanApplyPotionUse(state, buffs, source, reservedBuffSlots))
    {
      return false;
    }

    if (!buffs.TryAdd(
          PlayerPotionStateComponent.PotionSicknessBuffType,
          ItemDefinition.PotionDelayTicks,
          source))
    {
      return false;
    }

    state.Apply(ItemDefinition.PotionDelayTicks);
    return true;
  }

  public void ApplyPotionUse(
    ref PlayerPotionStateComponent state,
    BuffCollectionComponent buffs,
    PlayerHandle source,
    int reservedBuffSlots = 0)
  {
    if (!CanApplyPotionUse(state, buffs, source, reservedBuffSlots))
    {
      throw new InvalidOperationException("Potion delay cannot be applied to the player state.");
    }

    if (!TryApplyPotionUse(ref state, buffs, source, reservedBuffSlots))
    {
      throw new InvalidOperationException("Potion delay cannot be applied to the player state.");
    }
  }

  public void SynchronizeFromBuffs(
    ref PlayerPotionStateComponent state,
    BuffCollectionComponent buffs)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    if (state.PotionDelayTicks < 0)
    {
      state.PotionDelayTicks = 0;
    }

    int longestRemainingTicks = 0;
    for (int index = 0; index < buffs.Count; index++)
    {
      BuffEntry entry = buffs.Entries[index];
      if (entry.Type != PlayerPotionStateComponent.PotionSicknessBuffType)
      {
        continue;
      }

      longestRemainingTicks = Math.Max(longestRemainingTicks, entry.RemainingTicks);
    }

    state.PotionDelayTicks = longestRemainingTicks;
  }

  public void Clear(
    ref PlayerPotionStateComponent state,
    BuffCollectionComponent buffs)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    state.PotionDelayTicks = 0;
    for (int index = buffs.Count - 1; index >= 0; index--)
    {
      if (buffs.Entries[index].Type == PlayerPotionStateComponent.PotionSicknessBuffType)
      {
        buffs.RemoveAt(index);
      }
    }
  }
}
