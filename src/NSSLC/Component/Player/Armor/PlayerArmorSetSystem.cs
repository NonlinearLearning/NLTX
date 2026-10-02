using System;

namespace Terraria.Player.Armor;

// status: isolated-core
// crossSubsystemOwner: Buff storage, Combat, random sampling, presentation, network, and persistence
public sealed class PlayerArmorSetSystem
{
  public void BeginSolarDash(PlayerSolarArmorStateComponent solarState)
  {
    ArgumentNullException.ThrowIfNull(solarState);

    solarState.IsSolarDashing = true;
    solarState.SolarDashConsumedFlare = false;
  }

  public PlayerNebulaBuffUpdateResult UpdateNebulaBuff(
    PlayerNebulaResourceStateComponent nebulaState,
    in PlayerNebulaBuffUpdateInput input)
  {
    ArgumentNullException.ThrowIfNull(nebulaState);

    int resourceLevel = 1 + input.BuffType - input.BaseBuffType;
    int buffType = input.BuffType;
    int buffTime = input.BuffTime;
    bool changedBuffSlot = false;
    if (buffTime == 2 && resourceLevel > 1)
    {
      resourceLevel--;
      buffType--;
      buffTime = 480;
      changedBuffSlot = true;
    }

    switch (input.Resource)
    {
      case PlayerNebulaResourceKind.Life:
        nebulaState.LifeLevel = resourceLevel;
        break;
      case PlayerNebulaResourceKind.Mana:
        nebulaState.ManaLevel = resourceLevel;
        break;
      case PlayerNebulaResourceKind.Damage:
        nebulaState.DamageLevel = resourceLevel;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(input), input.Resource, null);
    }

    return new PlayerNebulaBuffUpdateResult(
      buffType,
      buffTime,
      resourceLevel,
      changedBuffSlot);
  }
}
