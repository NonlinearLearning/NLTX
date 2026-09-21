namespace Terraria.WorldInteraction.Wiring;

public sealed class WiringDeviceCooldownComponent
{
  private int _cannonCooldownTicks;
  private int _bunnyCannonCooldownTicks;
  private int _snowballCannonCooldownTicks;

  public int CannonCooldownTicks
  {
    get => _cannonCooldownTicks;
    internal set => _cannonCooldownTicks = ValidateNonNegative(value);
  }

  public int BunnyCannonCooldownTicks
  {
    get => _bunnyCannonCooldownTicks;
    internal set => _bunnyCannonCooldownTicks = ValidateNonNegative(value);
  }

  public int SnowballCannonCooldownTicks
  {
    get => _snowballCannonCooldownTicks;
    internal set => _snowballCannonCooldownTicks = ValidateNonNegative(value);
  }

  internal void Reset()
  {
    CannonCooldownTicks = 0;
    BunnyCannonCooldownTicks = 0;
    SnowballCannonCooldownTicks = 0;
  }

  private static int ValidateNonNegative(int value)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    return value;
  }
}
