using System;

namespace Terraria.Dome.Simulation.Combat.Systems;

public static class PlayerLuckCalculationPolicy
{
  public const int LadyBugGoodLuckTime = 43200;
  public const int LadyBugBadLuckTime = -10800;
  public const int MiscCounterPeriod = 300;

  public static PlayerLuckInputs AdvanceFactors(PlayerLuckInputs inputs, int dayRate)
  {
    Validate(inputs, dayRate);

    int ladyBugLuckTimeLeft = inputs.LadyBugLuckTimeLeft;
    if (ladyBugLuckTimeLeft > 0)
    {
      ladyBugLuckTimeLeft -= dayRate;
      if (ladyBugLuckTimeLeft < 0)
      {
        ladyBugLuckTimeLeft = 0;
      }
    }
    else if (ladyBugLuckTimeLeft < 0)
    {
      ladyBugLuckTimeLeft += dayRate;
      if (ladyBugLuckTimeLeft > 0)
      {
        ladyBugLuckTimeLeft = 0;
      }
    }

    float coinLuck = inputs.CoinLuck;
    if (coinLuck > 0.0f)
    {
      coinLuck *= (float)Math.Pow(0.9999, dayRate);
      if (coinLuck < 0.25f)
      {
        coinLuck = 0.0f;
      }
    }

    return inputs with
    {
      LadyBugLuckTimeLeft = ladyBugLuckTimeLeft,
      CoinLuck = coinLuck
    };
  }

  public static float Calculate(PlayerLuckInputs inputs)
  {
    Validate(inputs, dayRate: 0);

    float luck = GetLadyBugLuck(inputs.LadyBugLuckTimeLeft) * 0.2f +
      inputs.TorchLuck * 0.2f;
    luck += inputs.LuckPotionLevel * 0.1f;
    luck += inputs.KiteLuckLevel * 0.1f / 3.0f;
    if (inputs.UsedGalaxyPearl)
    {
      luck += 0.03f;
    }

    if (inputs.LanternsUp)
    {
      luck += 0.3f;
    }

    if (inputs.HasGardenGnomeNearby)
    {
      luck += 0.2f;
    }

    if (inputs.Stinky)
    {
      luck -= 0.25f;
    }

    luck += inputs.EquipmentBasedLuckBonus;
    luck += CalculateCoinLuck(inputs.CoinLuck);
    if (inputs.BrokenMirrorBadLuck)
    {
      luck -= 0.25f;
    }

    return luck;
  }

  private static float GetLadyBugLuck(int ladyBugLuckTimeLeft)
  {
    if (ladyBugLuckTimeLeft > 0)
    {
      return (float)ladyBugLuckTimeLeft / LadyBugGoodLuckTime;
    }

    if (ladyBugLuckTimeLeft < 0)
    {
      return -ladyBugLuckTimeLeft / (float)LadyBugBadLuckTime;
    }

    return 0.0f;
  }

  private static float CalculateCoinLuck(float coinLuck)
  {
    if (coinLuck == 0.0f)
    {
      return 0.0f;
    }

    if (coinLuck > 249000.0f)
    {
      return 0.2f;
    }

    if (coinLuck > 24900.0f)
    {
      return 0.175f;
    }

    if (coinLuck > 2490.0f)
    {
      return 0.15f;
    }

    if (coinLuck > 249.0f)
    {
      return 0.125f;
    }

    if (coinLuck > 24.9f)
    {
      return 0.1f;
    }

    if (coinLuck > 2.49f)
    {
      return 0.075f;
    }

    if (coinLuck > 0.249f)
    {
      return 0.05f;
    }

    return 0.025f;
  }

  private static void Validate(PlayerLuckInputs inputs, int dayRate)
  {
    if (dayRate < 0 ||
        !float.IsFinite(inputs.TorchLuck) ||
        !float.IsFinite(inputs.EquipmentBasedLuckBonus) ||
        !float.IsFinite(inputs.CoinLuck) ||
        inputs.CoinLuck < 0.0f ||
        inputs.LuckPotionLevel < 0 ||
        inputs.KiteLuckLevel < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(inputs));
    }
  }
}
