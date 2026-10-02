using System;

namespace Terraria.Player.Luck;

public static class PlayerLuckSystem
{
  public static PlayerLuckFactorUpdateResult UpdateFactors(
    in PlayerLuckFactorUpdateInput input,
    PlayerLuckAndRescanStateComponent luckState)
  {
    ArgumentNullException.ThrowIfNull(luckState);

    int ladyBugLuckTimeLeft = input.LadyBugLuckTimeLeft;
    if (ladyBugLuckTimeLeft > 0)
    {
      ladyBugLuckTimeLeft -= input.DayRate;
      if (ladyBugLuckTimeLeft < 0)
      {
        ladyBugLuckTimeLeft = 0;
      }
    }
    else if (ladyBugLuckTimeLeft < 0)
    {
      ladyBugLuckTimeLeft += input.DayRate;
      if (ladyBugLuckTimeLeft > 0)
      {
        ladyBugLuckTimeLeft = 0;
      }
    }

    float coinLuck = input.CoinLuck;
    if (!(coinLuck <= 0f))
    {
      coinLuck *= (float)Math.Pow(0.9999, input.DayRate);
      if ((double)coinLuck < 0.25)
      {
        coinLuck = 0f;
      }
    }

    if (input.LadyBugLuckTimeLeft != 0)
    {
      luckState.LadyBugLuckTimeLeft = ladyBugLuckTimeLeft;
    }

    if (!(input.CoinLuck <= 0f))
    {
      luckState.CoinLuck = coinLuck;
    }

    return new PlayerLuckFactorUpdateResult(ladyBugLuckTimeLeft, coinLuck);
  }

  public static float Recalculate(
    in PlayerLuckCalculationInput input,
    PlayerLuckAndRescanStateComponent luckState)
  {
    ArgumentNullException.ThrowIfNull(luckState);

    float luck = PlayerLuckCalculationQuery.Calculate(input);
    luckState.Luck = luck;
    return luck;
  }
}
