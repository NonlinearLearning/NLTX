using System;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Definitions;

namespace Terraria.Dome.Simulation.Player.Systems;

public sealed class PlayerNpcTargetingSystem
{
  private const ushort Item3090Type = 3090;

  public void AdvanceStealth(
    ref PlayerStealthStateComponent stealth,
    bool isUsingItem,
    float horizontalVelocity,
    float verticalVelocity,
    bool isMounted)
  {
    if (!float.IsFinite(horizontalVelocity) || !float.IsFinite(verticalVelocity))
    {
      throw new ArgumentOutOfRangeException(nameof(horizontalVelocity));
    }

    if (stealth.HasShroomiteStealth)
    {
      if (isUsingItem)
      {
        stealth.StealthTimer = 5;
      }

      if (MathF.Abs(horizontalVelocity) < 0.1f && MathF.Abs(verticalVelocity) < 0.1f &&
          !isMounted && stealth.StealthTimer == 0)
      {
        stealth.Stealth = MathF.Max(0.0f, stealth.Stealth - 0.015f);
      }
      else if (isMounted)
      {
        stealth.Stealth = 1.0f;
      }
      else
      {
        stealth.Stealth = MathF.Min(1.0f,
          stealth.Stealth + (MathF.Abs(horizontalVelocity) + MathF.Abs(verticalVelocity)) * 0.0075f);
      }
    }
    else if (stealth.IsVortexStealthActive)
    {
      stealth.Stealth = MathF.Max(0.0f, stealth.Stealth - 0.04f);
      if (isMounted)
      {
        stealth.IsVortexStealthActive = false;
      }
    }

    if (stealth.StealthTimer > 0)
    {
      stealth.StealthTimer--;
    }
  }

  public int CalculateAggro(
    int baseAggro,
    bool invisible,
    float stealth,
    bool shroomiteStealth,
    bool vortexStealthActive)
  {
    if (!float.IsFinite(stealth))
    {
      throw new ArgumentOutOfRangeException(nameof(stealth));
    }

    int aggro = baseAggro;
    if (invisible && aggro > -750)
    {
      aggro = -750;
    }

    float stealthReduction = 0.0f;
    if (shroomiteStealth)
    {
      stealthReduction = (1.0f - stealth) * 750.0f;
    }
    else if (vortexStealthActive)
    {
      stealthReduction = (1.0f - stealth) * 1200.0f;
    }

    if (float.IsFinite(stealthReduction))
    {
      float result = aggro - stealthReduction;
      if (result <= int.MinValue)
      {
        return int.MinValue;
      }

      if (result >= int.MaxValue)
      {
        return int.MaxValue;
      }

      return (int)result;
    }

    throw new ArgumentOutOfRangeException(nameof(stealth));
  }

  public void SetAggro(ref PlayerTargetingStateComponent targeting, int aggro)
  {
    targeting.Aggro = aggro;
  }

  public bool TryApplyMountSummon(
    ref PlayerMountStateComponent mount,
    ItemSummoningDefinition? summoning)
  {
    if (summoning is not ItemSummoningDefinition definition || definition.MountType < 0)
    {
      return false;
    }

    mount.Set(definition.MountType);
    return true;
  }

  public bool TryApplyMountControl(
    ref PlayerMountStateComponent mount,
    ushort? mountType)
  {
    mount.Set(mountType is ushort value ? value : -1);
    return true;
  }

  public bool TryApplyEarlyDismount(
    ref PlayerMountStateComponent mount,
    bool canDismount)
  {
    if (!mount.IsMounted || !canDismount ||
        !MountCapabilityRegistry.DismountsOnItemUse(mount.MountType))
    {
      return false;
    }

    mount.Set(-1);
    return true;
  }

  public bool TryConsumeFlightInput(
    ref PlayerMountStateComponent mount,
    bool upPressed)
  {
    return mount.TryConsumeFlightInput(upPressed);
  }

  public bool TryConsumeFlightInput(
    ref PlayerMountStateComponent mount,
    bool upPressed,
    int frameState)
  {
    return mount.TryConsumeFlightInput(upPressed, frameState);
  }

  public bool TryRecoverFlightResources(
    ref PlayerMountStateComponent mount,
    bool isGrounded,
    int amount)
  {
    if (!mount.IsMounted || !isGrounded || amount <= 0)
    {
      return false;
    }

    mount.RechargeFlightTime(amount);
    mount.RecoverFatigue(amount);
    return true;
  }

  public void RefreshAggro(
    ref PlayerTargetingStateComponent targeting,
    PlayerStealthStateComponent stealth,
    int baseAggro)
  {
    targeting.Aggro = CalculateAggro(
      baseAggro,
      stealth.IsInvisible,
      stealth.Stealth,
      stealth.HasShroomiteStealth,
      stealth.IsVortexStealthActive);
  }

  public void RefreshNoAggroCapabilities(
    ref PlayerTargetingStateComponent targeting,
    NpcNoAggroCapabilityRegistry registry,
    bool hasItem3090Effect)
  {
    ArgumentNullException.ThrowIfNull(registry);
    targeting.SetNoAggroNpcTypes(
      hasItem3090Effect
        ? registry.Item3090DefinitionIds
        : Array.Empty<int>());
  }

  public void RefreshFromEquipment(
    ref PlayerTargetingStateComponent targeting,
    EquipmentStateCollectionComponent equipmentStates,
    InventoryComponent inventory,
    NpcNoAggroCapabilityRegistry registry)
  {
    ArgumentNullException.ThrowIfNull(equipmentStates);
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(registry);
    bool hasItem3090Effect = false;
    foreach (ItemEquipmentStateComponent state in equipmentStates.States.Values)
    {
      if (state.IsVanity || state.SourceSlot < 0 ||
          state.SourceSlot >= InventoryComponent.SlotCount)
      {
        continue;
      }

      if (inventory.GetSlot(state.SourceSlot).ItemType == Item3090Type)
      {
        hasItem3090Effect = true;
        break;
      }
    }

    RefreshNoAggroCapabilities(ref targeting, registry, hasItem3090Effect);
  }
}
