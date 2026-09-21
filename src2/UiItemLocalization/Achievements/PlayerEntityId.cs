namespace Terraria.UiItemLocalization.Achievements;

// crossSubsystemOwner: integration-review
public readonly record struct PlayerEntityId(long Value)
{
  public PlayerEntityId(long value)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public long Value { get; }
}
