using System;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Systems;

public readonly record struct ExtractinatorResult(
  bool IsAccepted,
  ItemStack Output,
  bool IsTrade = false);

public sealed class ExtractinatorSystem
{
  public const ushort ChlorophyteExtractinatorTileType = 642;
  public const ushort ExtractinatorTileType = 219;

  public ExtractinatorResult Roll(
    ExtractinatorRuleRegistry rules,
    int extractionMode,
    ushort extractinatorTileType,
    ushort sourceItemType,
    bool isHardMode,
    ExtractinatorRandom random)
  {
    ArgumentNullException.ThrowIfNull(rules);
    ArgumentNullException.ThrowIfNull(random);
    if (extractinatorTileType != ExtractinatorTileType &&
        extractinatorTileType != ChlorophyteExtractinatorTileType)
    {
      return default;
    }

    if (extractinatorTileType == ChlorophyteExtractinatorTileType &&
        rules.TryGetChlorophyteTrade(sourceItemType, out ushort tradeResult))
    {
      return new ExtractinatorResult(true, new ItemStack(tradeResult, 1), true);
    }

    if (!ExtractinatorRuleRegistry.IsSupportedMode(extractionMode) || extractionMode < 0)
    {
      return default;
    }

    return new ExtractinatorResult(
      true,
      RollExtractinatorDrop(extractionMode, extractinatorTileType, isHardMode, random));
  }

  private static ItemStack RollExtractinatorDrop(
    int extractionMode,
    ushort extractinatorTileType,
    bool isHardMode,
    ExtractinatorRandom random)
  {
    int amberChance = 5000;
    int gemChance = 25;
    int extractinatorItemChance = 50;
    int coinChance = -1;
    int fossilMode = -1;
    int chlorophyteMode = -1;
    int coinMode = 1;
    int specialBlockMode = -1;
    int basicBlockMode = -1;
    int desertTorchMode = -1;
    int sandstormBottleMode = -1;
    switch (extractionMode)
    {
      case 1:
        amberChance /= 3;
        gemChance *= 2;
        extractinatorItemChance = 20;
        coinChance = 10;
        break;
      case 2:
        amberChance = -1;
        gemChance = -1;
        extractinatorItemChance = -1;
        coinChance = -1;
        fossilMode = 1;
        coinMode = -1;
        break;
      case 3:
        amberChance = -1;
        gemChance = -1;
        extractinatorItemChance = -1;
        coinChance = -1;
        fossilMode = -1;
        coinMode = -1;
        chlorophyteMode = 1;
        break;
      case 4:
        amberChance = -1;
        gemChance = -1;
        extractinatorItemChance = -1;
        coinMode = -1;
        basicBlockMode = 1;
        specialBlockMode = 50;
        break;
      case 5:
        amberChance = -1;
        gemChance = -1;
        extractinatorItemChance = -1;
        coinMode = -1;
        sandstormBottleMode = 1;
        break;
      case 6:
        amberChance = -1;
        gemChance = -1;
        extractinatorItemChance = -1;
        coinMode = -1;
        desertTorchMode = 1;
        break;
    }

    if (coinChance != -1 && random.Next(coinChance) == 0)
    {
      return new ItemStack(3380, RollCoinStack(random));
    }

    if (coinMode != -1 && random.Next(2) == 0)
    {
      return RollCoinOutput(random, 12000, 800, 60, 3, 6, 4);
    }

    if (amberChance != -1 && random.Next(amberChance) == 0)
    {
      return new ItemStack(1242, 1);
    }

    if (fossilMode != -1)
    {
      return new ItemStack((ushort)RollFossil(random), 1);
    }

    if (chlorophyteMode != -1)
    {
      return new ItemStack((ushort)RollChlorophyte(random, extractinatorTileType), 1);
    }

    if (specialBlockMode != -1 && random.Next(specialBlockMode) == 0)
    {
      return new ItemStack((ushort)RollSpecialBlock(random), 1);
    }

    if (basicBlockMode > 0)
    {
      return new ItemStack(2, 1);
    }

    if (sandstormBottleMode > 0)
    {
      return new ItemStack(1125, 1);
    }

    if (desertTorchMode > 0)
    {
      return new ItemStack(169, 1);
    }

    if (gemChance != -1 && random.Next(gemChance) == 0)
    {
      return new ItemStack((ushort)RollGem(random), RollOreStack(random));
    }

    if (extractinatorItemChance != -1 && random.Next(extractinatorItemChance) == 0)
    {
      return new ItemStack(999, RollOreStack(random));
    }

    if (random.Next(3) == 0)
    {
      return RollCoinOutput(random, 5000, 400, 30, 2, 5, 3);
    }

    ushort ore = extractinatorTileType == ChlorophyteExtractinatorTileType && isHardMode
      ? RollOreHardmode(random)
      : RollOreEarlymode(random);
    return new ItemStack(ore, RollOreStack(random));
  }

