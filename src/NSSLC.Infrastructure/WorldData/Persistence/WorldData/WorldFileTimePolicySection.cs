namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Versioned time, seasonal-seed, meteor-rain and team-spawn values before ExtraSpawnPointManager data.
/// </summary>
public sealed class WorldFileTimePolicySection
{
  public const string SectionId = "world.time-policy";

  public WorldFileTimePolicySection(
    bool fastForwardTimeToDusk,
    byte moondialCooldown,
    bool forceHalloweenForever,
    bool forceChristmasForever,
    bool vampireSeed,
    bool infectedSeed,
    int meteorShowerCount,
    int coinRain,
    bool teamBasedSpawnsSeed)
  {
    FastForwardTimeToDusk = fastForwardTimeToDusk;
    MoondialCooldown = moondialCooldown;
    ForceHalloweenForever = forceHalloweenForever;
    ForceChristmasForever = forceChristmasForever;
    VampireSeed = vampireSeed;
    InfectedSeed = infectedSeed;
    MeteorShowerCount = meteorShowerCount;
    CoinRain = coinRain;
    TeamBasedSpawnsSeed = teamBasedSpawnsSeed;
  }

  public bool FastForwardTimeToDusk { get; }
  public byte MoondialCooldown { get; }
  public bool ForceHalloweenForever { get; }
  public bool ForceChristmasForever { get; }
  public bool VampireSeed { get; }
  public bool InfectedSeed { get; }
  public int MeteorShowerCount { get; }
  public int CoinRain { get; }

  public bool TeamBasedSpawnsSeed { get; }

  public static WorldFileTimePolicySection Empty => new(
    fastForwardTimeToDusk: false,
    moondialCooldown: 0,
    forceHalloweenForever: false,
    forceChristmasForever: false,
    vampireSeed: false,
    infectedSeed: false,
    meteorShowerCount: 0,
    coinRain: 0,
    teamBasedSpawnsSeed: false);
}
