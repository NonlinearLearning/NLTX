namespace Terraria.WorldGeneration.Dungeon.Features;

public sealed class DungeonFeatureLifecycleComponent
{
  public bool Generated { get; private set; }

  public void MarkGenerated()
  {
    Generated = true;
  }

  public void Reset()
  {
    Generated = false;
  }
}
