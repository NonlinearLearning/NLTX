namespace Terraria.Player;

public static class BuilderOverlayQuery
{
  public static PlayerBuilderOverlayProjection Calculate(
    in PlayerBuilderOverlayInput input)
  {
    return new PlayerBuilderOverlayProjection(
      input.RulerGridEnabled,
      input.RulerLineEnabled);
  }
}
