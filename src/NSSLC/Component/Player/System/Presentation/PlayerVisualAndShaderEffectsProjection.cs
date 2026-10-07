namespace Terraria.Player.Presentation;

public sealed class PlayerVisualAndShaderEffectsProjection
{
  public PlayerVisualAndShaderEffectsSnapshot Project(
    in PlayerVisualAndShaderEffectsInput input)
  {
    return new PlayerVisualAndShaderEffectsSnapshot(
      input.DontStarveShader,
      input.NoirShader,
      input.EyebrellaCloud,
      input.Yoraiz0rEye,
      input.Yoraiz0rDarkness,
      input.HasUnicornHorn,
      input.HasAngelHalo,
      input.HasRainbowCursor,
      input.LeinforsHair,
      input.MusicBoxSilence,
      input.StardustMonolithShader,
      input.NebulaMonolithShader,
      input.VortexMonolithShader,
      input.SolarMonolithShader,
      input.MoonLordMonolithShader,
      input.BloodMoonMonolithShader,
      input.ShimmerMonolithShader,
      input.CrtMonolithShader,
      input.RetroMonolithShader,
      input.MusicBox,
      input.OverrideFishingBobber);
  }
}
