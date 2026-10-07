using System;

namespace Terraria.WorldGeneration.Actions;

/// <summary>
/// Describes one evidenced tile-solidity update around the Version4 larva pass.
/// </summary>
public readonly record struct UndergroundDesertLarvaTileSolidityCommand
{
  public enum Phase : byte
  {
    BeforeLarvaPlacement,
    AfterLarvaPlacement,
  }

  private UndergroundDesertLarvaTileSolidityCommand(
    long generationId,
    Phase phase,
    ushort tileTypeId,
    bool solid)
  {
    GenerationId = generationId;
    Timing = phase;
    TileTypeId = tileTypeId;
    Solid = solid;
  }

  public long GenerationId { get; }

  public Phase Timing { get; }

  public ushort TileTypeId { get; }

  public bool Solid { get; }

  internal static UndergroundDesertLarvaTileSolidityCommand BeforePlacement(
    long generationId)
  {
    return new UndergroundDesertLarvaTileSolidityCommand(
      generationId,
      Phase.BeforeLarvaPlacement,
      tileTypeId: 229,
      solid: true);
  }

  internal static UndergroundDesertLarvaTileSolidityCommand AfterPlacement(
    long generationId,
    ushort tileTypeId)
  {
    if (tileTypeId is not (162 or 232))
    {
      throw new ArgumentOutOfRangeException(nameof(tileTypeId));
    }

    return new UndergroundDesertLarvaTileSolidityCommand(
      generationId,
      Phase.AfterLarvaPlacement,
      tileTypeId,
      solid: true);
  }

  public bool IsWellFormed =>
    GenerationId >= 0 &&
    Solid &&
    (Timing, TileTypeId) switch
    {
      (Phase.BeforeLarvaPlacement, 229) => true,
      (Phase.AfterLarvaPlacement, 162 or 232) => true,
      _ => false,
    };

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "The underground-desert larva tile-solidity command is not well formed.",
        nameof(Timing));
    }
  }
}
