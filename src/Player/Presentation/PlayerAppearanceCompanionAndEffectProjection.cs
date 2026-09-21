namespace Terraria.Player.Presentation;

public sealed class PlayerAppearanceCompanionAndEffectProjection
{
  public PlayerAppearanceCompanionAndEffectSnapshot Project(
    in PlayerAppearanceCompanionAndEffectInput input)
  {
    return new PlayerAppearanceCompanionAndEffectSnapshot(
      input.Pet,
      input.Light,
      input.Yorai,
      input.PortableStool,
      input.UnicornHorn,
      input.AngelHalo,
      input.Beard,
      input.Minion,
      input.LeinShampoo,
      input.FlameWaker,
      input.Coat);
  }
}
