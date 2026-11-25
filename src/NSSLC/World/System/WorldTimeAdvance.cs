namespace Terraria.Unshared.World;

/// <summary>
/// World clock values produced by one accepted tick, computed by
/// <see cref="WorldTimeAdvanceSolver" />.
/// </summary>
public readonly record struct WorldTimeAdvance(
    long Tick,
    double TotalTimeSeconds,
    double TimeOfDaySeconds,
    bool DayTime,
    long Revision);
