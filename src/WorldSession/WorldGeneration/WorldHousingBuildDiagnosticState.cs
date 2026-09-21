namespace Terraria.WorldGeneration.Components;

public sealed class WorldHousingBuildDiagnosticState
{
  public bool BuiltHouseWithNoFurniture { get; private set; }

  public bool BuiltHouseWithNoLight { get; private set; }

  public void MarkMissingFurniture()
  {
    BuiltHouseWithNoFurniture = true;
  }

  public void MarkMissingLight()
  {
    BuiltHouseWithNoLight = true;
  }

  public void Reset()
  {
    BuiltHouseWithNoFurniture = false;
    BuiltHouseWithNoLight = false;
  }
}
