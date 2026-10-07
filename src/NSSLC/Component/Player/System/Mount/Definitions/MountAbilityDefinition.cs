namespace Terraria.Player.Mount;

public readonly record struct MountAbilityDefinition(
  int ChargeMax,
  int CooldownTicks,
  int DurationTicks)
{
  public void Validate()
  {
    if (ChargeMax < 0 || CooldownTicks < 0 || DurationTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ChargeMax));
    }
  }
}
