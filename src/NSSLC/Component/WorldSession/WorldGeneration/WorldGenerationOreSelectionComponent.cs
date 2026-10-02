using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the generation-session ore and bar selections from the GenVars boundary.
/// </summary>
public sealed class WorldGenerationOreSelectionComponent
{
  public const int DefaultCopperBar = 20;
  public const int DefaultIronBar = 22;
  public const int DefaultSilverBar = 21;
  public const int DefaultGoldBar = 19;

  public WorldGenerationOreSelectionComponent(
    long generationId,
    int copper = 0,
    int iron = 0,
    int silver = 0,
    int gold = 0,
    int copperBar = DefaultCopperBar,
    int ironBar = DefaultIronBar,
    int silverBar = DefaultSilverBar,
    int goldBar = DefaultGoldBar)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ValidateValues(
      copper,
      iron,
      silver,
      gold,
      copperBar,
      ironBar,
      silverBar,
      goldBar);

    GenerationId = generationId;
    Copper = copper;
    Iron = iron;
    Silver = silver;
    Gold = gold;
    CopperBar = copperBar;
    IronBar = ironBar;
    SilverBar = silverBar;
    GoldBar = goldBar;
  }

  public long GenerationId { get; }

  public int Copper { get; private set; }

  public int Iron { get; private set; }

  public int Silver { get; private set; }

  public int Gold { get; private set; }

  public int CopperBar { get; private set; }

  public int IronBar { get; private set; }

  public int SilverBar { get; private set; }

  public int GoldBar { get; private set; }

  public void ReplaceSelection(
    int copper,
    int iron,
    int silver,
    int gold,
    int copperBar,
    int ironBar,
    int silverBar,
    int goldBar)
  {
    ValidateValues(
      copper,
      iron,
      silver,
      gold,
      copperBar,
      ironBar,
      silverBar,
      goldBar);
    Copper = copper;
    Iron = iron;
    Silver = silver;
    Gold = gold;
    CopperBar = copperBar;
    IronBar = ironBar;
    SilverBar = silverBar;
    GoldBar = goldBar;
  }

  public WorldGenerationOreSelectionSnapshot CreateSnapshot()
  {
    return new WorldGenerationOreSelectionSnapshot(
      GenerationId,
      Copper,
      Iron,
      Silver,
      Gold,
      CopperBar,
      IronBar,
      SilverBar,
      GoldBar);
  }

  private static void ValidateValues(
    int copper,
    int iron,
    int silver,
    int gold,
    int copperBar,
    int ironBar,
    int silverBar,
    int goldBar)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(copper);
    ArgumentOutOfRangeException.ThrowIfNegative(iron);
    ArgumentOutOfRangeException.ThrowIfNegative(silver);
    ArgumentOutOfRangeException.ThrowIfNegative(gold);
    ArgumentOutOfRangeException.ThrowIfNegative(copperBar);
    ArgumentOutOfRangeException.ThrowIfNegative(ironBar);
    ArgumentOutOfRangeException.ThrowIfNegative(silverBar);
    ArgumentOutOfRangeException.ThrowIfNegative(goldBar);
  }
}
