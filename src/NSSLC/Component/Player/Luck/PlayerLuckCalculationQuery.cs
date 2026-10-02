namespace Terraria.Player.Luck;

public static class PlayerLuckCalculationQuery
{
  public static float Calculate(in PlayerLuckCalculationInput input)
  {
    float ladyBugLuck = 0f;
    if (input.LadyBugLuckTimeLeft > 0)
    {
      ladyBugLuck =
        (float)input.LadyBugLuckTimeLeft / input.LadyBugGoodLuckTime;
    }
    else if (input.LadyBugLuckTimeLeft < 0)
    {
      ladyBugLuck =
        (0f - (float)input.LadyBugLuckTimeLeft) / input.LadyBugBadLuckTime;
    }

    float luck = ladyBugLuck * 0.2f + input.TorchLuck * 0.2f;
    luck += (float)(int)input.LuckPotion * 0.1f;
    luck += (float)(int)input.KiteLuckLevel * 0.1f / 3f;
    if (input.UsedGalaxyPearl)
    {
      luck += 0.03f;
    }

    if (input.LanternsUp)
    {
      luck += 0.3f;
    }

    if (input.HasGardenGnomeNearby)
    {
      luck += 0.2f;
    }

    if (input.Stinky)
    {
      luck -= 0.25f;
    }

    luck += input.EquipmentBasedLuckBonus;
    luck += CalculateCoinLuck(input.CoinLuck);
    if (input.BrokenMirrorBadLuck)
    {
      luck -= 0.25f;
    }

    return luck;
  }

  private static float CalculateCoinLuck(float coinLuck)
  {
    if (coinLuck == 0f)
    {
      return 0f;
    }

    if (coinLuck > 249000f)
    {
      return 0.2f;
    }

    if (coinLuck > 24900f)
    {
      return 0.175f;
    }

    if (coinLuck > 2490f)
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
