using Terraria.Content.Items;

namespace Terraria.Combat.Items;

public static class ItemResourceUseQuery
{
  public enum RejectionReason
  {
    None,
    InsufficientMana
  }

  public readonly record struct ResourceAvailability
  {
    public ResourceAvailability(int currentLife, int maxLife, int currentMana, int maxMana)
    {
      if (currentLife < 0 || maxLife < 0 || currentLife > maxLife)
      {
        throw new ArgumentOutOfRangeException(nameof(currentLife));
      }

      if (currentMana < 0 || maxMana < 0 || currentMana > maxMana)
      {
        throw new ArgumentOutOfRangeException(nameof(currentMana));
      }

      CurrentLife = currentLife;
      MaxLife = maxLife;
      CurrentMana = currentMana;
      MaxMana = maxMana;
    }

    public int CurrentLife { get; }

    public int CurrentMana { get; }

    public int MaxLife { get; }

    public int MaxMana { get; }
  }

  public readonly record struct Result(
    bool CanUse,
    RejectionReason RejectionReason,
    int LifeRecovery,
    int ManaRecovery,
    int ManaCost,
    int ManaAfterCost);

  public static Result Evaluate(
    ItemResourceRecoveryDefinition definition,
    ResourceAvailability resources)
  {
    ArgumentNullException.ThrowIfNull(definition);

    if (definition.ManaCost > resources.CurrentMana)
    {
      return new Result(
        false,
        RejectionReason.InsufficientMana,
        definition.LifeRecovery,
        definition.ManaRecovery,
        definition.ManaCost,
        resources.CurrentMana);
    }

    return new Result(
      true,
      RejectionReason.None,
      definition.LifeRecovery,
      definition.ManaRecovery,
      definition.ManaCost,
      resources.CurrentMana - definition.ManaCost);
  }
}
