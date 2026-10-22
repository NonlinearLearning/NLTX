using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

public sealed partial class WorldOwner
{
  /// <summary>Commits an explicit world time supplied by the host or persistence loader.</summary>
  public WorldMutationResult SetWorldTime(ulong worldEpoch, WorldTimeState value)
  {
    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return WorldMutationResult.Rejected("The world epoch is stale or unavailable.");
      }

      try
      {
        _ = WorldTimeSystem.CalculateAdvance(value, 0, 0);
      }
      catch (ArgumentOutOfRangeException)
      {
        return WorldMutationResult.Rejected("The world time value is invalid.");
      }

      WorldTimeState current = new(_header!.DayTime, _header.Time, _header.MoonPhase);
      if (value == current)
      {
        return WorldMutationResult.Success(CurrentVersion, [], [], producesImmediateBroadcast: false);
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldMutationResult.Rejected("The world commit version is exhausted.");
      }

      _header = new WorldHeaderSnapshot(
          _header.Name,
          _header.Geometry,
          _header.Spawn,
          _header.UniqueId,
          value.DayTime,
          value.Time,
          value.MoonPhase,
          _header.WorldId,
          _header.Facts,
          _header.TeamSpawns);
      CommitVersion version = NextVersion();
      _commitVersion = version.Value;
      return WorldMutationResult.Success(version, [], [], producesImmediateBroadcast: false);
    }
  }
}
