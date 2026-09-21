namespace Terraria.NonAuthoritative.WorldSession.Calendar;

public sealed record WorldTemporaryEventContext
{
  public WorldTemporaryEventContext(
    double time,
    bool raining,
    float maxRain,
    int rainTime,
    bool dayTime,
    bool bloodMoon,
    bool eclipse,
    int moonPhase,
    int cultistDelay,
    bool partyGenuine,
    bool partyManual,
    int partyCooldown,
    IEnumerable<int> celebratingNpcIds,
    bool sandstormHappening,
    int sandstormTimeLeft,
    float sandstormSeverity,
    float sandstormIntendedSeverity,
    bool lanternNightGenuine,
    bool lanternNightManual,
    bool lanternNightNextNightIsGenuine,
    int lanternNightCooldown,
    int coinRain,
    int meteorShowerCount)
  {
    ArgumentNullException.ThrowIfNull(celebratingNpcIds);
    if (celebratingNpcIds.Any(id => id < 0))
    {
      throw new ArgumentException("Celebrating NPC ids cannot be negative.", nameof(celebratingNpcIds));
    }

    Time = time;
    Raining = raining;
    MaxRain = maxRain;
    RainTime = rainTime;
    DayTime = dayTime;
    BloodMoon = bloodMoon;
    Eclipse = eclipse;
    MoonPhase = moonPhase;
    CultistDelay = cultistDelay;
    PartyGenuine = partyGenuine;
    PartyManual = partyManual;
    PartyCooldown = partyCooldown;
    CelebratingNpcIds = Array.AsReadOnly(celebratingNpcIds.ToArray());
    SandstormHappening = sandstormHappening;
    SandstormTimeLeft = sandstormTimeLeft;
    SandstormSeverity = sandstormSeverity;
    SandstormIntendedSeverity = sandstormIntendedSeverity;
    LanternNightGenuine = lanternNightGenuine;
    LanternNightManual = lanternNightManual;
    LanternNightNextNightIsGenuine = lanternNightNextNightIsGenuine;
    LanternNightCooldown = lanternNightCooldown;
    CoinRain = coinRain;
    MeteorShowerCount = meteorShowerCount;
  }

  public double Time { get; init; }

  public bool Raining { get; init; }

  public float MaxRain { get; init; }

  public int RainTime { get; init; }

  public bool DayTime { get; init; }

  public bool BloodMoon { get; init; }

  public bool Eclipse { get; init; }

  public int MoonPhase { get; init; }

  public int CultistDelay { get; init; }

  public bool PartyGenuine { get; init; }

  public bool PartyManual { get; init; }

  public int PartyCooldown { get; init; }

  public IReadOnlyList<int> CelebratingNpcIds { get; init; }

  public bool SandstormHappening { get; init; }

  public int SandstormTimeLeft { get; init; }

  public float SandstormSeverity { get; init; }

  public float SandstormIntendedSeverity { get; init; }

  public bool LanternNightGenuine { get; init; }

  public bool LanternNightManual { get; init; }

  public bool LanternNightNextNightIsGenuine { get; init; }

  public int LanternNightCooldown { get; init; }

  public int CoinRain { get; init; }

  public int MeteorShowerCount { get; init; }

  public static WorldTemporaryEventContext Reset(WorldResetTimePolicy policy)
  {
    bool dayTime = !policy.GraveyardBloodmoonEnabled;
    return new WorldTemporaryEventContext(
      time: dayTime ? 13500.0 : 1.0,
      raining: false,
      maxRain: 0f,
      rainTime: 0,
      dayTime: dayTime,
      bloodMoon: policy.GraveyardBloodmoonEnabled,
      eclipse: false,
      moonPhase: 0,
      cultistDelay: 86400,
      partyGenuine: policy.TenthAnniversaryWorld && !policy.SkyblockWorld,
      partyManual: false,
      partyCooldown: 0,
      celebratingNpcIds: Array.Empty<int>(),
      sandstormHappening: false,
      sandstormTimeLeft: 0,
      sandstormSeverity: 0f,
      sandstormIntendedSeverity: 0f,
      lanternNightGenuine: false,
      lanternNightManual: false,
      lanternNightNextNightIsGenuine: false,
      lanternNightCooldown: 0,
      coinRain: 0,
      meteorShowerCount: 0);
  }
}
