namespace MinimalImplementation.Server.Player;

public readonly record struct PlayerLuckCalculationContext(
  bool LanternsUp,
  bool HasStinkyDebuff);

public static class PlayerLuckCalculator
{
  public const int LadyBugGoodLuckTime = 43_200;
  public const int LadyBugBadLuckTimeMagnitude = 10_800;

  public static float Calculate(
    PlayerLuck factors,
    bool usedGalaxyPearl,
    PlayerLuckCalculationContext context)
  {
    float ladyBugLuck = factors.LadyBugLuckTime switch
    {
      > 0 => (float)factors.LadyBugLuckTime / LadyBugGoodLuckTime,
      < 0 => (float)factors.LadyBugLuckTime / LadyBugBadLuckTimeMagnitude,
      _ => 0f
    };
    float luck = ladyBugLuck * 0.2f + factors.TorchLuck * 0.2f;
    luck += factors.LuckPotion * 0.1f;
    luck += factors.KiteLuckLevel * 0.1f / 3f;
    if (usedGalaxyPearl)
    {
      luck += 0.03f;
    }

    if (context.LanternsUp)
    {
      luck += 0.3f;
    }

    if (factors.HasGardenGnome)
    {
      luck += 0.2f;
    }

    if (context.HasStinkyDebuff)
    {
      luck -= 0.25f;
    }

    luck += factors.EquipmentLuckBonus;
    luck += CalculateCoinLuck(factors.CoinLuck);
    if (factors.BrokenMirrorBadLuck)
    {
      luck -= 0.25f;
    }

    return luck;
  }

  public static float CalculateCoinLuck(float coinLuck)
  {
    if (coinLuck == 0f)
    {
      return 0f;
    }

    if (coinLuck > 249_000f)
    {
      return 0.2f;
    }

    if (coinLuck > 24_900f)
    {
      return 0.175f;
    }

    if (coinLuck > 2_490f)
    {
      return 0.15f;
    }

    if (coinLuck > 249f)
    {
      return 0.125f;
    }

    if ((double)coinLuck > 24.9)
    {
      return 0.1f;
    }

    if ((double)coinLuck > 2.49)
    {
      return 0.075f;
    }

    if ((double)coinLuck > 0.249)
    {
      return 0.05f;
    }

    return 0.025f;
  }
}
