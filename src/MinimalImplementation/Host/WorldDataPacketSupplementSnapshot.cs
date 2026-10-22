using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Carries packet-7 facts that are owned outside the published P03 world header.
/// The composition boundary must capture these facts for the same world epoch and commit version
/// as the WorldSnapshot supplied to WorldDataPacketProducer.
/// </summary>
public sealed class WorldDataPacketSupplementSnapshot
{
  private readonly IReadOnlyList<byte> _backgroundTypes;
  private readonly IReadOnlyList<byte> _treeTopStyles;
  private readonly IReadOnlyList<byte> _worldFlagGroups;
  private readonly IReadOnlyList<short> _savedOreTiers;
  private readonly IReadOnlyList<Packet7ExtraSpawnPoint> _extraSpawnPoints;

  public WorldDataPacketSupplementSnapshot(
    ulong worldEpoch,
    CommitVersion commitVersion,
    IReadOnlyList<byte> backgroundTypes,
    IReadOnlyList<byte> treeTopStyles,
    float windSpeedTarget,
    byte cloudCount,
    bool bloodMoon,
    bool eclipse,
    bool isRaining,
    float maximumRain,
    IReadOnlyList<byte> worldFlagGroups,
    byte sundialCooldown,
    byte moondialCooldown,
    IReadOnlyList<short> savedOreTiers,
    sbyte invasionType,
    ulong lobbyId,
    float sandstormSeverity,
    IReadOnlyList<Packet7ExtraSpawnPoint> extraSpawnPoints)
  {
    ArgumentNullException.ThrowIfNull(backgroundTypes);
    ArgumentNullException.ThrowIfNull(treeTopStyles);
    ArgumentNullException.ThrowIfNull(worldFlagGroups);
    ArgumentNullException.ThrowIfNull(savedOreTiers);
    ArgumentNullException.ThrowIfNull(extraSpawnPoints);
    if (worldEpoch == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }

    if (!commitVersion.IsValid)
    {
      throw new ArgumentException("Packet 7 facts require a committed world version.",
        nameof(commitVersion));
    }

    RequireCount(backgroundTypes, 13, nameof(backgroundTypes));
    RequireCount(treeTopStyles, 13, nameof(treeTopStyles));
    RequireCount(worldFlagGroups, 11, nameof(worldFlagGroups));
    RequireCount(savedOreTiers, 7, nameof(savedOreTiers));
    if (extraSpawnPoints.Count > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(
        nameof(extraSpawnPoints), "Packet 7 supports at most 255 extra spawn points.");
    }

    if (!float.IsFinite(windSpeedTarget) || !float.IsFinite(maximumRain) ||
        !float.IsFinite(sandstormSeverity))
    {
      throw new ArgumentOutOfRangeException(nameof(windSpeedTarget));
    }

    WorldEpoch = worldEpoch;
    CommitVersion = commitVersion;
    _backgroundTypes = Array.AsReadOnly(backgroundTypes.ToArray());
    _treeTopStyles = Array.AsReadOnly(treeTopStyles.ToArray());
    WindSpeedTarget = windSpeedTarget;
    CloudCount = cloudCount;
    BloodMoon = bloodMoon;
    Eclipse = eclipse;
    IsRaining = isRaining;
    MaximumRain = maximumRain;
    _worldFlagGroups = Array.AsReadOnly(worldFlagGroups.ToArray());
    SundialCooldown = sundialCooldown;
    MoondialCooldown = moondialCooldown;
    _savedOreTiers = Array.AsReadOnly(savedOreTiers.ToArray());
    InvasionType = invasionType;
    LobbyId = lobbyId;
    SandstormSeverity = sandstormSeverity;
    _extraSpawnPoints = Array.AsReadOnly(extraSpawnPoints.ToArray());
  }

  public ulong WorldEpoch { get; }

  public CommitVersion CommitVersion { get; }

  public IReadOnlyList<byte> BackgroundTypes => _backgroundTypes;

  public IReadOnlyList<byte> TreeTopStyles => _treeTopStyles;

  public float WindSpeedTarget { get; }

  public byte CloudCount { get; }

  public bool BloodMoon { get; }

  public bool Eclipse { get; }

  public bool IsRaining { get; }

  public float MaximumRain { get; }

  public IReadOnlyList<byte> WorldFlagGroups => _worldFlagGroups;

  public byte SundialCooldown { get; }

  public byte MoondialCooldown { get; }

  public IReadOnlyList<short> SavedOreTiers => _savedOreTiers;

  public sbyte InvasionType { get; }

  public ulong LobbyId { get; }

  public float SandstormSeverity { get; }

  public IReadOnlyList<Packet7ExtraSpawnPoint> ExtraSpawnPoints => _extraSpawnPoints;

  private static void RequireCount<T>(IReadOnlyList<T> values, int expected, string name)
  {
    if (values.Count != expected)
    {
      throw new ArgumentException(
        $"Packet 7 {name} must contain exactly {expected} values.", name);
    }
  }
}
