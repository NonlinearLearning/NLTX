namespace Terraria.Player.Presentation;

public sealed class PlayerTraversalColorProjection
{
  public PlayerTraversalColorSnapshot Project(
    in PlayerTraversalColorInput input)
  {
    return new PlayerTraversalColorSnapshot(
      input.Wings,
      input.Carpet,
      input.FloatingTube,
      input.Grapple,
      input.Mount,
      input.Minecart);
  }
}
