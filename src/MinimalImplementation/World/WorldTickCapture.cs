using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>
/// Immutable World-owned inputs captured from one published revision before a server tick.
/// Runtime-only inputs such as elapsed ticks, day rate and liquid budget remain explicit at the
/// coordinator boundary.
/// </summary>
public sealed class WorldTickCapture
{
  internal WorldTickCapture(
    Guid ownerToken,
    WorldHeaderSnapshot header,
    WorldFacts facts,
    ulong worldEpoch,
    CommitVersion version)
  {
    ArgumentNullException.ThrowIfNull(header);
    ArgumentNullException.ThrowIfNull(facts);
    if (ownerToken == Guid.Empty)
    {
      throw new ArgumentOutOfRangeException(nameof(ownerToken));
    }

    if (worldEpoch == 0 || !version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }

    OwnerToken = ownerToken;
    Header = header;
    Facts = facts;
    WorldEpoch = worldEpoch;
    Version = version;
    Time = new WorldTimeState(header.DayTime, header.Time, header.MoonPhase);
  }

  internal Guid OwnerToken { get; }

  public WorldHeaderSnapshot Header { get; }

  public WorldHeaderFacts EnvironmentFacts => Header.Facts;

  public WorldFacts Facts { get; }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public WorldTimeState Time { get; }
}
