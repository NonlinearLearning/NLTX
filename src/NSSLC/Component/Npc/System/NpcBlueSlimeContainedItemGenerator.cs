namespace Terraria.Npc;

/// <summary>
/// Source-order item selector for AI_001_Slimes_GenerateItemInsideBody.
/// </summary>
public static class NpcBlueSlimeContainedItemGenerator
{
  public static NpcBlueSlimeTypeOneSelectionResult SelectForTypeOne(
    in NpcBlueSlimeTypeOneSelectionInput input,
    INpcBlueSlimeRandomPort random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (input.NetId != 1)
    {
      throw new ArgumentException(
        "The type-one contained-item selector only accepts Blue Slime net id 1.",
        nameof(input));
    }

    if (input.CurrentItemState != 0f || input.NetMode == 1 || input.NpcValue <= 0f)
    {
      return new NpcBlueSlimeTypeOneSelectionResult(false, input.CurrentItemState, false);
    }

    float itemState = -1f;
    int attempts = 1 + (input.LowTiles ? 4 : 0) + (input.SlimeRain ? 2 : 0);
    int helperChance = input.LowTiles ? 15 : 20;
    int additionalChance = input.LowTiles ? 20 : 40;

    int specialItemChance = input.NoTrapsWorld
      ? 20
      : input.GetGoodWorld
        ? 100
        : input.CenterY < input.WorldSurfaceTiles * 16d
          ? -1
          : 500;

    for (int attempt = 0; attempt < attempts && itemState == -1f; attempt++)
    {
      if (input.PositionInRockLayer &&
          (input.NoLifeCrystals || input.LowTiles) &&
          !input.AnyLifeCrystalSlime &&
          random.Next(200) == 0)
      {
        itemState = 29f;
      }
      else if (input.LowTiles &&
               input.PositionY / 16f > input.WorldSurfaceTiles &&
               random.Next(1000) == 0)
      {
        itemState = random.GetRandomVoiceItem();
      }
      else if (input.GenuineParty && input.CenterY < input.WorldSurfaceTiles * 16d)
      {
        itemState = random.Next(2) == 0
          ? random.Next(3736, 3739)
          : 1345;
      }
      else if (random.Next(helperChance) == 0)
      {
        var helperInput = new NpcBlueSlimeContainedItemInput(
          input.IsBallooned,
          input.LowTiles,
          input.MoonPhase,
          input.CenterInRockLayer,
          input.HardMode,
          input.NetMode);
        itemState = Generate(in helperInput, random);
      }
      else if (random.Next(additionalChance) == 0)
      {
        if (input.PositionY / 16f <= input.WorldSurfaceTiles)
        {
          itemState = 751f;
        }
        else if (!input.PositionInRockLayer)
        {
          itemState = random.Next(3) switch
          {
            1 => 3f,
            2 => 9f,
            _ => 2f,
          };
        }
        else if (random.Next(10) == 0)
        {
          itemState = 3609f;
        }
        else
        {
          itemState = random.Next(4) switch
          {
            1 => 150f,
            2 => 3086f,
            3 => 3081f,
            _ => 3f,
          };
        }
      }
      else if (specialItemChance > 0 && random.Next(specialItemChance) == 0)
      {
        itemState = 539f;
      }
      else if (input.GetGoodWorld &&
               input.PositionY / 16f > input.WorldSurfaceTiles &&
               random.Next(specialItemChance) == 0)
      {
        itemState = 147f;
      }
      else if (attempt == 0 && input.RemixWorld && !input.IsBallooned &&
               random.Next(3) == 0)
      {
        itemState = 75f;
      }
      else if (input.VampireSeed && !input.RemixWorld && random.Next(13) == 0 &&
               input.PositionY / 16f > input.WorldSurfaceTiles)
      {
        itemState = 9f;
      }
    }

    return new NpcBlueSlimeTypeOneSelectionResult(true, itemState, NetUpdateRequested: true);
  }

  public static int Generate(
    in NpcBlueSlimeContainedItemInput input,
    INpcBlueSlimeRandomPort random)
  {
    ArgumentNullException.ThrowIfNull(random);

    int category = random.Next(4);
    bool ballooned = input.IsBallooned;
    if (input.LowTiles)
    {
      if (random.Next(3) != 0)
      {
        category = random.Next(1, 3);
      }

      if (random.Next(3) != 0)
      {
        ballooned = false;
      }
    }

    if (ballooned)
    {
      return random.Next(13) switch
      {
        1 => 4368,
        2 => 4369,
        3 => 4370,
        4 => 4371,
        5 => 4612,
        6 => 4674,
        7 or 8 or 9 => 4343,
        10 or 11 or 12 => 4344,
        _ => 4367,
      };
    }

    return category switch
    {
      0 => GenerateCategoryZero(input.NetMode, random),
      1 => GenerateCategoryOne(input, random),
      2 => GenerateCategoryTwo(input, random),
      _ => GenerateCategoryThree(input.LowTiles, random),
    };
  }

  private static int GenerateCategoryZero(
    int netMode,
    INpcBlueSlimeRandomPort random)
  {
    return random.Next(7) switch
    {
      0 => 290,
      1 => 292,
      2 => 296,
      3 => 2322,
      _ when netMode != 0 && random.Next(2) == 0 => 2997,
      _ => 2350,
    };
  }

  private static int GenerateCategoryOne(
    in NpcBlueSlimeContainedItemInput input,
    INpcBlueSlimeRandomPort random)
  {
    int choice = random.Next(4);
    if (input.LowTiles)
    {
      if (input.MoonPhase == 0)
      {
        choice = random.Next(2);
      }

      if (choice == 2)
      {
        choice = random.Next(4);
      }
    }

    return choice switch
    {
      0 => 8,
      1 => 965,
      2 => 166,
      _ => 58,
    };
  }

  private static int GenerateCategoryTwo(
    in NpcBlueSlimeContainedItemInput input,
    INpcBlueSlimeRandomPort random)
  {
    if (input.InRockLayer && input.LowTiles && input.HardMode && random.Next(2) == 0)
    {
      return random.Next(6) switch
      {
        1 => 1104,
        2 => 365,
        3 => 1105,
        4 => 366,
        5 => 1106,
        _ => 364,
      };
    }

    if (random.Next(2) == 0)
    {
      return random.Next(11, 15);
    }

    return random.Next(699, 703);
  }

  private static int GenerateCategoryThree(
    bool lowTiles,
    INpcBlueSlimeRandomPort random)
  {
    int choice = random.Next(3);
    if (lowTiles && random.Next(5) != 0)
    {
      choice = 0;
    }

    return choice switch
    {
      0 => 71,
      1 => 72,
      _ => 73,
    };
  }
}