  private static ItemStack RollCoinOutput(
    ExtractinatorRandom random,
    int platinumChance,
    int goldChance,
    int silverChance,
    int copperMultiplierChance,
    int goldMultiplierChance,
    int silverMultiplierChance)
  {
    if (random.Next(platinumChance) == 0)
    {
      return new ItemStack(74, RollRepeatStack(random, 14, 0, 2, 3));
    }

    if (random.Next(goldChance) == 0)
    {
      return new ItemStack(73, RollCoinStack(random, 6, 1, 21, 20));
    }

    if (random.Next(silverChance) == 0)
    {
      return new ItemStack(72, RollCoinStack(random, silverMultiplierChance, 5, 26, 25));
    }

    return new ItemStack(71, RollCoinStack(random, copperMultiplierChance, 10, 26, 25));
  }

  private static int RollChlorophyte(ExtractinatorRandom random, ushort extractinatorTileType)
  {
    if (extractinatorTileType == ChlorophyteExtractinatorTileType && random.Next(10) == 1)
    {
      return random.Next(5) switch
      {
        0 => 4354,
        1 => 4389,
        2 => 4377,
        3 => 5127,
        _ => 4378
      };
    }

    return random.Next(5) switch
    {
      0 => 4349,
      1 => 4350,
      2 => 4351,
      3 => 4352,
      _ => 4353
    };
  }

  private static int RollCoinStack(ExtractinatorRandom random)
  {
    int stack = 1;
    stack += random.Next(5) == 0 ? random.Next(2) : 0;
    stack += random.Next(10) == 0 ? random.Next(3) : 0;
    stack += random.Next(15) == 0 ? random.Next(4) : 0;
    return stack;
  }

  private static int RollCoinStack(
    ExtractinatorRandom random,
    int chance,
    int minimum,
    int maximum,
    int lastMaximum)
  {
    int stack = 1;
    for (int index = 0; index < 4; index++)
    {
      if (random.Next(chance) == 0)
      {
        stack += random.Next(minimum, maximum);
      }
    }

    if (random.Next(chance) == 0)
    {
      stack += random.Next(minimum, lastMaximum);
    }

    return stack;
  }

  private static int RollFossil(ExtractinatorRandom random)
  {
    if (random.Next(4) != 1)
    {
      return 2674;
    }

    if (random.Next(3) != 1)
    {
      return 2006;
    }

    return random.Next(3) != 1 ? 2002 : 2675;
  }

  private static int RollGem(ExtractinatorRandom random)
  {
    return random.Next(6) switch
    {
      0 => 181,
      1 => 180,
      2 => 177,
      3 => 179,
      4 => 178,
      _ => 182
    };
  }

  private static int RollOreStack(ExtractinatorRandom random)
  {
    return RollRepeatStack(random, 20, 0, 2, 30, 0, 3, 40, 0, 4, 50, 0, 5, 60, 0, 6);
  }

  private static int RollRepeatStack(
    ExtractinatorRandom random,
    int chance,
    int minimum,
    int maximum,
    int repetitions)
  {
    int stack = 1;
    for (int index = 0; index < repetitions; index++)
    {
      if (random.Next(chance) == 0)
      {
        stack += random.Next(minimum, maximum);
      }
    }

    return stack;
  }

  private static int RollRepeatStack(
    ExtractinatorRandom random,
    int firstChance,
    int firstMinimum,
    int firstMaximum,
    int secondChance,
    int secondMinimum,
    int secondMaximum,
    int thirdChance,
    int thirdMinimum,
    int thirdMaximum,
    int fourthChance,
    int fourthMinimum,
    int fourthMaximum,
    int fifthChance,
    int fifthMinimum,
    int fifthMaximum)
  {
    int stack = 1;
    if (random.Next(firstChance) == 0)
    {
      stack += random.Next(firstMinimum, firstMaximum);
    }

    if (random.Next(secondChance) == 0)
    {
      stack += random.Next(secondMinimum, secondMaximum);
    }

    if (random.Next(thirdChance) == 0)
    {
      stack += random.Next(thirdMinimum, thirdMaximum);
    }

    if (random.Next(fourthChance) == 0)
    {
      stack += random.Next(fourthMinimum, fourthMaximum);
    }

    if (random.Next(fifthChance) == 0)
    {
      stack += random.Next(fifthMinimum, fifthMaximum);
    }

    return stack;
  }

  private static int RollSpecialBlock(ExtractinatorRandom random)
  {
    return random.Next(3) switch
    {
      0 => 62,
      1 => 195,
      _ => 194
    };
  }

  private static ushort RollOreEarlymode(ExtractinatorRandom random)
  {
    return random.Next(8) switch
    {
      0 => 12,
      1 => 11,
      2 => 14,
      3 => 13,
      4 => 699,
      5 => 700,
      6 => 701,
      _ => 702
    };
  }

  private static ushort RollOreHardmode(ExtractinatorRandom random)
  {
    return random.Next(14) switch
    {
      0 => 12,
      1 => 11,
      2 => 14,
      3 => 13,
      4 => 699,
      5 => 700,
      6 => 701,
      7 => 702,
      8 => 364,
      9 => 1104,
      10 => 365,
      11 => 1105,
      12 => 366,
      _ => 1106
    };
  }
}
