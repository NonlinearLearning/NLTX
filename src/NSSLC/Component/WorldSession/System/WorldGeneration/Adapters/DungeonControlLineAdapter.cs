using System;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Forwards the dual-dungeon distance value to the external control-line authority.
/// </summary>
public sealed class DungeonControlLineAdapter :
  IDualDungeonDistanceQuery,
  IDualDungeonDistanceControlPort
{
  private readonly IDungeonControlLinePort _controlLine;

  public DungeonControlLineAdapter(IDungeonControlLinePort controlLine)
  {
    ArgumentNullException.ThrowIfNull(controlLine);
    _controlLine = controlLine;
  }

  public double NormalizedDistanceSafeFromDither
  {
    get => _controlLine.NormalizedDistanceSafeFromDither;
    set => _controlLine.NormalizedDistanceSafeFromDither = value;
  }
}
