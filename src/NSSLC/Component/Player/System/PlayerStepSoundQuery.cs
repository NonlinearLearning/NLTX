namespace Terraria.Player;

public static class PlayerStepSoundQuery
{
  private const int DefaultSoundType = 17;
  private const int DefaultSoundStyle = -1;
  private const int DefaultIntendedCooldown = 9;
  private const int SpecialLegArmorId = 140;
  private const int SpecialSoundType = 2;
  private const int SpecialSoundStyle = 24;
  private const int SpecialIntendedCooldown = 6;

  public static PlayerStepSoundProjection Calculate(
    in PlayerStepSoundInput input)
  {
    if (input.LegArmorId == SpecialLegArmorId)
    {
      return new PlayerStepSoundProjection(
        SpecialSoundType,
        SpecialSoundStyle,
        SpecialIntendedCooldown);
    }

    return new PlayerStepSoundProjection(
      DefaultSoundType,
      DefaultSoundStyle,
      DefaultIntendedCooldown);
  }
}
