namespace Terraria.WorldGeneration.Dungeon.Halls;

public sealed class DungeonHallLifecycleComponent
{
  public bool Calculated { get; private set; }

  public bool Generated { get; private set; }

  public bool Processed => Generated;

  public void MarkCalculated()
  {
    Calculated = true;
  }

  public void MarkGenerated()
  {
    if (!Calculated)
    {
      throw new InvalidOperationException(
        "A hall cannot be generated before it is calculated.");
    }

    Generated = true;
  }

  public void Reset()
  {
    Calculated = false;
    Generated = false;
  }
}
