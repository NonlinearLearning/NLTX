namespace Terraria.NonAuthoritative.WorldSession;

public sealed class WorldRulesHandoff
{
  public WorldRulesHandoff(
    int gameMode,
    bool drunkWorld,
    bool notTheBees,
    bool forTheWorthy,
    bool anniversary,
    bool dontStarve,
    bool remixWorld,
    bool noTrapsWorld,
    bool zenithWorld,
    bool skyblockWorld,
    bool hasCorruption,
    bool isHardMode,
    bool defeatedMoonlord,
    IEnumerable<string> seedOptionsInOrder)
  {
    ArgumentNullException.ThrowIfNull(seedOptionsInOrder);
    string[] options = seedOptionsInOrder.ToArray();
    if (options.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentException("Seed option names cannot be empty.", nameof(seedOptionsInOrder));
    }

    GameMode = gameMode;
    DrunkWorld = drunkWorld;
    NotTheBees = notTheBees;
    ForTheWorthy = forTheWorthy;
    Anniversary = anniversary;
    DontStarve = dontStarve;
    RemixWorld = remixWorld;
    NoTrapsWorld = noTrapsWorld;
    ZenithWorld = zenithWorld;
    SkyblockWorld = skyblockWorld;
    HasCorruption = hasCorruption;
    IsHardMode = isHardMode;
    DefeatedMoonlord = defeatedMoonlord;
    SeedOptionsInOrder = Array.AsReadOnly(options);
  }

  public int GameMode { get; }

  public bool DrunkWorld { get; }

  public bool NotTheBees { get; }

  public bool ForTheWorthy { get; }

  public bool Anniversary { get; }

  public bool DontStarve { get; }

  public bool RemixWorld { get; }

  public bool NoTrapsWorld { get; }

  public bool ZenithWorld { get; }

  public bool SkyblockWorld { get; }

  public bool HasCorruption { get; }

  public bool HasCrimson => !HasCorruption;

  public bool IsHardMode { get; }

  public bool DefeatedMoonlord { get; }

  public IReadOnlyList<string> SeedOptionsInOrder { get; }

  public int SerializedSeedSum
  {
    get
    {
      int sum = 0;
      if (DrunkWorld)
      {
        sum += 1;
      }

      if (NotTheBees)
      {
        sum += 2;
      }

      if (ForTheWorthy)
      {
        sum += 4;
      }

      if (Anniversary)
      {
        sum += 8;
      }

      if (DontStarve)
      {
        sum += 16;
      }

      if (RemixWorld)
      {
        sum += 32;
      }

      if (NoTrapsWorld)
      {
        sum += 64;
      }

      if (ZenithWorld)
      {
        sum += 128;
      }

      if (SkyblockWorld)
      {
        sum += 256;
      }

      return sum;
    }
  }
}
