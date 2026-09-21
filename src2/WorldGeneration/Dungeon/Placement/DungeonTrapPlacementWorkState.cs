using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Placement;

public sealed class DungeonTrapPlacementWorkState
{
  private readonly List<DartTrapPlacementAttempt> _dartTraps = new();
  private readonly List<BoulderPlacementAttempt> _boulders = new();
  private readonly List<WirePlacementAttempt> _wires = new();
  private ExplosivePlacementAttempt? _explosive;

  public IReadOnlyList<DartTrapPlacementAttempt> DartTraps => _dartTraps;

  public IReadOnlyList<BoulderPlacementAttempt> Boulders => _boulders;

  public IReadOnlyList<WirePlacementAttempt> Wires => _wires;

  public ExplosivePlacementAttempt? Explosive => _explosive;

  public int NumberOfDartTraps { get; private set; }

  public int NumberOfBoulderTraps { get; private set; }

  public int NumberOfStepsBetweenBoulderTraps { get; private set; }

  public void Configure(int numberOfDartTraps, int numberOfBoulderTraps, int stepsBetweenBoulderTraps)
  {
    if (numberOfDartTraps < 0 || numberOfBoulderTraps < 0 || stepsBetweenBoulderTraps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(numberOfDartTraps));
    }

    NumberOfDartTraps = numberOfDartTraps;
    NumberOfBoulderTraps = numberOfBoulderTraps;
    NumberOfStepsBetweenBoulderTraps = stepsBetweenBoulderTraps;
  }

  public void AddDartTrap(DartTrapPlacementAttempt attempt) => _dartTraps.Add(attempt);

  public void AddBoulder(BoulderPlacementAttempt attempt) => _boulders.Add(attempt);

  public void AddWire(WirePlacementAttempt attempt) => _wires.Add(attempt);

  public void SetExplosive(ExplosivePlacementAttempt attempt) => _explosive = attempt;

  public void ClearAttempts()
  {
    _dartTraps.Clear();
    _boulders.Clear();
    _wires.Clear();
    _explosive = null;
  }
}
