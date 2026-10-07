namespace Terraria.WorldGeneration.Terrain.TreeTops;

public readonly record struct WorldTreeTopsStyleChangeResult(
  int AreaId,
  int PreviousStyle,
  int CurrentStyle)
{
  public bool Changed => PreviousStyle != CurrentStyle;
}
