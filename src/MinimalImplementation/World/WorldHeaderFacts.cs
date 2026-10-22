namespace MinimalImplementation.Server.World;

/// <summary>Immutable non-event Version4 header and environment facts read from a world document.</summary>
public sealed class WorldHeaderFacts
{
  private readonly IReadOnlyList<int> _treeX;
  private readonly IReadOnlyList<int> _treeStyle;
  private readonly IReadOnlyList<int> _caveBackX;
  private readonly IReadOnlyList<int> _caveBackStyle;

  public WorldHeaderFacts(
    string? seedText,
    ulong? worldGeneratorVersion,
    int leftWorld,
    int rightWorld,
    int topWorld,
    int bottomWorld,
    int gameMode,
    bool drunkWorld,
    bool getGoodWorld,
    bool tenthAnniversaryWorld,
    bool dontStarveWorld,
    bool notTheBeesWorld,
    bool remixWorld,
    bool noTrapsWorld,
    bool zenithWorld,
    bool skyblockWorld,
    DateTime? creationTime,
    DateTime? lastPlayed,
    byte moonType,
    IReadOnlyList<int> treeX,
    IReadOnlyList<int> treeStyle,
    IReadOnlyList<int> caveBackX,
    IReadOnlyList<int> caveBackStyle,
    int iceBackStyle,
    int jungleBackStyle,
    int hellBackStyle,
    double worldSurface,
    double rockLayer,
    bool bloodMoon,
    bool eclipse,
    int dungeonX,
    int dungeonY,
    bool crimson)
  {
    ArgumentNullException.ThrowIfNull(treeX);
    ArgumentNullException.ThrowIfNull(treeStyle);
    ArgumentNullException.ThrowIfNull(caveBackX);
    ArgumentNullException.ThrowIfNull(caveBackStyle);
    if (treeX.Count != 3 || treeStyle.Count != 4 || caveBackX.Count != 3 || caveBackStyle.Count != 4)
    {
      throw new ArgumentException("Version4 header tree and cave arrays have invalid lengths.");
    }

    if (!double.IsFinite(worldSurface) || !double.IsFinite(rockLayer))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurface));
    }

    SeedText = seedText;
    WorldGeneratorVersion = worldGeneratorVersion;
    LeftWorld = leftWorld;
    RightWorld = rightWorld;
    TopWorld = topWorld;
    BottomWorld = bottomWorld;
    GameMode = gameMode;
    DrunkWorld = drunkWorld;
    GetGoodWorld = getGoodWorld;
    TenthAnniversaryWorld = tenthAnniversaryWorld;
    DontStarveWorld = dontStarveWorld;
    NotTheBeesWorld = notTheBeesWorld;
    RemixWorld = remixWorld;
    NoTrapsWorld = noTrapsWorld;
    ZenithWorld = zenithWorld;
    SkyblockWorld = skyblockWorld;
    CreationTime = creationTime;
    LastPlayed = lastPlayed;
    MoonType = moonType;
    _treeX = Array.AsReadOnly(treeX.ToArray());
    _treeStyle = Array.AsReadOnly(treeStyle.ToArray());
    _caveBackX = Array.AsReadOnly(caveBackX.ToArray());
    _caveBackStyle = Array.AsReadOnly(caveBackStyle.ToArray());
    IceBackStyle = iceBackStyle;
    JungleBackStyle = jungleBackStyle;
    HellBackStyle = hellBackStyle;
    WorldSurface = worldSurface;
    RockLayer = rockLayer;
    BloodMoon = bloodMoon;
    Eclipse = eclipse;
    DungeonX = dungeonX;
    DungeonY = dungeonY;
    Crimson = crimson;
  }

  public string? SeedText { get; }

  public ulong? WorldGeneratorVersion { get; }

  public int LeftWorld { get; }

  public int RightWorld { get; }

  public int TopWorld { get; }

  public int BottomWorld { get; }

  public int GameMode { get; }

  public bool DrunkWorld { get; }

  public bool GetGoodWorld { get; }

  public bool TenthAnniversaryWorld { get; }

  public bool DontStarveWorld { get; }

  public bool NotTheBeesWorld { get; }

  public bool RemixWorld { get; }

  public bool NoTrapsWorld { get; }

  public bool ZenithWorld { get; }

  public bool SkyblockWorld { get; }

  public DateTime? CreationTime { get; }

  public DateTime? LastPlayed { get; }

  public byte MoonType { get; }

  public IReadOnlyList<int> TreeX => _treeX;

  public IReadOnlyList<int> TreeStyle => _treeStyle;

  public IReadOnlyList<int> CaveBackX => _caveBackX;

  public IReadOnlyList<int> CaveBackStyle => _caveBackStyle;

  public int IceBackStyle { get; }

  public int JungleBackStyle { get; }

  public int HellBackStyle { get; }

  public double WorldSurface { get; }

  public double RockLayer { get; }

  public bool BloodMoon { get; }

  public bool Eclipse { get; }

  public int DungeonX { get; }

  public int DungeonY { get; }

  public bool Crimson { get; }
}
