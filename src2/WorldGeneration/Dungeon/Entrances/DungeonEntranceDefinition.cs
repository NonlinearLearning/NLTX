using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Entrances;

public sealed class DungeonEntranceDefinition
{
  public DungeonEntranceDefinition(
    int entranceType,
    int randomSeed,
    DungeonStyleMaterialDefinition styleData)
  {
    if (entranceType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(entranceType));
    }

    if (randomSeed < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(randomSeed));
    }

    StyleData = styleData ?? throw new ArgumentNullException(nameof(styleData));
    EntranceType = entranceType;
    RandomSeed = randomSeed;
  }

  public int EntranceType { get; }

  public int RandomSeed { get; }

  public DungeonStyleMaterialDefinition StyleData { get; }

  public bool PrecalculateEntrancePosition { get; init; }

  public int BuriedEntranceYOffset { get; init; }

  public int BuriedEntranceSandDugoutYOffset { get; init; }

  public int RoughHeight { get; init; }

  public bool BuryEntrance { get; init; }
}
