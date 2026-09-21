namespace Terraria.WorldGeneration.Components;

public sealed class WorldTileMergePassContext
{
  public bool MergeUp { get; private set; }

  public bool MergeDown { get; private set; }

  public bool MergeLeft { get; private set; }

  public bool MergeRight { get; private set; }

  public void Replace(bool mergeUp, bool mergeDown, bool mergeLeft, bool mergeRight)
  {
    MergeUp = mergeUp;
    MergeDown = mergeDown;
    MergeLeft = mergeLeft;
    MergeRight = mergeRight;
  }

  public void Clear()
  {
    MergeUp = false;
    MergeDown = false;
    MergeLeft = false;
    MergeRight = false;
  }
}
