using Terraria.WorldGeneration.Dungeon.Bounds;

namespace Terraria.WorldGeneration.Dungeon.Generation;

public sealed class DungeonGenerationContextComponent
{
  public DungeonGenerationContextComponent(int type, int iteration)
  {
    if (iteration < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(iteration));
    }

    Type = type;
    Iteration = iteration;
  }

  public int Type { get; }

  public int Iteration { get; private set; }

  public int DungeonEntranceId { get; private set; }

  public bool MakeNextPitTrapFlooded { get; private set; }

  public bool UseSkewedDungeonEntranceHalls { get; private set; }

  public bool CreatedDungeonEntranceOnSurface { get; private set; }

  public float DungeonEntranceStrengthX { get; private set; }

  public float DungeonEntranceStrengthY { get; private set; }

  public float DungeonEntranceStrengthX2 { get; private set; }

  public float DungeonEntranceStrengthY2 { get; private set; }

  public int LastDungeonHallId { get; private set; }

  public DungeonBoundsRectangle ProtectedDungeonBounds { get; private set; }

  public DungeonBoundsRectangle OuterProgressionBounds { get; private set; }

  public void ConfigureIteration(
    int dungeonEntranceId,
    DungeonBoundsRectangle protectedDungeonBounds,
    DungeonBoundsRectangle outerProgressionBounds)
  {
    if (dungeonEntranceId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(dungeonEntranceId));
    }

    DungeonEntranceId = dungeonEntranceId;
    ProtectedDungeonBounds = protectedDungeonBounds;
    OuterProgressionBounds = outerProgressionBounds;
  }

  public void SetEntranceStrength(
    float strengthX,
    float strengthY,
    float strengthX2,
    float strengthY2)
  {
    ValidateFinite(strengthX, nameof(strengthX));
    ValidateFinite(strengthY, nameof(strengthY));
    ValidateFinite(strengthX2, nameof(strengthX2));
    ValidateFinite(strengthY2, nameof(strengthY2));
    DungeonEntranceStrengthX = strengthX;
    DungeonEntranceStrengthY = strengthY;
    DungeonEntranceStrengthX2 = strengthX2;
    DungeonEntranceStrengthY2 = strengthY2;
  }

  public void SetIterationFlags(
    bool makeNextPitTrapFlooded,
    bool useSkewedDungeonEntranceHalls,
    bool createdDungeonEntranceOnSurface)
  {
    MakeNextPitTrapFlooded = makeNextPitTrapFlooded;
    UseSkewedDungeonEntranceHalls = useSkewedDungeonEntranceHalls;
    CreatedDungeonEntranceOnSurface = createdDungeonEntranceOnSurface;
  }

  public void SetLastDungeonHall(int hallId)
  {
    if (hallId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(hallId));
    }

    LastDungeonHallId = hallId;
  }

  public void AdvanceIteration(int iteration)
  {
    if (iteration < Iteration)
    {
      throw new InvalidOperationException(
        "Dungeon generation iterations cannot move backwards.");
    }

    Iteration = iteration;
  }

  private static void ValidateFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
