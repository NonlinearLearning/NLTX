namespace Terraria.WorldGeneration.Dungeon.Generation;

public sealed class DungeonGenerationScalarStateComponent
{
  public int WallVariants { get; private set; }

  public int ChandelierItemType { get; private set; }

  public int PlatformItemType { get; private set; }

  public int DoorItemType { get; private set; }

  public IReadOnlyList<int> LanternStyles { get; private set; } = Array.Empty<int>();

  public IReadOnlyList<int> ShelfStyles { get; private set; } = Array.Empty<int>();

  public IReadOnlyList<int> BannerStyles { get; private set; } = Array.Empty<int>();

  public float GlobalFeatureScalar { get; private set; } = 1f;

  public float DungeonStepScalar { get; private set; } = 1f;

  public float HallStrengthScalar { get; private set; } = 1f;

  public float HallStepScalar { get; private set; } = 1f;

  public float HallInteriorToExteriorRatio { get; private set; } = 1f;

  public float HallSlantVariantScalar { get; private set; } = 1f;

  public float RoomStrengthScalar { get; private set; } = 1f;

  public float RoomStepScalar { get; private set; } = 1f;

  public float RoomInteriorToExteriorRatio { get; private set; } = 1f;

  public float RoomSlantVariantScalar { get; private set; } = 1f;

  public void ConfigureStyles(
    int wallVariants,
    int chandelierItemType,
    int platformItemType,
    int doorItemType,
    IEnumerable<int> lanternStyles,
    IEnumerable<int> shelfStyles,
    IEnumerable<int> bannerStyles)
  {
    if (wallVariants < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(wallVariants));
    }

    WallVariants = wallVariants;
    ChandelierItemType = chandelierItemType;
    PlatformItemType = platformItemType;
    DoorItemType = doorItemType;
    LanternStyles = Copy(lanternStyles, nameof(lanternStyles));
    ShelfStyles = Copy(shelfStyles, nameof(shelfStyles));
    BannerStyles = Copy(bannerStyles, nameof(bannerStyles));
  }

  public void ConfigureScalars(
    float globalFeatureScalar,
    float dungeonStepScalar,
    float hallStrengthScalar,
    float hallStepScalar,
    float hallInteriorToExteriorRatio,
    float hallSlantVariantScalar,
    float roomStrengthScalar,
    float roomStepScalar,
    float roomInteriorToExteriorRatio,
    float roomSlantVariantScalar)
  {
    float[] values =
    [
      globalFeatureScalar,
      dungeonStepScalar,
      hallStrengthScalar,
      hallStepScalar,
      hallInteriorToExteriorRatio,
      hallSlantVariantScalar,
      roomStrengthScalar,
      roomStepScalar,
      roomInteriorToExteriorRatio,
      roomSlantVariantScalar
    ];

    if (values.Any(value => !float.IsFinite(value) || value < 0f))
    {
      throw new ArgumentOutOfRangeException(nameof(globalFeatureScalar));
    }

    GlobalFeatureScalar = globalFeatureScalar;
    DungeonStepScalar = dungeonStepScalar;
    HallStrengthScalar = hallStrengthScalar;
    HallStepScalar = hallStepScalar;
    HallInteriorToExteriorRatio = hallInteriorToExteriorRatio;
    HallSlantVariantScalar = hallSlantVariantScalar;
    RoomStrengthScalar = roomStrengthScalar;
    RoomStepScalar = roomStepScalar;
    RoomInteriorToExteriorRatio = roomInteriorToExteriorRatio;
    RoomSlantVariantScalar = roomSlantVariantScalar;
  }

  private static IReadOnlyList<int> Copy(
    IEnumerable<int> values,
    string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values, parameterName);
    return Array.AsReadOnly(values.ToArray());
  }
}
