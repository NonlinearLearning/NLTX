using Terraria.WorldGeneration.Dungeon.Layout;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Generation;

public sealed class DungeonLegacyRuleStateComponent
{
  public DungeonStyleId DungeonStyle { get; private set; }

  public DungeonStyleCatalog? DungeonGenerationStyles { get; private set; }

  public DungeonControlLineComponent? DungeonDitherSnake { get; private set; }

  public bool IsCrackedBrick { get; private set; }

  public bool IsPitTrapTile { get; private set; }

  public bool IsDungeonTile { get; private set; }

  public bool IsDungeonWall { get; private set; }

  public bool IsDungeonWallGlass { get; private set; }

  public bool GeneratingDungeon { get; private set; }

  public int PreGeneratedEntranceSettingsId { get; private set; }

  public int DesertChestLootState { get; private set; }

  public void BeginGeneration(
    DungeonStyleId style,
    DungeonStyleCatalog styles,
    int preGeneratedEntranceSettingsId)
  {
    ArgumentNullException.ThrowIfNull(styles);
    if (preGeneratedEntranceSettingsId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(preGeneratedEntranceSettingsId));
    }

    DungeonStyle = style;
    DungeonGenerationStyles = styles;
    PreGeneratedEntranceSettingsId = preGeneratedEntranceSettingsId;
    GeneratingDungeon = true;
  }

  public void SetFlags(
    bool isCrackedBrick,
    bool isPitTrapTile,
    bool isDungeonTile,
    bool isDungeonWall,
    bool isDungeonWallGlass,
    int desertChestLootState)
  {
    if (desertChestLootState < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(desertChestLootState));
    }

    IsCrackedBrick = isCrackedBrick;
    IsPitTrapTile = isPitTrapTile;
    IsDungeonTile = isDungeonTile;
    IsDungeonWall = isDungeonWall;
    IsDungeonWallGlass = isDungeonWallGlass;
    DesertChestLootState = desertChestLootState;
  }

  public void SetDitherSnake(DungeonControlLineComponent? ditherSnake)
  {
    DungeonDitherSnake = ditherSnake;
  }

  public void EndGeneration()
  {
    GeneratingDungeon = false;
    DungeonDitherSnake = null;
  }
}
